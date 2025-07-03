
using YamlDotNet.Core.Tokens;

namespace LionFire.Trading.Automation;

/// <summary>
/// 1 or more batches of backtests.
/// Must all have the same start and end date.
/// 
/// There may be more than one BacktestJob per MultiSimContext (which could be shared for an Optimization Run)
/// </summary>
public sealed class BacktestJob
{
    #region Identity

    public Guid Guid { get; }

    #endregion

    #region Parent

    /// <summary>
    /// Multiple jobs may be run in the same MultiSimContext, as part of an optimization job.
    /// </summary>
    public MultiSimContext MultiSimContext { get; }

    #region (Convenience)

    public CancellationToken CancellationToken => MultiSimContext.CancellationToken;

    #endregion

    #endregion

    #region Backtest parameters

    /// <summary>
    /// An enumerable of enumerables of backtests.
    /// After each round of backtests, the producer may decide on the next round, such as in the case of 
    /// a non-comprehensive optimization that will delve deeper into paths that seem most promising.
    /// </summary>
    [SetOnce]
    public IEnumerable<IEnumerable<PBotWrapper>> BacktestBatches
    {
        get => backtestBatches;
        //private set
        //{
        //    ArgumentNullException.ThrowIfNull(value, nameof(value));

        //    backtestBatches = value;

        //    //var firstParameter = value.SelectMany(x => x).FirstOrDefault();// value.FirstOrDefault()?.FirstOrDefault();
        //    //if (firstParameter == null) throw new ArgumentException("Must contain at least one backtest");

        //    //DefaultTimeFrame = firstParameter.DefaultTimeFrame
        //    //    ?? throw new ArgumentNullException("firstParameter?.DefaultTimeFrame") // REVIEW - is it ever valid for this to be null
        //    //                                                                    //?? (firstParameter?.PBot as IPTimeFrameMarketProcessor)?.DefaultTimeFrame
        //    //    ;

        //    //ExchangeSymbol = firstParameter.ExchangeSymbol
        //    //    ?? (firstParameter.PBot as IPSymbolBot2)?.ExchangeSymbol
        //    //    ;
        //}
    }
    private readonly IEnumerable<IEnumerable<PBotWrapper>> backtestBatches;

    #region Derived (convenience)

    #region Backtests: Alternative collections

    //public IEnumerable<PBotWrapper> Backtests { set => BacktestBatches = [value]; }
    //public PBotWrapper Backtest { set => BacktestBatches = [[value]]; }

    public int Count => BacktestBatches.Aggregate(0, (acc, batch) => acc + batch.Count());

    #endregion

    #endregion

    #endregion

    #region Lifecycle

    public BacktestJob(Guid guid, MultiSimContext context)
    {
        Guid = guid;
        MultiSimContext = context;
    }

    public BacktestJob(Guid guid, MultiSimContext context, IEnumerable<IEnumerable<PBotWrapper>> backtestBatches) : this(guid, context)
    {
        ArgumentNullException.ThrowIfNull(backtestBatches, nameof(backtestBatches));
        this.backtestBatches = backtestBatches;
    }

    public BacktestJob(Guid guid, MultiSimContext context, List<PBotWrapper> backtests) : this(guid, context)
    {
        this.backtestBatches = [backtests];
    }

    public BacktestJob(Guid guid, MultiSimContext context, PBotWrapper backtest) : this(guid, context)
    {
        this.backtestBatches = [[backtest]];
    }


    #endregion

}
