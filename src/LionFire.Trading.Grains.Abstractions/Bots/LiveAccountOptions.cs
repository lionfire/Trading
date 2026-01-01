using System.ComponentModel.DataAnnotations;
using Orleans;

namespace LionFire.Trading.Grains.Bots;

/// <summary>
/// Configuration options for creating live trading accounts.
/// </summary>
/// <remarks>
/// <para>
/// This class provides settings for both <see cref="BotAccountMode.LiveSimulated"/> and
/// <see cref="BotAccountMode.LivePaper"/> accounts, including initial balance,
/// fill simulation mode, and slippage configuration.
/// </para>
/// </remarks>
[GenerateSerializer]
[Alias("live-account-options")]
public class LiveAccountOptions
{
    /// <summary>
    /// Gets or sets the initial balance for the account (in base currency).
    /// </summary>
    /// <remarks>
    /// For <see cref="BotAccountMode.LiveSimulated"/>: This is the starting balance that will change with P&amp;L.
    /// For <see cref="BotAccountMode.LivePaper"/>: This is the configured balance that always remains constant.
    /// </remarks>
    [Id(0)]
    [Range(0.01, double.MaxValue, ErrorMessage = "Initial balance must be positive")]
    public decimal InitialBalance { get; set; } = 10_000m;

    /// <summary>
    /// Gets or sets the base currency for the account.
    /// </summary>
    /// <remarks>
    /// Common values: "USD", "USDT", "BTC", "EUR".
    /// All balance and P&amp;L values are denominated in this currency.
    /// </remarks>
    [Id(1)]
    [Required]
    public string BaseCurrency { get; set; } = "USDT";

    /// <summary>
    /// Gets or sets whether to use realistic fill simulation with slippage.
    /// </summary>
    /// <remarks>
    /// When false, uses simple fill simulation (no slippage).
    /// When true, uses realistic fill simulation with configurable slippage.
    /// </remarks>
    [Id(2)]
    public bool UseRealisticFills { get; set; } = false;

    /// <summary>
    /// Gets or sets the default slippage in basis points (1/100th of a percent).
    /// </summary>
    /// <remarks>
    /// Only applies when <see cref="UseRealisticFills"/> is true.
    /// For example, 10 basis points = 0.1% slippage.
    /// </remarks>
    [Id(3)]
    [Range(0, 1000, ErrorMessage = "Slippage must be between 0 and 1000 basis points")]
    public decimal DefaultSlippageBps { get; set; } = 0m;

    /// <summary>
    /// Gets or sets the account leverage.
    /// </summary>
    /// <remarks>
    /// Leverage of 1 means no leverage (cash account).
    /// Higher values enable trading larger positions than the account balance.
    /// </remarks>
    [Id(4)]
    [Range(1, 125, ErrorMessage = "Leverage must be between 1 and 125")]
    public decimal Leverage { get; set; } = 1m;

    /// <summary>
    /// Gets or sets whether to track position count limits.
    /// </summary>
    /// <remarks>
    /// If true, the account will enforce limits on number of open positions.
    /// </remarks>
    [Id(5)]
    public bool EnforcePositionLimits { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum number of concurrent positions allowed.
    /// </summary>
    /// <remarks>
    /// Only applies when <see cref="EnforcePositionLimits"/> is true.
    /// </remarks>
    [Id(6)]
    [Range(1, 1000)]
    public int MaxPositions { get; set; } = 100;

    /// <summary>
    /// Gets the default options for live simulated accounts.
    /// </summary>
    public static LiveAccountOptions DefaultSimulated => new()
    {
        InitialBalance = 10_000m,
        BaseCurrency = "USDT",
        UseRealisticFills = false,
        DefaultSlippageBps = 0m,
        Leverage = 1m
    };

    /// <summary>
    /// Gets the default options for paper (infinite capital) accounts.
    /// </summary>
    public static LiveAccountOptions DefaultPaper => new()
    {
        InitialBalance = 10_000m,
        BaseCurrency = "USDT",
        UseRealisticFills = false,
        DefaultSlippageBps = 0m,
        Leverage = 1m
    };
}
