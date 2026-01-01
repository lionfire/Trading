using System.Numerics;

namespace LionFire.Trading.Automation.Accounts;

/// <summary>
/// Represents a live trading account that operates in real-time with either simulated or real order execution.
/// </summary>
/// <remarks>
/// <para>
/// This interface extends <see cref="IAccount2{TPrecision}"/> with live trading-specific properties and guarantees.
/// Unlike backtest accounts, live accounts provide non-nullable balance and equity values that bots can
/// rely on for position sizing decisions.
/// </para>
///
/// <para>
/// There are two primary implementations:
/// <list type="bullet">
///   <item>
///     <description>
///       <b>LiveSimAccount</b> - Realistic balance tracking where the balance changes based on realized P&amp;L.
///       The account can "blow up" if losses exceed the balance.
///     </description>
///   </item>
///   <item>
///     <description>
///       <b>LivePaperAccount</b> - Infinite capital mode where balance and equity always return a configured value.
///       Useful for testing strategies without capital constraints.
///     </description>
///   </item>
/// </list>
/// </para>
/// </remarks>
/// <typeparam name="TPrecision">The numeric precision type used for calculations (e.g., double, decimal).</typeparam>
public interface ILiveAccount<TPrecision> : IAccount2<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    /// <summary>
    /// Gets the operating mode for this account.
    /// </summary>
    /// <remarks>
    /// For live accounts, this will be one of:
    /// <list type="bullet">
    ///   <item><description><see cref="BotAccountMode.LiveSimulated"/> - Realistic balance tracking</description></item>
    ///   <item><description><see cref="BotAccountMode.LivePaper"/> - Infinite capital mode</description></item>
    ///   <item><description><see cref="BotAccountMode.LiveReal"/> - Real exchange trading (future)</description></item>
    /// </list>
    /// </remarks>
    BotAccountMode Mode { get; }

    /// <summary>
    /// Gets the current account balance (realized funds available).
    /// </summary>
    /// <remarks>
    /// <para>
    /// For <see cref="BotAccountMode.LiveSimulated"/>: Balance changes based on realized P&amp;L from closed positions.
    /// </para>
    /// <para>
    /// For <see cref="BotAccountMode.LivePaper"/>: Always returns the configured balance value.
    /// </para>
    /// <para>
    /// This is guaranteed to be non-null for live accounts, allowing bots to safely use it for position sizing.
    /// </para>
    /// </remarks>
    new TPrecision Balance { get; }

    /// <summary>
    /// Gets the current account equity (balance + unrealized P&amp;L).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Equity = Balance + UnrealizedPnL
    /// </para>
    /// <para>
    /// For <see cref="BotAccountMode.LiveSimulated"/>: Reflects current market value of the account.
    /// </para>
    /// <para>
    /// For <see cref="BotAccountMode.LivePaper"/>: Always returns the configured balance value.
    /// </para>
    /// <para>
    /// This is guaranteed to be non-null for live accounts.
    /// </para>
    /// </remarks>
    TPrecision Equity { get; }

    /// <summary>
    /// Gets the total unrealized profit/loss from open positions.
    /// </summary>
    /// <remarks>
    /// Positive values indicate unrealized gains; negative values indicate unrealized losses.
    /// This is calculated based on current market prices for all open positions.
    /// </remarks>
    TPrecision UnrealizedPnL { get; }

    /// <summary>
    /// Gets the total realized profit/loss from closed positions during this session.
    /// </summary>
    /// <remarks>
    /// This accumulates the P&amp;L from all positions closed since the account was created
    /// or the last reset (depending on implementation).
    /// </remarks>
    TPrecision RealizedPnL { get; }

    /// <summary>
    /// Gets the initial balance the account started with.
    /// </summary>
    /// <remarks>
    /// For <see cref="BotAccountMode.LiveSimulated"/>: The starting balance (default $10,000 USD equivalent).
    /// For <see cref="BotAccountMode.LivePaper"/>: The configured display balance.
    /// </remarks>
    TPrecision InitialBalance { get; }

    /// <summary>
    /// Gets the base currency for the account (e.g., "USD", "USDT", "BTC").
    /// </summary>
    /// <remarks>
    /// All balance, equity, and P&amp;L values are denominated in this currency.
    /// </remarks>
    string BaseCurrency { get; }
}
