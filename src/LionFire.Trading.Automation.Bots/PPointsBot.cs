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

    [TradingParameter(
        OptimizePriority = -10.0, 
        OptimizeOrderTiebreaker = -50, 
        DefaultValue = 1, 
        HardValueMin = 1, 
        HardValueMax = int.MaxValue, 
        DefaultMax = 30,
        ValueMax = 100,
        MinStep = 1,
        Step = 1,
        MaxStep = 4
        )]
    public int OpenThreshold { get; set; }

    [TradingParameter(OptimizePriority = -10.5, DefaultValue = 1,
        HardValueMin = 1, HardValueMax = int.MaxValue,
        ValueMax = 100,
        DefaultMax = 30)]
    public int CloseThreshold { get; set; }

    #endregion

    #region Opening

    [TradingParameter("If 1.0, open entire position, or a portion of position size if less than 1.0", OptimizePriority = -20, DefaultValue = 1.0f, HardValueMin = 0f, DefaultMin = 0.1, DefaultMax = 1.0f, HardValueMax = 1.0f, Step = 0.05f)]
    public float IncrementalOpenAmount { get; set; }

    [TradingParameter("Multiply the current open score by this after opening a position", 
        OptimizePriority = -20, // Depends on non-1 values for IncrementalOpenAmount
        DefaultValue = 0.5f, 
        HardValueMin = 0f, 
        DefaultMin = 0f, 
        DefaultMax = 1f, 
        HardValueMax = 1.0f, 
        Step = 0.05f
        )]
    public float OpenScoreMultiplierAfterOpen { get; set; }

    [TradingParameter("Multiply the current open score by this after closing a position",
        OptimizePriority = -21,
        DefaultValue = 0.25f, 
        HardValueMin = 0f, 
        DefaultMin = 0f, 
        DefaultMax = 1f, 
        HardValueMax = 1.0f, 
        Step = 0.05f)]
    public float OpenScoreMultiplierAfterClose { get; set; }

    #endregion

    #region Closing

    [TradingParameter("If 1.0, close entire position size, or a portion of position size if less than 1.0", 
        DefaultValue = 1.0f,
        OptimizePriority = -30, // Depends on non-1 values for IncrementalOpenAmount
        HardValueMin = 0f, 
        DefaultMin = 0.1, 
        DefaultMax = 1.0f, 
        HardValueMax = 1.0f, 
        Step = 0.05f)]
    public float IncrementalCloseAmount { get; set; }

    [TradingParameter("Multiply the current close score by this after closing a position", 
        DefaultValue = 1f,
        OptimizePriority = -30,
        HardValueMin = 0f, 
        DefaultMin = 0f, 
        DefaultMax = 0.9f, 
        HardValueMax = 1f, 
        Step = 0.05f)]
    public float CloseScoreMultiplierAfterClose { get; set; }

    [TradingParameter("Multiply the current close score by this after opening a position", 
        DefaultValue = 0.9f,
        OptimizePriority = -30,
        HardValueMin = 0f, 
        DefaultMin = 0f, 
        DefaultMax = 0.9f, 
        HardValueMax = 1f, 
        Step = 0.05f, 
        MinProbes = 1)]
    public float CloseScoreMultiplierAfterOpen { get; set; }

    #endregion
}
