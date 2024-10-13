namespace LionFire.Trading.Automation;

public interface IBotController<TPrecision> : ISimulationController<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    long Id { get;  }
    IBot2 Bot { get; }
    IBotBatchController BotBatchController { get; }

    ValueTask OnFinished();
}


