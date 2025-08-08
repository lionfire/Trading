using LionFire.Trading.DataFlow;

namespace LionFire.Trading.Automation;

public interface IMarketListener
{
    IPMarketProcessor Parameters { get; }

}

public interface IMarketListenerOrder : IMarketListener
{
    float ListenOrder { get; }
}
