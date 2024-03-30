using LionFire.Trading.Data;
using System.ComponentModel;
using System.Linq;

namespace LionFire.Trading.Indicators;

public class HistoricalTimeSeriesTypeAdapter<TInput, TOutput> : IHistoricalTimeSeries<TOutput>
{
    public Type ValueType => throw new NotImplementedException();

    public IHistoricalTimeSeries<TInput> Input { get; }

    public HistoricalTimeSeriesTypeAdapter(IHistoricalTimeSeries<TInput> input)
    {
        Input = input;

        if (typeof(TInput) == typeof(decimal) && typeof(TOutput) == typeof(double))
        {
            Converter = i => (TOutput)(object) decimal.ToDouble((decimal)(object)i!);
        }
        else
        {
            Converter = i => (TOutput)(object)i!;
        }
    }
    //public static HistoricalTimeSeriesTypeAdapter<TInput, TOutput> Create(IHistoricalTimeSeries input)
    //{
    //    return Activator.CreateInstance(typeof(HistoricalTimeSeriesTypeAdapter<,>).MakeGenericType(input.ValueType, typeof(TOutput)), input);
    //}

    public Func<TInput, TOutput> Converter { get; set; }

    public async ValueTask<HistoricalDataResult<TOutput>> Get(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var result = await Input.Get(start, endExclusive);
        if (!result.IsSuccess) return new HistoricalDataResult<TOutput> { IsSuccess = false, FailReason = result.FailReason };

        var output = new List<TOutput>(result.Items.Count);
        foreach (var item in result.Items)
        {
            output.Add(Converter(item));
        }
        return new HistoricalDataResult<TOutput> { IsSuccess = true, Items = output };
    }
}


public abstract class SingleInputIndicatorBase<TConcrete, TParameters, TInput, TOutput>
    : IndicatorBase<TConcrete, TParameters, TInput, TOutput>
    where TConcrete : IndicatorBase<TConcrete, TParameters, TInput, TOutput>, IIndicator<TConcrete, TParameters, TInput, TOutput>
{
    #region Input Handling

    // TODO: Can this be moved to a base class somehow?

    public override async Task<TInput[]> GetInputData(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive)
    {
        IHistoricalTimeSeries<TInput> source;
        if (sources[0].GetType().IsAssignableTo(typeof(IHistoricalTimeSeries<TInput>)))
        {
            source = (IHistoricalTimeSeries<TInput>)sources[0];
        }
        else
        {
            source = (IHistoricalTimeSeries<TInput>)Activator.CreateInstance(typeof(HistoricalTimeSeriesTypeAdapter<,>).MakeGenericType(sources[0].ValueType, typeof(TOutput)), sources[0])!;
        }
        var data = await source.Get(start, endExclusive).ConfigureAwait(false);

        if (!data.IsSuccess || data.Items?.Any() != true) throw new Exception("Failed to get data");

        return data.Items.ToArray(); // COPY
    }

    //public override void OnNextFromArray(object inputData, int index)
    //{
    //    var x = (IReadOnlyList<TInput>)inputData;
    //    OnNext(x[index]);
    //}

    #endregion
}

