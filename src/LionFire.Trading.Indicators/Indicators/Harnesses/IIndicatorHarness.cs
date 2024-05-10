using LionFire.Trading.Data;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Indicators.Harnesses;

public interface IIndicatorHarness
{
    TimeFrame TimeFrame { get; }
}
public interface IIndicatorHarness<TOutput> : IIndicatorHarness
{
    ValueTask<IValuesResult<TOutput>> TryGetValues(DateTimeOffset start, DateTimeOffset endExclusive, ref TOutput[] outputBuffer);

}

public interface IBufferingIndicatorHarness<TOutput> : IBufferingIndicatorHarness<TOutput>
{
    ValueTask<IValuesResult<TOutput>> TryGetValues(bool reverse, DateTimeOffset start, DateTimeOffset endExclusive, TimeFrameValuesWindowWithGaps<TOutput>? outputBuffer = null);
}

public interface IIndicatorHarness<TParameters, TInput, TOutput> : IIndicatorHarness<TOutput>
{
    TParameters Parameters { get; }
    IServiceProvider ServiceProvider { get; }

    Task<TInput[]> GetInputData(IReadOnlyList<IHistoricalTimeSeries> sources, DateTimeOffset start, DateTimeOffset endExclusive);
}

public static class IBufferingIndicatorHarnessX
{
    public static ValueTask<IValuesResult<TOutput>> TryGetReverseValues<TOutput>(this IBufferingIndicatorHarness<TOutput> @this, DateTimeOffset start, DateTimeOffset endExclusive, TimeFrameValuesWindowWithGaps<TOutput>? outputBuffer = null)
        => @this.TryGetValues(reverse: true, start, endExclusive, outputBuffer);

    public static ValueTask<IValuesResult<TOutput>> TryGetForwardValues<TOutput>(this IBufferingIndicatorHarness<TOutput> @this, DateTimeOffset start, DateTimeOffset endExclusive, TimeFrameValuesWindowWithGaps<TOutput>? outputBuffer = null)
        => @this.TryGetValues(reverse: false, start, endExclusive, outputBuffer);

    #region Throw on fail

    public static async ValueTask<IValuesResult<TOutput>> GetReverseValues<TOutput>(this IBufferingIndicatorHarness<TOutput> @this, DateTimeOffset start, DateTimeOffset endExclusive, TimeFrameValuesWindowWithGaps<TOutput>? outputBuffer = null)
    {
        var result = await @this.TryGetReverseValues(start, endExclusive, outputBuffer);
        if (result == null)
        {
            throw new Exception("Failed to get output");
        }
        return result;
    }
    public static async ValueTask<IValuesResult<TOutput>> GetForwardValues<TOutput>(this IBufferingIndicatorHarness<TOutput> @this, DateTimeOffset start, DateTimeOffset endExclusive, TimeFrameValuesWindowWithGaps<TOutput>? outputBuffer = null)
    {
        var result = await @this.TryGetForwardValues(start, endExclusive, outputBuffer);
        if (result == null)
        {
            throw new Exception("Failed to get output");
        }
        return result;
    }

    #endregion
}


