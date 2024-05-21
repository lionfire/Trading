
namespace LionFire.Trading.Automation;

public class BacktestBotController : BatchBotControllerBase
{
    public BacktestBotController(IServiceProvider serviceProvider, IPBacktestTask2 parameters) : base(serviceProvider, parameters)
    {
    }

    public override BotExecutionMode BotExecutionMode => BotExecutionMode.Backtest;

}

