using LionFire.Structures;
using System.Diagnostics;

namespace LionFire.Trading.Automation;

public class BacktestsRepository
{
    #region Dependencies

    public BotTypeRegistry BotTypeRegistry { get; }
    public BacktestBatchJournalCsvSerialization CsvSerialization { get; }

    #endregion

    #region Options

    public BacktestOptions BacktestOptions { get; }
    //public IOptionsMonitor<BacktestRepositoryOptions> RepositoryOptions { get; }
    public BacktestRepositoryOptions RepositoryOptions { get; }

    #endregion

    #region Lifecycle

    public BacktestsRepository(IOptionsMonitor<BacktestOptions> backtestOptions, IOptionsSnapshot<BacktestRepositoryOptions> executionOptions, BotTypeRegistry botTypeRegistry, BacktestBatchJournalCsvSerialization csvSerialization)
    {
        BacktestOptions = backtestOptions.CurrentValue;
        RepositoryOptions = executionOptions.Value;
        BotTypeRegistry = botTypeRegistry;
        CsvSerialization = csvSerialization;
    }

    #endregion

    #region Directory

    public string GetOptimizationRunsBaseDirectory(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, string botTypeName, DateTimeOffset? start, DateTimeOffset? endExclusive) // BLOCKING I/O
    {
        var path = BacktestOptions.Dir;

        if (RepositoryOptions.BotSubDir) { path = System.IO.Path.Combine(path, botTypeName); }
        if (RepositoryOptions.SymbolSubDir) { path = System.IO.Path.Combine(path, exchangeSymbolTimeFrame?.Symbol ?? "UnknownSymbol"); }

        if (RepositoryOptions.TimeFrameDir)
        {
            path = System.IO.Path.Combine(path, exchangeSymbolTimeFrame!.TimeFrame?.ToString() ?? "UnknownTimeFrame");
        }
        if (RepositoryOptions.DateRangeDir)
        {
            path = System.IO.Path.Combine(path, DateTimeFormatting.ToConciseFileName(start, endExclusive));
        }
        //if (RepositoryOptions.ExchangeSubDir) { path = System.IO.Path.Combine(path, ExchangeSymbol?.Exchange ?? "UnknownExchange"); }
        if (RepositoryOptions.ExchangeAndAreaSubDir)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(exchangeSymbolTimeFrame?.Exchange ?? "UnknownExchange");
            if (exchangeSymbolTimeFrame?.ExchangeArea != null)
            {
                sb.Append(".");
                sb.Append(exchangeSymbolTimeFrame.ExchangeArea);
            }
            path = System.IO.Path.Combine(path, sb.ToString());
        }
        return path;
    }

    //public string DirForOptimizationRun(OptimizationRunReference orr)
    //{
    //    return GetGuidOutputDirectory(orr, orr.Bot, orr.RunId,  start, endExclusive);
    //}

    private Task<(string dir, string runId)> GetNumberedRunDirectory(ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame, string botTypeName, DateTimeOffset? start, DateTimeOffset? endExclusive) // BLOCKING I/O
    {
        return Task.Run(() =>
        {
            var path = Path.Combine(GetOptimizationRunsBaseDirectory(ExchangeSymbolTimeFrame, botTypeName, start, endExclusive));

            return FilesystemUtils.GetUniqueDirectory(path, "", "", 4); // BLOCKING I/O
        });
    }

    public string GetOptimizationRunDirectory(OptimizationRunReference orr)
    {
        return GetOptimizationRunDirectory(new(orr.Exchange, orr.ExchangeArea, orr.Symbol, orr.TimeFrame), orr.Bot, orr.Start, orr.EndExclusive, orr.RunId);
    }
    public string GetOptimizationRunDirectory(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame,
        string botTypeName,
        DateTimeOffset start,
        DateTimeOffset endExclusive,
        string runId
        )
    {
        var dir = Path.Combine(GetOptimizationRunsBaseDirectory(exchangeSymbolTimeFrame, botTypeName, start, endExclusive), runId);

        return dir;
    }

    public async Task<(string dir, string runId)> GetAndCreateOptimizationRunDirectory(ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame,
        string botTypeName,
        DateTimeOffset start,
        DateTimeOffset endExclusive
        )
    {
        // TODO: integer mode
        switch (RepositoryOptions.BacktestIdKind)
        {
            case BacktestIdKind.Guid:
                var runId = Guid.NewGuid().ToString("N");
                var dir = GetOptimizationRunDirectory(ExchangeSymbolTimeFrame!,
                    botTypeName,
                    start,
                    endExclusive,
                    runId);

                await Task.Run(() =>
                {
                    Debug.WriteLine("Creating directory...   " + dir);
                    Directory.CreateDirectory(dir); // BLOCKING I/O
                    Debug.WriteLine("Creating directory...done.  " + dir);
                }).ConfigureAwait(false);
                return (dir, runId);
            case BacktestIdKind.Integer:
                return await GetNumberedRunDirectory(ExchangeSymbolTimeFrame, botTypeName, start, endExclusive);
            default:
                throw new NotImplementedException($"Unknown {nameof(BacktestIdKind)}: {RepositoryOptions.BacktestIdKind}");
        }
    }

    #endregion

    #region Load

    public async Task<IPBot2> Load(OptimizationBacktestReference obr)
    {
        ArgumentNullException.ThrowIfNull(obr.OptimizationRunReference, nameof(obr.OptimizationRunReference));

        string dir = GetOptimizationRunDirectory(obr.OptimizationRunReference);

        var pBotType = BotTypeRegistry.PBotRegistry.GetTypeFromNameOrThrow(obr.OptimizationRunReference.Bot);

        return await Task.Run(() =>
        {
            return CsvSerialization.LoadBacktest(pBotType, dir, obr);
        });
    }

    #endregion

}
