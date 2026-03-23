namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Generates formatted reports from Hessian interaction analysis results.
/// </summary>
public class InteractionReportGenerator
{
    /// <summary>
    /// Generate a plain-text summary for CLI output.
    /// </summary>
    public string GenerateTextReport(HessianAnalysisResult result)
    {
        var lines = new List<string>();

        lines.Add("Parameter Interaction Analysis");
        lines.Add("==============================");
        lines.Add($"Analyzed: {result.AnalyzedParameters.Count} parameters, {CountPairs(result.AnalyzedParameters.Count)} pairs");
        lines.Add("");

        var significant = result.SignificantInteractions.ToList();

        if (significant.Count == 0)
        {
            lines.Add("No significant interactions detected.");
            return string.Join(Environment.NewLine, lines);
        }

        // Group by strength
        var strong = significant.Where(i => i.Strength == InteractionStrength.Strong).ToList();
        var weak = significant.Where(i => i.Strength == InteractionStrength.Weak).ToList();

        if (strong.Count > 0)
        {
            lines.Add("Strong Interactions:");
            foreach (var interaction in strong.OrderByDescending(i => i.InteractionScore))
            {
                var directionStr = FormatDirection(interaction.Direction);
                lines.Add($"  {interaction.ParameterA} × {interaction.ParameterB}: Score={interaction.InteractionScore:F2}, {directionStr}, Confidence={interaction.Confidence:P0}");
                lines.Add($"    → Consider joint optimization of these parameters");
            }
            lines.Add("");
        }

        if (weak.Count > 0)
        {
            lines.Add("Weak Interactions:");
            foreach (var interaction in weak.OrderByDescending(i => i.InteractionScore))
            {
                var directionStr = FormatDirection(interaction.Direction);
                lines.Add($"  {interaction.ParameterA} × {interaction.ParameterB}: Score={interaction.InteractionScore:F2}, {directionStr}, Confidence={interaction.Confidence:P0}");
            }
            lines.Add("");
        }

        // List independent parameters
        var allPairs = GenerateAllPairs(result.AnalyzedParameters);
        var independentPairs = allPairs
            .Where(pair => !result.Interactions.Any(i =>
                (i.ParameterA == pair.Item1 && i.ParameterB == pair.Item2) ||
                (i.ParameterA == pair.Item2 && i.ParameterB == pair.Item1)))
            .Take(5)
            .ToList();

        if (independentPairs.Count > 0)
        {
            lines.Add("Independent (no significant interaction):");
            var pairStrs = independentPairs.Select(p => $"{p.Item1} × {p.Item2}");
            lines.Add($"  {string.Join(", ", pairStrs)}");
            if (allPairs.Count - significant.Count > 5)
            {
                lines.Add($"  ... and {allPairs.Count - significant.Count - 5} more");
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Generate warnings for strong interactions suitable for including in step-recommendations output.
    /// </summary>
    public List<string> GenerateWarnings(HessianAnalysisResult result)
    {
        var warnings = new List<string>();

        var significant = result.SignificantInteractions
            .OrderByDescending(i => i.InteractionScore)
            .ToList();

        foreach (var interaction in significant)
        {
            var directionStr = FormatDirection(interaction.Direction);
            var strengthAdj = interaction.Strength switch
            {
                InteractionStrength.Strong => "strong",
                InteractionStrength.Weak => "mild",
                _ => "weak"
            };

            var capitalizedStrength = char.ToUpper(strengthAdj[0]) + strengthAdj[1..];

            warnings.Add(
                $"{capitalizedStrength} interaction detected between {interaction.ParameterA} and {interaction.ParameterB} " +
                $"(score: {interaction.InteractionScore:F2}, {directionStr.ToLower()}). " +
                $"Consider joint optimization of these parameters."
            );
        }

        return warnings;
    }

    /// <summary>
    /// Generate a brief summary line for integration into other reports.
    /// </summary>
    public string GenerateSummaryLine(HessianAnalysisResult result)
    {
        var significant = result.SignificantInteractions.ToList();
        if (significant.Count == 0)
            return "No parameter interactions detected.";

        var strong = significant.Count(i => i.Strength == InteractionStrength.Strong);
        var weak = significant.Count(i => i.Strength == InteractionStrength.Weak);

        var parts = new List<string>();
        if (strong > 0) parts.Add($"{strong} strong");
        if (weak > 0) parts.Add($"{weak} weak");

        return $"Detected {string.Join(", ", parts)} interaction(s) among {result.AnalyzedParameters.Count} parameters.";
    }

    private static string FormatDirection(InteractionDirection direction) => direction switch
    {
        InteractionDirection.Synergistic => "Synergistic",
        InteractionDirection.Antagonistic => "Antagonistic",
        InteractionDirection.Mixed => "Mixed",
        _ => "Unknown"
    };

    private static int CountPairs(int paramCount)
    {
        return paramCount > 1 ? (paramCount * (paramCount - 1)) / 2 : 0;
    }

    private static List<(string, string)> GenerateAllPairs(List<string> parameters)
    {
        var pairs = new List<(string, string)>();
        for (int i = 0; i < parameters.Count; i++)
        {
            for (int j = i + 1; j < parameters.Count; j++)
            {
                pairs.Add((parameters[i], parameters[j]));
            }
        }
        return pairs;
    }
}
