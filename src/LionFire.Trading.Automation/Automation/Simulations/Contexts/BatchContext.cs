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
using NLog.LayoutRenderers.Wrappers;
using Polly;
using Polly.Registry;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation;



public interface IBatchContext : ISimContext
{
    #region Parent

    MultiSimContext MultiSimContext { get; }

    #endregion

    #region Parameters

    PBatch Parameters { get; }
    string OutputDirectory => MultiSimContext.OutputDirectory;

    // TODO: Eliminate set, and set once somewhere
    BacktestsJournal BacktestBatchJournal => MultiSimContext.Journal;

    #region Convenience

    /// <summary>
    /// Inherit parent configuration
    /// </summary>
    PMultiSim PMultiSim => MultiSimContext.Parameters;

    #endregion

    DateTimeOffset Start => PMultiSim.Start;
    DateTimeOffset EndExclusive => PMultiSim.EndExclusive;

    #endregion

    #region State

    DateTimeOffset SimulatedCurrentDate { get; }

    /// <summary>
    /// If true, this is not a backtest, but a live bot that is keeping up with real-time market data.
    /// </summary>
    bool IsKeepingUpWithReality { get; }

    #endregion

    #region Events

    CancellationToken CancellationToken { get; }

    #endregion

    // TRIAGE
    //bool TicksEnabled { get; }
    //void OnAccountAborted(ISimAccount<TPrecision> defaultSimAccount)
}


/// <summary>
/// Context for a Sim, with:
/// - bot backtesting support
/// 
/// </summary>
/// <typeparam name="TPrecision"></typeparam>
public sealed class BatchContext<TPrecision> : SimContext<TPrecision>, IValidatable, IBatchContext
    where TPrecision : struct, INumber<TPrecision>
{

    public ValidationContext ValidateThis(ValidationContext validationContext)
    {
        return validationContext;
        //return Parameters.ValidateThis(validationContext)
        //    .With(nameof(MultiSimContext), MultiSimContext)
        //    .With(nameof(Parameters), Parameters)
        //    .With(nameof(ExecutionOptions), ExecutionOptions);
    }


    #region Configuration

    public BacktestOptions BacktestOptions => ServiceProvider.GetRequiredService<IOptionsMonitor<BacktestOptions>>().CurrentValue;

    #endregion

    #region Parameters

    public PBatch Parameters { get; }

    #region (Derived)

    public POptimization? POptimization => MultiSimContext.Optimization.POptimization;

    #endregion

    public BacktestExecutionOptions ExecutionOptions
    {
        get => executionOptions ??= executionOptions
            ?? ServiceProvider.GetService<IOptionsMonitor<BacktestExecutionOptions>>()?.CurrentValue
            ?? new BacktestExecutionOptions();
        set => executionOptions = value;
    }
    private BacktestExecutionOptions? executionOptions;

    #region Derived

    public ExchangeSymbolTimeFrame? ExchangeSymbolTimeFrame => PMultiSim.ExchangeSymbolTimeFrame;
    public Type? BotType => Parameters.BotType;


    #endregion

    #endregion

    #region Lifecycle

    public BatchContext(MultiSimContext multiSimContext, PBatch parameters) : base(multiSimContext)
    {
        //parameters.ValidateOrThrow();
        Parameters = parameters;
    }

    #endregion

    #region State

    #region Bot Ids

    public long GetNextBotId() => NextBotId++;
    private long NextBotId = 0;

    #endregion

    public MultiBatchEvents BatchEvents { get; } = new();

    //public int TradeJournalCount { get; set; }

    #region Derived

    //public bool ShouldLogTradeDetails => (POptimization == null || TradeJournalCount < POptimization?.MaxDetailedJournals);

    #endregion
    #endregion


    //public UniqueFileWriter BatchInfoFileWriter { get; private set; }
}

