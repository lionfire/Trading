namespace LionFire.Trading.Automation;

public interface IBotHarness
{
    ISimContext ISimContext { get; }

    #region Convenience

    //DateTimeOffset Start => ISimContext.Start;
    //DateTimeOffset SimulatedCurrentDate => ISimContext.SimulatedCurrentDate;
    //DateTimeOffset EndExclusive => ISimContext.EndExclusive;

    #endregion

    //bool BarsEnabled { get; } // ENH - for ticks only

}

public interface IMultiBacktestHarness : IBotHarness
{
    IServiceProvider ServiceProvider { get; }

}
