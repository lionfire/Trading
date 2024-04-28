using LionFire.Trading.Indicators;

namespace LionFire.Trading;

public interface ITradingContext2
{
    bool IsLive { get; }

    //IIndicator GetIndicator<TValue>(TValue parameters);
    IIndicator2 GetIndicator<TIndicator, TParameters, TInput, TOutput>(TParameters parameters)
        where TIndicator : IIndicator2<TParameters, TInput, TOutput>;
}
