using LionFire.Trading.Optimization.Matrix;

namespace LionFire.Trading.Automation.Optimization.Matrix;

/// <summary>
/// Computes letter grades from AD scores and provides display utilities.
/// </summary>
public static class OptimizationGradeComputer
{
    /// <summary>
    /// Computes a letter grade from an AD score.
    /// </summary>
    /// <param name="adScore">The AD (Annualized ROI / Drawdown) score.</param>
    /// <returns>The corresponding letter grade.</returns>
    public static OptimizationGrade ComputeGrade(double adScore) => adScore switch
    {
        >= 3.0 => OptimizationGrade.APlus,
        >= 2.5 => OptimizationGrade.A,
        >= 2.0 => OptimizationGrade.AMinus,
        >= 1.5 => OptimizationGrade.BPlus,
        >= 1.0 => OptimizationGrade.B,
        >= 0.75 => OptimizationGrade.BMinus,
        >= 0.5 => OptimizationGrade.CPlus,
        >= 0.35 => OptimizationGrade.C,
        >= 0.2 => OptimizationGrade.CMinus,
        >= 0.1 => OptimizationGrade.D,
        _ => OptimizationGrade.F
    };

    /// <summary>
    /// Converts a grade enum to its display string (e.g., "A+", "B-").
    /// </summary>
    public static string GradeToString(OptimizationGrade grade) => grade switch
    {
        OptimizationGrade.APlus => "A+",
        OptimizationGrade.A => "A",
        OptimizationGrade.AMinus => "A-",
        OptimizationGrade.BPlus => "B+",
        OptimizationGrade.B => "B",
        OptimizationGrade.BMinus => "B-",
        OptimizationGrade.CPlus => "C+",
        OptimizationGrade.C => "C",
        OptimizationGrade.CMinus => "C-",
        OptimizationGrade.D => "D",
        OptimizationGrade.F => "F",
        OptimizationGrade.Error => "ERR",
        _ => "?"
    };

    /// <summary>
    /// Returns a short human-readable description of the grade including the AD threshold.
    /// </summary>
    public static string GradeDescription(OptimizationGrade grade) => grade switch
    {
        OptimizationGrade.APlus => "Excellent (AD >= 3.0)",
        OptimizationGrade.A => "Very good (AD >= 2.5)",
        OptimizationGrade.AMinus => "Good (AD >= 2.0)",
        OptimizationGrade.BPlus => "Above average (AD >= 1.5)",
        OptimizationGrade.B => "Average (AD >= 1.0)",
        OptimizationGrade.BMinus => "Below average (AD >= 0.75)",
        OptimizationGrade.CPlus => "Mediocre (AD >= 0.5)",
        OptimizationGrade.C => "Poor (AD >= 0.35)",
        OptimizationGrade.CMinus => "Very poor (AD >= 0.2)",
        OptimizationGrade.D => "Bad (AD >= 0.1)",
        OptimizationGrade.F => "Failing (AD < 0.1)",
        OptimizationGrade.Error => "Error - no backtests completed",
        _ => ""
    };

    /// <summary>
    /// Returns a hex color string for the given grade, suitable for use in UI badges.
    /// Green tones for A grades, blue for B, amber/orange for C, red for D/F.
    /// </summary>
    public static string GradeToColor(OptimizationGrade grade) => grade switch
    {
        OptimizationGrade.APlus => "#2E7D32",   // Dark green
        OptimizationGrade.A => "#388E3C",        // Green
        OptimizationGrade.AMinus => "#43A047",   // Medium green
        OptimizationGrade.BPlus => "#1565C0",    // Dark blue
        OptimizationGrade.B => "#1976D2",        // Blue
        OptimizationGrade.BMinus => "#1E88E5",   // Light blue
        OptimizationGrade.CPlus => "#F57F17",    // Dark amber
        OptimizationGrade.C => "#FF8F00",        // Amber
        OptimizationGrade.CMinus => "#FFA000",   // Light amber
        OptimizationGrade.D => "#E65100",        // Dark orange
        OptimizationGrade.F => "#C62828",        // Red
        OptimizationGrade.Error => "#B71C1C",    // Deep red
        _ => "#757575"                           // Grey fallback
    };
}
