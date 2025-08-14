namespace LionFire.Trading.DataFlow.Indicators;

/// <summary>
/// Implementation hint for indicator selection
/// </summary>
public enum ImplementationHint
{
    /// <summary>
    /// Automatically select best implementation based on context
    /// </summary>
    Auto = 0,
    
    /// <summary>
    /// Use QuantConnect implementation
    /// </summary>
    QuantConnect = 1,
    
    /// <summary>
    /// Use first-party implementation
    /// </summary>
    FirstParty = 2,
    
    /// <summary>
    /// Use optimized implementation (if available)
    /// </summary>
    Optimized = 3
}