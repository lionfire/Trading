using LionFire.Trading.Backtesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface HistoricalTrade
    {
        double Balance { get; }
        int ClosingDealId { get; }
        double ClosingPrice { get; }
        DateTime ClosingTime { get; }
        string Comment { get; }
        double Commissions { get; }
        double EntryPrice { get; }
        DateTime EntryTime { get; }
        double GrossProfit { get; }
        string Label { get; }
        double NetProfit { get; }
        double Pips { get; }
        int PositionId { get; }
        double Quantity { get; }
        double Swap { get; }
        string SymbolCode { get; }
        TradeType TradeType { get; }
        double Volume { get; }
    }

    public class _HistoricalTrade : HistoricalTrade
    {
        public _HistoricalTrade()
        {
        }
        internal _HistoricalTrade(BacktestAccount account, Position position)
        {
            this.Balance = account.Balance;
            //ClosingDealId = 
            ClosingPrice = position.CurrentExitPrice;
            ClosingTime = account.Server.Time; // REVIEW - use extrapolated time?
            Comment = position.Comment;
            Commissions = position.Commissions;
            EntryPrice = position.EntryPrice;
            GrossProfit = position.GrossProfit;
            Label = position.Label;
            NetProfit = position.NetProfit;
            Pips = position.Pips;
            PositionId = position.Id;
            Quantity = position.Quantity;
            Swap = position.Swap;
            SymbolCode = position.SymbolCode;
            TradeType = position.TradeType;
            Volume = position.Volume;
        }

        public double Balance {
            get;set;
        }

        public int ClosingDealId {
            get; set;
        }

        public double ClosingPrice {
            get; set;
        }

        public DateTime ClosingTime {
            get; set;
        }

        public string Comment {
            get; set;
        }

        public double Commissions {
            get; set;
        }

        public double EntryPrice {
            get; set;
        }

        public DateTime EntryTime {
            get; set;
        }

        public double GrossProfit {
            get; set;
        }

        public string Label {
            get; set;
        }

        public double NetProfit {
            get; set;
        }

        public double Pips {
            get; set;
        }

        public int PositionId {
            get; set;
        }

        public double Quantity {
            get; set;
        }

        public double Swap {
            get; set;
        }

        public string SymbolCode {
            get; set;
        }

        public TradeType TradeType {
            get; set;
        }

        public double Volume {
            get; set;
        }
    }
}
