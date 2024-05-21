
namespace LionFire.Trading.Automation;

public class LiveBotController : BatchBotControllerBase
{
    public LiveBotController(IServiceProvider serviceProvider, IPBacktestTask2 parameters) : base(serviceProvider, parameters)
    {
    }

    public override BotExecutionMode BotExecutionMode => BotExecutionMode.Live;
}

