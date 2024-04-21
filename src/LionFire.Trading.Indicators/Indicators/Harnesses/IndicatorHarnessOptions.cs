namespace LionFire.Trading.Indicators.Harnesses;

public record IndicatorHarnessOptions<TParameters>
{

    public required TParameters Parameters { get; init; }

    public required TimeFrame TimeFrame { get; init; }

    public required object[] InputReferences { get; init; }

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

public class ValueWindow
{

}

