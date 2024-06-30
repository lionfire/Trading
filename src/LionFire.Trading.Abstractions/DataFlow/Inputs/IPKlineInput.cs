#nullable enable
namespace LionFire.Trading;

public interface IPKlineInput : IPInput
{

    string Exchange { get; }
    string ExchangeArea { get; }
    string Symbol { get; }
    TimeFrame TimeFrame { get; }

}
