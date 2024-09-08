//#define BacktestAccountSlottedParameters // FUTURE Maybe, though I think we just typically need 1 hardcoded slot for the bars
namespace LionFire.Trading.Automation;

public interface IHasInstanceInputInfos
{
    public List<InstanceInputInfo> InstanceInputInfos { get; }
    public object Instance { get; }
}
