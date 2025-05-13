namespace LionFire.Trading.Automation;

public class BacktestOptions
{
    public const string ConfigurationLocation = "Trading:Backtesting";

    public string Dir { get; set; } = @"F:\st\Investing-Output\.local\Backtests\"; // TODO: Better default

}
