namespace LionFire.Trading.Optimization.Plans;

/// <summary>
/// Result of plan validation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Whether validation passed with no errors.
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Validation errors that prevent saving.
    /// </summary>
    public List<string> Errors { get; } = [];

    /// <summary>
    /// Validation warnings that don't prevent saving.
    /// </summary>
    public List<string> Warnings { get; } = [];
}

/// <summary>
/// Exception thrown when plan validation fails.
/// </summary>
public class PlanValidationException : Exception
{
    /// <summary>
    /// The validation errors.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }

    public PlanValidationException(IEnumerable<string> errors)
        : base($"Validation failed: {string.Join(", ", errors)}")
    {
        Errors = errors.ToList();
    }
}

/// <summary>
/// Validates optimization plans before saving.
/// </summary>
public class OptimizationPlanValidator
{
    /// <summary>
    /// Validates an optimization plan.
    /// </summary>
    public ValidationResult Validate(OptimizationPlan plan)
    {
        var result = new ValidationResult();

        // Required fields
        if (string.IsNullOrWhiteSpace(plan.Id))
            result.Errors.Add("Plan ID is required");
        else if (!IsValidId(plan.Id))
            result.Errors.Add("Plan ID must be a valid slug (lowercase letters, numbers, hyphens only, no leading/trailing hyphens)");

        if (string.IsNullOrWhiteSpace(plan.Name))
            result.Errors.Add("Plan name is required");

        if (string.IsNullOrWhiteSpace(plan.Bot))
            result.Errors.Add("Bot type is required");

        // Symbols
        if (plan.Symbols.IsDynamic && string.IsNullOrWhiteSpace(plan.Symbols.CollectionId))
            result.Errors.Add("Dynamic symbol type requires a collection ID");

        if (!plan.Symbols.IsDynamic && plan.Symbols.Snapshot.Count == 0)
            result.Errors.Add("Static symbol type requires at least one symbol");

        // Timeframes
        if (plan.Timeframes.Count == 0)
            result.Errors.Add("At least one timeframe is required");

        // Date ranges
        if (plan.DateRanges.Count == 0)
            result.Errors.Add("At least one date range is required");

        foreach (var range in plan.DateRanges)
        {
            if (string.IsNullOrWhiteSpace(range.Name))
                result.Errors.Add("Date range name is required");

            if (!DateRangeParser.TryValidate(range.Start, out var startError))
                result.Errors.Add($"Invalid start date in range '{range.Name}': {startError}");

            if (!DateRangeParser.TryValidate(range.End, out var endError))
                result.Errors.Add($"Invalid end date in range '{range.Name}': {endError}");
        }

        // Resolution
        if (plan.Resolution.MaxBacktests <= 0)
            result.Errors.Add("Max backtests must be greater than 0");

        if (plan.Resolution.MinParameterPriority < 0)
            result.Errors.Add("Min parameter priority cannot be negative");

        // Warnings
        if (plan.Symbols.Snapshot.Count > 100)
            result.Warnings.Add($"Large symbol count ({plan.Symbols.Snapshot.Count}) may result in long optimization times");

        if (plan.Timeframes.Count > 6)
            result.Warnings.Add($"Many timeframes ({plan.Timeframes.Count}) may result in long optimization times");

        if (plan.Resolution.MaxBacktests > 50000)
            result.Warnings.Add($"High backtest limit ({plan.Resolution.MaxBacktests}) may take a very long time");

        return result;
    }

    private static bool IsValidId(string id) =>
        !string.IsNullOrWhiteSpace(id) &&
        id.All(c => char.IsLetterOrDigit(c) || c == '-' || c == '_') &&
        !id.StartsWith('-') &&
        !id.EndsWith('-') &&
        id == id.ToLowerInvariant();
}
