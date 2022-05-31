#nullable enable


using LionFire;

namespace LionFire.Trading.Algos.Modular.Bias
{
    public interface IBiasProvider
    {
        // bias: -1.0 short, 1.0 long
    }


    /// <summary>
    /// Translates bias to desired position size.
    /// When trade signals are received, a ITradeSignalInterpreter can choose to look at all DesiredPositionSizes for the modular algo to determine what the new position size should be.
    /// </summary>
    public interface IBiasToDesiredPositionSize
    {

    }
}
