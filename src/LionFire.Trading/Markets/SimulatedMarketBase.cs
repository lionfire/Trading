
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Subjects;
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

        #region Attached MarketParticipants

        public void Add(MarketParticipant actor)
        {
            participants.Add(actor);
            actor.Market = this;
        }
        List<MarketParticipant> participants = new List<MarketParticipant>();

        #endregion

        #region State

        #region SimulationTime

        public bool IsStarted { get { return simulationTime != default(DateTime); } }

        public DateTime SimulationTime {
            get { return simulationTime; }
            set {
                if (!IsStarted && value != default(DateTime))
                {
                    foreach (var actor in participants)
                    {
                        //actor.OnStarting();
                    }
                }
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
        
        public IObservable<bool> Started { get { return started; } }
        BehaviorSubject<bool> started = new BehaviorSubject<bool>(false);

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
            if (lastExecutedTime == default(DateTime) && time != default(DateTime))
            {
                started.OnNext(true);
            }
            // OPTIMIZE: Sync up index between live and historical series to make it faster
            foreach (var series in Data.ActiveLiveSeries)
            {
                if ((series.OpenTime.LastValue + series.TimeFrame.TimeSpan) <= time)
                {
                    HistoricalPlaybackState state = series.HistoricalPlaybackState;
                    if (state == null)
                    {
                        state = series.HistoricalPlaybackState = new Trading.HistoricalPlaybackState();

                        TimeFrame sourceTimeFrame = series.TimeFrame;
                        // Find a matching historical source, or find a more granular one that can be used.
                        // FUTURE: Get factors of a timeframe instead of the simple MoreGranular iteration logic.  e.g. from h12 search for h6 h4 h3 h2 h1 m30 m15 m12 m10 m6 m5 m4 m2 m1 t*
                        // FUTURE: After simulation complete, save downsampled backtest data
                        for (; state.HistoricalSource == null && sourceTimeFrame != null; sourceTimeFrame = sourceTimeFrame.MoreGranular())
                        {
                            state.HistoricalSource = Data.HistoricalDataSources.GetMarketSeries(series.SymbolCode, sourceTimeFrame, this.StartDate, this.EndDate);
                        }
                        if (state.HistoricalSource == null)
                        {
                            Console.WriteLine($"Could not find historical data source for {series}");
                            continue;
                        }
                    }
                    if (state.HistoricalSource == null)
                    {
                        continue;
                    }

                    // TODO: Run coarser timeframes from finer historical data
                    // MAJOR OPTIMIZE - Don't use FindIndex -- it is incredibly slow.

                    
                    if (state.NextHistoricalIndex < 0)
                    {
                        state.NextHistoricalIndex = state.HistoricalSource.FindIndex(time);
                        if (state.NextHistoricalIndex < 0)
                        {
                            continue;
                        }
                    }

                    if (state.HistoricalSource.TimeFrame.TimeSpan > series.TimeFrame.TimeSpan)
                    {
                        // FUTURE: Somehow support this?
                        Console.WriteLine("Not supported: state.HistoricalSource.TimeFrame.TimeSpan > series.TimeFrame.TimeSpan");
                        continue;
                    }

                    int mergeCount = 0;
                    for (; ; state.NextHistoricalIndex++)
                    {
                        if (state.NextHistoricalIndex >= state.HistoricalSource.Count)
                        {
                            //state.NextHistoricalIndex--;
                            break;
                        }

                        var bar = state.HistoricalSource[state.NextHistoricalIndex];

                        if (bar.OpenTime == time)
                        {
                            if (state.HistoricalSource.TimeFrame == series.TimeFrame)
                            {
                                ((IMarketSeriesInternal)series).OnBar(bar, true);
                                state.NextHistoricalIndex++;
                                break;
                            }
                            else
                            {
                                if (state.NextBarInProgress != null && !double.IsNaN(state.NextBarInProgress.High))
                                {
                                    ((IMarketSeriesInternal)series).OnBar(state.NextBarInProgress, true);
                                }
                                state.NextBarInProgress = bar.Clone();
                                mergeCount = 0;
                            }
                            #region Optimization
                            state.NextHistoricalIndex++;
                            break;
                            #endregion
                        }
                        else if (bar.OpenTime < time)
                        {
                            if (state.NextBarInProgress == null)
                            {
                                state.NextBarInProgress = bar.Clone();
                                mergeCount = 0;
                            }
                            else
                            {
                                state.NextBarInProgress.Merge(bar);
                                mergeCount++;
                            }
                        }
                        else // bar.OpenTime > time
                        {
                            if (state.NextBarInProgress == null)
                            {
                                state.NextBarInProgress = new TimedBar()
                                {
                                    OpenTime = time,
                                    Open = double.NaN,
                                    High = double.NaN,
                                    Low = double.NaN,
                                    Close = double.NaN,
                                    Volume = 0
                                };
                            }
                            break;
                        }
                    }
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
            Console.WriteLine();
            Console.WriteLine($"Simulation finished in {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)}");
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
                var progress = (100.0 * (simulationTime - StartDate).TotalMilliseconds / simulationMilliseconds).ToString("N1") ;
                var date = simulationTime.ToString("yyyy-MM-dd");
                Console.Write($"\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b{progress}% {date}");
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
