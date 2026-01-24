using System.Threading;
using LionFire.Trading.Optimization.Plans;
using LionFire.Trading.Symbols;

namespace LionFire.Trading.Optimization.Execution;

/// <summary>
/// Generates a matrix of optimization jobs from a plan.
/// Each job is one combination of symbol × timeframe × dateRange.
/// </summary>
public class JobMatrixGenerator
{
    private readonly ISymbolCollectionRepository? _symbolRepository;

    public JobMatrixGenerator(ISymbolCollectionRepository? symbolRepository = null)
    {
        _symbolRepository = symbolRepository;
    }

    /// <summary>
    /// Generate jobs from an optimization plan.
    /// </summary>
    /// <param name="plan">The optimization plan.</param>
    /// <param name="options">Generation options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of optimization jobs.</returns>
    /// <exception cref="InvalidOperationException">If job count exceeds maximum.</exception>
    public async Task<IReadOnlyList<OptimizationJob>> GenerateAsync(
        OptimizationPlan plan,
        JobMatrixOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new JobMatrixOptions();

        // Resolve symbols
        var symbols = await ResolveSymbolsAsync(plan, options, cancellationToken);

        if (symbols.Count == 0)
        {
            throw new InvalidOperationException("No symbols found for optimization plan");
        }

        if (plan.Timeframes.Count == 0)
        {
            throw new InvalidOperationException("No timeframes specified in optimization plan");
        }

        if (plan.DateRanges.Count == 0)
        {
            throw new InvalidOperationException("No date ranges specified in optimization plan");
        }

        // Calculate total job count
        var totalJobs = symbols.Count * plan.Timeframes.Count * plan.DateRanges.Count;

        if (totalJobs > options.MaxJobs)
        {
            throw new InvalidOperationException(
                $"Job count ({totalJobs}) exceeds maximum ({options.MaxJobs}). " +
                $"Reduce symbols ({symbols.Count}), timeframes ({plan.Timeframes.Count}), " +
                $"or date ranges ({plan.DateRanges.Count}).");
        }

        // Reference time for resolving relative dates
        var referenceTime = options.ReferenceTime ?? DateTimeOffset.UtcNow;

        // Generate cartesian product
        var jobs = new List<OptimizationJob>(totalJobs);

        foreach (var symbol in symbols)
        {
            foreach (var timeframe in plan.Timeframes)
            {
                foreach (var dateRange in plan.DateRanges)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Resolve date expressions
                    var (startDate, endDate) = ResolveDateRange(dateRange, referenceTime);

                    var job = new OptimizationJob
                    {
                        Id = GenerateJobId(plan.Id, symbol, timeframe, dateRange.Name),
                        PlanId = plan.Id,
                        Bot = plan.Bot,
                        Exchange = options.DefaultExchange,
                        ExchangeArea = options.DefaultExchangeArea,
                        Symbol = symbol,
                        Timeframe = timeframe,
                        DateRange = dateRange,
                        StartDate = startDate,
                        EndDate = endDate,
                        Resolution = plan.Resolution,
                        Status = JobStatus.Pending
                    };

                    jobs.Add(job);
                }
            }
        }

        return jobs;
    }

    /// <summary>
    /// Resolve date range expressions to actual dates.
    /// </summary>
    private static (DateTimeOffset start, DateTimeOffset end) ResolveDateRange(
        OptimizationDateRange dateRange,
        DateTimeOffset referenceTime)
    {
        var start = ParseDateExpression(dateRange.Start, referenceTime);
        var end = ParseDateExpression(dateRange.End, referenceTime);
        return (start, end);
    }

    /// <summary>
    /// Parse a date expression into an actual date.
    /// Supports: "now", "-1M", "-3M", "-1Y", "-30D", or absolute dates like "2025-01-01".
    /// </summary>
    private static DateTimeOffset ParseDateExpression(string expression, DateTimeOffset referenceTime)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            return referenceTime;
        }

        expression = expression.Trim().ToLowerInvariant();

        if (expression == "now")
        {
            return referenceTime;
        }

        // Relative date expressions: -1M, -3M, -1Y, -30D
        if (expression.StartsWith("-"))
        {
            var amount = int.Parse(expression[1..^1]);
            var unit = char.ToUpperInvariant(expression[^1]);

            return unit switch
            {
                'D' => referenceTime.AddDays(-amount),
                'W' => referenceTime.AddDays(-amount * 7),
                'M' => referenceTime.AddMonths(-amount),
                'Y' => referenceTime.AddYears(-amount),
                _ => throw new ArgumentException($"Unknown date unit: {unit}")
            };
        }

        // Absolute date
        if (DateTimeOffset.TryParse(expression, out var absoluteDate))
        {
            return absoluteDate;
        }

        throw new ArgumentException($"Cannot parse date expression: {expression}");
    }

    /// <summary>
    /// Resolve symbols from plan configuration.
    /// </summary>
    private async Task<IReadOnlyList<string>> ResolveSymbolsAsync(
        OptimizationPlan plan,
        JobMatrixOptions options,
        CancellationToken cancellationToken)
    {
        var symbolConfig = plan.Symbols;

        // Check if using dynamic collection
        if (symbolConfig.IsDynamic && options.ResolveDynamicSymbols)
        {
            if (string.IsNullOrEmpty(symbolConfig.CollectionId))
            {
                throw new InvalidOperationException(
                    "Dynamic symbol collection specified but no CollectionId provided");
            }

            if (_symbolRepository == null)
            {
                throw new InvalidOperationException(
                    "Dynamic symbol collection specified but no ISymbolCollectionRepository available");
            }

            var collection = await _symbolRepository.LoadAsync(symbolConfig.CollectionId, cancellationToken);

            if (collection == null)
            {
                throw new InvalidOperationException(
                    $"Symbol collection '{symbolConfig.CollectionId}' not found");
            }

            // Get active symbols from collection, excluding any specified exclusions
            var activeSymbols = collection.ActiveSymbols
                .Select(s => s.Symbol)
                .Except(symbolConfig.ExcludedSymbols, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (activeSymbols.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Symbol collection '{symbolConfig.CollectionId}' has no active symbols");
            }

            return activeSymbols;
        }

        // Use static snapshot from plan
        return symbolConfig.EffectiveSymbols.ToList();
    }

    /// <summary>
    /// Generate a deterministic job ID for deduplication.
    /// </summary>
    private static string GenerateJobId(string planId, string symbol, string timeframe, string dateRangeName)
    {
        // Create a short but readable ID
        var combined = $"{planId}:{symbol}:{timeframe}:{dateRangeName}";
        var hash = combined.GetHashCode();
        return $"{planId[..Math.Min(8, planId.Length)]}-{symbol}-{timeframe}-{dateRangeName}-{hash:X8}";
    }
}
