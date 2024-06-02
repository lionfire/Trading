using LionFire.Trading.Data;

namespace LionFire.Trading.Indicators;

public class HistoricalTimeSeriesTypeAdapter<TInput, TOutput> : IHistoricalTimeSeries<TOutput>
{
    public Type ValueType => throw new NotImplementedException();

    public IHistoricalTimeSeries<TInput> Input { get; }
    public TimeFrame TimeFrame => Input.TimeFrame;

    public HistoricalTimeSeriesTypeAdapter(IHistoricalTimeSeries<TInput> input)
    {
        Input = input;

        if (typeof(TInput) == typeof(decimal) && typeof(TOutput) == typeof(double))
        {
            Converter = i => (TOutput)(object)decimal.ToDouble((decimal)(object)i!);
        }
        else
        {
            Converter = i => (TOutput)(object)i!;
        }
    }
    //public static HistoricalTimeSeriesTypeAdapter<InputSlot, TOutput> Create(IHistoricalTimeSeries input)
    //{
    //    return Activator.CreateInstance(typeof(HistoricalTimeSeriesTypeAdapter<,>).MakeGenericType(input.ValueType, typeof(TOutput)), input);
    //}

    public Func<TInput, TOutput> Converter { get; set; }

    public async ValueTask<HistoricalDataResult<TOutput>> Get(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var result = await Input.Get(start, endExclusive);
        if (!result.IsSuccess) return new HistoricalDataResult<TOutput> { IsSuccess = false, FailReason = result.FailReason };

        var output = new TOutput[result.Values.Count];
        int i = 0;
        foreach (var item in result.Values)
        {
            output[i++] = Converter(item);
        }
        return new HistoricalDataResult<TOutput> { IsSuccess = true, Values = output };
    }
}

