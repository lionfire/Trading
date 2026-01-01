using System.Numerics;
using LionFire.Trading.Automation.Accounts;

namespace LionFire.Trading.Automation;

/// <summary>
/// Provides the complete execution context for a trading bot running in live/real-time mode.
/// </summary>
/// <remarks>
/// <para>
/// This class wraps a <see cref="LiveContext{TPrecision}"/> and an <see cref="ILiveAccount{TPrecision}"/>
/// to provide all the services a bot needs for live trading operations.
/// </para>
///
/// <para>
/// This is the live equivalent of <see cref="BotContext{TPrecision}"/> which is used for backtesting.
/// While BotContext uses SimContext with simulated time, LiveBotContext uses LiveContext with real time.
/// </para>
///
/// <para>
/// Bots can access:
/// <list type="bullet">
///   <item><description>Current market time via <see cref="MarketContext"/>.<see cref="IMarketContext{TPrecision}.CurrentTime"/></description></item>
///   <item><description>Account operations via <see cref="DefaultAccount"/> or <see cref="LiveAccount"/></description></item>
///   <item><description>Dependency injection via <see cref="ServiceProvider"/></description></item>
///   <item><description>SL/TP order management via the account's order methods</description></item>
/// </list>
/// </para>
/// </remarks>
/// <typeparam name="TPrecision">The numeric precision type used for calculations (e.g., double, decimal).</typeparam>
public class LiveBotContext<TPrecision> : IBotContext2<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    #region Identity

    /// <inheritdoc />
    public string Id { get; }

    #endregion

    #region Dependencies

    /// <summary>
    /// Gets the underlying live market context.
    /// </summary>
    public LiveContext<TPrecision> LiveContext { get; }

    /// <inheritdoc />
    IMarketContext<TPrecision> IBotContext2<TPrecision>.MarketContext => LiveContext;

    /// <summary>
    /// Gets the live account with strongly-typed access to live account properties.
    /// </summary>
    public ILiveAccount<TPrecision> LiveAccount { get; }

    /// <inheritdoc />
    public IAccount2<TPrecision> DefaultAccount => LiveAccount;

    /// <inheritdoc />
    public IServiceProvider ServiceProvider => LiveContext.ServiceProvider;

    #endregion

    #region Configuration

    /// <inheritdoc />
    public TimeFrame TimeFrame { get; set; }

    /// <summary>
    /// Gets or sets the primary trading symbol for this bot context.
    /// </summary>
    public ExchangeSymbol? Symbol { get; set; }

    #endregion

    #region Lifecycle

    /// <summary>
    /// Initializes a new instance of <see cref="LiveBotContext{TPrecision}"/>.
    /// </summary>
    /// <param name="liveContext">The live market context to wrap.</param>
    /// <param name="account">The live account for trading operations.</param>
    /// <param name="timeFrame">The primary trading time frame.</param>
    /// <param name="id">Optional custom ID. If not provided, a new GUID will be generated.</param>
    public LiveBotContext(
        LiveContext<TPrecision> liveContext,
        ILiveAccount<TPrecision> account,
        TimeFrame timeFrame,
        string? id = null)
    {
        LiveContext = liveContext ?? throw new ArgumentNullException(nameof(liveContext));
        LiveAccount = account ?? throw new ArgumentNullException(nameof(account));
        TimeFrame = timeFrame ?? throw new ArgumentNullException(nameof(timeFrame));
        Id = id ?? Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Initializes a new instance of <see cref="LiveBotContext{TPrecision}"/> with a trading symbol.
    /// </summary>
    /// <param name="liveContext">The live market context to wrap.</param>
    /// <param name="account">The live account for trading operations.</param>
    /// <param name="symbol">The primary trading symbol.</param>
    /// <param name="timeFrame">The primary trading time frame.</param>
    /// <param name="id">Optional custom ID. If not provided, a new GUID will be generated.</param>
    public LiveBotContext(
        LiveContext<TPrecision> liveContext,
        ILiveAccount<TPrecision> account,
        ExchangeSymbol symbol,
        TimeFrame timeFrame,
        string? id = null)
        : this(liveContext, account, timeFrame, id)
    {
        Symbol = symbol;
    }

    #endregion

    #region Convenience Properties

    /// <summary>
    /// Gets the current UTC time from the market context.
    /// </summary>
    public DateTimeOffset CurrentTime => LiveContext.CurrentTime;

    /// <summary>
    /// Gets a value indicating whether this context is for live trading.
    /// </summary>
    /// <remarks>
    /// For LiveBotContext, this always returns <c>true</c>.
    /// </remarks>
    public bool IsLive => true;

    /// <summary>
    /// Gets the cancellation token for graceful shutdown.
    /// </summary>
    public CancellationToken CancellationToken => LiveContext.CancellationToken;

    /// <summary>
    /// Gets the account mode (LiveSimulated, LivePaper, or LiveReal).
    /// </summary>
    public BotAccountMode Mode => LiveAccount.Mode;

    /// <summary>
    /// Gets the current account balance.
    /// </summary>
    public TPrecision Balance => LiveAccount.Balance;

    /// <summary>
    /// Gets the current account equity.
    /// </summary>
    public TPrecision Equity => LiveAccount.Equity;

    #endregion
}
