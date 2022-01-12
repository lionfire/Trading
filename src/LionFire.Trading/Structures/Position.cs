using System;

namespace LionFire.Trading
{

    public interface IPositionDouble_CTraderCompat
    {
        double VolumeInUnits { get; }
    }
    
    

    public class PositionDouble : IPositionDouble_CTraderCompat
    {
        public IAccount Account { get; set; }

        public PositionKind Kind
        {
            get
            {
                var kind = PositionKind.Unspecified;

                if (Account != null)
                {
                    if (Account.IsRealMoney)
                    {
                        kind |= PositionKind.Live;
                    }
                    else
                    {
                        kind |= PositionKind.Demo;
                    }
                }

                if (kind == PositionKind.Unspecified)
                {
                    kind = PositionKind.Paper;
                }
                return kind;
            }
        }
        public bool IsPaper { get { return Kind == PositionKind.Paper; } }

        public string Comment { get; set; }
        public double Commissions { get; set; } // REVEW - verify this is getting set

        public double EntryPrice { get; set; }

        public double ExitPrice { get; set; }

        public DateTime EntryTime { get; set; }

        public DateTime CreateTime { get; set; }

        /// <summary>
        /// If NaN, will be calculated based on CurrentExitPrice
        /// </summary>
        public double GrossProfit
        {
            get
            {
                if (!double.IsNaN(grossProfit)) { return grossProfit; }
                var result = ((CurrentExitPrice - EntryPrice) / Symbol.TickSize) * Symbol.TickValue;

                if (TradeType == TradeType.Sell)
                {
                    result *= -1;
                }
                return result;
            }
            set { grossProfit = value; }
        }
        private double grossProfit = double.NaN;

        public string GrossProfitString { get { return GrossProfit.CentsToCurrencyString(); } }
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int DealId { get; set; }

        public string Label { get; set; }
        public double NetProfit { get { return GrossProfit - (Commissions * 2.0) - Swap; } }
        public string NetProfitString { get { return NetProfit.CentsToCurrencyString(); } }
        public double Pips { get; set; }
        public double Quantity { get; set; }
        public double? StopLoss { get; set; }
        public double Swap { get; set; } // TODO - calculate this
        public string SymbolCode { get; set; }

        public Symbol Symbol { get; set; }

        public double? TakeProfit { get; set; }
        public TradeType TradeType { get; set; }
        public double Volume { get; set; }
        //double IPosition_CTraderCompat.VolumeInUnits => Volume;
        public double VolumeInUnits => Volume;

        public double FilledVolume { get; set; }

        #region Derived

        public double CurrentExitPrice
        {
            get
            {
                return TradeType == TradeType.Buy ? Symbol.Bid : Symbol.Ask;
            }
        }

        #endregion

        public PositionCloseDetails CloseDetails { get; set; }

        public override string ToString()
        {
            var sl = StopLoss.HasValue ? $" sl:{StopLoss.Value}" : "";
            var tp = TakeProfit.HasValue ? $" tp:{TakeProfit.Value}" : "";
            return $"{TradeType} {Volume} {SymbolCode} @ {EntryPrice} {sl}{tp} gross:{GrossProfitString} net:{NetProfitString}";
        }

    }


}
