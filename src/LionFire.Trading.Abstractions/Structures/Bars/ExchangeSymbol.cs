#nullable enable
using LionFire.Validation;
using Orleans;

namespace LionFire.Trading;

//public record ExchangeArea(string Exchange, string ExchangeArea) { }
[Alias("exchange-symbol")]
[GenerateSerializer]
public record ExchangeSymbol(string? Exchange, string? ExchangeArea, string? Symbol)
    : IKeyed<string>
    , IValidatable
{


    public virtual string Key => $"{Exchange}.{ExchangeArea}:{Symbol}";

    public static ExchangeSymbol Unknown = new("UnknownExchange", "UnknownArea", "UnknownSymbol");

    public ValidationContext ValidateThis(ValidationContext v)
    {
        return v
            .PropertyNotNullOrEmptyString(nameof(Exchange), Exchange)
            .PropertyNotNullOrEmptyString(nameof(ExchangeArea), ExchangeArea)
            .PropertyNotNullOrEmptyString(nameof(Symbol), Symbol)
            ;

    }
}

