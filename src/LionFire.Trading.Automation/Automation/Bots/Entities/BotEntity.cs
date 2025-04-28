using DynamicData;
using LionFire.Ontology;
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
    public List<BacktestReference>? Backtests { get; set; }
    //public IObservableCache<BacktestReference, DateTime> Backtests => backtests;
    //private SourceCache<BacktestReference, DateTime> backtests = new SourceCache<BacktestReference, DateTime>() ;
}

public class BacktestReference
{
    //public required BacktestInfo BacktestInfo { get; set; }
    public BacktestBatchInfo? BacktestBatchInfo { get; set; }

    public BacktestBatchJournalEntry? JournalEntry { get; set; }


    public required object PBot { get; set; }

}
