using System;

namespace LionFire.Trading.Exchanges;

public record ExchangeTrade
{
    public required string Symbol { get; init; }
    public required decimal Price { get; init; }
    public required decimal Quantity { get; init; }
    public required bool IsBuyerMaker { get; init; }
    public required DateTime Timestamp { get; init; }
    public string? TradeId { get; init; }
}

public record ExchangeOrderBook
{
    public required string Symbol { get; init; }
    public required DateTime Timestamp { get; init; }
    public required OrderBookEntry[] Bids { get; init; }
    public required OrderBookEntry[] Asks { get; init; }
    public long? LastUpdateId { get; init; }
}

public record OrderBookEntry
{
    public required decimal Price { get; init; }
    public required decimal Quantity { get; init; }
}

public record ExchangeTicker
{
    public required string Symbol { get; init; }
    public required decimal BidPrice { get; init; }
    public required decimal BidQuantity { get; init; }
    public required decimal AskPrice { get; init; }
    public required decimal AskQuantity { get; init; }
    public decimal? LastPrice { get; init; }
    public decimal? Volume24h { get; init; }
    public required DateTime Timestamp { get; init; }
}

public record ExchangeSymbolInfo
{
    public required string Symbol { get; init; }
    public required string BaseAsset { get; init; }
    public required string QuoteAsset { get; init; }
    public required SymbolStatus Status { get; init; }
    public decimal? MinOrderQuantity { get; init; }
    public decimal? MaxOrderQuantity { get; init; }
    public decimal? TickSize { get; init; }
    public int? PricePrecision { get; init; }
    public int? QuantityPrecision { get; init; }
}

public enum SymbolStatus
{
    Unknown,
    Trading,
    Halted,
    Delisted
}