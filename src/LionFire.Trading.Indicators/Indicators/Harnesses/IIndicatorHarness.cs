using LionFire.Trading.Data;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Indicators.Harnesses;

public interface IIndicatorHarness: IHistoricalTimeSeries
{
}
public interface IIndicatorHarness<TOutput> : IIndicatorHarness, IHistoricalTimeSeries<TOutput>
{
    ValueTask<HistoricalDataResult<TOutput>> TryGetValues(DateTimeOffset start, DateTimeOffset endExclusive, ref TOutput[]? outputBuffer);
}

public interface IBufferingIndicatorHarness<TOutput> : IIndicatorHarness<TOutput>
{
    ValueTask<IValuesResult<TOutput>> TryGetValues(bool reverse, DateTimeOffset start, DateTimeOffset endExclusive, TimeFrameValuesWindowWithGaps<TOutput>? outputBuffer = null);
}

public interface IIndicatorHarness<TParameters, TInput, TOutput> : IIndicatorHarness<TOutput>
{
    TParameters Parameters { get; }
    IServiceProvider ServiceProvider { get; }

    Task<ArraySegment<TInput>> GetInputData(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive);
}

public static class IBufferingIndicatorHarnessX
{
    public static ValueTask<IValuesResult<TOutput>> TryGetReverseValues<TOutput>(this IBufferingIndicatorHarness<TOutput> @this, DateTimeOffset start, DateTimeOffset endExclusive, TimeFrameValuesWindowWithGaps<TOutput>? outputBuffer = null)
        => @this.TryGetValues(reverse: true, start, endExclusive, outputBuffer);

    public static ValueTask<IValuesResult<TOutput>> TryGetForwardValues<TOutput>(this IBufferingIndicatorHarness<TOutput> @this, DateTimeOffset start, DateTimeOffset endExclusive, TimeFrameValuesWindowWithGaps<TOutput>? outputBuffer = null)
        => @this.TryGetValues(reverse: false, start, endExclusive, outputBuffer);

    #region Throw on fail

    public static async ValueTask<IReadOnlyList<TOutput>> GetReverseValues<TOutput>(this IBufferingIndicatorHarness<TOutput> @this, DateTimeOffset start, DateTimeOffset endExclusive, TimeFrameValuesWindowWithGaps<TOutput>? outputBuffer = null)
    {
        var result = await @this.TryGetReverseValues(start, endExclusive, outputBuffer);
        if (result == null || result.IsSuccess == false || result.Values == null)
        {
            throw new Exception("Failed to get output");
        }
        return result.Values;
    }
    public static async ValueTask<IReadOnlyList<TOutput>> GetForwardValues<TOutput>(this IBufferingIndicatorHarness<TOutput> @this, DateTimeOffset start, DateTimeOffset endExclusive, TimeFrameValuesWindowWithGaps<TOutput>? outputBuffer = null)
    {
        var result = await @this.TryGetForwardValues(start, endExclusive, outputBuffer);
        if (result == null || result.IsSuccess == false || result.Values == null)
        {
            throw new Exception("Failed to get output");
        }
        return result.Values;
    }

    #endregion
}


