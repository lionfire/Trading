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

        public void LoadSymbolInfo(SymbolInfo info)
        {
            this.Digits = info.Digits;
            this.Leverage = (int)info.Leverage;
            this.LotSize = info.LotSize;
            this.PreciseLeverage = info.Leverage;
            this.PipSize = info.PipSize;
            this.PointSize = info.PointSize;
            this.TickSize = info.TickSize;
            this.VolumeMin = info.VolumeMin;
            this.VolumeMax = info.VolumeMax;
            this.VolumeStep = info.VolumeStep;

        }

        public int Digits {
            get;private set;
        }

        public int Leverage {
            get; private set;
        }

        public long LotSize {
            get;set;
        }

        public double PipSize {
            get;private set;
        }

        public double PipValue {
            get {
                throw new NotImplementedException();
            }
        }

        public double PointSize {
            get; private set;
        }

        public double PreciseLeverage {
            get; private set;
        }

        public long VolumeMax {
            get; private set;
        }

        public long VolumeMin {
            get; private set;
        }

        public long VolumeStep {
            get; private set;
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
            get;private set;
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
