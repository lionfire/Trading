namespace LionFire.Trading.Automation;

public interface IBotController<TPrecision> : ISimulationController<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    IBot2 Bot { get; }
    IBotBatchController BotBatchController { get; }

}


