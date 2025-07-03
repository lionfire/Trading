using CsvHelper;
using CsvHelper.Configuration;
using DynamicData;
using LionFire.IO;
using LionFire.Persistence.Persisters;
using LionFire.Resilience;
using LionFire.Serialization.Csv;
using LionFire.Structures;
using LionFire.Trading.Automation;
using Polly;
using Polly.Registry;
using ReactiveUI;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;

/// <summary>
/// One per MultiSimContext (Optimization context)
/// - Unlimited backtests from unlimited sims
/// </summary>
public class BacktestsJournal : IAsyncDisposable
{
    #region Dependencies

    ILogger<BacktestsJournal> Logger { get; }
    public IOptionsMonitor<OptimizationOptions> OptimizationOptionsMonitor { get; }
    public IOptionsMonitor<BacktestRepositoryOptions> BacktestRepositoryOptionsMonitor { get; }

    ResiliencePipeline fsRetry;

    #endregion

    #region Parameters

    public static string DefaultJournalFilename { get; set; } = "backtests";
    public string JournalFilename { get; set; } = DefaultJournalFilename;
    public MultiSimContext Context { get; }
    public string BatchDirectory => Context.OutputDirectory;
    public Type PBotType { get; }
    public bool RetainInMemory { get; }

    #region Derived

    //private ParameterMetadata ParameterMetadata { get; }
    public bool ZipOnDispose => BacktestRepositoryOptionsMonitor.CurrentValue.ZipOutput;

    #endregion

    #endregion

    #region Lifecycle

    public BacktestsJournal(MultiSimContext context, 
        Type pBotType, 
        ResiliencePipelineProvider<string> resiliencePipelineProvider, 
        ILogger<BacktestsJournal> logger, 
        IOptionsMonitor<OptimizationOptions> optimizationOptionsMonitor,
        IOptionsMonitor<BacktestRepositoryOptions> backtestRepositoryOptionsMonitor,
        bool retainInMemory = false)
    {
        fsRetry = resiliencePipelineProvider.GetPipeline(FilesystemRetryPolicy.Default);

        Context = context;

        var path = Path.Combine(Context.OutputDirectory, JournalFilename + ".csv");
        var fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        var writer = new StreamWriter(fs);
        csv = new CsvWriter(writer, BacktestBatchJournalCsvSerialization.CsvConfiguration);

        consumeTask = Consume();
        PBotType = pBotType;
        RetainInMemory = retainInMemory;
        Logger = logger;
        OptimizationOptionsMonitor = optimizationOptionsMonitor;
        BacktestRepositoryOptionsMonitor = backtestRepositoryOptionsMonitor;
        if (retainInMemory)
        {
            sourceCache = new(e => (e.BatchId, e.Id));
        }
        csv.Context.RegisterClassMap(new ParametersMapper(pBotType));
        //typeof(CsvContext).GetMethod(nameof(CsvContext.RegisterClassMap), new Type[] { })!.MakeGenericMethod(mapType).Invoke(csv.MultiSimContext, null);
    }

    public IObservableCache<BacktestBatchJournalEntry, (int, long)>? ObservableCache => sourceCache;
    SourceCache<BacktestBatchJournalEntry, (int, long)>? sourceCache = null;

    public class ParametersMapper : DynamicSplitMap<BacktestBatchJournalEntry>
    {
        static JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
            Converters = { 
                       //new System.Text.Json.Serialization.JsonStringEnumConverter()
                       new IgnoreEmptyArrayConverter<IKeyed<string>>(),
                   }
        };
        public ParametersMapper(Type pType) : base(typeof(BacktestBatchJournalEntry).GetProperty(nameof(BacktestBatchJournalEntry.Parameters))!, pType, true)
        {
            Map(m => m.BatchId);
            Map(m => m.Id);
            Map(m => m.Fitness);
            Map(m => m.AD);
            Map(m => m.AMWT);
            Map(m => m.Wins);
            Map(m => m.Losses);
            Map(m => m.Breakevens);
            //Map(m => m.UnknownTrades);
            Map(m => m.MaxBalanceDrawdown);
            Map(m => m.MaxBalanceDrawdownPerunum);
            //Map(m => m.MaxEquityDrawdown);
            //Map(m => m.MaxEquityDrawdownPerunum);
            InitBase();

            //Map(m => m.PMultiSim).Convert(row =>
            //{
            //    //var entry = row.Value;
            //    //.GetRecord<BacktestBatchJournalEntry>();
            //    //foreach(var pi in ParameterMetadata)
            //    //{
            //    //    object value = pi.GetValue(entry);
            //    //}
            //    var json = System.Text.Json.JsonSerializer.Serialize(row.Value.PMultiSim, options);
            //    //var json =  JsonConvert.SerializeObject(row.Value.PMultiSim);
            //    var hjson = JsonValue.Parse(json).ToString(Stringify.Hjson);
            //    return json;
            //    //return string.Join(", ", row.Value.PMultiSim!); // parameterMetadata.Items.Select(kvp => kvp.GetValue(row.Value)));
            //});

            //var bpi = BotParametersInfo.Get(pType);
            //foreach (var kvp in bpi.PMultiSim)
            //{
            //    //Map(_ => kvp.Key).Convert((ConvertFromString<string>) (row =>
            //    //{
            //    // TODO
            //    //}));
            //    //Map(kvp.Value.PropertyInfo.DeclaringType!, kvp.Value.PropertyInfo)

            //    Map(e => kvp.Key).Convert((ConvertToString<BacktestBatchJournalEntry>)(row =>
            //    {
            //        return PropertyFlattener.GetValueFromPath(row.Value.PMultiSim, kvp.Key)
            //            ?.ToString() // TODO: Serialize using Json or Hjson?
            //            ;
            //    }));
            //}
        }
    }

    Task consumeTask;

    public async ValueTask DisposeAsync()
    {
        channel.Writer.Complete();

        while (channel.Reader.TryPeek(out var _))
        {
            Debug.WriteLine($"{this.GetType().Name} - Waiting for Reader to be emptied. Remaining: {channel.Reader.Count}");
            await Task.Delay(100);
        }
        if (csv != null) { await csv.DisposeAsync(); }

        if (ZipOnDispose) { await ZipBatchDir(); }
    }

    public async ValueTask ZipBatchDir()
    {
        var zipPath = Path.Combine(Path.GetDirectoryName(BatchDirectory)!, Path.GetFileName(BatchDirectory) + ".zip");

       await Task.Delay(100);
       await fsRetry.ExecuteAsync(ct =>
        {
            if (File.Exists(zipPath))
            {
                Logger.LogWarning("Deleting failed zip file before retrying: {0}", zipPath);
                try
                {
                    File.Delete(zipPath);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Failed to delete failed zip file: {0}", zipPath);
                    throw;
                }
            }
                ZipFile.CreateFromDirectory(BatchDirectory, zipPath);
                Directory.Delete(BatchDirectory, true);
            return ValueTask.CompletedTask;
        });

    }

    #endregion

    #region State

    CsvWriter csv;
    Channel<BacktestBatchJournalEntry> channel = Channel.CreateUnbounded<BacktestBatchJournalEntry>();

    #endregion

    #region (Public) Methods

    public bool TryWrite(BacktestBatchJournalEntry e)
    {
        sourceCache?.AddOrUpdate(e);
        return channel.Writer.TryWrite(e);
    }
    public async ValueTask WriteAsync(BacktestBatchJournalEntry e)
    {
        sourceCache?.AddOrUpdate(e);
        await channel.Writer.WriteAsync(e);
    }

    public int FlushEvery { get; set; } = 50;
    public async Task Consume()
    {
        //CancellationTokenSource localCTS = new();

        //_ = Task.Run(() =>
        //{
        //    ((ManualResetEvent)MultiSimContext.CancellationToken.WaitHandle).WaitOne();
        //}
        //    );

        int flushCounter = 0;
        int flushEvery = FlushEvery;
        //try
        //{
        await foreach (var item in channel.Reader.ReadAllAsync()
            //.WithCancellation(MultiSimContext.CancellationToken)
            )
        {
            await csv.WriteRecordsAsync([item]);
            if (flushCounter++ == flushEvery) { await csv.FlushAsync(); flushCounter = 0; }
        }
        //}
        //catch (OperationCanceledException) { }
    }

    #endregion
}

//public class ParameterMetadata
//{
//    #region (static)

//    private static ConcurrentDictionary<Type, ParameterMetadata> cache = new();

//    public static ParameterMetadata Get(Type type)
//    {
//        return cache.GetOrAdd(type, k => new ParameterMetadata(type));
//    }
//    #endregion

//    public List<Key> Items { get; } = new();

//    public ParameterMetadata(Type type)
//    {

//        foreach (var pi in type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
//        {
//            //if (pi.GetCustomAttributes<SignalAttribute>().Any()) continue;

//            //if (!pi.PropertyType.IsPrimitive) continue;

//            Items.Add(pi);
//        }
//    }
//}