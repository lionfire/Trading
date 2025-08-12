using LionFire.Trading;
using System.Numerics;

namespace LionFire.Trading.Indicators.Parameters;

public class PATR<TInput, TOutput> : IndicatorParameters<TInput, TOutput>
    where TInput : struct
    where TOutput : struct, INumber<TOutput>
{
    public int Period { get; set; } = 14;
    public MovingAverageType MovingAverageType { get; set; } = MovingAverageType.Wilders;
}