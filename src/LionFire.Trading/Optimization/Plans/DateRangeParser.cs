using System.Globalization;
using System.Text.RegularExpressions;

namespace LionFire.Trading.Optimization.Plans;

/// <summary>
/// Parses date expressions for optimization date ranges.
/// </summary>
public static partial class DateRangeParser
{
    /// <summary>
    /// Parse a date expression to a DateTime.
    /// </summary>
    /// <param name="expression">Date expression (relative or absolute)</param>
    /// <param name="reference">Reference date for relative expressions (defaults to UtcNow)</param>
    /// <returns>Resolved DateTime</returns>
    public static DateTime Parse(string expression, DateTime? reference = null)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Date expression cannot be empty", nameof(expression));

        var refDate = reference ?? DateTime.UtcNow;
        var expr = expression.Trim().ToLowerInvariant();

        // Special keywords
        if (expr == "now" || expr == "today")
            return refDate.Date;

        if (expr == "yesterday")
            return refDate.Date.AddDays(-1);

        // Start of month: som, som-1, som-3, som+1
        var somMatch = SomPattern().Match(expr);
        if (somMatch.Success)
        {
            var offset = somMatch.Groups[1].Success ? int.Parse(somMatch.Groups[1].Value) : 0;
            var firstOfCurrentMonth = new DateTime(refDate.Year, refDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            return firstOfCurrentMonth.AddMonths(offset);
        }

        // Start of year: soy, soy-1
        var soyMatch = SoyPattern().Match(expr);
        if (soyMatch.Success)
        {
            var offset = soyMatch.Groups[1].Success ? int.Parse(soyMatch.Groups[1].Value) : 0;
            return new DateTime(refDate.Year + offset, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        // Relative expression: -1M, -3M, -1Y, -30D, etc.
        var relativeMatch = RelativePattern().Match(expr);
        if (relativeMatch.Success)
        {
            var sign = relativeMatch.Groups[1].Value == "-" ? -1 : 1;
            var amount = int.Parse(relativeMatch.Groups[2].Value) * sign;
            var unit = relativeMatch.Groups[3].Value.ToUpperInvariant();

            return unit switch
            {
                "D" => refDate.AddDays(amount),
                "W" => refDate.AddDays(amount * 7),
                "M" => refDate.AddMonths(amount),
                "Y" => refDate.AddYears(amount),
                _ => throw new ArgumentException($"Unknown time unit: {unit}")
            };
        }

        // Absolute date: 2025-01-01, 2025-01-01T00:00:00Z
        if (DateTime.TryParse(expression, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var absolute))
            return DateTime.SpecifyKind(absolute, DateTimeKind.Utc);

        // ISO format
        if (DateTimeOffset.TryParse(expression, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto))
            return dto.UtcDateTime;

        throw new ArgumentException($"Cannot parse date expression: {expression}");
    }

    /// <summary>
    /// Resolves a date range to concrete DateTime values.
    /// </summary>
    public static (DateTime Start, DateTime End) Resolve(OptimizationDateRange range, DateTime? referenceDate = null)
    {
        var reference = referenceDate ?? DateTime.UtcNow;
        return (
            Parse(range.Start, reference),
            Parse(range.End, reference)
        );
    }

    /// <summary>
    /// Validates a date expression without parsing.
    /// </summary>
    public static bool TryValidate(string expression, out string? error)
    {
        try
        {
            Parse(expression);
            error = null;
            return true;
        }
        catch (ArgumentException ex)
        {
            error = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// Common relative date ranges for UI convenience.
    /// </summary>
    public static IReadOnlyList<OptimizationDateRange> CommonRanges { get; } =
    [
        // Completed months (snapped to month boundaries)
        new() { Name = "Prev 1mo", Start = "som-1", End = "som" },
        new() { Name = "Prev 3mo", Start = "som-3", End = "som" },
        new() { Name = "Prev 6mo", Start = "som-6", End = "som" },
        new() { Name = "Prev 1yr", Start = "som-12", End = "som" },
        new() { Name = "Prev 2yr", Start = "som-24", End = "som" },
        // Rolling (relative to now)
        new() { Name = "1 Week", Start = "-1W", End = "now" },
        new() { Name = "1 Month", Start = "-1M", End = "now" },
        new() { Name = "3 Months", Start = "-3M", End = "now" },
        new() { Name = "6 Months", Start = "-6M", End = "now" },
        new() { Name = "1 Year", Start = "-1Y", End = "now" },
        new() { Name = "2 Years", Start = "-2Y", End = "now" },
        // Calendar
        new() { Name = "YTD", Start = "soy", End = "now" },
        new() { Name = "Prev Year", Start = "soy-1", End = "soy" },
    ];

    /// <summary>
    /// Date range presets organized by category for UI chip-toggle panels.
    /// </summary>
    public static IReadOnlyDictionary<string, IReadOnlyList<OptimizationDateRange>> CategorizedRanges { get; } =
        new Dictionary<string, IReadOnlyList<OptimizationDateRange>>
        {
            ["Completed Months"] =
            [
                new() { Name = "Prev 1mo", Start = "som-1", End = "som" },
                new() { Name = "Prev 2mo", Start = "som-2", End = "som" },
                new() { Name = "Prev 3mo", Start = "som-3", End = "som" },
                new() { Name = "Prev 6mo", Start = "som-6", End = "som" },
                new() { Name = "Prev 1yr", Start = "som-12", End = "som" },
                new() { Name = "Prev 2yr", Start = "som-24", End = "som" },
            ],
            ["Rolling"] =
            [
                new() { Name = "1 Week", Start = "-1W", End = "now" },
                new() { Name = "1 Month", Start = "-1M", End = "now" },
                new() { Name = "3 Months", Start = "-3M", End = "now" },
                new() { Name = "6 Months", Start = "-6M", End = "now" },
                new() { Name = "1 Year", Start = "-1Y", End = "now" },
                new() { Name = "2 Years", Start = "-2Y", End = "now" },
            ],
            ["Calendar"] =
            [
                new() { Name = "YTD", Start = "soy", End = "now" },
                new() { Name = "Prev Year", Start = "soy-1", End = "soy" },
            ],
        };

    [GeneratedRegex(@"^([+-]?)(\d+)([DWMY])$", RegexOptions.IgnoreCase)]
    private static partial Regex RelativePattern();

    [GeneratedRegex(@"^som([+-]\d+)?$", RegexOptions.IgnoreCase)]
    private static partial Regex SomPattern();

    [GeneratedRegex(@"^soy([+-]\d+)?$", RegexOptions.IgnoreCase)]
    private static partial Regex SoyPattern();
}
