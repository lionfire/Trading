namespace LionFire.Trading.Automation.Bots;

/// <summary>
/// Unidirectional trading (long or short)
/// 
/// Points system for opening and closing positions
/// 
/// Support for incremental opening and closing
/// </summary>
public class PPointsBot
{

    #region Common for points systems

    [Parameter(DefaultValue = 1, MinValue = 1, MaxValue = int.MaxValue, DefaultMax = 30)]
    public int OpenThreshold { get; set; }

    [Parameter(DefaultValue = 1, MinValue = 1, MaxValue = int.MaxValue, DefaultMax = 30)]
    public int CloseThreshold { get; set; }

    #endregion

    #region Opening

    [Parameter("If 1.0, open entire position, or a portion of position size if less than 1.0", DefaultValue = 1.0f, MinValue = 0f, DefaultMin = 0.1, DefaultMax = 1.0f, Step = 0.05f)]
    public float IncrementalOpenAmount { get; set; }

    [Parameter("Multiply the current open score by this after opening a position", DefaultValue = 1f, MinValue = -10f, DefaultMin = 0f, DefaultMax = 1f, Step = 0.05f)]
    public float OpenScoreMultiplierAfterOpen { get; set; }

    [Parameter("Multiply the current open score by this after closing a position", DefaultValue = 1f, MinValue = -10f, DefaultMin = 0f, DefaultMax = 1f, Step = 0.05f)]
    public float OpenScoreMultiplierAfterClose { get; set; }

    #endregion

    #region Closing

    [Parameter("If 1.0, close entire position size, or a portion of position size if less than 1.0", DefaultValue = 1.0f, MinValue = 0f, DefaultMin = 0.1, DefaultMax = 1.0f, Step = 0.05f)]
    public float IncrementalCloseAmount { get; set; }

    [Parameter("Multiply the current close score by this after closing a position", DefaultValue = 1f, MinValue = -10f, DefaultMin = 0f, DefaultMax = 1f, Step = 0.05f)]
    public float CloseScoreMultiplierAfterClose { get; set; }

    [Parameter("Multiply the current close score by this after opening a position", DefaultValue = 1f, MinValue = -10f, DefaultMin = 0f, DefaultMax = 1f, Step = 0.05f, MinProbes = 1)]
    public float CloseScoreMultiplierAfterOpen { get; set; }

    #endregion
}
