using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Trading.Backtesting;

namespace LionFire.Trading
{
    public interface IBacktestSymbol
    {
        BacktestSymbolSettings BacktestSymbolSettings { get; set; }
    }

    public interface Symbol : IBacktestSymbol
    {
        double Ask { get; }
        
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

        IMarketSeries GetMarketSeries(TimeFrame timeFrame);

    }

    public class SymbolImpl : SymbolImplBase, IBacktestSymbol
    {
        public SymbolImpl(string symbolCode, IMarket market) : base(symbolCode, market) { }

        public override IMarketSeries GetMarketSeries(TimeFrame timeFrame)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class SymbolImplBase : Symbol
    {
        #region Config

        // MOVE to BacktestSymbol

        public BacktestSymbolSettings BacktestSymbolSettings { get; set; }

        #endregion

        #region Identity

        public string Code {
            get; private set;
        }

        #endregion

        #region Relationships

        public IMarket Market { get; protected set; }

        public IAccount Account { get; set; }

        #endregion

        #region Construction

        public SymbolImplBase(string symbolCode, IMarket market)
        {
            this.Code = symbolCode;
            this.Market = market;
        }

        #endregion

        #region Current Market State

        public  double Ask {
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
            this.QuantityPerHundredThousandVolume = info.QuantityPerHundredThousandVolume;
            this.VolumePerHundredThousandQuantity = info.VolumePerHundredThousandQuantity;
            this.Currency = info.Currency;
        }
        private double QuantityPerHundredThousandVolume;
        private long VolumePerHundredThousandQuantity;
        public string Currency;

        public int Digits {
            get; private set;
        }

        public int Leverage {
            get; private set;
        }

        public long LotSize {
            get; set;
        }

        public double PipSize {
            get; private set;
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
            return (long)(quantity * VolumePerHundredThousandQuantity / 100000);
        }


        public double VolumeToQuantity(long volume)
        {
            return volume * QuantityPerHundredThousandVolume / 100000.0;
        }

        #endregion

        public double TickSize {
            get; private set;
        }

        public double TickValue {
            get {
                if (Account == null) { throw new ArgumentException("Requires Account to be set"); }
                if (Account.Currency == this.Currency)
                {
                    return TickSize;
                }

                return Convert(TickSize, Account.Currency, this.Currency, null);
            }
        }

        public double PipValue {
            get {
                if (Account == null) { throw new ArgumentException("Requires Account to be set"); }
                if (Account.Currency == this.Currency)
                {
                    return PipSize;
                }

                return Convert(TickSize, Account.Currency, this.Currency, null);
            }
        }

        public double Convert(double amount, string from, string to, TradeType? tradeType)
        {
            var symbol = Market.GetSymbol(to + from );
            bool inverse = false;
            if (symbol == null)
            {
                inverse = true;
                symbol = Market.GetSymbol(from + to);
            }
            if (symbol == null) return double.NaN;

            double result = amount;

            double conversion = !tradeType.HasValue ? ((symbol.Ask + symbol.Bid) / 2.0) : 
                (!inverse ? tradeType == TradeType.Buy ? symbol.Ask : symbol.Bid 
                : 1.0 / (tradeType == TradeType.Buy ? symbol.Bid: symbol.Ask)); // REVIEW

            result *= conversion;

            return result;            
        }

        #region Account Current Positions

        public virtual double UnrealizedGrossProfit {
            get {
                throw new NotImplementedException();
            }
        }

        public virtual double UnrealizedNetProfit {
            get {
                throw new NotImplementedException();
            }
        }

        #endregion

        public abstract IMarketSeries GetMarketSeries(TimeFrame timeFrame);
    }

}
