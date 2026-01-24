namespace LionFire.Trading.Cli.Commands;

/// <summary>
/// Options for crypto market cap command.
/// </summary>
public class CryptoMcapOptions
{
    /// <summary>
    /// Maximum number of results to return.
    /// </summary>
    public int Limit { get; set; } = 50;

    /// <summary>
    /// Quote currency filter (e.g., USDT, USD).
    /// </summary>
    public string QuoteCurrency { get; set; } = "USDT";

    /// <summary>
    /// Output as JSON instead of table.
    /// </summary>
    public bool Json { get; set; }
}

/// <summary>
/// Options for crypto volume command.
/// </summary>
public class CryptoVolOptions
{
    /// <summary>
    /// Maximum number of results to return.
    /// </summary>
    public int Limit { get; set; } = 50;

    /// <summary>
    /// Exchange to query (default: Binance).
    /// </summary>
    public string Exchange { get; set; } = "Binance";

    /// <summary>
    /// Exchange area (futures, spot).
    /// </summary>
    public string Area { get; set; } = "futures";

    /// <summary>
    /// Quote currency filter (e.g., USDT, USD).
    /// </summary>
    public string QuoteCurrency { get; set; } = "USDT";

    /// <summary>
    /// Output as JSON instead of table.
    /// </summary>
    public bool Json { get; set; }
}
