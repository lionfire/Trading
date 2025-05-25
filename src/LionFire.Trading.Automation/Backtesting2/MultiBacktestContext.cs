using Hjson;
using LionFire.IO;
using LionFire.Persistence.Handles;
using LionFire.Structures;
using LionFire.Trading.Automation.Journaling.Trades;
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Journal;
using LionFire.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly;
using Polly.Registry;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation;

public class MultiBacktestContext : IValidatable
{
    #region Dependencies

    public IServiceProvider ServiceProvider { get; }
    public ResiliencePipeline? FilesystemRetryPipeline { get; }

    public BacktestsRepository BacktestsRepository { get; }

    #endregion

    #region Id

    public string Guid { get; set; }

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

    public ExchangeSymbolTimeFrame? ExchangeSymbolTimeFrame => Parameters.PMultiBacktest.ExchangeSymbolTimeFrame;
    public Type? BotType => Parameters.BotType;

    public CancellationToken CancellationToken { get; set; }

    #endregion

    #endregion

    #region Lifecycle

    public static async Task<MultiBacktestContext> Create(IServiceProvider serviceProvider, PMultiBacktestContext parameters)
    {
        parameters.ValidateOrThrow();

        var MachineName = Environment.MachineName; // REVIEW - make configurable

        OptimizationRunInfo optimizationRunInfo = new()
        {
            //Guid = Guid.NewGuid(),
            BotName = BotTyping.TryGetBotType(parameters.PBotType!)?.Name ?? throw new ArgumentNullException("BotName"),
            BotTypeName = parameters.PBotType!.FullName,

            ExchangeSymbolTimeFrame = parameters.PMultiBacktest.ExchangeSymbolTimeFrame ?? throw new ArgumentNullException("ExchangeSymbol"),

            Start = parameters.PMultiBacktest.Start ?? throw new ArgumentException(nameof(parameters.PMultiBacktest.Start)),
            EndExclusive = parameters.PMultiBacktest.EndExclusive ?? throw new ArgumentException(nameof(parameters.PMultiBacktest.EndExclusive)),

            TicksEnabled = parameters.PMultiBacktest.Features.Ticks(),

            BotAssemblyNameString = parameters.PBotType!.Assembly.FullName ?? throw new ArgumentNullException(nameof(OptimizationRunInfo.BotAssemblyNameString)),
            OptimizationExecutionDate = DateTime.UtcNow,

            MachineName = MachineName,

        };
        optimizationRunInfo.TryHydrateBuildDates(parameters.PBotType!);

        //OptimizationRunInfo getOptimizationRunInfo()
        //{
        //    var p = Context.Parameters.PMultiBacktest;
        //    return new()
        //    {
        //        ExchangeSymbol = p.ExchangeSymbolTimeFrame ?? throw new ArgumentNullException(nameof(p.ExchangeSymbolTimeFrame)),
        //        TimeFrame = p.TimeFrame,

        //        Start = p.Start!.Value,
        //        EndExclusive = p.EndExclusive!.Value,
        //        TicksEnabled = p.Features.HasFlag(BotHarnessFeatures.Ticks),
        //        //BotName = ,

        //        BotAssemblyNameString = p.PBotType!.Assembly.FullName ?? throw new ArgumentNullException("PBacktests[...].PBot"),
        //        OptimizationExecutionDate = DateTime.UtcNow,
        //        MachineName = MachineName,
        //    };
        //}

        MultiBacktestContext result = ActivatorUtilities.CreateInstance<MultiBacktestContext>(serviceProvider, parameters);
        //result.Guid = optimizationRunInfo.Guid!;
        result.OptimizationRunInfo = optimizationRunInfo;

        await result.Init().ConfigureAwait(false);
        return result;
    }

    public MultiBacktestContext(IServiceProvider serviceProvider, BacktestsRepository backtestsRepository, PMultiBacktestContext parameters)
    {
        ServiceProvider = serviceProvider;
        BacktestsRepository = backtestsRepository;

        if (serviceProvider.GetService<ResiliencePipelineProvider<string>>()?.TryGetPipeline(FilesystemRetryPolicy.Default, out var p) == true)
        {
            FilesystemRetryPipeline = p;
        }

        Parameters = parameters;
        if (Parameters.POptimization == null) { throw new ArgumentNullException(nameof(Parameters.POptimization)); }
    }

    public async Task Init()
    {
        var botTypeName = BacktestsRepository.BotTypeRegistry.GetBotName(BotType!);
        
        (outputDirectory, Guid) = await BacktestsRepository.GetAndCreateOptimizationRunDirectory(
            ExchangeSymbolTimeFrame!,
            botTypeName,
            Parameters.PMultiBacktest.Start!.Value,
            Parameters.PMultiBacktest.EndExclusive!.Value
            //,Guid.ToString()
            ).ConfigureAwait(false);
        await WriteOptimizationRunInfo();
    }

    #endregion

    #region Validation

    public ValidationContext ValidateThis(ValidationContext validationContext)
    {
        return validationContext
            .PropertyNotNull(nameof(OptimizationRunInfo), OptimizationRunInfo)
            .Validate(POptimization)
            ;
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



    #endregion

    public OptimizationRunInfo? OptimizationRunInfo { get; set; }
    //    => optimizationRunInfo;
    //private OptimizationRunInfo optimizationRunInfo;
    //private object optimizationRunInfoLock = new();

    HjsonOptions hjsonOptions = new HjsonOptions() { EmitRootBraces = false };

    private async Task WriteOptimizationRunInfo(
        //Func<OptimizationRunInfo> getter
        )
    {
        ArgumentNullException.ThrowIfNull(OptimizationRunInfo);

        //lock (optimizationRunInfoLock)
        //{
        //    if (optimizationRunInfo != null) return;
        //    else optimizationRunInfo = getter();
        //}

        //await Task.Yield(); // Hjson is synchronous
        var json = JsonConvert.SerializeObject(OptimizationRunInfo, new JsonSerializerSettings
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

