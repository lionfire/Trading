//#define BacktestAccountSlottedParameters // FUTURE Maybe, though I think we just typically need 1 hardcoded slot for the bars
namespace LionFire.Trading.Automation;

internal class PInputToEnumerator
{
    #region Identity

    // UNUSED - REFACTOR - Index is not actually needed
    ///// <summary>
    ///// Array index for the bot's Inputs
    ///// </summary>
    //public int Index { get; set; }

    #endregion

    #region Relationships

    /// <summary>
    /// Resolvable by IMarketDataResolver
    /// </summary>
    public required IPInput PInput { get; init; }

    #endregion

    #region Parameters

    /// <summary>
    /// Size of the sliding window in Enumerator
    /// </summary>
    public int Lookback { get; set; }
    
    #endregion

    ///// <summary>
    ///// Contains the sliding window of actual values
    ///// </summary>
    //public InputEnumeratorBase? Enumerator { get; set; }
}
