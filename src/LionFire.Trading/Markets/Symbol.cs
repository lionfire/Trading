using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Trading.Backtesting;
using System.Collections.Concurrent;

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

        event Action<SymbolTick> Tick;
    }

    public interface ISymbolInternal : Symbol
    {
        event Action<Symbol, bool> TickHasObserversChanged;
        void OnTick(SymbolTick tick);
    }

    public class SymbolImpl : SymbolImplBase, IBacktestSymbol
    {
        public SymbolImpl(string symbolCode, IAccount market) : base(symbolCode, market) { }
        
    }

    public abstract class SymbolImplBase : Symbol, ISymbolInternal
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

        public IAccount Market { get; protected set; }

        public IAccount Account { get; set; }

        #endregion

        #region Construction

        public SymbolImplBase(string symbolCode, IAccount market)
        {
            this.Code = symbolCode;
            this.Market = market;
            this.Account = market as IAccount;
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

        #region Tick Events

        public event Action<SymbolTick> Tick
        {
            add
            {
                lock (eventLock)
                {
                    if (tickEvent == null)
                    {
                        TickHasObserversChanged?.Invoke(this, true);
                    }
                    tickEvent += value;
                }
            }
            remove
            {
                lock (eventLock)
                {
                    tickEvent -= value;
                    if (tickEvent == null)
                    {
                        TickHasObserversChanged?.Invoke(this, false);
                    }
                }
            }
        }
        private event Action<SymbolTick> tickEvent;
        private object eventLock = new object();

        public event Action<Symbol, bool> TickHasObserversChanged;

        void ISymbolInternal.OnTick(SymbolTick tick)
        {
            if(tick.HasBid) this.Bid = tick.Bid;
            if(tick.HasAsk) this.Ask = tick.Ask;
            tickEvent?.Invoke(tick);
        }

        #endregion

        #region 

        #region Series

        #region Series

        ConcurrentDictionary<string, IMarketSeries> seriesByTimeFrame = new ConcurrentDictionary<string, IMarketSeries>();

        public IMarketSeries GetMarketSeries(TimeFrame timeFrame)
        {
            return seriesByTimeFrame.GetOrAdd(timeFrame.Name, timeFrameName =>
            {
                var task = this.Account.CreateMarketSeries(Code, timeFrame);
                task.Wait();
                return task.Result;
            });
        }


        #endregion


        #endregion

        public void LoadSymbolInfo(SymbolInfo info)
        {
            this.Digits = info.Digits;
            this.Leverage = (int)info.Leverage;
            this.LotSize = info.LotSize;
            this.PreciseLeverage = info.Leverage;
            this.PipSize = info.PipSize;
            this.PointSize = info.PointSize;
            this.TickSize = info.TickSize;
            //this.TickValue = info.TickValue;
            this.VolumeMin = info.VolumeMin;
            this.VolumeMax = info.VolumeMax;
            this.VolumeStep = info.VolumeStep;
            this.QuantityPerHundredThousandVolume = info.QuantityPerHundredThousandVolume;
            this.VolumePerHundredThousandQuantity = info.VolumePerHundredThousandQuantity;
            this.Currency = info.Currency;

            if (double.IsNaN(TickSize)) { throw new ArgumentException("Failed to load TickSize for symbol: " + info.Code); }
            //if (double.IsNaN(TickValue)) { throw new ArgumentException("Failed to load TickValue for symbol: " + info.Code); }
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

        #region Properties: Info

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
            var symbol = Market.GetSymbol(to + from);
            bool inverse = false;
            if (symbol == null)
            {
                inverse = true;
                symbol = Market.GetSymbol(from + to);
            }
            if (symbol == null) return double.NaN;

            double result = amount;

            var bid = symbol.Bid;
            var ask = symbol.Ask;

            if (double.IsNaN(bid) || double.IsNaN(ask))
            {
                throw new Exception("Currency conversion symbol pricing is not available for {from} to {to}");
            }

            double conversion = !tradeType.HasValue ? ((ask + bid) / 2.0) : 
                (!inverse ? tradeType == TradeType.Buy ? ask : bid 
                : 1.0 / (tradeType == TradeType.Buy ? bid: ask)); // REVIEW

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

        
    }

}
