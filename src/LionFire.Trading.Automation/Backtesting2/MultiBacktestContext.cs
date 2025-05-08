using Hjson;
using LionFire.IO;
using LionFire.Persistence.Handles;
using LionFire.Structures;
using LionFire.Trading.Automation.Journaling.Trades;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Journal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Registry;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation;

public class MultiBacktestContext
{
    #region Dependencies

    public IServiceProvider ServiceProvider { get; }
    public ResiliencePipeline? FilesystemRetryPipeline { get; }

    #endregion

    #region Id

    public Guid Guid { get; set; }

    #endregion

    #region Configuration

    public BacktestOptions BacktestOptions => ServiceProvider.GetRequiredService<IOptionsMonitor<BacktestOptions>>().CurrentValue;

    #endregion

    #region Parameters

    public PMultiBacktestContext Parameters { get; }
    public POptimization POptimization => Parameters.POptimization;

    public BacktestExecutionOptions ExecutionOptions
    {
        get => executionOptions ??= executionOptions
            ?? ServiceProvider.GetService<IOptionsMonitor<BacktestExecutionOptions>>()?.CurrentValue
            ?? new BacktestExecutionOptions();
        set => executionOptions = value;
    }
    private BacktestExecutionOptions? executionOptions;

    #region Derived

    public ExchangeSymbol ExchangeSymbol => Parameters.ExchangeSymbol;
    public Type BotType => Parameters.BotType;

    public CancellationToken CancellationToken { get; set; }

    #endregion

    #endregion

    #region Lifecycle

    public static async Task<MultiBacktestContext> Create(IServiceProvider serviceProvider, PMultiBacktestContext parameters)
    {
        MultiBacktestContext result = ActivatorUtilities.CreateInstance<MultiBacktestContext>(serviceProvider, parameters);

        var MachineName = Environment.MachineName; // REVIEW - make configurable

        OptimizationRunInfo optimizationRunInfo = new()
        {
            BotName = BotTyping.TryGetBotType(parameters.PBotType)?.Name ?? throw new ArgumentNullException("BotName"),

            ExchangeSymbol = parameters.ExchangeSymbol ?? throw new ArgumentNullException("ExchangeSymbol"),
            TimeFrame = parameters.PMultiBacktest.TimeFrame,
            Start = parameters.PMultiBacktest.Start ?? throw new ArgumentException(nameof(parameters.PMultiBacktest.Start)),
            EndExclusive = parameters.PMultiBacktest.EndExclusive ?? throw new ArgumentException(nameof(parameters.PMultiBacktest.EndExclusive)),

            TicksEnabled = parameters.PMultiBacktest.Features.Ticks(),

            BotAssemblyNameString = parameters.PBotType.Assembly.FullName ?? throw new ArgumentNullException("PBacktests[...].PBot"),
            BacktestExecutionDate = DateTime.UtcNow,

            MachineName = MachineName,
        };

        result.Guid = Guid.NewGuid();

        await result.Init().ConfigureAwait(false);

        return result;
    }

    public MultiBacktestContext(IServiceProvider serviceProvider, PMultiBacktestContext parameters)
    {
        ServiceProvider = serviceProvider;

        if (serviceProvider.GetService<ResiliencePipelineProvider<string>>()?.TryGetPipeline(FilesystemRetryPolicy.Default, out var p) == true)
        {
            FilesystemRetryPipeline = p;
        }

        Parameters = parameters;
        if (Parameters.POptimization == null) { throw new ArgumentNullException(nameof(Parameters.POptimization)); }
    }

    public async Task Init()
    {
        outputDirectory = await GetGuidOutputDirectory().ConfigureAwait(false);
    }

    #endregion

    #region State

    public BestJournalsTracker BestJournalsTracker => bestJournalsTracker ??= new(this);
    private BestJournalsTracker? bestJournalsTracker;
    public MultiBacktestEvents Events { get; } = new();

    //public int TradeJournalCount { get; set; }

    #region Derived

    //public bool ShouldLogTradeDetails => (POptimization == null || TradeJournalCount < POptimization?.MaxDetailedJournals);
    public bool ShouldLogTradeDetails => POptimization.TradeJournalOptions.EffectiveEnabled;

    #endregion
    #endregion

    #region LogDirectory

    // BLOCKING I/O, first time used
    public string OutputDirectory
    {
        get
        {
            if (outputDirectory == null)
            {
                throw new InvalidOperationException("Not initialized");
                //outputDirectory = GetNumberedRunDirectory(); // BLOCKING I/O
                //BatchInfoFileWriter = new(Path.Combine(OutputDirectory, $"BatchInfo.hjson"));
            }
            return outputDirectory;
        }
    }
    private string? outputDirectory;

    private string GetParentDirectory() // BLOCKING I/O
    {
        var path = BacktestOptions.Dir;

        string botTypeName = BotType.Name;

        if (BotType.IsGenericType)
        {
            int i = botTypeName.IndexOf('`');
            if (i >= 0) { botTypeName = botTypeName[..i]; }
        }

        if (ExecutionOptions.BotSubDir) { path = System.IO.Path.Combine(path, botTypeName); }
        if (ExecutionOptions.SymbolSubDir) { path = System.IO.Path.Combine(path, ExchangeSymbol?.Symbol ?? "UnknownSymbol"); }

        if (ExecutionOptions.TimeFrameDir)
        {
            path = System.IO.Path.Combine(path, Parameters.PMultiBacktest.TimeFrame?.ToString() ?? "UnknownTimeFrame");
        }
        if (ExecutionOptions.DateRangeDir)
        {
            path = System.IO.Path.Combine(path, DateTimeFormatting.ToConciseFileName(Parameters.PMultiBacktest.Start, Parameters.PMultiBacktest.EndExclusive));
        }
        //if (ExecutionOptions.ExchangeSubDir) { path = System.IO.Path.Combine(path, ExchangeSymbol?.Exchange ?? "UnknownExchange"); }
        if (ExecutionOptions.ExchangeAndAreaSubDir)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(ExchangeSymbol?.Exchange ?? "UnknownExchange");
            if (ExchangeSymbol?.ExchangeArea != null)
            {
                sb.Append(".");
                sb.Append(ExchangeSymbol.ExchangeArea);
            }
            path = System.IO.Path.Combine(path, sb.ToString());
        }

        return path;
    }

    private async Task<string> GetGuidOutputDirectory()
    {
        var dir = Path.Combine(GetParentDirectory(), Guid.ToString());
        await Task.Run(() =>
        {
            Debug.WriteLine("Creating directory...   " + dir);
            Directory.CreateDirectory(dir); // BLOCKING I/O
            Debug.WriteLine("Creating directory...done.  " + dir);
        }).ConfigureAwait(false);
        return dir;
    }

    private string GetNumberedRunDirectory() // BLOCKING I/O
    {
        var path = GetParentDirectory();

        path = FilesystemUtils.GetUniqueDirectory(path, "", "", 4); // BLOCKING I/O

        return path;
    }

    #endregion

    public OptimizationRunInfo? OptimizationRunInfo
    {
        get => optimizationRunInfo;
    }
    private OptimizationRunInfo optimizationRunInfo;
    private object optimizationRunInfoLock = new();

    HjsonOptions hjsonOptions = new HjsonOptions() { EmitRootBraces = false };

    public async Task TrySetOptimizationRunInfo(Func<OptimizationRunInfo> getter)
    {
        lock (optimizationRunInfoLock)
        {
            if (optimizationRunInfo != null) return;
            else optimizationRunInfo = getter();
        }

        await Task.Yield(); // Hjson is synchronous
        var json = JsonConvert.SerializeObject(optimizationRunInfo, new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
        });

        var hjsonValue = Hjson.JsonValue.Parse(json);
        var hjson = hjsonValue.ToString(hjsonOptions);

        if (FilesystemRetryPipeline != null)
        {
            await FilesystemRetryPipeline.ExecuteAsync(async _ =>
            {
                await write().ConfigureAwait(false);
                return ValueTask.CompletedTask;
            }).ConfigureAwait(false);
        }
        else { await write().ConfigureAwait(false); }

        async ValueTask write() => await File.WriteAllBytesAsync(Path.Combine(OutputDirectory, "OptimizationRunInfo.hjson"), System.Text.Encoding.UTF8.GetBytes(hjson));

    }

    //public UniqueFileWriter BatchInfoFileWriter { get; private set; }
}

