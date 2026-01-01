using System.Numerics;

namespace LionFire.Trading.Automation;

/// <summary>
/// Represents the execution context for a trading bot, providing access to market context,
/// accounts, and services needed for trading operations.
/// </summary>
/// <remarks>
/// This interface is the primary context interface for bot execution and is implemented by:
/// <list type="bullet">
///   <item><description><see cref="BotContext{TPrecision}"/> - For backtesting with simulated accounts</description></item>
///   <item><description>LiveBotContext - For real-time trading with live or paper accounts</description></item>
/// </list>
///
/// <para>
/// The bot context wraps an <see cref="IMarketContext{TPrecision}"/> and provides additional
/// bot-specific properties like accounts, time frames, and dependency injection.
/// </para>
/// </remarks>
/// <typeparam name="TPrecision">The numeric precision type used for calculations (e.g., double, decimal).</typeparam>
public interface IBotContext2<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    /// <summary>
    /// Gets the unique identifier for this bot context instance.
    /// </summary>
    /// <remarks>
    /// This ID can be used for logging, tracking, and correlating bot operations.
    /// For simulation contexts, this is typically a numeric ID.
    /// For live contexts, this may be a GUID or other unique identifier.
    /// </remarks>
    string Id { get; }

    /// <summary>
    /// Gets the underlying market context that provides market-level information.
    /// </summary>
    /// <remarks>
    /// The market context provides:
    /// <list type="bullet">
    ///   <item><description>Current time (simulated or real)</description></item>
    ///   <item><description>Whether the context is live or simulated</description></item>
    ///   <item><description>Cancellation token for graceful shutdown</description></item>
    /// </list>
    /// </remarks>
    IMarketContext<TPrecision> MarketContext { get; }

    /// <summary>
    /// Gets the default account for trading operations.
    /// </summary>
    /// <remarks>
    /// This account is used when no specific account is specified for trading operations.
    /// For simulation contexts, this is typically a <see cref="ISimAccount{TPrecision}"/>.
    /// For live contexts, this may be a live trading account or a paper trading account.
    /// </remarks>
    IAccount2<TPrecision> DefaultAccount { get; }

    /// <summary>
    /// Gets the service provider for resolving dependencies.
    /// </summary>
    /// <remarks>
    /// Bots can use this to resolve services like loggers, data providers, and other
    /// infrastructure components registered in the dependency injection container.
    /// </remarks>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the primary time frame for this bot's trading operations.
    /// </summary>
    /// <remarks>
    /// This is typically the main time frame the bot operates on, though bots may
    /// also use other time frames for analysis.
    /// </remarks>
    TimeFrame TimeFrame { get; }

    /// <summary>
    /// Gets the current time in the bot's context.
    /// </summary>
    /// <remarks>
    /// This is a convenience property that delegates to <see cref="MarketContext"/>.<see cref="IMarketContext{TPrecision}.CurrentTime"/>.
    /// For simulation, this is the simulated time. For live trading, this is the current UTC time.
    /// </remarks>
    DateTimeOffset CurrentTime => MarketContext.CurrentTime;

    /// <summary>
    /// Gets a value indicating whether this is a live trading context.
    /// </summary>
    /// <remarks>
    /// This is a convenience property that delegates to <see cref="MarketContext"/>.<see cref="IMarketContext{TPrecision}.IsLive"/>.
    /// </remarks>
    bool IsLive => MarketContext.IsLive;
}
