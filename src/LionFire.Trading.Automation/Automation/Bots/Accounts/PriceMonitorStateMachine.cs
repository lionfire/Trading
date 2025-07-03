#if UNUSED // What was this going to be?
namespace LionFire.Trading.Automation;

public interface IPriceMonitorStateMachine
{
}
public class PriceMonitorStateMachine : IPriceMonitorStateMachine
{
    public ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame { get; }

    public PriceMonitorStateMachine(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame)
    {
        ExchangeSymbolTimeFrame = exchangeSymbolTimeFrame;
    }
}

#endif