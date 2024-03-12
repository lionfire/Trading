using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.Indicators.Inputs;
using LionFire.Trading.ValueWindows;

namespace LionFire.Trading.Indicators.Harness;

/// <summary>
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
public class IndicatorHarness<TIndicator, TParameters, TInput, TOutput>
where TIndicator : IIndicator<TParameters, TInput, TOutput>
{

    #region Dependencies

    public IServiceProvider ServiceProvider { get; }
    public IBars Bars { get; }

    #endregion

    #region Identity (incl. Parameters)

    public TParameters Parameters { get; }
    public TimeFrame TimeFrame { get; }

    #endregion

    #region Lifecycle

    public IndicatorHarness(IServiceProvider serviceProvider, IndicatorHarnessOptions<TParameters> options, IBars bars, MarketDataResolver inputResolver)
    {
        ServiceProvider = serviceProvider;
        Bars = bars;
        Parameters = options.Parameters;
        TimeFrame = options.TimeFrame;
        indicator = CreateIndicator();
        memory = new TimeFrameValuesWindowWithGaps<TOutput>(options.Memory, options.TimeFrame);


        foreach(var input in options.InputReferences)
        {

            inputResolver.TryResolve(input, out var series);    
        }
    }

    #endregion

    #region State

    protected IIndicator<TParameters, TInput, TOutput> indicator;

    public int Lookback { get; init; }
    TimeFrameValuesWindowWithGaps<TOutput> memory;

    #region Derived

    public int InputSize => memory.Size + Lookback;
    public int MemorySize => memory.Size;
    
    #endregion

    #endregion

    #region Methods

    public IEnumerable<TOutput>? TryGetReverseOutput(DateTimeOffset firstOpenTime, DateTimeOffset? lastOpenTime = null)
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

    IIndicator<TParameters, TInput, TOutput> CreateIndicator()
    {
        var indicator = TIndicator.Create(Parameters);
        return indicator;
    }
}


