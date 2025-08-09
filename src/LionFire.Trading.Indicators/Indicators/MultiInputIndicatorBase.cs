#if false // TODO Maybe
using LionFire.Trading.Data;

namespace LionFire.Trading.Indicators;

public abstract class MultiInputIndicatorBase<TConcrete, TParameters, TInput1, TInput2, TOutput>
    : IndicatorBase<TConcrete, TParameters, (TInput1,TInput2), TOutput>

    where TConcrete : IndicatorBase<TConcrete, TParameters, (TInput1, TInput2), TOutput>, IIndicator2<TConcrete, TParameters, (TInput1, TInput2), TOutput>
    //where TInput : (TInput1, TInput2)
{
    #region Input Handling

    private async Task<(IReadOnlyList<T> data, int count)> GetData<T>(IHistoricalTimeSeries source, DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var typedSource = (IHistoricalTimeSeries<T>)source;
        var data = await typedSource.Get(start, endExclusive).ConfigureAwait(false);
        if (!data.IsSuccess || data.Items?.Any() != true) throw new Exception("Failed to get data");
        return (data.Items, data.Items.Count);

    }

    //public async Task<(IReadOnlyList<TInput>, int)> GetInputArrays(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive)
    //{
    //    var d1 = GetData<TInput1>(sources[0], start, endExclusive);
    //    var d2 = GetData<TInput2>(sources[1], start, endExclusive);
    //    await Task.WhenAll(d1, d2);

    //    int count = d1.Result.count;
    //    if (count != d2.Result.count) throw new ArgumentException("Input data counts do not match");

    //    return (new object[] { d1.Result.data!, d2.Result.data! }, count);
    //}

    public override async Task<(TInput1,TInput2)[]> GetInputData(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var d1 = GetData<TInput1>(sources[0], start, endExclusive);
        var d2 = GetData<TInput2>(sources[1], start, endExclusive);
        await Task.WhenAll(d1, d2);

        int count = d1.Result.count;
        if (count != d2.Result.count) throw new ArgumentException("Input data counts do not match");

        var array = new (TInput1, TInput2)[count];

        for (int i = 0; i < count; i++)
        {
            array[i] = (d1.Result.data[i], d2.Result.data[i]);
        }

        return array;
    }

    //public override void OnNextFromArray(object inputData, int index)
    //{
    //    var x = (IReadOnlyList<(TInput1, TInput2)>)inputData;
    //    OnNext(x[index]);
    //}

    //protected virtual (TInput1, TInput2) InputFromComponents(TInput1 input1, TInput2 input2) => ValueTuple.Create(input1, input2);

    #endregion
}

#endif