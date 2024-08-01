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
        public FuturesPositionBase(IAccount2 account, string symbol) : base(account, symbol)
        {
        }

        public int Leverage { get; set; }
        public bool Isolated { get; set; }
    }

    public class PositionBase : IPosition
    {
        public PositionBase(IAccount2 account, string symbol)
        {
            Account = account;
            Symbol = symbol;
        }

        public IAccount2 Account { get; set; }
        public string? Comment { get; set; }

        public decimal Commissions { get; set; }

        public decimal EntryPrice { get; set; }
        public decimal? LastPrice { get; set; }
        public decimal? MarkPrice { get; set; }
        public decimal? LiqPrice { get; set; }

        public DateTime EntryTime { get; set; }

        public decimal GrossProfit { get; set; }

        public int Id { get; set; }

        public string? Label { get; set; }

        public decimal NetProfit { get; set; }

        public decimal Pips { get; set; }

        public decimal Quantity { get; set; }

        public decimal? StopLoss { get; set; }
        public string? StopLossWorkingType { get; set; }

        public decimal Swap { get; set; }

        public string Symbol { get; set; }
        //{
        //    get => SymbolId.Symbol;
        //    set => SymbolId.Symbol = new SymbolId { Symbol = value };
        //}
        public SymbolId? SymbolId { get; set; }

        public decimal? TakeProfit { get; set; }

        public TradeKind TradeType { get; set; }

        public long Volume { get; set; }

        public decimal? UsdEquivalentQuantity { get; set; }

        public override string ToString() => $"{TradeType} {Symbol}: {GrossProfit}";
    }
}
