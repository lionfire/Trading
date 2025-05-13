using DynamicData;
using LionFire.IO.Reactive.Hjson;
using LionFire.Ontology;
using LionFire.Serialization;
using LionFire.Structures;
using LionFire.Trading.Automation;
using Newtonsoft.Json;
using Nito.Disposables;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.ComponentModel;
using System.Globalization;
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

public class ParsableConverter<T> : TypeConverter
    where T : IParsableSlim<T>
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
    {
        if (value is string str)
        {
            try
            {
                // Delegate to the Parse method of the type T
                return T.Parse(str);
            }
            catch (Exception ex)
            {
                throw new FormatException($"Failed to convert '{str}' to {typeof(T).Name}: {ex.Message}", ex);
            }
        }

        return base.ConvertFrom(context, culture, value);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object value, Type? destinationType)
    {
        if (destinationType == typeof(string) && value is OptimizationRunReference reference)
        {
            return reference.ToString();
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}

[TypeConverter(typeof(ParsableConverter<OptimizationRunReference>))]
public record OptimizationRunReference(
    string Bot,
    string Exchange,
    string ExchangeArea,
    string Symbol,
    string TimeFrame,
    string DateRange,
    string RunId 
    )
    :IParsableSlim<OptimizationRunReference>
{
    public static implicit operator OptimizationRunReference(OptimizationRunInfo? optimizationRunInfo) => FromOptimizationRunInfo(optimizationRunInfo);

    public static OptimizationRunReference? FromOptimizationRunInfo(OptimizationRunInfo? optimizationRunInfo)
    {
        return optimizationRunInfo == null ? null : new OptimizationRunReference(
            optimizationRunInfo.BotName ?? "UnknownBot",
            optimizationRunInfo.Exchange ?? "UnknownExchange",
            optimizationRunInfo.ExchangeArea ?? "UnknownExchangeArea",
            optimizationRunInfo.Symbol ?? "UnknownSymbol",
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

    public const string Separator = "/";

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(Bot ?? "UnknownBot");
        sb.Append(Separator);
        sb.Append(Symbol ?? "UnknownSymbol");
        sb.Append(Separator);
        sb.Append(TimeFrame ?? "UnknownTimeFrame");
        sb.Append(Separator);

        sb.Append(Exchange ?? "UnknownExchange");
        sb.Append(Separator);
        sb.Append(ExchangeArea ?? "UnknownExchangeArea");
        sb.Append(Separator);

        sb.Append(DateRange ?? "UnknownDateRange");
        sb.Append(Separator);
        sb.Append(RunId ?? "UnknownRunId");
        return sb.ToString();
    }
    public static OptimizationRunReference Parse(string s)
    {
        var parts = s.Split('/');
        if (parts.Length != 7)
        {
            throw new FormatException($"Invalid format: {s}");
        }
        return new OptimizationRunReference(
            parts[0],
            parts[1],
            parts[2],
            parts[3],
            parts[4],
            parts[5],
            parts[6]
        );
    }
}


public class OptimizationRunReferenceConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
    {
        if (value is string str) { return OptimizationRunReference.Parse(str); }
        return base.ConvertFrom(context, culture, value);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType)
    {
        if (destinationType == typeof(string) && value is OptimizationRunReference reference)
        {
            return reference.ToString();
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}


public class OptimizationBacktestReference
{
    public OptimizationRunReference? OptimizationRunReference { get; set; }
    public int BatchId { get; set; }
    public long BacktestId { get; set; }
}

[TypeConverter(typeof(ParsableConverter<BacktestReference>))]
public record BacktestReference(int BatchId, long BacktestId) : IParsableSlim<BacktestReference>
{
    public static BacktestReference Parse(string s)
    {
        var parts = s.Split('-');
        if (parts.Length != 2)
        {
            throw new FormatException($"Invalid format: {s}");
        }
        return new BacktestReference(int.Parse(parts[0]), long.Parse(parts[1]));
    }

    public override string ToString()
    {
        return $"{BatchId}-{BacktestId}";
    }
}

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
