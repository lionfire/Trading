#if FUTURE
using LionFire.Results;
using LionFire.Trading.Data;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.Indicators.Inputs;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Indicators.Harnesses;

/// <summary>
/// Combination Realtime and Historical Indicator Harness
/// 
/// WIP:
/// - realtime indicators 
///   - repaint current bar
///   - repaint previous bars
/// - historical indicators  
/// </summary>
/// <typeparam name="TIndicator"></typeparam>
/// <typeparam name="TParameters"></typeparam>
/// <typeparam name="TInput"></typeparam>
/// <typeparam name="TOutput"></typeparam>
public class IndicatorHarness<TIndicator, TParameters, TInput, TOutput> : IndicatorExecutorBase<TIndicator, TParameters, TInput, TOutput>
    where TIndicator : IIndicator<TParameters, TInput, TOutput>
{

    #region Lifecycle

    public IndicatorHarness(IServiceProvider serviceProvider, IndicatorHarnessOptions<TParameters> options, IMarketDataResolver inputResolver) : base(serviceProvider, options, inputResolver)
    {
        memory = new TimeFrameValuesWindowWithGaps<TOutput>(options.Memory, options.TimeFrame);
    }

    #endregion

    #region State

    public int Lookback { get; init; }
    TimeFrameValuesWindowWithGaps<TOutput> memory;

    #region Derived

    public int InputSize => memory.Size + Lookback;
    public int MemorySize => memory.Size;

    #endregion

    #endregion

    #region Methods

    public IEnumerable<TOutput>? QueryReverseOutput(DateTimeOffset firstOpenTime, DateTimeOffset? lastOpenTime = null)
    {
        throw new NotImplementedException();
    }

    public override ValueTask<IValueResult<IEnumerable<TOutput>>?> TryGetReverseOutput(DateTimeOffset firstOpenTime, DateTimeOffset? lastOpenTime = null)
    {
        var inputCount = MemorySize;

        if (!lastOpenTime.HasValue)
        {
            if (TimeFrame.TimeSpan < TimeSpan.Zero) throw new NotImplementedException();
            lastOpenTime = TimeFrame.NextBarOpen(DateTime.UtcNow) - TimeFrame.TimeSpan;
        }
        return memory.TryGetReverseValues(firstOpenTime, lastOpenTime.Value);
    }

    #endregion


}



#endif