namespace LionFire.Trading.Automation;

public class BacktestOptions
{
    public const string ConfigurationLocation = "Trading:Backtesting";

    /// <summary>
    /// Directory for storing backtest results.
    /// Configured via Trading:Backtesting:Windows:Dir or Trading:Backtesting:Unix:Dir in appsettings.json.
    /// </summary>
    public string? Dir { get; set; }

}
