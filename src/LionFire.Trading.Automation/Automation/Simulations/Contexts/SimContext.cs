
using LionFire.Trading.Automation.Optimization;
using LionFire.Trading.Journal;
using LionFire.Trading.ValueWindows;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using Durian;

namespace LionFire.Trading.Automation;

/// <summary>
/// May only have one bot.
/// Backtesting batches: many bots.
/// </summary>
public interface ISimContext
{
}

/// <summary>
/// Context for a Sim:
/// - State machine that traverses forward only from Start to EndExclusive (or it may never end)
/// 
/// Uses:
/// - Backtesting: BatchContext
/// - Single live bot: TODO
/// </summary>
/// <typeparam name = "TPrecision" ></ typeparam >
//[FriendClass(typeof(BatchHarness<>))]  // TODO - get this working again. Not sure why it broke
public class SimContext<TPrecision> : ISimContext
where TPrecision : struct, INumber<TPrecision>
{
    #region Id

    public Guid Guid { get; } = Guid.NewGuid();

    #endregion

    #region Parent

    public MultiSimContext MultiSimContext { get; init; }

    #endregion

    #region Dependencies

    #region Derived

    public IServiceProvider ServiceProvider => MultiSimContext.ServiceProvider;

    #endregion

    #endregion

    #region Child Components

    #region Derived

    //public POptimization? POptimization => MultiSimContext?.Parameters.POptimization;

    #endregion

    #endregion

    #region Parameters

    public PMultiSim PMultiSim => MultiSimContext.Parameters;
    //PMultiSim ISimContext.Parameters => PSimContext;

    #region Convenience

    //public DateTimeOffset Start => MultiSimContext.Parameters.Start;
    //public DateTimeOffset EndExclusive => MultiSimContext.Parameters.EndExclusive;

    #endregion

    #endregion

    #region Lifecycle

    public SimContext(MultiSimContext multiSimContext)
    {
        MultiSimContext = multiSimContext;

        if (PMultiSim.DefaultExchangeArea != null)
        {
            DefaultAccount = new PSimAccount<TPrecision>(PMultiSim.DefaultExchangeArea)
            {
                DefaultHolding = PSimulatedHolding<TPrecision>.DefaultForBacktesting
            };
            //DefaultAccount = new SimAccount<TPrecision>(this, );
        }
    }

    internal void Start()
    {
        SimulatedCurrentDate = MultiSimContext.Parameters.Start;

        if (CancellationTokenSource != null) { throw new AlreadyException(); }
        CancellationTokenSource = new();
        EffectiveCancellationTokenSource = CancellationTokenSource;
        //if (cancellationToken.CanBeCanceled) // OLD
        //{
        //    EffectiveCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(CancellationTokenSource.Token, cancellationToken);
        //}
    }

    public void Cancel() => CancellationTokenSource?.Cancel();

    #endregion

    #region State

    #region Simulation Time

    public DateTimeOffset SimulatedCurrentDate { get; internal set; }

    #region (Derived)

    /// <summary>
    /// REVIEW: Don't rely on this until the calculation can be confirmed to be robust, but it may be useful for information purposes
    /// </summary>
    public bool IsKeepingUpWithReality =>
         PMultiSim.DefaultTimeFrame?.HasFixedTimeSpan == true
        ? (SimulatedCurrentDate - DateTimeOffset.UtcNow < PMultiSim.DefaultTimeFrame!.TimeSpan * 2)
        : false;

    #endregion

    #endregion

    #region Is Cancelled

    public bool IsCancelled => EffectiveCancellationTokenSource?.IsCancellationRequested == true;

    /// <remarks>
    /// If not null, the task has already been started.
    /// </remarks>
    private CancellationTokenSource? CancellationTokenSource;
    private CancellationTokenSource? EffectiveCancellationTokenSource; // OLD - always same as CTS, but TODO: Also observe MultiSimContext.CancellationToken

    #endregion


    #endregion

    #region Events

    public CancellationToken CancellationToken => EffectiveCancellationTokenSource?.Token ?? CancellationToken.None;

    // TODO: On Finished or errored
    // TODO: On Caught up to present

    #endregion

    #region Accounts

    public PSimAccount<TPrecision>? DefaultAccount { get; }

    // Moved to bot
    //public Dictionary<ExchangeSymbol, ISimAccount<TPrecision>>? Accounts { get; }
    //IEnumerable<IAccount2<TPrecision>> Accounts => PrimarySimulatedAccount == null ? [] : [PrimarySimulatedAccount];

    #endregion

}

