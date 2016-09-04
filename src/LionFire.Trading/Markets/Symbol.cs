using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Trading.Backtesting;

namespace LionFire.Trading
{
    public interface Symbol
    {
        double Ask { get; }
        BacktestSymbolSettings BacktestSymbolSettings { get; set; }
        double Bid { get; }
        string Code { get; }
        int Digits { get; }
        [Obsolete("Use PreciseLeverage instead")]
        int Leverage { get; }
        long LotSize { get; }
        double PipSize { get; }
        double PipValue { get; }
        [Obsolete("Use TickSize instead")]
        double PointSize { get; }
        double PreciseLeverage { get; }
        double Spread { get; }
        double TickSize { get; }
        double TickValue { get; }
        double UnrealizedGrossProfit { get; }
        double UnrealizedNetProfit { get; }
        long VolumeMax { get; }
        long VolumeMin { get; }
        long VolumeStep { get; }

        long NormalizeVolume(double volume, RoundingMode roundingMode = RoundingMode.ToNearest);
        long QuantityToVolume(double quantity);
        double VolumeToQuantity(long volume);
    }

    public class SymbolImpl : Symbol
    {

        #region Identity

        public string Code {
            get; private set;
        }

        #endregion

        #region Relationships

        public IMarket Market { get; set; }

        


        #endregion

        #region Config

        public BacktestSymbolSettings BacktestSymbolSettings { get; set; }

        #endregion

        #region Construction

        public SymbolImpl(string symbolCode, IMarket market)
        {
            this.Code = symbolCode;
            this.Market = market;
        }

        #endregion

        #region Current Market State

        public double Ask {
            get; set;
        } = double.NaN;

        public double Bid {
            get; set;
        } = double.NaN;

        public double Spread {
            get {
                return Ask - Bid;
            }
        }

        #endregion

        #region 

        public int Digits {
            get {
                throw new NotImplementedException();
            }
        }

        public int Leverage {
            get {
                throw new NotImplementedException();
            }
        }

        public long LotSize {
            get {
                throw new NotImplementedException();
            }
        }

        public double PipSize {
            get {
                throw new NotImplementedException();
            }
        }

        public double PipValue {
            get {
                throw new NotImplementedException();
            }
        }

        public double PointSize {
            get {
                throw new NotImplementedException();
            }
        }

        public double PreciseLeverage {
            get {
                throw new NotImplementedException();
            }
        }

        public long VolumeMax {
            get {
                throw new NotImplementedException();
            }
        }

        public long VolumeMin {
            get {
                throw new NotImplementedException();
            }
        }

        public long VolumeStep {
            get {
                throw new NotImplementedException();
            }
        }

        public long NormalizeVolume(double volume, RoundingMode roundingMode = RoundingMode.ToNearest)
        {
            throw new NotImplementedException();
        }

        public long QuantityToVolume(double quantity)
        {
            throw new NotImplementedException();
        }

        public double VolumeToQuantity(long volume)
        {
            throw new NotImplementedException();
        }

        #endregion

        public double TickSize {
            get {
                throw new NotImplementedException();
            }
        }

        public double TickValue {
            get {
                throw new NotImplementedException();
            }
        }

        #region Account Current Positions

        public double UnrealizedGrossProfit {
            get {
                throw new NotImplementedException();
            }
        }

        public double UnrealizedNetProfit {
            get {
                throw new NotImplementedException();
            }
        }

        #endregion

    }

}
