namespace LionFire.Trading.Optimization.Execution;

/// <summary>
/// Status of a plan execution.
/// </summary>
public enum PlanExecutionStatus
{
    /// <summary>Plan has not been executed yet.</summary>
    NotStarted = 0,

    /// <summary>Plan execution is in progress.</summary>
    Running = 1,

    /// <summary>Plan execution was paused and can be resumed.</summary>
    Paused = 2,

    /// <summary>Plan execution completed successfully.</summary>
    Completed = 3,

    /// <summary>Plan execution failed with errors.</summary>
    Failed = 4
}
