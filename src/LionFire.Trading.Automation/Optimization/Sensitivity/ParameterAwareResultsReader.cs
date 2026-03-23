using CsvHelper;
using System.Globalization;

namespace LionFire.Trading.Automation.Optimization.Sensitivity;

/// <summary>
/// Reads backtest results from CSV files, extracting both fixed metrics and dynamic parameter columns.
/// Supports two formats:
/// - New format: has "P." prefix on parameter columns, BatchId, AMWT, Wins, Losses, Breakevens
/// - Old format: parameter columns directly after fixed metric columns (Id, Fitness, AD, MaxBalanceDrawdown, MaxBalanceDrawdownPerunum)
/// </summary>
public static class ParameterAwareResultsReader
{
    private const string ParameterColumnPrefix = "P.";

    /// <summary>
    /// Fixed metric columns that are NOT parameters (old format).
    /// Any column not in this set is treated as a parameter.
    /// </summary>
    private static readonly HashSet<string> FixedColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "Id", "BatchId", "Fitness", "AD", "AMWT",
        "Wins", "Losses", "Breakevens",
        "MaxBalanceDrawdown", "MaxBalanceDrawdownPerunum",
        "IsAborted",
    };

    /// <summary>
    /// Reads backtest entries with parameter values from a CSV file or zip archive.
    /// </summary>
    public static List<BacktestResultWithParameters> ReadFromPath(string path, SensitivityAnalysisOptions options)
    {
        if (path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
        {
            return ReadFromZip(path, options);
        }
        else if (Directory.Exists(path))
        {
            var csvPath = Path.Combine(path, "backtests.csv");
            return ReadFromCsv(csvPath, options);
        }
        else
        {
            return ReadFromCsv(path, options);
        }
    }

    /// <summary>
    /// Reads from a zip archive containing backtests.csv.
    /// </summary>
    public static List<BacktestResultWithParameters> ReadFromZip(string zipPath, SensitivityAnalysisOptions options)
    {
        using var archive = System.IO.Compression.ZipFile.OpenRead(zipPath);
        var entry = archive.GetEntry("backtests.csv");
        if (entry == null) return [];

        using var stream = entry.Open();
        return ReadFromStream(stream, options);
    }

    /// <summary>
    /// Reads from a CSV file on disk.
    /// </summary>
    public static List<BacktestResultWithParameters> ReadFromCsv(string csvPath, SensitivityAnalysisOptions options)
    {
        if (!File.Exists(csvPath)) return [];

        using var stream = File.OpenRead(csvPath);
        return ReadFromStream(stream, options);
    }

    /// <summary>
    /// Reads from a stream containing CSV data. Extracts fixed fields and parameter columns.
    /// Auto-detects format based on presence of "P." prefixed columns.
    /// </summary>
    public static List<BacktestResultWithParameters> ReadFromStream(Stream stream, SensitivityAnalysisOptions options)
    {
        var results = new List<BacktestResultWithParameters>();

        var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null,
        };

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);

        if (!csv.Read()) return results; // Empty file
        csv.ReadHeader();

        var headerRecord = csv.HeaderRecord;
        if (headerRecord == null || headerRecord.Length == 0) return results;

        // Detect format: new format has "P." prefixed columns, old format doesn't
        var prefixedColumns = headerRecord
            .Where(h => h.StartsWith(ParameterColumnPrefix, StringComparison.Ordinal))
            .ToList();

        bool isNewFormat = prefixedColumns.Count > 0;
        bool hasBatchId = headerRecord.Contains("BatchId");
        bool hasTradeStats = headerRecord.Contains("Wins");

        // Determine parameter columns based on format
        List<string> parameterColumns;
        if (isNewFormat)
        {
            parameterColumns = prefixedColumns;
        }
        else
        {
            // Old format: everything not in the fixed columns set is a parameter
            parameterColumns = headerRecord
                .Where(h => !FixedColumns.Contains(h))
                .ToList();
        }

        if (parameterColumns.Count == 0) return results;

        while (csv.Read())
        {
            try
            {
                var ad = csv.GetField<double>("AD");
                var isAborted = csv.TryGetField<bool>("IsAborted", out var abortedField)
                    ? abortedField
                    : double.IsNaN(ad);

                if (isAborted) continue;

                var entry = new BacktestBatchJournalEntry
                {
                    BatchId = hasBatchId && csv.TryGetField<int>("BatchId", out var batchId) ? batchId : 0,
                    Id = csv.GetField<long>("Id"),
                    Fitness = csv.GetField<double>("Fitness"),
                    AD = ad,
                    AMWT = csv.TryGetField<double>("AMWT", out var amwt) ? amwt : 0,
                    Wins = hasTradeStats && csv.TryGetField<int>("Wins", out var wins) ? wins : 0,
                    Losses = hasTradeStats && csv.TryGetField<int>("Losses", out var losses) ? losses : 0,
                    Breakevens = csv.TryGetField<int>("Breakevens", out var breakevens) ? breakevens : 0,
                    MaxBalanceDrawdown = csv.GetField<double>("MaxBalanceDrawdown"),
                    MaxBalanceDrawdownPerunum = csv.GetField<double>("MaxBalanceDrawdownPerunum"),
                    IsAborted = false,
                };

                // Extract parameter values
                var parameterValues = new Dictionary<string, double>();
                foreach (var col in parameterColumns)
                {
                    if (csv.TryGetField<double>(col, out var value) && !double.IsNaN(value))
                    {
                        // Normalize key: ensure "P." prefix for consistency
                        var key = isNewFormat ? col : ParameterColumnPrefix + col;
                        parameterValues[key] = value;
                    }
                }

                if (parameterValues.Count == 0) continue;

                // Compute confidence based on trade count
                var totalTrades = entry.TotalTrades;
                double confidence;
                if (hasTradeStats && totalTrades < options.MinTradeCount)
                {
                    continue;
                }
                else if (!hasTradeStats)
                {
                    // Old format without trade stats — use moderate confidence based on AD
                    confidence = double.IsNaN(ad) || ad == 0 ? 0.3 : 0.6;
                }
                else
                {
                    confidence = Math.Min(1.0, (double)totalTrades / options.FullConfidenceTradeCount);
                }

                results.Add(new BacktestResultWithParameters
                {
                    Entry = entry,
                    ParameterValues = parameterValues,
                    Confidence = confidence,
                });
            }
            catch (CsvHelper.MissingFieldException)
            {
                continue;
            }
            catch (Exception ex) when (ex.GetType().Name.Contains("TypeConverter"))
            {
                continue;
            }
        }

        return results;
    }
}
