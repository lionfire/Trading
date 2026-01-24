using LionFire.Trading.Automation;
using LionFire.Trading.Automation.Optimization.Scoring;
using LionFire.Trading.Automation.Optimization.Scoring.Formula;

namespace LionFire.Trading.Automation.Tests.Scoring;

public class ScoringTests
{
    [Fact]
    public void AdCalculator_ExtractsAdValues()
    {
        // Arrange
        var entries = new List<BacktestBatchJournalEntry>
        {
            new() { AD = 1.5, IsAborted = false },
            new() { AD = 2.0, IsAborted = false },
            new() { AD = 0.5, IsAborted = false },
            new() { AD = 3.0, IsAborted = true }, // Should be excluded
        };

        // Act
        var adValues = AdCalculator.ExtractAdValues(entries);

        // Assert
        adValues.Should().HaveCount(3);
        adValues.Should().Contain(new[] { 1.5, 2.0, 0.5 });
        adValues.Should().NotContain(3.0);
    }

    [Fact]
    public void AdCalculator_CalculatesSummary()
    {
        // Arrange
        var adValues = new List<double> { 0.5, 1.0, 1.5, 2.0, 3.0 };

        // Act
        var summary = AdCalculator.CalculateSummary(adValues, threshold: 1.0);

        // Assert
        summary.TotalBacktests.Should().Be(5);
        summary.PassingCount.Should().Be(4); // 1.0, 1.5, 2.0, 3.0 >= 1.0
        summary.PassingPercent.Should().Be(80);
        summary.MaxAd.Should().Be(3.0);
        summary.MinAd.Should().Be(0.5);
        summary.AvgAd.Should().Be(1.6);
        summary.MedianAd.Should().Be(1.5);
        summary.GoodCount.Should().Be(2); // 2.0, 3.0 >= 2.0
        summary.StrongCount.Should().Be(1); // 3.0 >= 3.0
        summary.ExceptionalCount.Should().Be(0); // none >= 5.0
    }

    [Fact]
    public void HistogramGenerator_GeneratesCorrectBuckets()
    {
        // Arrange
        var adValues = new List<double> { -0.5, 0.2, 0.7, 1.2, 2.5, 4.0, 6.0 };

        // Act
        var histogram = HistogramGenerator.GenerateAdHistogram(adValues);

        // Assert
        histogram.TotalCount.Should().Be(7);
        histogram.Buckets.Should().HaveCount(7); // Default 7 buckets

        // Check specific buckets
        var lessThanZero = histogram.Buckets.First(b => b.Max == 0);
        lessThanZero.Count.Should().Be(1); // -0.5

        var zeroToHalf = histogram.Buckets.First(b => b.Min == 0 && b.Max == 0.5);
        zeroToHalf.Count.Should().Be(1); // 0.2
    }

    [Fact]
    public void OptimizationScorer_CalculatesScore()
    {
        // Arrange
        var entries = new List<BacktestBatchJournalEntry>
        {
            new() { AD = 0.5 },
            new() { AD = 1.0 },
            new() { AD = 1.5 },
            new() { AD = 2.0 },
        };

        // Act
        var scorer = new OptimizationScorer(entries);
        var score = scorer.Calculate();

        // Assert
        score.Value.Should().Be(3); // 3 entries >= 1.0
        score.Summary.Should().NotBeNull();
        score.Summary!.PassingCount.Should().Be(3);
        score.AdHistogram.Should().NotBeNull();
    }

    [Fact]
    public void FormulaParser_ParsesSimpleExpression()
    {
        // Arrange
        var parser = new FormulaParser();

        // Act
        var ast = parser.Parse("1 + 2 * 3");

        // Assert
        ast.Should().BeOfType<BinaryOpNode>();
        var context = new EvaluationContext(new List<BacktestMetrics>());
        ast.Evaluate(context).Should().Be(7); // 1 + (2 * 3)
    }

    [Fact]
    public void FormulaParser_ParsesCountWhereCondition()
    {
        // Arrange
        var parser = new FormulaParser();

        // Act
        var ast = parser.Parse("countWhere(ad >= 1.0)");

        // Assert
        ast.Should().BeOfType<FunctionNode>();

        var metrics = new List<BacktestMetrics>
        {
            new() { Ad = 0.5 },
            new() { Ad = 1.0 },
            new() { Ad = 1.5 },
            new() { Ad = 2.0 },
        };
        var context = new EvaluationContext(metrics);
        ast.Evaluate(context).Should().Be(3); // 3 entries >= 1.0
    }

    [Fact]
    public void FormulaParser_ParsesAvgFunction()
    {
        // Arrange
        var parser = new FormulaParser();

        // Act
        var ast = parser.Parse("avg(ad)");

        // Assert
        ast.Should().BeOfType<FunctionNode>();

        var metrics = new List<BacktestMetrics>
        {
            new() { Ad = 1.0 },
            new() { Ad = 2.0 },
            new() { Ad = 3.0 },
        };
        var context = new EvaluationContext(metrics);
        ast.Evaluate(context).Should().Be(2.0);
    }

    [Fact]
    public void FormulaParser_ParsesMathFunctions()
    {
        // Arrange
        var parser = new FormulaParser();
        var metrics = new List<BacktestMetrics>();
        var context = new EvaluationContext(metrics);

        // Act & Assert
        parser.Parse("pow(2, 3)").Evaluate(context).Should().Be(8);
        parser.Parse("sqrt(16)").Evaluate(context).Should().Be(4);
        parser.Parse("abs(-5)").Evaluate(context).Should().Be(5);
    }

    [Fact]
    public void Tokenizer_TokenizesFormula()
    {
        // Arrange
        var tokenizer = new Tokenizer("countWhere(ad >= 1.0) * 0.7 + avg(winRate) * 0.3");

        // Act
        var tokens = tokenizer.Tokenize();

        // Assert
        tokens.Should().Contain(t => t.Type == TokenType.Identifier && t.Value == "countWhere");
        tokens.Should().Contain(t => t.Type == TokenType.GreaterEqual);
        tokens.Should().Contain(t => t.Type == TokenType.Number && t.Value == "1.0");
        tokens.Should().Contain(t => t.Type == TokenType.Multiply);
        tokens.Should().Contain(t => t.Type == TokenType.Plus);
        tokens.Should().Contain(t => t.Type == TokenType.EndOfInput);
    }
}
