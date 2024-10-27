namespace LionFire.Trading;

public enum OptimizationDistributionKind
{
    Unspecified = 0,

    Period = 1 << 0,


    ///// <summary>
    ///// May have a minor change on results
    ///// </summary>
    //Minor = 1 << 0,

    //Major = 1 << 1,

    /// <summary>
    /// Operates in a completely different mode
    /// </summary>
    Category = 1 << 2,

    /// <summary>
    /// Operates in a completely different mode, but various modes may fall on a spectrum.  For example: moving average types.
    /// </summary>
    SpectralCategory = 1 << 3,

    /// <summary>
    /// A major reversal in logic
    /// </summary>
    Reversal = 1 << 4,
}
