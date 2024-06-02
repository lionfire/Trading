using LionFire.Trading.Data;
using System.ComponentModel;
using System.Linq;

namespace LionFire.Trading.Indicators;

public abstract class SingleInputIndicatorBase<TConcrete, TParameters, TInput, TOutput>
    : IndicatorBase<TConcrete, TParameters, TInput, TOutput>
    where TConcrete : IndicatorBase<TConcrete, TParameters, TInput, TOutput>, IIndicator2<TConcrete, TParameters, TInput, TOutput>
{
    //IHistoricalTimeSeries<TInput> input;

    //public override ValueTask<HistoricalDataResult<TOutput>> Get(DateTimeOffset start, DateTimeOffset endExclusive)
    //{


    //}
}

//public static class IndicatorFeeder
//{
//    // Move HistoricalIndicatorHarness GetInputData here?  Also use it in IndicatorBase implementation of IHTS.Get?

//    public static async Task<ArraySegment<TInput>> GetInputData(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive)
//    {

//    }
//}
