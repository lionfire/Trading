using DynamicData;
using LionFire.IO.Reactive.Hjson;
using LionFire.Ontology;
using LionFire.Trading.Automation;
using LionFire.Trading.Automation.Accounts;
using LionFire.Validation;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nito.Disposables;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Globalization;
using System.Reflection;
using AliasAttribute = LionFire.Ontology.AliasAttribute;

namespace LionFire.Trading.Automation;


[Alias("Bot")]
public partial class BotEntity : ReactiveObject, IValidatable
{
    public ValidationContext ValidateThis(ValidationContext validationContext) => validationContext
        .PropertyNotNull(nameof(Parameters), Parameters)
        ;

    //public Dictionary<string, object>? ParametersDictionary { get; set; }
    [JsonProperty("Parameters", TypeNameHandling = TypeNameHandling.Auto)]
    public IPBot2? Parameters { get; set; }

    public OptimizationBacktestReference? BacktestReference { get; set; }


    [Reactive]
    [property: JsonProperty("Enabled")]
    bool _enabled;

    /// <summary>
    /// If true, actively trade this bot on the attached trading account.
    /// (The account may be real money or demo.)
    /// </summary>
    [Reactive]
    [property: JsonProperty("Live")]
    bool _live;

    [Reactive]
    [property: JsonProperty("Name")]
    private string? _name;

    [Reactive]
    [property: JsonProperty("Comments")]
    private string? _comments;

    [Reactive]
    [property: JsonProperty("Description")]
    private string? _description;

    [Reactive]
    [property: JsonProperty("BotTypeName")]
    private string? botTypeName;

    [Reactive]
    [property: JsonProperty("BotTypeParameters")]
    private Type[]? botTypeParameters;

    /// <summary>
    /// Override the numeric type for live trading.
    /// If set, bots will use this type for live trading regardless of the saved parameter type.
    /// Common values: typeof(decimal) for maximum accuracy, typeof(double) for performance.
    /// </summary>
    [Reactive]
    [property: JsonProperty("LiveNumericTypeOverride")]
    private Type? liveNumericTypeOverride;

    /// <summary>
    /// Minimum log level to capture for this bot's harness.
    /// Null means use the system default log level.
    /// Logs below this level will be filtered out.
    /// </summary>
    [Reactive]
    [property: JsonProperty("LogLevel")]
    private LogLevel? logLevel;

    /// <summary>
    /// Account mode for live trading.
    /// </summary>
    /// <remarks>
    /// <list type="bullet">
    ///   <item><description><see cref="BotAccountMode.LiveSimulated"/> - Realistic balance tracking (default)</description></item>
    ///   <item><description><see cref="BotAccountMode.LivePaper"/> - Infinite capital mode</description></item>
    ///   <item><description><see cref="BotAccountMode.LiveReal"/> - Real exchange trading (future)</description></item>
    /// </list>
    /// If null, defaults to <see cref="BotAccountMode.LiveSimulated"/>.
    /// </remarks>
    [Reactive]
    [property: JsonProperty("AccountMode")]
    private BotAccountMode? accountMode;

    [Reactive]
    [property: JsonProperty("Symbol")]
    private string? symbol;

    [Reactive]
    [property: JsonProperty("Exchange")]
    private string? exchange;

    [Reactive]
    [property: JsonProperty("ExchangeArea")]
    private string? exchangeArea;

    [Reactive]
    [property: JsonProperty("TimeFrame")]
    private TimeFrame? timeFrame;

    #region Derived

    [JsonIgnore]
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

    [JsonIgnore]
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
        set
        {
            Symbol = value?.Symbol;
            Exchange = value?.Exchange;
            ExchangeArea = value?.Area;
            TimeFrame = value?.TimeFrame;
        }
    }

    #endregion

    /// <summary>
    /// PMultiSim that still exist on the current version of the bot must match exactly
    /// </summary>
    //public List<BacktestHandle>? Backtests { get; set; } // Maybe
    //public IObservableCache<OptimizationBacktestReference, string> Backtests => backtests;
    public SourceCache<OptimizationBacktestReference, OptimizationBacktestReference> Backtests => backtests;
    private SourceCache<OptimizationBacktestReference, OptimizationBacktestReference> backtests = new SourceCache<OptimizationBacktestReference, OptimizationBacktestReference>(o=>o);

    
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