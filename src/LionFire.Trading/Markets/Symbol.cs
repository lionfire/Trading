//#define DEBUG_TICK
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Trading.Backtesting;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.ComponentModel;

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

        MarketSeries GetMarketSeries(TimeFrame timeFrame);
        MarketSeriesBase GetMarketSeriesBase(TimeFrame timeFrame);

        event Action<SymbolTick> Ticked;

        Task<TimedBar> GetLastBar(TimeFrame timeFrame);
        Task<Tick> GetLastTick();

        MarketTickSeries MarketTickSeries { get; }

        IAccount Account { get; }
        Task EnsureHasLatestTick();
        double GetStopLossFromPips(TradeType tradeType, double stopLossInPips);
        double GetTakeProfitFromPips(TradeType tradeType, double takeProfitInPips);

        string PriceStringFormat { get; }
    }

    public interface ISymbolInternal : Symbol
    {
        event Action<Symbol, bool> TickHasObserversChanged;
        bool TickHasObservers
        {
            get;
        }
        void OnTick(SymbolTick tick);

    }

    public class SymbolImpl : SymbolImplBase, IBacktestSymbol
    {
        public SymbolImpl(string symbolCode, IFeed market) : base(symbolCode, market) { }

        public override Task<TimedBar> GetLastBar(TimeFrame timeFrame)
        {
            throw new NotImplementedException();
        }
        public override Task<Tick> GetLastTick()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class SymbolImplBase : Symbol, ISymbolInternal
    {
        public string PriceStringFormat
        {
            get
            {
                if (priceStringFormat == null)
                {
                    priceStringFormat = "0.";
                    for (int i = 0; i < PipSize.ToString().Length - 1; i++)
                    {
                        priceStringFormat += "0";
                    }
                }
                return priceStringFormat;
            }
        }
        private string priceStringFormat;

        #region Config

        // MOVE to BacktestSymbol

        public BacktestSymbolSettings BacktestSymbolSettings { get; set; }

        #endregion

        #region Identity

        public string Code
        {
            get; private set;
        }

        #endregion

        #region Relationships

        public IFeed Feed { get; protected set; }

        public IAccount Account { get; set; }

        #endregion

        #region Construction

        public SymbolImplBase(string symbolCode, IFeed feed)
        {
            this.Code = symbolCode;
            this.Feed = feed;
            this.Account = feed as IAccount;
        }

        #endregion

        #region IsSubscribed

        public bool IsSubscribed
        {
            get { return isSubscribed; }
            set
            {
                if (isSubscribed == value) return;
                isSubscribed = value;
                // TODO: Actually subscribe
                OnPropertyChanged(nameof(IsSubscribed));
            }
        }
        protected bool isSubscribed;

        #endregion

        #region Current Market State



        public double Ask
        {
            get
            {
                if (double.IsNaN(ask))
                {
                    if (isSubscribed)
                    {
                        LoadLatestTicks();
                    }
                }
                return ask;
            }
            set
            {
                ask = value;
            }
        }
        double ask = double.NaN;

        public double Bid
        {
            get
            {
                if (double.IsNaN(bid) && isSubscribed)
                {
                    LoadLatestTicks();
                }
                return bid;
            }
            set
            {
                bid = value;
            }
        }
        double bid = double.NaN;

        public void LoadLatestTicks()
        {
            MarketTickSeries.EnsureDataAvailable(null, DateTime.UtcNow, DefaultTicks).Wait();
            UpdateBidAsk();
        }
        public static readonly int DefaultTicks = 300; // Try a large enough number to get both bid/ask

        private void UpdateBidAsk()
        {
            var index = MarketTickSeries.LastIndex;

            bool gotBid = false;
            bool gotAsk = false;


            for (int i = 500; i > 0 && (!gotBid || !gotAsk) && index > MarketTickSeries.FirstIndex; i--, index--)
            {
                if (!gotBid)
                {
                    if (MarketTickSeries[index].HasBid)
                    {
                        this.Bid = MarketTickSeries[index].Bid;
                        gotBid = true;
                    }
                }
                if (!gotAsk)
                {
                    if (MarketTickSeries[index].HasAsk)
                    {
                        this.Ask = MarketTickSeries[index].Ask;
                        gotAsk = true;
                    }
                }
            }
            Debug.WriteLine($"[{Code} last aggregated tick] b:{Bid} a:{Ask} s:{Spread}");
        }

        public double Spread
        {
            get
            {
                return Ask - Bid;
            }
        }

        #endregion

        #region Tick Events

        public event Action<SymbolTick> Ticked
        {
            add
            {
                lock (eventLock)
                {
                    if (tickedEvent == null)
                    {
                        TickHasObserversChanged?.Invoke(this, true);
                    }
                    tickedEvent += value;
                }
            }
            remove
            {
                lock (eventLock)
                {
                    tickedEvent -= value;
                    if (tickedEvent == null)
                    {
                        TickHasObserversChanged?.Invoke(this, false);
                    }
                }
            }
        }
        private event Action<SymbolTick> tickedEvent;
        private object eventLock = new object();

        public event Action<Symbol, bool> TickHasObserversChanged;
        public bool TickHasObservers { get { return tickedEvent != null; } }

        void ISymbolInternal.OnTick(SymbolTick tick)
        {
            if (tick.HasBid) this.Bid = tick.Bid;
            if (tick.HasAsk) this.Ask = tick.Ask;
#if DEBUG_TICK
            Debug.WriteLine("[tick] " + tick.ToString());
#endif
            tickedEvent?.Invoke(tick);
        }

        #endregion

        #region 

        #region Series

        #region Series

        ConcurrentDictionary<string, MarketSeries> seriesByTimeFrame = new ConcurrentDictionary<string, MarketSeries>();
        public MarketSeries GetMarketSeries(TimeFrame timeFrame)
        {
            if (timeFrame == null) return null;
            if (timeFrame.Name == "t1") { throw new ArgumentException("Use MarketTickSeries instead for t1"); }
            return seriesByTimeFrame.GetOrAdd(timeFrame.Name, timeFrameName => this.Account.CreateMarketSeries(Code, timeFrame));
        }

        public MarketSeriesBase GetMarketSeriesBase(TimeFrame timeFrame)
        {
            if (timeFrame.Name == "t1") { return MarketTickSeries; }
            return GetMarketSeries(timeFrame);
        }

        public MarketTickSeries MarketTickSeries
        {
            get
            {
                if (marketTickSeries == null)
                {
                    lock (_lock)
                    {
                        if (marketTickSeries == null)
                        {
                            marketTickSeries = new MarketTickSeries(this.Account, this.Code);
                        }
                    }
                }
                return marketTickSeries;
            }
        }
        MarketTickSeries marketTickSeries;
        private object _lock = new object();

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

        public int Digits
        {
            get; private set;
        }

        public int Leverage
        {
            get; private set;
        }

        public long LotSize
        {
            get; set;
        }

        public double PipSize
        {
            get; private set;
        }

        public double PointSize
        {
            get; private set;
        }

        public double PreciseLeverage
        {
            get; private set;
        }

        public long VolumeMax
        {
            get; private set;
        }

        public long VolumeMin
        {
            get; private set;
        }

        public long VolumeStep
        {
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

        public double TickSize
        {
            get; private set;
        }

        public double TickValue
        {
            get
            {
                if (Account == null) { throw new ArgumentException("Requires Account to be set"); }
                if (Account.Currency == this.Currency)
                {
                    return TickSize;
                }

                return Convert(TickSize, Account.Currency, this.Currency, null);
            }
        }

        public double PipValue
        {
            get
            {
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
            var symbol = Feed.GetSymbol(to + from);
            bool inverse = false;
            if (symbol == null)
            {
                inverse = true;
                symbol = Feed.GetSymbol(from + to);
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
                : 1.0 / (tradeType == TradeType.Buy ? bid : ask)); // REVIEW

            result *= conversion;

            return result;
        }

        #region Account Current Positions

        public virtual double UnrealizedGrossProfit
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual double UnrealizedNetProfit
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        public double GetStopLossFromPips(TradeType tradeType, double stopLossInPips)
        {
            if (double.IsNaN(stopLossInPips)) return double.NaN;

            if (tradeType == TradeType.Buy)
            {
                // TOVERIFY
                return Bid - PipValue * stopLossInPips;
            }
            else
            {
                // TOVERIFY
                return Ask + PipValue * stopLossInPips;
            }
        }
        public double GetTakeProfitFromPips(TradeType tradeType, double takeProfitInPips)
        {
            if (double.IsNaN(takeProfitInPips)) return double.NaN;
            if (tradeType == TradeType.Buy)
            {
                // TOVERIFY
                return Ask  + PipValue * takeProfitInPips;
            }
            else
            {
                // TOVERIFY
                return Bid - PipValue * takeProfitInPips;
            }
        }

        public abstract Task<TimedBar> GetLastBar(TimeFrame timeFrame);
        public abstract Task<Tick> GetLastTick();


        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
        public async Task EnsureHasLatestTick()
        {
            await MarketTickSeries.EnsureDataAvailable(null, DateTime.UtcNow, 1).ConfigureAwait(false);
            UpdateBidAsk();
        }
    }

}
