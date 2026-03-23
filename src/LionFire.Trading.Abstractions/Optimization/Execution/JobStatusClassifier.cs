namespace LionFire.Trading.Optimization.Execution;

/// <summary>
/// Classifies job failure exceptions into specific <see cref="JobStatus"/> values.
/// </summary>
public static class JobStatusClassifier
{
    /// <summary>
    /// Determines whether a failure is a generic error or a known category like NoHistory.
    /// Inspects the full exception chain (including InnerException and AggregateException).
    /// </summary>
    public static JobStatus Classify(Exception ex)
    {
        if (IsHistoricalDataError(ex))
            return JobStatus.NoHistory;

        return JobStatus.Failed;
    }

    /// <summary>
    /// Returns true if the error indicates historical data was unavailable.
    /// </summary>
    public static bool IsHistoricalDataError(Exception ex)
    {
        return ContainsMessage(ex, "Failed to get historical data");
    }

    private static bool ContainsMessage(Exception? ex, string pattern)
    {
        while (ex != null)
        {
            if (ex.Message.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                return true;

            if (ex is AggregateException agg)
            {
                foreach (var inner in agg.InnerExceptions)
                {
                    if (ContainsMessage(inner, pattern))
                        return true;
                }
            }

            ex = ex.InnerException;
        }

        return false;
    }
}
