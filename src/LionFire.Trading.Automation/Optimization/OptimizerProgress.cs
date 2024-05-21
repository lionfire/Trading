namespace LionFire.Trading.Automation.Optimization;

public class OptimizerProgress
{
    public bool IsComprehensive { get; set; }

    public int Completed { get; set; }

    #region Progress

    public double MinProgress { get; set; }
    public double CurrentProgress { get; set; }
    public double EstimatedProgress => (double)(MinProgress + (SelectionRate * (MaxProgress - MinProgress)));
    public double MaxProgress { get; set; }

    #endregion

    #region Total

    public int MinTotal { get; set; }
    public int CurrentTotal { get; set; }
    public int EstimatedTotal => (int)(MinTotal + (SelectionRate * (MaxTotal - MinTotal)));
    public int MaxTotal { get; set; }

    #endregion

    #region Remaining

    public int MinRemaining => MinTotal - Completed;
    public int CurrentRemaining => CurrentTotal - Completed;
    public int EstimatedRemaining => EstimatedTotal - Completed;
    public int MaxRemaining => MaxTotal - Completed;

    #endregion

    #region Selection rate

    // Irrelevant for Comprehensive optimizations

    public int OptionalBacktestsSelected { get; set; }
    public int OptionalBacktestsDiscarded { get; set; }
    public double SelectionRate => OptionalBacktestsSelected / (OptionalBacktestsSelected + OptionalBacktestsDiscarded);

    #endregion

    #region Time elapsed

    public TimeSpan TotalElapsed { get; set; }
    public TimeSpan AveragePerBacktest => Completed == 0 ? TimeSpan.Zero : (TotalElapsed / Completed);

    #endregion

    #region Time remaining

    public TimeSpan MinRemainingTime => AveragePerBacktest * (MinTotal - Completed);
    public TimeSpan CurrentRemainingTime => AveragePerBacktest * (CurrentTotal - Completed);
    public TimeSpan EstimatedRemainingTime => AveragePerBacktest * EstimatedRemaining;
    public TimeSpan MaxRemainingTime => AveragePerBacktest * (MaxTotal - Completed);
    
    #endregion

}
