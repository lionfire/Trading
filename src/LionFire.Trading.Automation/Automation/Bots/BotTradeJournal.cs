using CsvHelper;
using CsvHelper.Configuration;
using LionFire.ExtensionMethods.Dumping;
using LionFire.Threading;
using LionFire.Trading.Automation;
using LionFire.Trading.Journal;
using MemoryPack;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Context;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Reactive.Concurrency;

namespace LionFire.Trading.Automation.Journaling.Trades;

/// <summary>
/// Records trades.
/// One per bot.
/// </summary>
/// <typeparam name="TPrecision"></typeparam>
public sealed partial class BotTradeJournal<TPrecision> : IBotTradeJournal<TPrecision>, IDisposable
    where TPrecision : struct, INumber<TPrecision>
{
    #region Dependencies

    private readonly ILogger<BotTradeJournal<TPrecision>> logger;
    readonly BatchContext<TPrecision> BatchContext;

    #region (Derived)

    private BestJournalsTracker bestJournalsTracker;

    public MultiSimContext MultiSimContext => BatchContext.MultiSimContext;

    #endregion

    #endregion

    #region Options

    public TradeJournalOptions Options => options;
    private readonly TradeJournalOptions options;


    private string? pathWithoutExtension { get; set; }
    private void UpdatePath() => pathWithoutExtension = Path.Combine(JournalDirectory ?? throw new ArgumentNullException(nameof(JournalDirectory)), contextName);
    public string? FileName { get; set; }
    public ExchangeSymbol? ExchangeSymbol
    {
        get => exchangeSymbol;
        set
        {
            exchangeSymbol = value;
            UpdatePath();
        }
    }

    public string ContextName
    {
        get => contextName;
        set
        {
            contextName = value;
            UpdatePath();
        }
    }
    private string contextName = "JobJournal-" + Guid.NewGuid();

    #region Derived

    public string? JournalDirectory => MultiSimContext.OutputDirectory; // OLD => Options?.JournalDir;
    //{
    //    get
    //    {
    //        var path = JournalDirectory;
    //        if (Options.ExchangeSubDir)
    //        {
    //            path = System.IO.Path.Combine(path, ExchangeSymbol?.Exchange ?? TradingConstants.UnknownExchange);
    //        }
    //        if (Options.ExchangeAndAreaSubDir && ExchangeSymbol?.DefaultExchangeArea != null)
    //        {
    //            path = System.IO.Path.Combine(path, ExchangeSymbol.DefaultExchangeArea);
    //        }
    //        if (Options.SymbolSubDir)
    //        {
    //            path = System.IO.Path.Combine(path, ExchangeSymbol?.DefaultSymbol ?? TradingConstants.UnknownSymbol);
    //        }
    //        return path;
    //    }
    //}

    #endregion

    #endregion

    #region Lifecycle

    public BotTradeJournal(IServiceProvider serviceProvider, ILogger<BotTradeJournal<TPrecision>> logger, TradeJournalOptions options, BatchContext<TPrecision> batchContext, ExchangeSymbol? exchangeSymbol = null)
    {
        BatchContext = batchContext;

        bestJournalsTracker = batchContext.MultiSimContext.Optimization.BestJournalsTracker;

        this.logger = logger;
        this.options = options;

        ExchangeSymbol = exchangeSymbol;
        UpdatePath();
    }

    public void Dispose()
    {
        if (entries == null) return;
        DisposeAsync().AsTask().Wait();
    }
    public ValueTask DisposeAsync()
    {
        entries = null!;
        return ValueTask.CompletedTask;
    }

    //private string GetPath(string filename) => Path.Combine(JournalDirectory ?? throw new ArgumentNullException(nameof(JournalDirectory)), filename);

    private async ValueTask OnClosed(string path)
    {
        //MultiSimContext.BestJournalsTracker = new BestJournalsTracker()
        if (FileName != null)
        {
            int i = 0;
            string newPath;

            string getPath() => Path.Combine(JournalDirectory ?? throw new ArgumentNullException(nameof(JournalDirectory)), FileName) + (i++ == 0 ? "" : $" ({i:000})") + Path.GetExtension(path);

            if (Options.ReplaceOutput && File.Exists(newPath = getPath())) // BLOCKING I/O
            {
                File.Delete(newPath); // BLOCKING I/O
            }
            else
            {
                do
                {
                    newPath = getPath();
                } while (File.Exists(newPath)); // BLOCKING I/O
            }

            if (DiscardDetails || !KeepAborted && IsAborted)
            {
                await DeleteWithRetry(path);
            }
            else
            {
                pathsToCleanupOnDiscard?.Add(newPath);
                File.Move(path, newPath); // BLOCKING I/O
                pathsToCleanupOnDiscard?.Remove(newPath);
            }
        }
    }
    private static readonly bool KeepAborted = false;

    public bool IsAborted { get; set; }
    public bool DiscardDetails { get; set; }
    public static async ValueTask DeleteWithRetry(string path, bool waitForFileToExist = false)
    {
        for (int retries = 0; retries < 30; retries++)
        {
            try
            {
                if (!waitForFileToExist && !File.Exists(path)) break;
                File.Delete(path);
                break;
            }
            catch { await Task.Delay(500); }
        }
    }

#if UNUSED
    public async ValueTask Close(string context)
    {
        if (fileStreams.TryRemove((context, JournalFormat.Binary), out var fs)) { fs.Dispose(); }
        if (fileStreams.TryRemove((context, JournalFormat.Text), out fs)) { fs.Dispose(); }
        if (csvWriters.TryRemove(context, out var csv))
        {
            await csv.FlushAsync();
            await csv.DisposeAsync();
        }
    }
#endif

    #region Delete saved files because we no longer exceed

    #endregion
    static async ValueTask WeGotBumped(object obj)
    {
        if (obj is Func<IEnumerable<string>?> func && func() is IEnumerable<string> list)
        {
            await Task.Delay(1000);
            foreach (var path in list.ToArray())
            {
                Debug.WriteLine($"Cleaning up discarded journal: {path}");
                await DeleteWithRetry(path);
            }
        }
    }

    List<string>? pathsToCleanupOnDiscard;

    public async ValueTask Finish(double fitness)
    {
        DiscardDetails |= !bestJournalsTracker.PeekShouldAdd(fitness);

        if (!DiscardDetails) { await _Write(forceWriteToDisk: true); }

        DiscardDetails |= !bestJournalsTracker.ShouldAdd(fitness, WeGotBumped, () => pathsToCleanupOnDiscard);

        if (fileStreams.Count > 0)
        {
            pathsToCleanupOnDiscard = new(fileStreams.Select(s => s.Value.Name));
        }

        {
            var copy = csvWriters?.ToArray();
            if (copy != null)
            {
                csvWriters = null!;
                foreach (var kvp in copy)
                {
                    if (!DiscardDetails)
                    {
                        await kvp.Value.FlushAsync();
                    }

                    kvp.Value.Dispose();
                }
            }
        }
        {
            var copy = fileStreams?.ToArray();
            if (copy != null)
            {
                fileStreams = null!;
                foreach (var kvp in copy.Where(c => c.Key.Item2 != JournalFormat.CSV)) { kvp.Value.Dispose(); }
                foreach (var c in copy) await OnClosed(c.Value.Name);
            }
        }
    }

    #endregion

    #region State

    private JournalStatsCollector statsCollector = new();
    public JournalStats JournalStats => statsCollector.JournalStats;

    ConcurrentDictionary<(string, JournalFormat), FileStream> fileStreams = new();
    ConcurrentDictionary<string, CsvWriter> csvWriters = new();

    #endregion

    #region ITradeJournal

    AsyncLock writeLockBinary = new();
    AsyncLock writeLock = new();
    private ExchangeSymbol? exchangeSymbol;

    ConcurrentQueue<JournalEntry<TPrecision>> entries = new(); // TODO: Replace with channel

    public IEnumerable<JournalEntry<TPrecision>> MemoryEntries => memoryEntries ?? Enumerable.Empty<JournalEntry<TPrecision>>();
    ConcurrentQueue<JournalEntry<TPrecision>>? memoryEntries = new();

    public bool IsDisposed => entries == null;

    Dictionary<int, IPosition<TPrecision>> openPositions = new();

    void UpdateStats(JournalEntry<TPrecision> entry)
    {
        switch (entry.EntryType)
        {
            case JournalEntryType.Unspecified:
                break;
            case JournalEntryType.Open:
                //openPositions.Add(entry.)
                break;
            case JournalEntryType.Close:

                statsCollector.OnClose(entry);
                break;
            case JournalEntryType.Modify:
                break;
            case JournalEntryType.CreateOrder:
                break;
            case JournalEntryType.ModifyOrder:
                break;
            case JournalEntryType.CancelOrder:
                break;
            case JournalEntryType.SwapFee:
                break;
            case JournalEntryType.InterestFee:
                break;
            case JournalEntryType.Abort:
                break;
            case JournalEntryType.Start:
                break;
            case JournalEntryType.End:
                break;
            default:
                break;
        }

    }

    /// <summary>
    /// Contains async, but is fire and forget.
    /// </summary>
    /// <param name="entry"></param>
    public void Write(JournalEntry<TPrecision> entry)
    {
        UpdateStats(entry);
        memoryEntries?.Enqueue(entry);
        if (entry.EntryType == JournalEntryType.Abort) IsAborted = true;

        if (!Options.EffectiveEnabled) return;
        entries.Enqueue(entry);
        if (entries.Count > Options.BufferEntries) { _Write(forceWriteToDisk: true).FireAndForget(); }
    }

    private async Task _Write(bool forceWriteToDisk = false)
    {
        if (!Options.EffectiveEnabled || DiscardDetails) return;
        if (!forceWriteToDisk && Options.PreferInMemory) return;

        if (entries == null) { return; } // Disposed
        while (entries.TryDequeue(out var entry))
        {
            if (Options.LogLevel != LogLevel.None)
            {
                logger.Log(Options.LogLevel, $"JobJournal: {entry.Dump()}");
            }

            if ((Options.JournalFormat & JournalFormat.Binary) == JournalFormat.Binary)
            {
                var bin = MemoryPackSerializer.Serialize(entry);
                var stream = GetStream(JournalFormat.Binary);

                await stream.WriteAsync(bin)
                    //.ConfigureAwait(false)
                    ;
            }

            if ((Options.JournalFormat & JournalFormat.CSV) == JournalFormat.CSV)
            {
                var writer = GetCsvWriter();
                writeLock.Wait(() =>
                {
                    //{
                    //await writer.WriteRecordsAsync([entry]);
                    //writer.WriteRecord(entry);
                    writer.WriteRecords([entry]);
                    //}
                    //await writer.FlushAsync();
                });
            }

            if ((Options.JournalFormat & JournalFormat.Text) == JournalFormat.Text)
            {
                var stream = GetStream(JournalFormat.Text);
                await stream.WriteAsync(Encoding.UTF8.GetBytes(entry.ToXamlProperties() + Environment.NewLine)).ConfigureAwait(false);
            }
        }
    }

    #endregion

    #region (Private) IO

    private readonly CsvConfiguration CsvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        //HasHeaderRecord = true,
        //NewLine = "\n",
        NewLine = "\r\n",
    };

    CsvWriter GetCsvWriter()
    {
        return csvWriters.GetOrAdd(ContextName, key =>
        {
            //var ext = Options.CsvSeparator switch
            //{
            //    ',' => ".csv",
            //    ';' => ".csv",
            //    '\t' => ".tsv",
            //    _ => ".csv",
            //};

            //var fs = new FileStream(Path.Combine(JournalDirectory, key + ext), FileMode.Append, FileAccess.Write, FileShare.Read);
            var writer = new StreamWriter(GetStream(JournalFormat.CSV));
            var csv = new CsvWriter(writer, CsvConfiguration);

            //Write(new TradeJournalEntry<TPrecision>
            //{
            //    EntryType = JournalEntryType.Start,
            //    Time = DateTimeOffset.Now,
            //}, fs);
            return csv;
        });
    }

    FileStream GetStream(JournalFormat journalFormat)
    {
        return fileStreams.GetOrAdd((ContextName, journalFormat), key =>
        {
            var ext = journalFormat switch
            {
                JournalFormat.Binary => ".dat",
                JournalFormat.Text => ".txt",
                JournalFormat.CSV => Options.CsvSeparator switch
                {
                    '\t' => ".tsv",
                    _ => ".csv",
                },
                _ => throw new NotSupportedException($"Specify a single {nameof(journalFormat)}"),
            };

            if (!Directory.Exists(Path.GetDirectoryName(pathWithoutExtension)))
            {
                Debug.WriteLine($"WARNING - dir missing: {JournalDirectory}");
                Debug.WriteLine(Environment.StackTrace);

                //return null;
            }

            var fs = new FileStream(pathWithoutExtension + ext, FileMode.Append, FileAccess.Write, FileShare.Read);

            //Write(new TradeJournalEntry<TPrecision>
            //{
            //    EntryType = JournalEntryType.Start,
            //    Time = DateTimeOffset.Now,
            //}, fs);
            return fs;
        });
    }

    #endregion

}
