#if UNUSED // Replaced by OptimizationRunReference?
namespace LionFire.Trading.Automation;

public record MultiBacktestId (
    Type PBotType,
    ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame,
    DateTimeOffset Start,
    DateTimeOffset EndExclusive,
    string Id
    )
{

}
#endif