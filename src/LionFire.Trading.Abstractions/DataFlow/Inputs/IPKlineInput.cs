#nullable enable
namespace LionFire.Trading;

public interface IPKlineInput : IPInput
{

    string Exchange { get; }
    string Area { get; }
    string Symbol { get; }
    TimeFrame TimeFrame { get; }

}
