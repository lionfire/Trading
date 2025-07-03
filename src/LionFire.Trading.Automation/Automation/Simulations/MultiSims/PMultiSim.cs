using LionFire.Trading.Automation.Optimization;
using LionFire.Validation;
using ReactiveUI;
using ReactiveUI.SourceGenerators;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Automation;

public class PMultiSim<TPrecision> : PMultiSim
    where TPrecision : struct, INumber<TPrecision>
{
    public PMultiSim(Type PBotType, ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame, DateTimeOffset Start, DateTimeOffset EndExclusive, SimFeatures Features = SimFeatures.Unspecified) : base(PBotType, ExchangeSymbolTimeFrame, Start, EndExclusive, Features)
    {
    }
}

public partial class PMultiSim : ReactiveObject, IValidatable
{
    #region Validation

    public ValidationContext ValidateThis(ValidationContext v) =>
        v.PropertyNotNull(nameof(PBotType), PBotType)
        .PropertyNotNull(nameof(ExchangeSymbolTimeFrame), ExchangeSymbolTimeFrame)
        .PropertyNotNull(nameof(Start), Start)
        .PropertyNotNull(nameof(EndExclusive), EndExclusive)
        ;

    public bool IsValid =>
        this.Validate().Valid;
    // OLD IsValid
    //(PBotType != null || ExchangeSymbolTimeFrame != null) &&
    //(Start != null && EndExclusive != null) &&
    //(ExchangeSymbolTimeFrame == null || ExchangeSymbolTimeFrame.Exchange != null);

    #endregion

    #region Components

    public POptimization? POptimization { get; set; }

    #endregion

    #region Lifecycle

    public PMultiSim() { }

    public PMultiSim(
        Type PBotType,
        ExchangeSymbolTimeFrame ExchangeSymbolTimeFrame,
        DateTimeOffset Start,
        DateTimeOffset EndExclusive,
        SimFeatures Features = SimFeatures.Unspecified
    )
    {
        this.PBotType = PBotType;
        this.ExchangeSymbolTimeFrame = ExchangeSymbolTimeFrame;
        this.Start = Start;
        this.EndExclusive = EndExclusive;
        this.Features = Features;

        this.WhenAnyValue(x => x.PBotType).Subscribe(pBotType =>
        {
            BotType ??= PBotType == null ? null : BotTyping.TryGetBotType(PBotType);
        });
    }

    #endregion

    #region Properties

    [Reactive]
    private Type? pBotType;

    //public Type? BotType => botType ??= PBotType == null ? null : BotTyping.TryGetBotType(PBotType);
    [Reactive]
    private Type? botType;


    [JsonIgnore]
    public string? Exchange
    {
        get => ExchangeSymbolTimeFrame?.Exchange;
        set => ExchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame(value, Area, DefaultSymbol, DefaultTimeFrame);
    }
    [JsonIgnore]
    public string? Area
    {
        get => ExchangeSymbolTimeFrame?.Area;
        set => ExchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame(Exchange, value, DefaultSymbol, DefaultTimeFrame);
    }
    public ExchangeArea? DefaultExchangeArea => (Exchange != null && Area != null) ? new ExchangeArea(Exchange, Area) : null;

    public ExchangeSymbol? ExchangeSymbol => (Exchange != null && Area != null && DefaultSymbol != null) ? new ExchangeSymbol(Exchange, Area, DefaultSymbol) : null;

    [JsonIgnore]
    public string? DefaultSymbol
    {
        get => ExchangeSymbolTimeFrame?.Symbol;
        set => ExchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame(Exchange, Area, value, DefaultTimeFrame);
    }

    [JsonIgnore]
    public TimeFrame? DefaultTimeFrame
    {
        get => ExchangeSymbolTimeFrame?.TimeFrame;
        set => ExchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame(Exchange, Area, DefaultSymbol, value);
    }
    [JsonIgnore]
    public string? TimeFrameString { get => DefaultTimeFrame; set => DefaultTimeFrame = value; }

    public ExchangeSymbolTimeFrame? ExchangeSymbolTimeFrame
    {
        get => (Exchange != null && Area != null && DefaultSymbol != null && DefaultTimeFrame != null) ? new ExchangeSymbolTimeFrame(Exchange, Area, DefaultSymbol, DefaultTimeFrame) : null;
        set
        {
            Exchange = value?.Exchange;
            Area = value?.Area;
            DefaultSymbol = value?.Symbol;
            DefaultTimeFrame = value?.TimeFrame;
        }
    }

    [Reactive]
    private DateTimeOffset start;
    [Reactive]
    private DateTimeOffset endExclusive;

    #region Derived

    public DateTime? StartDateTime { get => Start.DateTime; set => Start = DateCoercion.Coerce(value); }
    public DateTime? EndExclusiveDateTime { get => EndExclusive.DateTime; set => EndExclusive = DateCoercion.Coerce(value); }

    #endregion

    [Reactive]
    private SimFeatures features;

    #endregion


    #region Performance tuning

    public bool ShortChunks { get; init; }

    #endregion


}

public static class BotTyping
{

    public static Type TryGetBotType(Type pBotType)
    {
        Type? botType;
        if (pBotType.IsAssignableTo(typeof(IPBot2Static)))
        {
            botType = (Type)pBotType.GetProperty(nameof(IPBot2Static.StaticMaterializedType))!.GetValue(null)!;
        }
        else
        {
            throw new ArgumentException($"Provide {nameof(botType)} or a {nameof(pBotType)} of type IPBot2Static");
        }

        return botType;
    }

}

