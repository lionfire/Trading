//#define BacktestAccountSlottedParameters // FUTURE Maybe, though I think we just typically need 1 hardcoded slot for the bars
namespace LionFire.Trading.Automation;

internal class IndexedInput
{
    #region Identity

    /// <summary>
    /// Array index for the bot's Inputs
    /// </summary>
    public int Index { get; set; }

    #endregion

    #region Relationships

    public required IPInput PInput { get; init; }

    #endregion

    #region Parameters

    public int Lookback { get; set; }
    
    #endregion

    public InputEnumeratorBase? Enumerator { get; set; }
}
