using System.Text.RegularExpressions;

namespace LionFire.Trading.Automation.Optimization.Scoring;

/// <summary>
/// Parses and evaluates scoring formulas against backtest results.
/// </summary>
/// <remarks>
/// Supported features:
/// - Metrics: ad, winRate, tradeCount
/// - Functions: countWhere(metric op value), percentWhere(metric op value), avg(metric), max(metric), min(metric), sum(metric), pow(x, y), log(x)
/// - Operators: +, -, *, /, comparison (>=, >, <=, <, ==, !=)
/// - Parentheses for grouping
///
/// Examples:
/// - countWhere(ad >= 1.0)
/// - countWhere(ad >= 1.0) * 0.7 + countWhere(winRate >= 0.5) * 0.3
/// - pow(countWhere(ad >= 2.0), 1.5) * log(avg(tradeCount) + 1)
/// </remarks>
public class ScoringFormulaParser
{
    private readonly IReadOnlyList<BacktestBatchJournalEntry> _results;
    private readonly Lazy<IReadOnlyList<double>> _adValues;
    private readonly Lazy<IReadOnlyList<double>> _winRateValues;
    private readonly Lazy<IReadOnlyList<double>> _tradeCountValues;

    public ScoringFormulaParser(IEnumerable<BacktestBatchJournalEntry> results)
    {
        _results = results.Where(r => !r.IsAborted).ToList();
        _adValues = new Lazy<IReadOnlyList<double>>(() => _results.Select(r => r.AD).ToList());
        _winRateValues = new Lazy<IReadOnlyList<double>>(() => _results.Select(r => r.WinRate).Where(w => !double.IsNaN(w)).ToList());
        _tradeCountValues = new Lazy<IReadOnlyList<double>>(() => _results.Select(r => (double)r.TotalTrades).ToList());
    }

    /// <summary>
    /// Evaluates a formula and returns the result.
    /// </summary>
    public double Evaluate(string formula)
    {
        if (string.IsNullOrWhiteSpace(formula))
            return 0;

        var normalized = formula.Trim();

        try
        {
            return EvaluateExpression(normalized);
        }
        catch (Exception ex)
        {
            throw new FormulaParseException($"Failed to evaluate formula: {formula}", ex);
        }
    }

    private double EvaluateExpression(string expr)
    {
        expr = expr.Trim();

        // Handle addition and subtraction (lowest precedence)
        var (left, op, right) = SplitAtOperator(expr, ['+', '-']);
        if (op.HasValue)
        {
            var leftVal = EvaluateExpression(left);
            var rightVal = EvaluateExpression(right);
            return op.Value == '+' ? leftVal + rightVal : leftVal - rightVal;
        }

        // Handle multiplication and division
        (left, op, right) = SplitAtOperator(expr, ['*', '/']);
        if (op.HasValue)
        {
            var leftVal = EvaluateExpression(left);
            var rightVal = EvaluateExpression(right);
            return op.Value == '*' ? leftVal * rightVal : (rightVal != 0 ? leftVal / rightVal : 0);
        }

        // Handle parentheses
        if (expr.StartsWith("(") && FindMatchingParen(expr, 0) == expr.Length - 1)
        {
            return EvaluateExpression(expr.Substring(1, expr.Length - 2));
        }

        // Handle functions
        return EvaluateFunction(expr);
    }

    private (string left, char? op, string right) SplitAtOperator(string expr, char[] operators)
    {
        int parenDepth = 0;

        // Scan from right to left for left-to-right associativity
        for (int i = expr.Length - 1; i >= 0; i--)
        {
            char c = expr[i];
            if (c == ')') parenDepth++;
            else if (c == '(') parenDepth--;
            else if (parenDepth == 0 && operators.Contains(c))
            {
                // Don't split on operators that are part of comparison operators
                if (i > 0 && (expr[i - 1] == '>' || expr[i - 1] == '<' || expr[i - 1] == '=' || expr[i - 1] == '!'))
                    continue;
                if (i < expr.Length - 1 && expr[i + 1] == '=')
                    continue;

                var left = expr.Substring(0, i).Trim();
                var right = expr.Substring(i + 1).Trim();

                // Avoid splitting unary minus
                if (string.IsNullOrEmpty(left) && c == '-')
                    continue;

                return (left, c, right);
            }
        }

        return (expr, null, "");
    }

    private int FindMatchingParen(string expr, int openIndex)
    {
        int depth = 1;
        for (int i = openIndex + 1; i < expr.Length; i++)
        {
            if (expr[i] == '(') depth++;
            else if (expr[i] == ')') depth--;
            if (depth == 0) return i;
        }
        return -1;
    }

    private double EvaluateFunction(string expr)
    {
        expr = expr.Trim();

        // Try to parse as a number
        if (double.TryParse(expr, out var number))
            return number;

        var lowerExpr = expr.ToLowerInvariant();

        // countWhere(metric op value)
        if (lowerExpr.StartsWith("countwhere("))
            return EvaluateCountWhere(expr);

        // percentWhere(metric op value)
        if (lowerExpr.StartsWith("percentwhere("))
            return EvaluatePercentWhere(expr);

        // avg(metric)
        if (lowerExpr.StartsWith("avg(") || lowerExpr.StartsWith("average("))
            return EvaluateAggregate(expr, values => values.Count > 0 ? values.Average() : 0);

        // max(metric)
        if (lowerExpr.StartsWith("max("))
            return EvaluateAggregate(expr, values => values.Count > 0 ? values.Max() : 0);

        // min(metric)
        if (lowerExpr.StartsWith("min("))
            return EvaluateAggregate(expr, values => values.Count > 0 ? values.Min() : 0);

        // sum(metric)
        if (lowerExpr.StartsWith("sum("))
            return EvaluateAggregate(expr, values => values.Sum());

        // pow(x, y)
        if (lowerExpr.StartsWith("pow("))
            return EvaluatePow(expr);

        // log(x) - natural log
        if (lowerExpr.StartsWith("log("))
            return EvaluateLog(expr);

        // sqrt(x)
        if (lowerExpr.StartsWith("sqrt("))
            return EvaluateSqrt(expr);

        throw new FormulaParseException($"Unknown function or expression: {expr}");
    }

    private double EvaluateCountWhere(string expr)
    {
        var (metric, comparison, value) = ParseCondition(expr, "countwhere");
        var values = GetMetricValues(metric);
        var predicate = CreatePredicate(comparison, value);
        return values.Count(predicate);
    }

    private double EvaluatePercentWhere(string expr)
    {
        var (metric, comparison, value) = ParseCondition(expr, "percentwhere");
        var values = GetMetricValues(metric);
        var predicate = CreatePredicate(comparison, value);
        var count = values.Count(predicate);
        return values.Count > 0 ? (count * 100.0 / values.Count) : 0;
    }

    private double EvaluateAggregate(string expr, Func<IReadOnlyList<double>, double> aggregator)
    {
        var parenStart = expr.IndexOf('(');
        var parenEnd = FindMatchingParen(expr, parenStart);
        var metric = expr.Substring(parenStart + 1, parenEnd - parenStart - 1).Trim().ToLowerInvariant();
        var values = GetMetricValues(metric);
        return aggregator(values);
    }

    private double EvaluatePow(string expr)
    {
        var parenStart = expr.IndexOf('(');
        var parenEnd = FindMatchingParen(expr, parenStart);
        var args = expr.Substring(parenStart + 1, parenEnd - parenStart - 1);
        var parts = SplitArguments(args);

        if (parts.Count != 2)
            throw new FormulaParseException($"pow() requires exactly 2 arguments: {expr}");

        var baseVal = EvaluateExpression(parts[0]);
        var expVal = EvaluateExpression(parts[1]);
        return Math.Pow(baseVal, expVal);
    }

    private double EvaluateLog(string expr)
    {
        var parenStart = expr.IndexOf('(');
        var parenEnd = FindMatchingParen(expr, parenStart);
        var arg = expr.Substring(parenStart + 1, parenEnd - parenStart - 1).Trim();
        var value = EvaluateExpression(arg);
        return value > 0 ? Math.Log(value) : 0;
    }

    private double EvaluateSqrt(string expr)
    {
        var parenStart = expr.IndexOf('(');
        var parenEnd = FindMatchingParen(expr, parenStart);
        var arg = expr.Substring(parenStart + 1, parenEnd - parenStart - 1).Trim();
        var value = EvaluateExpression(arg);
        return value >= 0 ? Math.Sqrt(value) : 0;
    }

    private (string metric, string comparison, double value) ParseCondition(string expr, string funcName)
    {
        // Extract content between parentheses
        var parenStart = expr.IndexOf('(');
        var parenEnd = FindMatchingParen(expr, parenStart);
        var content = expr.Substring(parenStart + 1, parenEnd - parenStart - 1).Trim();

        // Parse the condition: "metric op value"
        var match = Regex.Match(content, @"^(\w+)\s*(>=|<=|>|<|==|!=)\s*([\d.]+)$", RegexOptions.IgnoreCase);
        if (!match.Success)
            throw new FormulaParseException($"Invalid condition in {funcName}: {content}");

        return (match.Groups[1].Value.ToLowerInvariant(), match.Groups[2].Value, double.Parse(match.Groups[3].Value));
    }

    private IReadOnlyList<double> GetMetricValues(string metric)
    {
        return metric switch
        {
            "ad" => _adValues.Value,
            "winrate" => _winRateValues.Value,
            "tradecount" or "trades" or "totaltrades" => _tradeCountValues.Value,
            _ => throw new FormulaParseException($"Unknown metric: {metric}. Valid metrics: ad, winRate, tradeCount")
        };
    }

    private static Func<double, bool> CreatePredicate(string comparison, double value)
    {
        return comparison switch
        {
            ">=" => v => v >= value,
            ">" => v => v > value,
            "<=" => v => v <= value,
            "<" => v => v < value,
            "==" => v => Math.Abs(v - value) < 0.0001,
            "!=" => v => Math.Abs(v - value) >= 0.0001,
            _ => throw new FormulaParseException($"Unknown comparison operator: {comparison}")
        };
    }

    private List<string> SplitArguments(string args)
    {
        var result = new List<string>();
        int depth = 0;
        int start = 0;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == '(') depth++;
            else if (args[i] == ')') depth--;
            else if (args[i] == ',' && depth == 0)
            {
                result.Add(args.Substring(start, i - start).Trim());
                start = i + 1;
            }
        }

        result.Add(args.Substring(start).Trim());
        return result;
    }
}

/// <summary>
/// Exception thrown when a formula cannot be parsed or evaluated.
/// </summary>
public class FormulaParseException : Exception
{
    public FormulaParseException(string message) : base(message) { }
    public FormulaParseException(string message, Exception inner) : base(message, inner) { }
}
