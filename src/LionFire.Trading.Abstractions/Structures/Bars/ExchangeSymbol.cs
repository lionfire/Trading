#nullable enable
using LionFire.Validation;
using Microsoft.CodeAnalysis;
using Orleans;

namespace LionFire.Trading;

[Alias("exchange-symbol")]
[GenerateSerializer]
public record ExchangeSymbol(string Exchange, string Area, string Symbol)
    : ExchangeArea(Exchange, Area)
{

    #region Static

    public static ExchangeSymbol GenericUSD = new(TradingConstants.UnknownExchange, TradingConstants.UnknownExchange, "USD");
    public static ExchangeSymbol Unknown = new(TradingConstants.UnknownExchange, TradingConstants.UnknownArea, TradingConstants.UnknownSymbol);

    #endregion

    public ExchangeSymbol(ExchangeArea exchangeArea, string symbol) : this(exchangeArea.Exchange, exchangeArea.Area, symbol) { }

    public override string Key => $"{Exchange}.{Area}:{Symbol}";

    #region Validation

    public override ValidationContext ValidateThis(ValidationContext v) => v
            .PropertyNotNullOrEmptyString(nameof(Exchange), Exchange)
            .PropertyNotNullOrEmptyString(nameof(Area), Area)
            .PropertyNotNullOrEmptyString(nameof(Symbol), Symbol)
            ;

    #endregion
}

