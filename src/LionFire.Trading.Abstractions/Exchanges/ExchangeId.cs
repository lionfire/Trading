﻿namespace LionFire.Trading;

public readonly record struct ExchangeId(string Exchange, string ExchangeArea)
{
    public const char Separator = '.';

    public bool HasValue => !string.IsNullOrEmpty(Exchange);

    public string Id => string.IsNullOrEmpty(ExchangeArea) ? Exchange : $"{Exchange}{Separator}{ExchangeArea}";

    public static implicit operator ExchangeId(string exchangeId)
    {
        var x = exchangeId.Split(Separator, 2);
        return new ExchangeId(x[0], x.Length == 2 ? x[1] : "");
    }
    public static implicit operator ExchangeId(ExchangeSymbol exchangeSymbol) => new(exchangeSymbol.Exchange, exchangeSymbol.Area);


    public override string ToString() => Id;
}
