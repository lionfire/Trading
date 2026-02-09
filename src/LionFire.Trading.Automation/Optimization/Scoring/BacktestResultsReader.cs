using CsvHelper;
using System.Globalization;
using System.IO.Compression;
using LionFire.Trading.Automation;

namespace LionFire.Trading.Automation.Optimization.Scoring;

/// <summary>
/// Reads backtest results from CSV files.
/// </summary>
public static class BacktestResultsReader
{
    /// <summary>
    /// Reads backtest journal entries from backtests.csv inside a zip archive.
    /// </summary>
    /// <param name="zipPath">Path to the zip file containing backtests.csv</param>
    /// <returns>List of backtest entries, or empty list if backtests.csv not found in zip</returns>
    public static List<BacktestBatchJournalEntry> ReadFromZip(string zipPath)
    {
        using var archive = ZipFile.OpenRead(zipPath);
        var entry = archive.GetEntry("backtests.csv");
        if (entry == null) return new List<BacktestBatchJournalEntry>();

        using var stream = entry.Open();
        return ReadFromStream(stream);
    }
    /// <summary>
    /// Reads backtest journal entries from a CSV file in the optimization output directory.
    /// </summary>
    /// <param name="outputDirectory">The optimization output directory containing backtests.csv</param>
    /// <returns>List of backtest entries, or empty list if file doesn't exist</returns>
    public static List<BacktestBatchJournalEntry> ReadFromDirectory(string outputDirectory)
    {
        var csvPath = Path.Combine(outputDirectory, "backtests.csv");
        return ReadFromCsv(csvPath);
    }

    /// <summary>
    /// Reads backtest journal entries from a CSV file.
    /// </summary>
    /// <param name="csvPath">Path to the backtests.csv file</param>
    /// <returns>List of backtest entries, or empty list if file doesn't exist</returns>
    public static List<BacktestBatchJournalEntry> ReadFromCsv(string csvPath)
    {
        if (!File.Exists(csvPath)) return new List<BacktestBatchJournalEntry>();

        using var stream = File.OpenRead(csvPath);
        return ReadFromStream(stream);
    }

    /// <summary>
    /// Reads backtest journal entries from a stream containing CSV data.
    /// </summary>
    public static List<BacktestBatchJournalEntry> ReadFromStream(Stream stream)
    {
        var results = new List<BacktestBatchJournalEntry>();

        var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null, // Ignore missing fields for partial reads
        };

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, config);

        // Read header
        csv.Read();
        csv.ReadHeader();

        // Read records
        while (csv.Read())
        {
            try
            {
                var entry = new BacktestBatchJournalEntry
                {
                    BatchId = csv.GetField<int>("BatchId"),
                    Id = csv.GetField<long>("Id"),
                    Fitness = csv.GetField<double>("Fitness"),
                    AD = csv.GetField<double>("AD"),
                    AMWT = csv.GetField<double>("AMWT"),
                    Wins = csv.GetField<int>("Wins"),
                    Losses = csv.GetField<int>("Losses"),
                    Breakevens = csv.GetField<int>("Breakevens"),
                    MaxBalanceDrawdown = csv.GetField<double>("MaxBalanceDrawdown"),
                    MaxBalanceDrawdownPerunum = csv.GetField<double>("MaxBalanceDrawdownPerunum"),
                    MaxEquityDrawdown = csv.GetField<double>("MaxEquityDrawdown"),
                    MaxEquityDrawdownPerunum = csv.GetField<double>("MaxEquityDrawdownPerunum"),
                };

                results.Add(entry);
            }
            catch (CsvHelper.MissingFieldException)
            {
                // Skip records with missing required fields
                continue;
            }
            catch (Exception ex) when (ex.GetType().Name.Contains("TypeConverter"))
            {
                // Skip records with conversion errors
                continue;
            }
        }

        return results;
    }
}
