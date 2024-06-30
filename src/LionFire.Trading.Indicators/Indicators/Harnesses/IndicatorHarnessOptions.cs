namespace LionFire.Trading.Indicators.Harnesses;

public interface IIndicatorHarnessOptions
{
    IIndicatorParameters IndicatorParameters { get; }

    TimeFrame TimeFrame { get; set; }
    IPInput[] Signals { get; set; }
}

public class IndicatorHarnessOptions<TParameters> 
    : IIndicatorHarnessOptions
    where TParameters : IIndicatorParameters
{

    public IndicatorHarnessOptions(TParameters indicatorParameters
        //, TimeFrame timeFrame, object[] inputReferences
        )
    {
        IndicatorParameters = indicatorParameters;
        //TimeFrame = timeFrame;
        //Inputs = inputReferences;
    }

    public TParameters IndicatorParameters { get; init; }
    IIndicatorParameters IIndicatorHarnessOptions.IndicatorParameters => IndicatorParameters;

    public  TimeFrame TimeFrame { get; set; }

    public  IPInput[] Signals { get; set; }

}

public record OutputComponentOptions
{
    #region (static)

    public static int DefaultChunkSize = 4096;
    public static OutputComponentOptions FallbackToDefaults(OutputComponentOptions options)
    {
        if (options.Memory < 0) { options = options with { Memory = DefaultChunkSize }; }

        return options;
    }

    #endregion

    /// <summary>
    /// Allow/prefer forgetting values older than this many bars.
    /// If 0, the user should subscribe to output events.
    /// </summary>
    public int Memory { get; init; } = 1;
}

