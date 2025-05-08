using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Automation;

public partial class PMultiBacktest : ReactiveObject
{
    public bool IsValid =>
        (PBotType != null || ExchangeSymbolTimeFrame != null) &&
        (Start != null && EndExclusive != null) &&
        (ExchangeSymbolTimeFrame == null || ExchangeSymbolTimeFrame.Exchange != null);

    public PMultiBacktest(
        Type? PBotType = null,
        ExchangeSymbolTimeFrame? ExchangeSymbolTimeFrame = null,
        DateTimeOffset? Start = null,
        DateTimeOffset? EndExclusive = null,
        BotHarnessFeatures Features = BotHarnessFeatures.Unspecified
    )
    {
        this.PBotType = PBotType;
        this.ExchangeSymbolTimeFrame = ExchangeSymbolTimeFrame;
        this.Start = Start;
        this.EndExclusive = EndExclusive;
        this.Features = Features;
    }

    [Reactive]
    private Type? pBotType;

    [JsonIgnore]
    public string? Exchange
    {
        get => ExchangeSymbolTimeFrame?.Exchange;
        set => ExchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame(value, ExchangeArea, Symbol, TimeFrame);
    }
    [JsonIgnore]
    public string? ExchangeArea
    {
        get => ExchangeSymbolTimeFrame?.ExchangeArea;
        set => ExchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame(Exchange, value, Symbol, TimeFrame);
    }

    [JsonIgnore]
    public string? Symbol
    {
        get => ExchangeSymbolTimeFrame?.Symbol;
        set => ExchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame(Exchange, ExchangeArea, value, TimeFrame);
    }

    [JsonIgnore]
    public TimeFrame? TimeFrame
    {
        get => ExchangeSymbolTimeFrame?.TimeFrame;
        set => ExchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame(Exchange, ExchangeArea, Symbol, value);
    }
    [JsonIgnore]
    public string? TimeFrameString { get => TimeFrame; set => TimeFrame = value; }

    [Reactive]
    private ExchangeSymbolTimeFrame? exchangeSymbolTimeFrame;
    [Reactive]
    private DateTimeOffset? start;
    [Reactive]
    private DateTimeOffset? endExclusive;

    public DateTime? StartDateTime { get => Start?.DateTime; set => Start = DateCoercion.Coerce(value); }
    public DateTime? EndExclusiveDateTime { get => EndExclusive?.DateTime; set => EndExclusive = DateCoercion.Coerce(value); }


    [Reactive]
    private BotHarnessFeatures features;

}

