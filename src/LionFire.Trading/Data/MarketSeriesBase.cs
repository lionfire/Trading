//#define DEBUG_BARSCOPIED
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Reactive.Subjects;
using LionFire.Validation;
using LionFire.Instantiating;
using LionFire;
using System.Collections.Concurrent;
using LionFire.Execution;
using LionFire.Execution.Jobs;
#if BarStruct
using BarType = LionFire.Trading.TimedBarStruct;
#else
using BarType = LionFire.Trading.TimedBar;
#endif
using System.Threading.Tasks;
using TickType = LionFire.Trading.Tick;
using LionFire.Trading.Data;
using System.Threading;
using System.ComponentModel;

namespace LionFire.Trading
{
    [Flags]
    public enum BackwardForward
    {
        None = 0,
        Backward = 1 << 0,
        Forward = 1 << 1,
    }

    public class TMarketSeriesBase : ITemplate
    {
        //[Ignore]
        //[Required]
        public IFeed Feed { get; set; }

        public IAccount Account => Feed as IAccount;

        //[Required]
        public string Symbol { get; set; }

        //[Required]
        public string TimeFrame { get; set; }

        public string Key { get { return MarketSeriesUtilities.GetSeriesKey(Symbol, TimeFrame); } }
    }



    public abstract class MarketSeriesBase<TMarketSeriesType, DataType> : MarketSeriesBase, ITemplateInstance
    where TMarketSeriesType : TMarketSeriesBase, new()
        where DataType : IMarketDataPoint
    {

        #region Template

        public TMarketSeriesType Template
        {
            get
            {
                if (template == null)
                {
                    template = new TMarketSeriesType
                    {
                        Feed = this.Feed,
                        Symbol = this.SymbolCode,
                        TimeFrame = this.TimeFrame.Name,
                    };
                }
                return template;
            }
            set { template = value; }
        }
        public TMarketSeriesType template;

        #endregion

        //protected abstract void Add(IMarketDataPoint dataPoint);

        #region Construction

        public MarketSeriesBase() : base() { }
        public MarketSeriesBase(IFeed feed, string key) : base(feed, key)
        {
        }
        public MarketSeriesBase(IFeed feed, string symbol, TimeFrame timeFrame) : base(feed, symbol, timeFrame)
        {
        }

        #endregion

        #region Data

        public DataType First
        {
            get
            {
                return this[FirstIndex];
            }
        }

        public DataType Last
        {
            get
            {
                return this[LastIndex];
                //#if BarStruct
                //                return bars.LastValue;
                //#else
                //                return new TimedBar
                //                {
                //                    OpenTime = openTime.LastValue,
                //                    Open = open.LastValue,
                //                    High = high.LastValue,
                //                    Low = low.LastValue,
                //                    Close = close.LastValue,
                //                    Volume = tickVolume.LastValue,
                //                };
                //#endif
            }
        }

        #endregion

        public abstract DataType this[int index] { get; set; }
        protected abstract void Add(DataType dataPoint);

        public void Add(List<DataType> dataPoints, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null)
        {
            if (dataPoints == null) { Debug.WriteLine("MarketSeriesBase Add - NULL dataPoints - FIXME !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!"); return; }
            var lastDataEndDate = this[LastIndex].Time;

            // TODO: Fill range from startDate to endDate with "NoData" and if a range is loaded that creates a gap with existing ranges, fill that with "MissingData"

            //if (bars.Count == 0) return;
            var resultsStartDate = dataPoints.Count == 0 ? default : dataPoints[0].Time;
            var resultsEndDate = dataPoints.Count == 0 ? default : dataPoints[dataPoints.Count - 1].Time;
            if (!startDate.HasValue) { startDate = resultsStartDate; Debug.WriteLine("WARN -!startDate.HasValue in MarketSeries.Add"); }
            if (!endDate.HasValue) { endDate = resultsEndDate; Debug.WriteLine("WARN -!endDate.HasValue in MarketSeries.Add"); }

#if DEBUG_BARSCOPIED
            int dataPointsCopied = 0;
#endif

            if (this.Count == 0 && DataEndDate == default(DateTimeOffset) && DataStartDate == default(DateTimeOffset))
            {
                foreach (var b in dataPoints)
                {
                    Add(b);
                    //Add(b.Time,b.Open,b.High,b.Low,b.Close,b.Volume);
                    //this.openTime.Add(b.Time);
                    //this.open.Add(b.Open);
                    //this.high.Add(b.High);
                    //this.low.Add(b.Low);
                    //this.close.Add(b.Close);
                    //this.tickVolume.Add(b.Volume);
                }
            }
            else
            {

                if (startDate <= DataStartDate) // prepending data
                {
                    int lastIndexToCopy;
                    for (lastIndexToCopy = dataPoints.Count - 1; lastIndexToCopy >= 0 && dataPoints[lastIndexToCopy].Time >= DataStartDate; lastIndexToCopy--) ; // OPTIMIZE?

                    for (int dataIndex = OpenTime.Count == 0 ? 0 : OpenTime.FirstIndex - 1; lastIndexToCopy >= 0; dataIndex--, lastIndexToCopy--)
                    {
                        //var bar = bars[lastIndexToCopy] as TimedBar;
                        //if (bar == null)
                        //{
                        //    bar = new TimedBar(bars[lastIndexToCopy]);
                        //}
                        this[dataIndex] = dataPoints[lastIndexToCopy];
#if DEBUG_BARSCOPIED
                        dataPointsCopied++;
#endif
                    }

                    if (DataStartDate != default(DateTimeOffset) && endDate.Value + TimeFrame.TimeSpan < DataStartDate)
                    {
                        Debug.WriteLine($"[DATA GAP] endDate: {endDate.Value}, DataStartDate: {DataStartDate}");
                        throw new NotImplementedException("TODO REVIEW this gap length");
                        //AddGap(endDate.Value + TimeFrame.TimeSpan, DataStartDate - TimeSpanApproximation.TimeSpan);
                    }

                }

                // Both above and below may get run

                if (endDate >= DataEndDate) // append data
                {
                    int lastIndexToCopy;
                    for (lastIndexToCopy = 0; lastIndexToCopy < dataPoints.Count && dataPoints[lastIndexToCopy].Time <= DataEndDate; lastIndexToCopy++) ; // OPTIMIZE?

                    for (int dataIndex = OpenTime.LastIndex + 1; lastIndexToCopy < dataPoints.Count; dataIndex++, lastIndexToCopy++)
                    {
                        //var bar = bars[lastIndexToCopy] as TimedBar;
                        //if (bar == null)
                        //{
                        //    bar = new TimedBar(bars[lastIndexToCopy]);
                        //}
                        this[dataIndex] = dataPoints[lastIndexToCopy];
#if DEBUG_BARSCOPIED
                        dataPointsCopied++;
#endif
                    }

                    if (DataEndDate != default(DateTimeOffset) && startDate.Value - TimeFrame.TimeSpan > DataEndDate)
                    {
                        Debug.WriteLine($"[DATA GAP] startDate: {startDate.Value}, DataEndDate: {DataEndDate}");
                        throw new NotImplementedException("TODO REVIEW this gap length");
                        //AddGap(DataEndDate + TimeFrame.TimeSpan, startDate.Value - TimeSpanApproximation.TimeSpan);
                    }
                }
            }

            var oldEnd = DataEndDate;
            var oldStart = DataStartDate;
            if (DataEndDate == default(DateTimeOffset) || startDate.Value - TimeFrame.TimeSpan <= DataEndDate && endDate.Value > DataEndDate)
            {
                DataEndDate = endDate.Value;
            }
            if (DataStartDate == default(DateTimeOffset) || endDate.Value + TimeFrame.TimeSpan >= DataStartDate && startDate.Value < DataStartDate)
            {
                DataStartDate = startDate.Value;
            }

            var addedSpan = this[LastIndex].Time - (lastDataEndDate == default(DateTimeOffset) ? this[FirstIndex].Time : lastDataEndDate);
            if (addedSpan > TimeSpan.Zero)
            {
                BackwardForward bf = BackwardForward.None;
                if (oldEnd != DataEndDate) bf |= BackwardForward.Forward;
                if (oldStart != DataStartDate) bf |= BackwardForward.Backward;
                Debug.WriteLine($"[{this}] ADDED DATA: {addedSpan} {bf} ");

                DataAdded?.Invoke(bf);
            }
            //Debug.WriteLine($"[{this} - new range] {DataStartDate} - {DataEndDate} (last data: {this[LastIndex].Time})           (was {oldStart} - {oldEnd} (last data: {lastDataEndDate}))");
#if DEBUG_BARSCOPIED
            Debug.WriteLine($"{SymbolCode}-{TimeFrame.Name} Imported {dataPointsCopied} bars");
#endif
            EraseGap(startDate.Value, endDate.Value);
        }

        public event Action<BackwardForward> DataAdded;



        #region Data Gaps (Incomplete

        private void AddGap(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            if (HasGap(startDate, endDate))
            {
                //Gap in data would be created.  Fill with MissingValue
                throw new NotImplementedException("TODO: Resolve Overlapping gaps");
            }
            Debug.WriteLine($"UNTESTED - {this.ToString()} GAP: {startDate} - {endDate}");
            if (Gaps == null) { Gaps = new SortedDictionary<DateTimeOffset, DateTimeOffset>(); }
            if (Gaps.ContainsKey(startDate))
            {
                var existingEnd = Gaps[startDate];
                if (existingEnd < endDate)
                {
                    Gaps[startDate] = endDate;
                }
            }
            Gaps.Add(startDate, endDate);
        }
        private void EraseGap(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            if (HasGap(startDate, endDate))
            {
                if (Gaps.Contains(new KeyValuePair<DateTimeOffset, DateTimeOffset>(startDate, endDate)))
                {
                    Gaps.Remove(startDate);
                }
                else
                {
                    if (!AntiGaps.Contains(new KeyValuePair<DateTimeOffset, DateTimeOffset>(startDate, endDate)))
                    {
                        AntiGaps.Add(startDate, endDate); // TODO  - make sense of this
                    }
                    else
                    {
                        if (AntiGaps[startDate] < endDate)
                        {
                            AntiGaps[startDate] = endDate;
                        }
                    }
                }
            }
        }




        public bool HasData(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            if (Last.Time >= endDate && First.Time <= startDate) return true;

            foreach (var kvp in Gaps)
            {
                if (kvp.Key > endDate) break;
                if (kvp.Key < endDate && kvp.Value > startDate) return false;
            }
            return true;
        }
        public bool HasGap(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            if (Gaps == null || Gaps.Count == 0) { return false; }
            foreach (var kvp in Gaps)
            {
                if (kvp.Key > endDate) break;
                if (kvp.Key < endDate && kvp.Value > startDate) return true;
            }
            return false;
        }
        private SortedDictionary<DateTimeOffset, DateTimeOffset> Gaps;
        private SortedDictionary<DateTimeOffset, DateTimeOffset> AntiGaps = new SortedDictionary<DateTimeOffset, DateTimeOffset>();


        #endregion

        public async Task LoadMoreData()
        {
            await EnsureDataAvailable(null, DataEndDate - TimeFrame.TimeSpanApproximation).ConfigureAwait(false);
        }



    }
    public abstract class MarketSeriesBase : INotifyPropertyChanged
    {
        #region Identity

        #region Derived

        public string Key
        {
            get
            {
                if (key == null && SymbolCode != null)
                {
                    key = SymbolCode.GetSeriesKey(TimeFrame);
                }
                return key;
            }
        }
        private string key;

        protected TimeSpan TimeFrameTimeSpan
        {
            get
            {
                return TimeFrame.TimeSpanApproximation;
                // OLD
                //if (TimeFrame != TimeFrame.t1) return TimeFrame.TimeSpan;

                //switch (TimeFrame.TimeFrameUnit)
                //{
                //    case TimeFrameUnit.Tick:
                //        return TimeSpan.FromMilliseconds(1); // REVIEW
                //    //case TimeFrameUnit.Second:
                //    //    break;
                //    //case TimeFrameUnit.Minute:
                //    //    break;
                //    //case TimeFrameUnit.Hour:
                //    //    break;
                //    //case TimeFrameUnit.Day:
                //    //    break;
                //    //case TimeFrameUnit.Week:
                //    //    break;
                //    //case TimeFrameUnit.Month:
                //    //    break;
                //    //case TimeFrameUnit.Year:
                //    //    break;
                //    default:
                //        throw new NotImplementedException();
                //}
            }
        }

        #endregion

        public JobQueue LoadDataJobs { get; private set; } = new JobQueue();
        //public async Task WaitForLoadData()
        //{
        //    var arr = LoadDataJobs.Where(t => !t.IsCompleted).ToArray();
        //    await Task.Factory.StartNew(() => Task.WaitAll(arr));
        //    foreach (var t in arr)
        //    {
        //        LoadDataJobs.Remove(t);
        //    }
        //}

        public string SymbolCode
        {
            get; protected set;
        }
        public Symbol Symbol => Feed.GetSymbol(SymbolCode); // MICROOPTIMIZE

        public TimeFrame TimeFrame
        {
            get; protected set;
        }

        #endregion

        #region Construction

        public MarketSeriesBase() { }
        public MarketSeriesBase(IFeed account, string key)
        {
            this.Feed = account;
            string symbol;
            TimeFrame timeFrame;
            MarketSeriesUtilities.DecodeKey(key, out symbol, out timeFrame);
            this.SymbolCode = symbol;
            this.TimeFrame = timeFrame;
        }
        public MarketSeriesBase(IFeed market, string symbol, TimeFrame timeFrame)
        {
            this.Feed = market;
            this.SymbolCode = symbol;
            this.TimeFrame = timeFrame;
        }

        #endregion

        #region Relationships

        public IFeed Feed { get; protected set; }
        // Obsolete?
        public IDataSource Source { get; set; }

        public IAccount Account { get { return Feed as IAccount; } protected set { this.Feed = value; } }
        public bool HasAccount => Account != null;

        #endregion

        #region State

        #region IsLoading

        public bool IsLoading
        {
            get { return isLoading; }
            set
            {
                if (isLoading == value) return;
                isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }
        private bool isLoading;

        HashSet<object> loaders = new HashSet<object>();
        private object loadersLock = new object();
        public void OnLoadingStarted(object obj)
        {
            lock (loadersLock)
            {
                loaders.Add(obj);
                IsLoading = true;
            }
        }
        public void OnLoadingFinished(object obj)
        {
            lock (loadersLock)
            {
                loaders.Remove(obj);
                IsLoading = loaders.Count > 0;
            }
        }


        #endregion

        #endregion

        public DateTimeOffset? CloseTime
        {
            get
            {
                var result = DateTimeOffset.UtcNow;
                if ((result.DayOfWeek == DayOfWeek.Friday && result.Hour >= 22) ||
                    result.DayOfWeek == DayOfWeek.Saturday ||
                    (result.DayOfWeek == DayOfWeek.Sunday && result.Hour < 21))
                {
                    while (result.DayOfWeek != DayOfWeek.Friday)
                    {
                        result = result - TimeSpan.FromDays(1);
                    }
                    result = new DateTimeOffset(result.Year, result.Month, result.Day, 22, 0, 0, TimeSpan.Zero);
                }
                return result;
            }
        }


        public int Count { get { return OpenTime.Count; } }

        //        public override int Count
        //        {
        //            get
        //            {
        //                base.Count
        //#if BarStruct
        //                return bars.Count; // FUTURE
        //#else
        //                return OpenTime.Count;
        //#endif
        //            }
        //        }


        #region Data



        #region DataStartDate

        public DateTimeOffset DataStartDate
        {
            get { return dataStartDate; }
            set
            {
                if (dataStartDate == value) return;
                dataStartDate = value;
                OnPropertyChanged(nameof(DataStartDate));
            }
        }
        private DateTimeOffset dataStartDate;

        #endregion

        #region DataEndDate

        public DateTimeOffset DataEndDate
        {
            get { return dataEndDate; }
            set
            {
                if (dataEndDate == value) return;
                dataEndDate = value;
                OnPropertyChanged(nameof(DataEndDate));
            }
        }
        private DateTimeOffset dataEndDate;

        #endregion


        public int FirstIndex { get { return OpenTime.FirstIndex; } }
        public int LastIndex { get { return OpenTime.LastIndex; } }

        public ConcurrentDictionary<DateTimeOffset, DataLoadResult> DataLoadResults
        {
            get; private set;
        } = new ConcurrentDictionary<DateTimeOffset, DataLoadResult>();

        public TimeSeries OpenTime
        {
            get { return openTime; }
        }
        protected TimeSeries openTime = new TimeSeries();

        /// <param name="time"></param>
        /// <param name="loadHistoricalData">If true, this may block for a long time!</param>
        /// <returns></returns>
        public int FindIndex(DateTimeOffset time, bool loadHistoricalData = false)
        {
            return openTime.FindIndex(time);

            // OLD - result -1 doens't work anymore since we allow negative indices
            //if (openTime[result] == default(DateTimeOffset) && loadHistoricalData)
            //{
            //    var first = OpenTime.First();
            //    if (time < first)
            //    {
            //        //var span = time - first;
            //        //var estimatedBars = span.TotalMilliseconds / TimeFrame.TimeSpan.TotalMilliseconds;
            //        EnsureDataAvailable(time, first).Wait(); // BLOCKING!
            //    }
            //    result = openTime.FindIndex(time);
            //}
            //else
            //{

            //}
            //return result;
        }
        #endregion

        #region Historical Data


        public SemaphoreSlim DataLock { get; private set; } = new SemaphoreSlim(1);

        public Task EnsureDataAvailable(DateTimeOffset? startDate, DateTimeOffset endDate, int minBars = 0, bool forceRetrieve = false)
        {
            return Feed.Data.EnsureDataAvailable(this, startDate, endDate, minBars, forceRetrieve);
        }

        #endregion

        #region Events

        public event Action<DateTimeOffset, DateTimeOffset> LoadHistoricalDataCompleted;
        public void RaiseLoadHistoricalDataCompleted(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            LoadHistoricalDataCompleted?.Invoke(startDate, endDate);
            OnPropertyChanged(nameof(Count));
        }

        #endregion

        #region Misc


        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        public abstract string DataPointName { get; }

        public override string ToString()
        {
            return $"{SymbolCode}-{TimeFrame}";
        }

        #endregion
    }

}
