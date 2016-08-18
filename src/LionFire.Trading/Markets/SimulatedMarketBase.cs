
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading
{


    public abstract class SimulatedMarketBase : MarketBase, ISimulatedMarket, IMarket
    {
        #region Parameters

        #region Simulation Parameters

        public int StepDelayMilliseconds { get; set; }

        public TimeZoneInfo TimeZone {
            get {
                return timeZone;
            }
            set {
                if (value != TimeZoneInfo.Utc) throw new NotSupportedException("Only UTC currently supported.)");
                timeZone = value;
            }
        }
        private TimeZoneInfo timeZone = TimeZoneInfo.Utc;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public TimeFrame TimeFrame { get; set; }

        #endregion

        public TimeFrame SimulationTimeStep { get; set; }


        #region Derived

        #endregion

        #endregion

        #region Construction

        public SimulatedMarketBase()
        {
            SimulationTimeStep = TimeFrame.m1;

        }

        #endregion

        #region Attached Actors

        public void AddActor(Actor actor)
        {
            actor.Market = this;
        }

        #endregion

        #region State

        #region SimulationTime

        public DateTime SimulationTime {
            get { return simulationTime; }
            set {
                simulationTime = value;
                SimulationTimeChanged?.Invoke();
            }
        }
        private DateTime simulationTime;

        public event Action SimulationTimeChanged;

        #endregion

        DateTime lastExecutedTime;

        #region Derived

        public bool IsFinished {
            get {
                return SimulationTime >= EndDate;
            }
        }

        #endregion

        #endregion

        #region Events

        public event Action ExecutedTime;

        #endregion

        #region Execution Methods

        public int DefaultBackFillMinutes = 60 * 24 * 7;

        public void Reset()
        {
            lastExecutedTime = DateTime.MinValue;
            SimulationTime = StartDate;
        }
        public void Initialize(int? backFillMinutes = null)
        {
            Reset();
            ExecuteBackfillUpToDate(StartDate, backFillMinutes);
        }

        protected void ExecuteBackfillUpToDate(DateTime time, int? backFillMinutes = null)
        {
            if (!backFillMinutes.HasValue) backFillMinutes = DefaultBackFillMinutes;

            SimulationTime = time;
            Execute(time);

            // TODO: Backfill the symbols that were missed
            //var done = HashSet<string>();

            //foreach (var kvp in subscriptions)
            //{
            //    var subscription = kvp.Value;
            //    foreach (var subscriber in subscription.Subscribers)
            //    {
            //        subscription.GetBar(
            //        subscriber.OnBar(new SymbolBar(subscription.Symbol, new Bar())),
            //    }
            //    kvp.Value.Symbol
            //}

            foreach (var series in Data.ActiveLiveSeries)
            {
                if (series.OpenTime.Count == 0)
                {
                    ((MarketSeries)series).AddDataPointAtTime(time);
                }
            }

            ExecutedTime?.Invoke();
        }

        Dictionary<TimeFrame, MarketSeries> timeFrameMarketSeries = new Dictionary<TimeFrame, MarketSeries>();
        private void InjectHistoricalData(DateTime time)
        {
            foreach (var kvp in timeFrameMarketSeries)
            {
                // If time advances to next step, send the bar for the previous step
            }
        }

        protected void Execute(DateTime time)
        {

            // OPTIMIZE: Sync up index between live and historical series to make it faster
            foreach (var series in Data.ActiveLiveSeries)
            {
                if ((series.OpenTime.LastValue + series.TimeFrame.TimeSpan) < time)
                {
                    HistoricalPlaybackState state = series.HistoricalPlaybackState;
                    if (state == null)
                    {
                        state = series.HistoricalPlaybackState = new Trading.HistoricalPlaybackState();

                        if (state.HistoricalSource == null)
                        {
                            state.HistoricalSource = Data.HistoricalDataSources.GetMarketSeries(series.Key, this.StartDate, this.EndDate);
                        }
                    }
                    if (state.HistoricalSource == null) continue;


                    // TODO: Run coarser timeframes from finer historical data
                    // MAJOR OPTIMIZE - Don't use FindIndex -- it is incredibly slow.

                    TimedBar nextLiveBar = null;
                    TimedBar peekBar = null;

                    if (state.LastHistoricalIndexExecuted < 0)
                    {
                        state.LastHistoricalIndexExecuted = state.HistoricalSource.FindIndex(time);
                        if (state.LastHistoricalIndexExecuted < 0)
                        {
                            continue;
                        }
                    }

                    int mergeCount = 0;
                    for (state.LastHistoricalIndexExecuted++; ;)
                    {
                        if (state.LastHistoricalIndexExecuted >= state.HistoricalSource.Count)
                        {
                            state.LastHistoricalIndexExecuted--;
                            break;
                        }

                        peekBar = state.HistoricalSource[state.LastHistoricalIndexExecuted];

                        if (peekBar.OpenTime > time)
                        {
                            state.LastHistoricalIndexExecuted--;
                            break;
                        }

                        if (nextLiveBar == null)
                        {
                            nextLiveBar = peekBar.Clone();
                        }
                        else
                        {
                            nextLiveBar.Merge(peekBar);
                            mergeCount++;
                        }

                        if (peekBar.OpenTime == time)
                        {
                            break;
                        }
                    }

                    if (nextLiveBar != null)
                    {
                        Console.WriteLine($"mergeCount {mergeCount}");
                        ((IMarketSeriesInternal)series).OnBar(nextLiveBar, true);
                    }

                    //TimedBar bar = state.HistoricalSource[time];
                    //if (bar == null) continue;

                    //((IMarketSeriesInternal)series).OnBar(bar, true);
                }
            }

            //foreach (var kvp in subscriptions)
            //{
            //    var subscription = kvp.Value;
            //    foreach (var subscriber in subscription.Subscribers)
            //    {
            //        var marketData = GetMarketSeries(subscription.Symbol, subscription.TimeFrame);
            //        var bars = marketData.GetBars(lastExecutedTime, time);
            //        //var bar = marketData[time];
            //        subscriber.OnBars(bars);
            //        //subscription.Symbol
            //        //subscriber.OnBar(new SymbolBar(subscription.Symbol, new Bar())),
            //    }

            //}
            lastExecutedTime = time;
        }

        int delayBetweenSteps = 0;

        public void Run(int delayBetweenSteps = 0)
        {
            simulationMilliseconds = (EndDate - StartDate).TotalMilliseconds;
            Initialize();

            var sw = Stopwatch.StartNew();
            this.delayBetweenSteps = delayBetweenSteps;
            do
            {
                if (delayBetweenSteps > 0)
                {
                    Thread.Sleep(delayBetweenSteps);
                }
            } while (ExecuteNextStep());
            Console.WriteLine($"Simulation finished in {sw.ElapsedMilliseconds / 100}s");
        }

        public const int progressReportInterval = 100;
        int i = progressReportInterval;
        double simulationMilliseconds;


        /// <returns>True if there are still more steps</returns>
        public bool ExecuteNextStep()
        {
#if SanityChecks
            if (SimulationTime == default(DateTime))
            {
                throw new Exception("Cannot call ExecuteNextStep when SimulationTime is not initialized.");
            }
#endif

            var timeSpan = this.TimeFrame.TimeSpan;
            if (timeSpan == TimeSpan.Zero)
            {
                switch (TimeFrame.TimeFrameUnit)
                {
                    //case TimeFrameUnit.Tick: TODO
                    //    break;
                    //case TimeFrameUnit.Month: TODO?
                    //    break;
                    default:
                        throw new NotImplementedException();
                }
            }
            SimulationTime += timeSpan;

            if (i++ > progressReportInterval)
            {
                i = 0;
                var progress = (100.0 * (simulationTime - StartDate).TotalMilliseconds / simulationMilliseconds).ToString("N1");
                Console.Write($"\b\b\b\b\b{progress}%");
            }

            Execute(simulationTime);

            return !IsFinished;
        }

        #region Advance

        public bool AdvanceTo(DateTime time)
        {
            return !IsFinished;
        }

        #endregion

        public bool AdvanceOneTick()
        {
            // Out of all data feeds currently active, find the next tick and simulate it.  If multiple ticks happen to occur at the exact same time, simulate them all.
            throw new NotImplementedException();
            //return !IsFinished;
        }

        #endregion

        #region IMarket Implementation

        public bool IsSimulation {
            get {
                return true;
            }
        }

        public bool IsRealMoney {
            get {
                return false;
            }
        }

        public abstract bool IsBacktesting { get; }

        #endregion
    }
}
