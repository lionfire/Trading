//#define BacktestAccountSlottedParameters // FUTURE Maybe, though I think we just typically need 1 hardcoded slot for the bars
namespace LionFire.Trading.Automation;


// REVIEW - this is a BotHarness state, maybe only used during init?  Reconsider naming and class hierarchies on backtest and live bot sides.
public interface IHasInputMappings
{
    public List<InputMapping> InputMappings { get; }
    public IBarListener Instance { get; }
}
