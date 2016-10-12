using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    [Flags]
    public enum AccountAssetType
    {
        None = 0,
        Equity = 1 << 0,
        Balance = 1 << 1,
    }
    public enum StakeType
    {
        None = 0,
        Position = 1 << 0,
        Order = 1 << 1,
    }

    public class ExposureItem
    {
        public ExposureItem(string code, IAccount account)
        {
            this.SymbolCode = code;
            this.Account = account;

        }
        public string SymbolCode { get; set; }
        public IAccount Account { get; set; }
        public IPositions Positions { get { return Account.Positions; } }
        public IPendingOrders Orders { get { return Account.PendingOrders; } }



        public double LongPositions {
            get {
                double sum = 0.0;
                foreach (var position in Positions)
                {
                    sum += position.NetProfit;
                }
                return sum;
            }
        }


        public double GetExposure(AccountAssetType assetType, TradeType? tradeType = null, StakeType? stakeType=null, string symbol = null)
        {
            double sum = 0.0;

            if ((stakeType & StakeType.Position) == StakeType.Position)
            {
            }
            if ((stakeType & StakeType.Order) == StakeType.Order)
            {
            }
            //var source
            return sum;
        }

        public double LongPositionsExposure {
            get {
                //return GetExposure(AccountAssetType.Equity
                double sum = 0.0;

                //foreach (var position in Positions)
                //{
                //    if (!position.StopLoss.HasValue) return double.PositiveInfinity;

                //    if (position.TradeType == TradeType.Buy)
                //    {
                //        sum += position.CurrentExitPrice - ;
                //    }
                //    else
                //    {
                //        sum += position.CurrentExitPrice;
                //    }

                //    //position.Symbol.PipValue
                //    sum += position.NetProfit;
                //}
                return sum;
            }
        }

        public double ShortPositions { get; set; }

        public double NetShort { get; set; }
        public double NetLong { get; set; }

        public bool IsMatch(string code)
        {
            if (code == "*") return true;

            if (code == SymbolCode) return true;

            if (SymbolCode.Length == 6)
            {
                if (SymbolCode.Substring(0, 3) == code) return true;
                if (SymbolCode.Substring(3, 3) == code) return true;
            }

            // TODO other detections?

            return false;
        }

    }
    public class PositionStats
    {
        public IAccount Account { get; set; }
        public PositionStats(IAccount account) { this.Account = account; }

        public Dictionary<string, ExposureItem> Exposure { get; set; }

        public ExposureItem GetExposure(string symbol)
        {
            ExposureItem item;
            if (!Exposure.TryGetValue(symbol, out item))
            {
                item = new ExposureItem(symbol, Account);
                Exposure.Add(symbol, item);
            }

            return item;
        }
    }
}
