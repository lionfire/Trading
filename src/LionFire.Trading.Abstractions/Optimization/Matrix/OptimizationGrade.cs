namespace LionFire.Trading.Optimization.Matrix;

/// <summary>
/// Letter grades for optimization results based on AD (Annualized ROI / Drawdown) scores.
/// </summary>
public enum OptimizationGrade
{
    APlus,
    A,
    AMinus,
    BPlus,
    B,
    BMinus,
    CPlus,
    C,
    CMinus,
    D,
    F,
    /// <summary>
    /// Job(s) completed but produced zero backtests, indicating an error condition.
    /// </summary>
    Error
}
