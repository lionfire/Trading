namespace LionFire.Trading.Indicators.Harnesses;

public record IndicatorHarnessOptions<TParameters>
{
    #region (static)

    public static int DefaultChunkSize = 4096;
    public static IndicatorHarnessOptions<TParameters> FallbackToDefaults(IndicatorHarnessOptions<TParameters> options)
    {
        if (options.Memory <= 0) { options = options with { Memory = DefaultChunkSize }; }

        return options;
    }

    #endregion

    public required TParameters Parameters { get; init; }

    /// <summary>
    /// Allow/prefer forgetting values older than this many bars.
    /// </summary>
    public int Memory { get; init; } = 1;

    public required TimeFrame TimeFrame { get; init; }

    public required object[] InputReferences { get; init; }

}

