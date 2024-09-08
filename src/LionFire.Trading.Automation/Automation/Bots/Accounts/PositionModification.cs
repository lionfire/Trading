//#define BacktestAccountSlottedParameters // FUTURE Maybe, though I think we just typically need 1 hardcoded slot for the bars
namespace LionFire.Trading.Automation;

public class PositionModification : IDisposable
{
    public IPosition Position { get; }

    public PositionModification(IPosition position)
    {
        Position = position;
    }


    public void Dispose()
    {
    }
}
