using System.Numerics;
using LionFire.Trading.Automation.FillSimulation;
using LionFire.Trading.Automation.PriceMonitoring;
using LionFire.Trading.PriceMonitoring;

namespace LionFire.Trading.Automation.Accounts;

/// <summary>
/// A paper trading account with infinite capital - balance never changes.
/// </summary>
/// <remarks>
/// <para>
/// This account type uses real-time market data with simulated order execution,
/// but unlike <see cref="LiveSimAccount{TPrecision}"/>, the balance never changes:
/// </para>
/// <list type="bullet">
///   <item><description>Balance always returns <see cref="LiveAccountOptions.InitialBalance"/></description></item>
///   <item><description>Equity always returns <see cref="LiveAccountOptions.InitialBalance"/></description></item>
///   <item><description>Positions are tracked but don't affect balance</description></item>
///   <item><description>The account can never "blow up"</description></item>
/// </list>
/// <para>
/// This is useful for testing strategies without capital constraints, focusing on
/// entry/exit logic rather than position sizing.
/// </para>
/// </remarks>
/// <typeparam name="TPrecision">The numeric precision type used for calculations.</typeparam>
public class LivePaperAccount<TPrecision> : LiveAccountBase<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    #region Properties

    /// <inheritdoc />
    public override BotAccountMode Mode => BotAccountMode.LivePaper;

    /// <inheritdoc />
    /// <remarks>
    /// For LivePaperAccount, balance always returns the configured initial balance.
    /// It never changes regardless of trading activity.
    /// </remarks>
    public override TPrecision Balance => InitialBalance;

    /// <inheritdoc />
    /// <remarks>
    /// For LivePaperAccount, equity always returns the configured initial balance.
    /// Unrealized P&amp;L does not affect the displayed equity.
    /// </remarks>
    public override TPrecision Equity => InitialBalance;

    #endregion

    #region Lifecycle

    /// <summary>
    /// Initializes a new instance of <see cref="LivePaperAccount{TPrecision}"/>.
    /// </summary>
    /// <param name="exchangeArea">The exchange and area for this account.</param>
    /// <param name="options">The account configuration options.</param>
    /// <param name="priceMonitor">Optional price monitor for getting current prices.</param>
    /// <param name="pendingOrderManager">Optional pending order manager for SL/TP orders.</param>
    /// <param name="fillSimulator">Optional fill simulator for calculating execution prices.</param>
    public LivePaperAccount(
        ExchangeArea exchangeArea,
        LiveAccountOptions? options = null,
        ILivePriceMonitor? priceMonitor = null,
        IPendingOrderManager<TPrecision>? pendingOrderManager = null,
        IFillSimulator<TPrecision>? fillSimulator = null)
        : base(exchangeArea, options ?? LiveAccountOptions.DefaultPaper, priceMonitor, pendingOrderManager, fillSimulator)
    {
    }

    #endregion

    #region Balance Tracking

    /// <inheritdoc />
    /// <remarks>
    /// For paper accounts, position closes still track P&amp;L but don't affect balance.
    /// </remarks>
    protected override void OnPositionClosed(TPrecision pnl)
    {
        // P&L is tracked in RealizedPnL (base class handles this)
        // but balance doesn't change for paper accounts
    }

    #endregion
}
