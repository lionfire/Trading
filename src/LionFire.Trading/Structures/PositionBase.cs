using LionFire.Trading;
using System;

namespace LionFire.Trading
{

    public interface IFuturesPosition
    {
        int Leverage { get; set; }
        bool Isolated { get; set; }
    }
    public class FuturesPositionBase : PositionBase, IFuturesPosition
    {
        public int Leverage { get; set; }
        public bool Isolated { get; set; }
    }

    public class PositionBase : IPosition
    {
        public string Comment { get; set; }

        public decimal Commissions { get; set; }

        public decimal EntryPrice { get; set; }
        public decimal? LastPrice { get; set; }
        public decimal? MarkPrice { get; set; }
        public decimal? LiqPrice { get; set; }

        public DateTime EntryTime { get; set; }

        public decimal GrossProfit { get; set; }

        public int Id { get; set; }

        public string Label { get; set; }

        public decimal NetProfit { get; set; }

        public decimal Pips { get; set; }

        public decimal Quantity { get; set; }

        public decimal? StopLoss { get; set; }
        public string? StopLossWorkingType { get; set; }

        public decimal Swap { get; set; }

        public string SymbolCode { get; set; }

        public decimal? TakeProfit { get; set; }

        public TradeKind TradeType { get; set; }

        public long Volume { get; set; }

        public decimal? UsdEquivalentQuantity { get; set; }

        public override string ToString() => $"{TradeType} {SymbolCode}: {GrossProfit}";
    }
}
