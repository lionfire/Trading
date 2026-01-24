namespace LionFire.Trading.Automation.Optimization.Scoring.Formula;

/// <summary>
/// Base class for AST nodes.
/// </summary>
public abstract class AstNode
{
    public abstract double Evaluate(EvaluationContext context);
}

/// <summary>
/// Number literal node.
/// </summary>
public class NumberNode : AstNode
{
    public double Value { get; }

    public NumberNode(double value) => Value = value;

    public override double Evaluate(EvaluationContext context) => Value;

    public override string ToString() => Value.ToString();
}

/// <summary>
/// Metric reference node (e.g., "ad", "winRate").
/// </summary>
public class MetricNode : AstNode
{
    public string Name { get; }

    public MetricNode(string name) => Name = name.ToLowerInvariant();

    public override double Evaluate(EvaluationContext context)
    {
        // This is for per-backtest evaluation
        throw new FormulaException($"Metric '{Name}' cannot be evaluated directly. Use in aggregate functions.");
    }

    public double EvaluateForBacktest(BacktestMetrics metrics)
    {
        return Name switch
        {
            "ad" => metrics.Ad,
            "winrate" => metrics.WinRate,
            "profitfactor" => metrics.ProfitFactor,
            "tradecount" or "trades" => metrics.TradeCount,
            "netprofit" or "profit" => metrics.NetProfit,
            "maxdrawdown" or "drawdown" => metrics.MaxDrawdown,
            "avgtrade" or "averagetrade" => metrics.AvgTrade,
            _ => throw new FormulaException($"Unknown metric: {Name}")
        };
    }

    public override string ToString() => Name;
}

/// <summary>
/// Binary operation node (+, -, *, /).
/// </summary>
public class BinaryOpNode : AstNode
{
    public AstNode Left { get; }
    public AstNode Right { get; }
    public TokenType Operator { get; }

    public BinaryOpNode(AstNode left, TokenType op, AstNode right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public override double Evaluate(EvaluationContext context)
    {
        var left = Left.Evaluate(context);
        var right = Right.Evaluate(context);

        return Operator switch
        {
            TokenType.Plus => left + right,
            TokenType.Minus => left - right,
            TokenType.Multiply => left * right,
            TokenType.Divide => right != 0 ? left / right : 0,
            _ => throw new FormulaException($"Unknown operator: {Operator}")
        };
    }
}

/// <summary>
/// Comparison node for conditions (>=, <=, >, <, ==, !=).
/// </summary>
public class ComparisonNode : AstNode
{
    public AstNode Left { get; }
    public AstNode Right { get; }
    public TokenType Operator { get; }

    public ComparisonNode(AstNode left, TokenType op, AstNode right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public override double Evaluate(EvaluationContext context)
    {
        throw new FormulaException("Comparison nodes must be evaluated with EvaluateCondition");
    }

    public bool EvaluateCondition(BacktestMetrics metrics)
    {
        var left = Left is MetricNode m ? m.EvaluateForBacktest(metrics) : ((NumberNode)Left).Value;
        var right = Right is MetricNode mr ? mr.EvaluateForBacktest(metrics) : ((NumberNode)Right).Value;

        return Operator switch
        {
            TokenType.GreaterEqual => left >= right,
            TokenType.LessEqual => left <= right,
            TokenType.Greater => left > right,
            TokenType.Less => left < right,
            TokenType.Equal => Math.Abs(left - right) < 0.0001,
            TokenType.NotEqual => Math.Abs(left - right) >= 0.0001,
            _ => throw new FormulaException($"Unknown comparison operator: {Operator}")
        };
    }
}

/// <summary>
/// Logical AND/OR node for compound conditions.
/// </summary>
public class LogicalNode : AstNode
{
    public AstNode Left { get; }
    public AstNode Right { get; }
    public TokenType Operator { get; }

    public LogicalNode(AstNode left, TokenType op, AstNode right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public override double Evaluate(EvaluationContext context)
    {
        throw new FormulaException("Logical nodes must be evaluated with EvaluateCondition");
    }

    public bool EvaluateCondition(BacktestMetrics metrics)
    {
        var leftResult = EvaluateNodeAsCondition(Left, metrics);
        var rightResult = EvaluateNodeAsCondition(Right, metrics);

        return Operator switch
        {
            TokenType.And => leftResult && rightResult,
            TokenType.Or => leftResult || rightResult,
            _ => throw new FormulaException($"Unknown logical operator: {Operator}")
        };
    }

    private static bool EvaluateNodeAsCondition(AstNode node, BacktestMetrics metrics)
    {
        return node switch
        {
            ComparisonNode c => c.EvaluateCondition(metrics),
            LogicalNode l => l.EvaluateCondition(metrics),
            _ => throw new FormulaException("Expected condition in logical expression")
        };
    }
}

/// <summary>
/// Function call node (countWhere, avg, max, pow, etc.).
/// </summary>
public class FunctionNode : AstNode
{
    public string Name { get; }
    public List<AstNode> Arguments { get; }

    public FunctionNode(string name, List<AstNode> arguments)
    {
        Name = name.ToLowerInvariant();
        Arguments = arguments;
    }

    public override double Evaluate(EvaluationContext context)
    {
        return Name switch
        {
            // Aggregate functions
            "countwhere" => EvaluateCountWhere(context),
            "percentwhere" => EvaluatePercentWhere(context),
            "avg" or "average" => EvaluateAvg(context),
            "max" => EvaluateMax(context),
            "min" => EvaluateMin(context),
            "sum" => EvaluateSum(context),
            "count" => context.Metrics.Count,

            // Math functions
            "pow" => EvaluatePow(context),
            "log" or "ln" => EvaluateLog(context),
            "log10" => EvaluateLog10(context),
            "sqrt" => EvaluateSqrt(context),
            "abs" => EvaluateAbs(context),

            _ => throw new FormulaException($"Unknown function: {Name}")
        };
    }

    private double EvaluateCountWhere(EvaluationContext context)
    {
        if (Arguments.Count != 1)
            throw new FormulaException("countWhere requires exactly 1 argument");

        var condition = Arguments[0];
        return context.Metrics.Count(m => EvaluateConditionForMetrics(condition, m));
    }

    private double EvaluatePercentWhere(EvaluationContext context)
    {
        if (Arguments.Count != 1)
            throw new FormulaException("percentWhere requires exactly 1 argument");

        var count = EvaluateCountWhere(context);
        return context.Metrics.Count > 0 ? (count * 100.0 / context.Metrics.Count) : 0;
    }

    private double EvaluateAvg(EvaluationContext context)
    {
        if (Arguments.Count != 1 || Arguments[0] is not MetricNode metric)
            throw new FormulaException("avg requires exactly 1 metric argument");

        return context.Metrics.Count > 0
            ? context.Metrics.Average(m => metric.EvaluateForBacktest(m))
            : 0;
    }

    private double EvaluateMax(EvaluationContext context)
    {
        if (Arguments.Count != 1 || Arguments[0] is not MetricNode metric)
            throw new FormulaException("max requires exactly 1 metric argument");

        return context.Metrics.Count > 0
            ? context.Metrics.Max(m => metric.EvaluateForBacktest(m))
            : 0;
    }

    private double EvaluateMin(EvaluationContext context)
    {
        if (Arguments.Count != 1 || Arguments[0] is not MetricNode metric)
            throw new FormulaException("min requires exactly 1 metric argument");

        return context.Metrics.Count > 0
            ? context.Metrics.Min(m => metric.EvaluateForBacktest(m))
            : 0;
    }

    private double EvaluateSum(EvaluationContext context)
    {
        if (Arguments.Count != 1 || Arguments[0] is not MetricNode metric)
            throw new FormulaException("sum requires exactly 1 metric argument");

        return context.Metrics.Sum(m => metric.EvaluateForBacktest(m));
    }

    private double EvaluatePow(EvaluationContext context)
    {
        if (Arguments.Count != 2)
            throw new FormulaException("pow requires exactly 2 arguments");

        var baseVal = Arguments[0].Evaluate(context);
        var exp = Arguments[1].Evaluate(context);
        return Math.Pow(baseVal, exp);
    }

    private double EvaluateLog(EvaluationContext context)
    {
        if (Arguments.Count != 1)
            throw new FormulaException("log requires exactly 1 argument");

        var val = Arguments[0].Evaluate(context);
        return val > 0 ? Math.Log(val) : 0;
    }

    private double EvaluateLog10(EvaluationContext context)
    {
        if (Arguments.Count != 1)
            throw new FormulaException("log10 requires exactly 1 argument");

        var val = Arguments[0].Evaluate(context);
        return val > 0 ? Math.Log10(val) : 0;
    }

    private double EvaluateSqrt(EvaluationContext context)
    {
        if (Arguments.Count != 1)
            throw new FormulaException("sqrt requires exactly 1 argument");

        var val = Arguments[0].Evaluate(context);
        return val >= 0 ? Math.Sqrt(val) : 0;
    }

    private double EvaluateAbs(EvaluationContext context)
    {
        if (Arguments.Count != 1)
            throw new FormulaException("abs requires exactly 1 argument");

        return Math.Abs(Arguments[0].Evaluate(context));
    }

    private static bool EvaluateConditionForMetrics(AstNode node, BacktestMetrics metrics)
    {
        return node switch
        {
            ComparisonNode c => c.EvaluateCondition(metrics),
            LogicalNode l => l.EvaluateCondition(metrics),
            _ => throw new FormulaException("Expected condition in countWhere/percentWhere")
        };
    }
}

/// <summary>
/// Backtest metrics used during formula evaluation.
/// </summary>
public record BacktestMetrics
{
    public double Ad { get; init; }
    public double WinRate { get; init; }
    public double ProfitFactor { get; init; }
    public int TradeCount { get; init; }
    public double NetProfit { get; init; }
    public double MaxDrawdown { get; init; }
    public double AvgTrade { get; init; }
}

/// <summary>
/// Context for formula evaluation containing all backtest metrics.
/// </summary>
public class EvaluationContext
{
    public IReadOnlyList<BacktestMetrics> Metrics { get; }

    public EvaluationContext(IReadOnlyList<BacktestMetrics> metrics)
    {
        Metrics = metrics;
    }

    public static EvaluationContext FromBacktestEntries(IEnumerable<BacktestBatchJournalEntry> entries)
    {
        var metrics = entries
            .Where(e => !e.IsAborted)
            .Select(e => new BacktestMetrics
            {
                Ad = e.AD,
                WinRate = e.WinRate,
                ProfitFactor = 0, // Not available in BacktestBatchJournalEntry
                TradeCount = e.TotalTrades,
                NetProfit = 0, // Would need to be calculated
                MaxDrawdown = e.MaxEquityDrawdown,
                AvgTrade = 0 // Would need to be calculated
            })
            .ToList();

        return new EvaluationContext(metrics);
    }
}
