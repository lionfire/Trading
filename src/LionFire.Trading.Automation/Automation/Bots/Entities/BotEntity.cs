using DynamicData;
using LionFire.IO.Reactive.Hjson;
using LionFire.Ontology;
using LionFire.Trading.Automation;
using Newtonsoft.Json;
using Nito.Disposables;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Globalization;
using System.Reflection;
using AliasAttribute = LionFire.Ontology.AliasAttribute;

namespace LionFire.Trading.Automation;


[Alias("Bot")]
public partial class BotEntity : ReactiveObject
{
    public PBotHarness? PBotHarness { get; set; }

    public OptimizationBacktestReference? BacktestReference { get; set; }


    [Reactive]
    bool _enabled;

    /// <summary>
    /// If true, actively trade this bot on the attached trading account.
    /// (The account may be real money or demo.)
    /// </summary>
    [Reactive]
    bool _live;

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
    //public List<BacktestHandle>? Backtests { get; set; } // Maybe
    public IObservableCache<OptimizationBacktestReference, string> Backtests => backtests;
    private SourceCache<OptimizationBacktestReference, string> backtests = new SourceCache<OptimizationBacktestReference, string>(o=>o.ToString());
}

#if FUTURE // Maybe
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
#endif