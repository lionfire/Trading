using LionFire.Serialization;
using LionFire.Structures;
using Newtonsoft.Json;
using System.ComponentModel;

namespace LionFire.Trading.Automation;

[TypeConverter(typeof(ParsableConverter<OptimizationRunReference>))]
public record OptimizationRunReference(
    string Bot,
    string Exchange,
    string ExchangeArea,
    string Symbol,
    string TimeFrame,
    DateTimeOffset Start,
    DateTimeOffset EndExclusive,
    string RunId
    )
    : IParsableSlim<OptimizationRunReference>
{
    public static implicit operator OptimizationRunReference(OptimizationRunInfo? optimizationRunInfo) => FromOptimizationRunInfo(optimizationRunInfo);

    public static implicit operator ExchangeSymbolTimeFrame(OptimizationRunReference? r) =>
        new(r.Exchange, r.ExchangeArea, r.Symbol, LionFire.Trading.TimeFrame.Parse(r.TimeFrame));

    public static OptimizationRunReference? FromOptimizationRunInfo(OptimizationRunInfo? optimizationRunInfo)
    {
        return optimizationRunInfo == null ? null : new OptimizationRunReference(
            optimizationRunInfo.BotName ?? TradingConstants.UnknownBot,
            optimizationRunInfo.Exchange ?? TradingConstants.UnknownExchange,
            optimizationRunInfo.ExchangeArea ?? TradingConstants.UnknownExchangeArea,
            optimizationRunInfo.Symbol ?? TradingConstants.UnknownSymbol,
            optimizationRunInfo.TimeFrame?.ToString() ?? TradingConstants.UnknownTimeFrame,
            //DateTimeFormatting.ToConciseFileName(optimizationRunInfo.Start, optimizationRunInfo.EndExclusive),
            optimizationRunInfo.Start,
            optimizationRunInfo.EndExclusive,
            optimizationRunInfo.Guid
        );
    }

    [JsonIgnore]
    [Ignore]
    public string StartEndString => DateTimeFormatting.ToConciseFileName(Start, EndExclusive);

    //[JsonIgnore]
    //[Ignore]
    //public Type? PBotType { get; set; }
    //[JsonIgnore]
    //[Ignore]
    //public Type? BotType { get; set; }

    //public string? Bot { get; set; }
    //public string? DefaultSymbol { get; set; }
    //public string? DefaultTimeFrame { get; set; }
    //public string? DateRange { get; set; }
    //public string? Exchange { get; set; }
    //public string? DefaultExchangeArea { get; set; }

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
    //    //    DefaultSymbol = parts[1],
    //    //    DefaultTimeFrame = parts[2],
    //    //    Exchange = parts[3],
    //    //    DefaultExchangeArea = parts[4],
    //    //    DateRange = parts.Length > 5 ? parts[5] : null,
    //    //    RunId = parts.Length > 6 ? parts[6] : null
    //    //};
    //}

    public const string Separator = "/";

    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(Bot ?? TradingConstants.UnknownBot);
        sb.Append(Separator);
        sb.Append(Symbol ?? TradingConstants.UnknownSymbol);
        sb.Append(Separator);
        sb.Append(TimeFrame ?? TradingConstants.UnknownTimeFrame);
        sb.Append(Separator);

        sb.Append(Exchange ?? TradingConstants.UnknownExchange);
        sb.Append(Separator);
        sb.Append(ExchangeArea ?? TradingConstants.UnknownExchangeArea);
        sb.Append(Separator);

        sb.Append(DateTimeFormatting.ToConciseFileName(Start, EndExclusive));
        //sb.Append(DateRange ?? "UnknownDateRange");

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

        var dateParts = parts[5].Split(" - ");
        if(dateParts.Length != 2)
        {
            throw new FormatException($"Invalid date range format: {parts[5]}");
        }
        if (!DateTimeOffset.TryParse(dateParts[0].TrimEnd(), out var start) || !DateTimeOffset.TryParse(dateParts[1].TrimStart(), out var endExclusive))
        {
            throw new FormatException($"Invalid date range format: {parts[5]}");
        }

        return new OptimizationRunReference(
            parts[0],
            parts[1],
            parts[2],
            parts[3],
            parts[4],
            start,
            endExclusive,
            parts[6]
        );
    }
}
