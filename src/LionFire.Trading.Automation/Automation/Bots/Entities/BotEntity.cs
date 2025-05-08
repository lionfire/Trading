using DynamicData;
using LionFire.Ontology;
using LionFire.Structures;
using LionFire.Trading.Automation;
using Newtonsoft.Json;
using Nito.Disposables;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Reflection;
using AliasAttribute = LionFire.Ontology.AliasAttribute;

namespace LionFire.Trading.Automation;


[Alias("Bot")]
public partial class BotEntity : ReactiveObject
{
    public PBotHarness? PBotHarness { get; set; }

    [Reactive]
    bool _enabled;

    [Reactive]
    private string? _name;

    [Reactive]
    private string? _comments;

    [Reactive]
    private string? _description;

    [Reactive]
    private string? botTypeName;

    [Reactive]
    private string? symbol;

    [Reactive]
    private string? exchange;
    [Reactive]
    private string? exchangeArea;
    [Reactive]
    private TimeFrame? timeFrame;

    #region Derived

    public ExchangeSymbol? ExchangeSymbol
    {
        get
        {
            if (string.IsNullOrEmpty(exchange) || string.IsNullOrEmpty(exchangeArea) || string.IsNullOrEmpty(symbol))
            {
                return null;
            }
            return new ExchangeSymbol(exchange, exchangeArea, symbol);
        }
    }

    public ExchangeSymbolTimeFrame? ExchangeSymbolTimeFrame
    {
        get
        {
            if (string.IsNullOrEmpty(exchange) || string.IsNullOrEmpty(exchangeArea) || string.IsNullOrEmpty(symbol) || timeFrame == null)
            {
                return null;
            }
            return new ExchangeSymbolTimeFrame(exchange, exchangeArea, symbol, timeFrame);
        }
    }

    #endregion

    /// <summary>
    /// Parameters that still exist on the current version of the bot must match exactly
    /// </summary>
    public List<BacktestHandle>? Backtests { get; set; }
    //public IObservableCache<OptimizationBacktestReference, DateTime> Backtests => backtests;
    //private SourceCache<OptimizationBacktestReference, DateTime> backtests = new SourceCache<OptimizationBacktestReference, DateTime>() ;
}

public record MultiBacktestId (
    Type PBotType,
    ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame,
    DateTimeOffset Start,
    DateTimeOffset EndExclusive,
    string Id
    )
{

}

public record OptimizationRunReference(
    string Bot,
    string Exchange,
    string ExchangeArea,
    string Symbol,
    string TimeFrame,
    string DateRange,
    string RunId 
    )
{
    public static OptimizationRunReference FromOptimizationRunInfo(OptimizationRunInfo optimizationRunInfo)
    {
        return new OptimizationRunReference(
            optimizationRunInfo.BotAssemblyNameString ?? "UnknownBot",
            optimizationRunInfo.ExchangeSymbol?.Exchange ?? "UnknownExchange",
            optimizationRunInfo.ExchangeSymbol?.ExchangeArea ?? "UnknownExchangeArea",
            optimizationRunInfo.ExchangeSymbol.Symbol ?? "UnknownSymbol",
            optimizationRunInfo.TimeFrame?.ToString() ?? "UnknownTimeFrame",
            DateTimeFormatting.ToConciseFileName(optimizationRunInfo.Start, optimizationRunInfo.EndExclusive),
            optimizationRunInfo.Guid.ToString()
        );
    }
    //public string? Bot { get; set; }
    //public string? Symbol { get; set; }
    //public string? TimeFrame { get; set; }
    //public string? DateRange { get; set; }
    //public string? Exchange { get; set; }
    //public string? ExchangeArea { get; set; }

    //public string? RunId { get; set; }

    //public static OptimizationRunReference ParseFromDirectory(string dir)
    //{
    //    // TODO - confirm this implementation
    //    //throw new Exception("generated first draft");
    //    //var parts = dir.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
    //    //if (parts.Length < 5)
    //    //{
    //    //    throw new ArgumentException("Invalid directory format");
    //    //}
    //    //return new OptimizationRunReference
    //    //{
    //    //    Bot = parts[0],
    //    //    Symbol = parts[1],
    //    //    TimeFrame = parts[2],
    //    //    Exchange = parts[3],
    //    //    ExchangeArea = parts[4],
    //    //    DateRange = parts.Length > 5 ? parts[5] : null,
    //    //    RunId = parts.Length > 6 ? parts[6] : null
    //    //};
    //}

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(Bot ?? "UnknownBot");
        sb.Append("/");
        sb.Append(Symbol ?? "UnknownSymbol");
        sb.Append("/");
        sb.Append(TimeFrame ?? "UnknownTimeFrame");
        sb.Append("/");

        sb.Append(Exchange ?? "UnknownExchange");
        sb.Append(".");
        sb.Append(ExchangeArea ?? "UnknownExchangeArea");
        sb.Append("/");

        sb.Append(DateRange ?? "UnknownDateRange");
        sb.Append("/");
        sb.Append(RunId ?? "UnknownRunId");
        return sb.ToString();
    }
}

public class OptimizationBacktestReference
{
    public OptimizationRunReference? OptimizationRunReference { get; set; }
    public int BatchId { get; set; }
    public long BacktestId { get; set; }
}

public record BacktestReference(int BatchId, long BacktestId);

public class BacktestHandle
{
    public OptimizationBacktestReference? BacktestReference { get; set; }

    #region Values

    //public required BacktestInfo BacktestInfo { get; set; }
    public OptimizationRunInfo? OptimizationRunInfo { get; set; }

    public BacktestBatchJournalEntry? JournalEntry { get; set; }
    public object? PBot { get; set; }

    #endregion

}
