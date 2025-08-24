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

    public ValidationContext ValidateThis(ValidationContext v)
    {
        BotType ??= PBotType == null ? null : BotTyping.TryGetBotType(PBotType);

        return v
            .PropertyNotNull(nameof(PBotType), PBotType)
            .PropertyNotNull(nameof(BotType), BotType)
            .PropertyNotNull(nameof(ExchangeSymbolTimeFrame), ExchangeSymbolTimeFrame)
            .PropertyNotNull(nameof(Start), Start)
            .PropertyNotNull(nameof(EndExclusive), EndExclusive)
            ;
    }

    public bool IsValid =>
        this.Validate().Valid;
    // OLD IsValid
    //(PBotType != null || ExchangeSymbolTimeFrame != null) &&
    //(Start != null && EndExclusive != null) &&
    //(ExchangeSymbolTimeFrame == null || ExchangeSymbolTimeFrame.Exchange != null);

    #endregion

    #region Components

    public POptimization? POptimization 
    { 
        get => pOptimization;
        set
        {
            //if (pOptimization != null && value != null)
            //{
            //    throw new InvalidOperationException($"{nameof(POptimization)} is already set and cannot be changed once set unless first set back to null.");
            //}
            pOptimization = value;
        }
    }
    private POptimization? pOptimization;

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

    private Type? pBotType;

    [JsonIgnore]
    public Type? PBotType 
    { 
        get => pBotType;
        set => this.RaiseAndSetIfChanged(ref pBotType, value);
    }

    private Type? botType;

    [JsonIgnore]
    public Type? BotType 
    { 
        get => botType;
        set => this.RaiseAndSetIfChanged(ref botType, value);
    }


    // Backing fields to avoid circular references
    private string? exchange;
    private string? area;
    private string? defaultSymbol;
    private TimeFrame? defaultTimeFrame;

    [JsonIgnore]
    public string? Exchange
    {
        get => exchange;
        set => exchange = value;
    }
    [JsonIgnore]
    public string? Area
    {
        get => area;
        set => area = value;
    }
    public ExchangeArea? DefaultExchangeArea => (Exchange != null && Area != null) ? new ExchangeArea(Exchange, Area) : null;

    public ExchangeSymbol? ExchangeSymbol => (Exchange != null && Area != null && DefaultSymbol != null) ? new ExchangeSymbol(Exchange, Area, DefaultSymbol) : null;

    [JsonIgnore]
    public string? DefaultSymbol
    {
        get => defaultSymbol;
        set => defaultSymbol = value;
    }

    [JsonIgnore]
    public TimeFrame? DefaultTimeFrame
    {
        get => defaultTimeFrame;
        set => defaultTimeFrame = value;
    }
    [JsonIgnore]
    public string? TimeFrameString { get => DefaultTimeFrame; set => DefaultTimeFrame = value; }

    public ExchangeSymbolTimeFrame? ExchangeSymbolTimeFrame
    {
        get => (Exchange != null && Area != null && DefaultSymbol != null && DefaultTimeFrame != null) ? new ExchangeSymbolTimeFrame(Exchange, Area, DefaultSymbol, DefaultTimeFrame) : null;
        set
        {
            exchange = value?.Exchange;
            area = value?.Area;
            defaultSymbol = value?.Symbol;
            defaultTimeFrame = value?.TimeFrame;
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

