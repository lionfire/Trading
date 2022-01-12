using LionFire.Trading.Backtesting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Structures;
using LionFire.Validation;
using LionFire.Instantiating;
using System.Collections;
using LionFire.Trading.Bots;

namespace LionFire.Trading.Bots
{
    public partial class SingleSeriesSignalBotBase<IndicatorType, TSingleSeriesSignalBotBase, TIndicator>
    {
        public MarketSeries MarketSeries { get; set; }

        protected override async Task OnStarting()
        {
            this.Validate()
            .PropertyNonDefault(nameof(Template.Symbol), Template.Symbol)
            .PropertyNonDefault(nameof(Template.TimeFrame), Template.TimeFrame)
            .EnsureValid();

            if (!Account.Started.Value)
            {
                throw new InvalidOperationException("Can't start until Account is started");
            }
            await base.OnStarting().ConfigureAwait(false);

            if (Template.Indicator.Symbol == null) Template.Indicator.Symbol = this.Template.Symbol;
            if (Template.Indicator.TimeFrame == null) Template.Indicator.TimeFrame = this.Template.TimeFrame;

            this.Symbol = Account.GetSymbol(Template.Symbol);
            this.MarketSeries = Account.GetMarketSeries(this.Template.Symbol, this.Template.TimeFrame /*, Market.IsBacktesting*/);

            CreateIndicator();

            //this.Indicator.Template = this.Template.Indicator;

            this.Indicator.Account = Account; // 

            await Indicator.StartAsync().ConfigureAwait(false);

            //UpdateDesiredSubscriptions();

            var sim = Account as ISimulatedAccount;
            if (Account as ISimulatedAccount != null)
            {
                Account.Ticked += Market_Ticked;
            }
            else
            {
                this.MarketSeries.Bar += MarketSeries_Bar;
            }
            //this.Symbol.GetLastBar(TimeFrame.m1).Wait(); // Gets server time
        }

        protected override async Task OnStarted()
        {
            await base.OnStarted().ConfigureAwait(false);

            this.Symbol.Ticked += Symbol_Tick;
            Evaluate();
        }

        private void Market_Ticked()
        {
            // Unlikely OPTIMIZE: convey which symbols/tf's ticked and only evaluate here if relevant?  Backtests are usually focused on one bot though.
            Evaluate();
        }

        public DateTime End = default(DateTime);

        private void Symbol_Tick(SymbolTick obj) // MOVE unsubscribe logic  to Base
        {
            if (End == default(DateTime))  // REFACTOR to base
            {
                End = DateTime.Now + TimeSpan.FromSeconds(150);
            }
            //Console.WriteLine($"[liontrender] [tick] {z} {obj}    (Test time remaining: {(End-DateTime.Now).TotalSeconds.ToString("N")} seconds)");

#if !cAlgo
            if (End <= DateTime.Now)
            {
                Console.WriteLine("[test] Test complete.  Unsubscribing from tick events.");
                this.Symbol.Ticked -= Symbol_Tick;
            }
#endif
        }

        private void MarketSeries_Bar(TimedBar obj)
        {
            //Console.WriteLine("[bar] " + obj);
            Evaluate();
        }

        //protected virtual void UpdateDesiredSubscriptions()
        //{
        //    // Disabled for now
        //    //this.DesiredSubscriptions = new List<MarketDataSubscription>()
        //    //{
        //    //    new MarketDataSubscription(this.Template.Symbol, Template.TimeFrame)
        //    //    //new MarketDataSubscription(this.Config.Symbol, "t1")
        //    //};
        //}

        //protected void OnSimulationTickFinished()
        //{
        //    //logger.LogInformation("bot OnSimulationTickFinished " + (Market as BacktestMarket).SimulationTime);
        //    Evaluate();
        //}


        //long i = 0;
        public override void OnBar(string symbolCode, TimeFrame timeFrame, TimedBar bar)
        {
            if (bar.IsValid)
            {
                Console.WriteLine("OnBar: " + bar);
                Evaluate();
            }
            //if (i++ % 48 == 0)
            //{
            //    Console.WriteLine($"{this.GetType().Name} [{timeFrame.Name}] {symbolCode} {bar}");
            //}
        }


#if false // Just do a backtest instead
        #region ReversePositions

        public IEnumerable<Position> ReversePositions
        {
            get
            {
                return new ReversePositionEnumerator(this);
            }
        }

        public class ReversePositionEnumerable : IEnumerable<Position>
        {
            ISingleSeriesSignalBot bot;
            public ReversePositionEnumerable(ISignalBot bot)
            {
                this.bot = bot;
            }
            public IEnumerator<Position> GetEnumerator()
            {
                return new ReversePositionEnumerator(bot);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }


        public class ReversePositionEnumerator : IEnumerator<Position>
        {
            ISingleSeriesSignalBot bot;
            //public DateTime OpenTimeCursor;
            public int index = int.MaxValue;

            public ReversePositionEnumerator(ISingleSeriesSignalBot bot)
            {
                this.bot = bot;
            }

            public Position Current
            {
                get; private set;
            }

            object IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public void Dispose()
            {
                bot = null;
            }

            public bool MoveNext()
            {

                if (index == int.MaxValue && bot.Indicator != null)
                {
                    index = bot.Indicator.OpenLongPoints.LastIndex;
                }
                while (true)
                {
                    var openLong = bot.Indicator.OpenLongPoints[index];
                    var openShort = bot.Indicator.OpenShortPoints[index];

                    Position pos = null;

                    if (openLong >= 1.0)
                    {
                        pos = new Position
                        {
                            TradeType = TradeType.Buy
                        };
                    }
                    else if (-openShort >= 1.0)
                    {
                        pos = new Position
                        {
                            TradeType = TradeType.Sell
                        };
                    }
                    if (pos != null)
                    {
                        pos.SymbolCode = bot.Template.Symbol,
                        pos.EntryPrice = bot.MarketSeries.Close[index-1]: // TOTEST Does this sync up?
                    }

                    if (pos.TradeType == TradeType.Buy) {
                        for (int i = index; true; i++)
                        {
                            if (bot.Indicator.CloseLongPoints[i] >= 1.0)
                            {
                                pos.ExitP
                                pos.CloseDetails = new PositionCloseDetails()
                                {
                                    
                                };

                                    //bot.MarketSeries.Close[index - 1]: // TOTEST Does this sync up?

                            }
                        }
                    }
                    if (pos.TradeType == TradeType.Sell)
                    {
                        for (int i = index; true; i++)
                        {
                        }
                    }

                    if (double.IsNaN(openLong) && double.IsNaN(openShort)
                        && double.IsNaN(bot.Indicator.CloseLongPoints[index]) && double.IsNaN(bot.Indicator.CloseShortPoints[index])
                        )
                    {
                        Current = null;
                        return false;
                    }
                }
            }

            public void Reset()
            {
                index = int.MaxValue;
            }
        }

        #endregion

#endif

        public int LastLongOpenIndex
        {
            get
            {
                for (int i = Indicator.OpenLongPoints.LastIndex; i >= Indicator.OpenLongPoints.FirstIndex; i--)
                {
                    if (Indicator.OpenLongPoints[i] >= 1.0)
                    {
                        return i;
                    }
                }
                return int.MinValue;
            }
        }
        public int LastLongCloseIndex
        {
            get
            {
                for (int i = LastLongOpenIndex; i <= Indicator.OpenLongPoints.LastIndex; i++)
                {
                    if (Indicator.CloseLongPoints[i] >= 1.0 || MarketSeries.Low[i] <= Indicator.LongStopLoss[i])
                    {
                        return i;
                    }
                }
                return int.MaxValue;
            }
        }

        public DateTime LastLongOpenTime
        {
            get
            {
                var index = LastLongOpenIndex;
                if (index == int.MinValue) return default(DateTime);

                return MarketSeries.OpenTime[index];
            }
        }

        // Does not take SL into effect yet
        public DateTime LastLongCloseTime
        {
            get
            {
                var index = LastLongCloseIndex;
                if (index == int.MaxValue) return default(DateTime);

                return MarketSeries.OpenTime[index];
            }
        }

        public int LastShortOpenIndex
        {
            get
            {
                for (int i = Indicator.OpenShortPoints.LastIndex; i >= Indicator.OpenShortPoints.FirstIndex; i--)
                {
                    if (Indicator.OpenShortPoints[i] >= 1.0)
                    {
                        return i;
                    }
                }
                return int.MinValue;
            }
        }
        public int LastShortCloseIndex
        {
            get
            {
                for (int i = LastShortOpenIndex; i <= Indicator.OpenShortPoints.LastIndex; i++)
                {
                    if (Indicator.CloseShortPoints[i] >= 1.0 || MarketSeries.Low[i] >= Indicator.ShortStopLoss[i])
                    {
                        return i;
                    }
                }
                return int.MaxValue;
            }
        }

        public DateTime LastShortOpenTime
        {
            get
            {
                var index = LastShortOpenIndex;
                if (index == int.MinValue) return default(DateTime);

                return MarketSeries.OpenTime[index];
            }
        }

        // Does not take SL into effect yet
        public DateTime LastShortCloseTime
        {
            get
            {
                var index = LastShortCloseIndex;
                if (index == int.MaxValue) return default(DateTime);

                return MarketSeries.OpenTime[index];
            }
        }

        public int LastOpenIndex
        {
            get
            {
                return Math.Max(LastLongOpenIndex, LastShortOpenIndex);
            }
        }
        public int LastCloseIndex
        {
            get
            {
                return Math.Max(LastLongCloseIndex, LastShortCloseIndex);

            }
        }

        public DateTime LastOpenTime
        {
            get
            {
                var index = LastOpenIndex;
                if (index == int.MinValue) return default(DateTime);
                return MarketSeries.OpenTime[index];
            }
        }

        // Does not take SL into effect yet
        public DateTime LastCloseTime
        {
            get
            {
                var index = LastCloseIndex;
                if (index == int.MaxValue) return default(DateTime);

                return MarketSeries.OpenTime[index];
            }
        }

        public TradeKind? LastTradeType
        {
            get
            {
                var longIndex = LastLongOpenIndex;
                var shortIndex = LastShortOpenIndex;
                if (longIndex == int.MinValue && shortIndex == int.MinValue) return null;

                return longIndex > shortIndex ? TradeKind.Buy : TradeKind.Sell;

            }
        }
    }
}
