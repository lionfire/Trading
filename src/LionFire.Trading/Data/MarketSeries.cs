#define DEBUG_BARSCOPIED
//#define BarStruct
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Reactive.Subjects;
using LionFire.Validation;
using LionFire.Templating;
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

namespace LionFire.Trading
{

    public class TMarketSeriesBase : ITemplate
    {
        //[Ignore]
        //[Required]
        public IAccount Account { get; set; }

        //[Required]
        public string Symbol { get; set; }

        //[Required]
        public string TimeFrame { get; set; }

        public string Key { get { return MarketSeriesUtilities.GetSeriesKey(Symbol, TimeFrame); } }
    }

    public class TMarketSeries : TMarketSeriesBase, ITemplate<MarketSeries>, IValidatesCreate
    {
        public ValidationContext ValidateCreate(ValidationContext context)
        {
            context.MemberNonNull(Account, nameof(Account));
            if (TimeFrame == "t1")
            {
                context.AddIssue(new ValidationIssue
                {
                    Message = "t1 not supported for TMarketSeries.  Use TMarketTickSeries instead.",
                    MemberName = nameof(TimeFrame),
                    Kind = ValidationIssueKind.InvalidConfiguration | ValidationIssueKind.ParameterOutOfRange,
                });
            }
            return context;
        }
    }

    public class TMarketTickSeries : TMarketSeriesBase, ITemplate<MarketTickSeries>, IValidatesCreate

    {
        public ValidationContext ValidateCreate(ValidationContext context)
        {
            context.MemberNonNull(Account, nameof(Account));
            if (TimeFrame != "t1")
            {
                context.AddIssue(new ValidationIssue
                {
                    Message = "Only t1 supported for TMarketTickSeries.  Use TMarketSeries instead for other timeframes.",
                    MemberName = nameof(TimeFrame),
                    Kind = ValidationIssueKind.InvalidConfiguration | ValidationIssueKind.ParameterOutOfRange,
                });
            }

            return context;
        }
    }

    public class MarketSeriesBase<MarketSeriesTemplate> : MarketSeriesBase, ITemplateInstance
        where MarketSeriesTemplate : TMarketSeriesBase, new()
    {

        #region Template

        ITemplate ITemplateInstance.Template { get { return Template; } set { Template = (MarketSeriesTemplate)value; } }
        public MarketSeriesTemplate Template
        {
            get
            {
                if (template == null)
                {
                    template = new MarketSeriesTemplate
                    {
                        Account = this.Account,
                        Symbol = this.SymbolCode,
                        TimeFrame = this.TimeFrame.Name,
                    };
                }
                return template;
            }
            set { template = value; }
        }
        public MarketSeriesTemplate template;

        #endregion

    }
    public class MarketSeriesBase
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
        public TimeFrame TimeFrame
        {
            get; protected set;
        }

        #endregion


        #region Relationships

        public IAccount Market { get; protected set; }
        // Obsolete?
        public IDataSource Source { get; set; }

        public IAccount Account { get { return Market; } protected set { this.Market = value; } }

        #endregion



        #region Data

        public TimeSeries OpenTime
        {
            get { return openTime; }
        }
        protected TimeSeries openTime = new TimeSeries();

        /// <param name="time"></param>
        /// <param name="loadHistoricalData">If true, this may block for a long time!</param>
        /// <returns></returns>
        public int FindIndex(DateTime time, bool loadHistoricalData = false)
        {
            var result = openTime.FindIndex(time);
            if (result == -1 && loadHistoricalData)
            {
                var first = OpenTime.First();
                if (time < first)
                {
                    //var span = time - first;
                    //var estimatedBars = span.TotalMilliseconds / TimeFrame.TimeSpan.TotalMilliseconds;
                    EnsureDataAvailable(time, first).Wait(); // BLOCKING!
                }
                result = openTime.FindIndex(time);
            }
            //else
            //{

            //}
            return result;
        }

        #endregion

        #region Historical Data

        public Task EnsureDataAvailable(DateTime? startDate, DateTime endDate, int minBars = 0)
        {
            return Account.Data.EnsureDataAvailable(this, startDate, endDate, minBars);
        }

        #endregion
    }

    public sealed class MarketTickSeries : MarketSeriesBase<TMarketTickSeries>, ITemplateInstance<TMarketTickSeries>

    {

        #region Construction

        public MarketTickSeries() { }
        public MarketTickSeries(IAccount account, string symbolCode)
        {
            this.Account = account;
            this.SymbolCode = symbolCode;
        }

        #endregion

        #region Data

        public TickType this[DateTime time]
        {
            get
            {
                var index = FindIndex(time);
                if (index < 0) return default(TickType);
                return this[index];
            }
        }

        public TickType this[int index]
        {
            get
            {
                return new TickType
                {
                    Time = openTime[index],
                    Bid = bid[index],
                    Ask = ask[index],
                };
            }
            set
            {
                openTime[index] = value.Time;
                bid[index] = value.Bid;
                ask[index] = value.Ask;
            }
        }

        public IDataSeries Bid
        {
            get { return bid; }
        }
        private DoubleDataSeries bid = new DoubleDataSeries();
        public IDataSeries Ask
        {
            get { return ask; }
        }
        private DoubleDataSeries ask = new DoubleDataSeries();

        #endregion
    }

    public sealed class MarketSeries : MarketSeriesBase<TMarketSeries>, IMarketSeries, IMarketSeriesInternal, ITemplateInstance<TMarketSeries>
    {

        #region Configuration

        public static readonly bool FillMissingBars = false; // REVIEW - 
        // If true, each index represents an increment of the TimeFrame.  
        public static readonly bool UniformBars = false;

        #region Derived

        private TimeSpan TimeSpanIncrement
        {
            get
            {
                var timeSpan = TimeFrame.TimeSpan;
                if (timeSpan != TimeSpan.Zero) return timeSpan;

                switch (TimeFrame.TimeFrameUnit)
                {
                    case TimeFrameUnit.Tick:
                        return TimeSpan.Zero; // REVIEW
                    //case TimeFrameUnit.Second:
                    //    break;
                    //case TimeFrameUnit.Minute:
                    //    break;
                    //case TimeFrameUnit.Hour:
                    //    break;
                    //case TimeFrameUnit.Day:
                    //    break;
                    //case TimeFrameUnit.Week:
                    //    break;
                    //case TimeFrameUnit.Month:
                    //    break;
                    //case TimeFrameUnit.Year:
                    //    break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        #endregion

        #endregion

        #region Construction

        public MarketSeries() { }
        public MarketSeries(IAccount account, string key)
        {
            this.Market = account;
            string symbol;
            TimeFrame timeFrame;
            MarketSeriesUtilities.DecodeKey(key, out symbol, out timeFrame);
            this.SymbolCode = symbol;
            this.TimeFrame = timeFrame;
        }
        public MarketSeries(IAccount market, string symbol, TimeFrame timeFrame)
        {
            this.Market = market;
            this.SymbolCode = symbol;
            this.TimeFrame = timeFrame;
        }

        #endregion

        #region Data

        public BarType this[DateTime time]
        {
            get
            {
                var index = FindIndex(time);
                if (index < 0) return default(BarType);
                return this[index];
            }
        }

        public IBarSeries Bars
        {
            get { return bars; }
        }
        private BarSeries bars = new BarSeries();


        public IDataSeries Open
        {
            get { return open; }
        }
        private DoubleDataSeries open = new DoubleDataSeries();

        public IDataSeries High
        {
            get { return high; }
        }
        private DoubleDataSeries high = new DoubleDataSeries();

        public IDataSeries Low
        {
            get { return low; }
        }
        private DoubleDataSeries low = new DoubleDataSeries();

        public IDataSeries Close
        {
            get { return close; }
        }
        private DoubleDataSeries close = new DoubleDataSeries();

        public IDataSeries TickVolume
        {
            get { return tickVolume; }
        }
        private DoubleDataSeries tickVolume = new DoubleDataSeries();

        #region Derived

        public int Count
        {
            get
            {
#if BarStruct
                return bars.Count;
#else
                return OpenTime.Count;
#endif
            }
        }

        public TimedBar FirstBar
        {
            get
            {
                return this[OpenTime.MinIndex];
            }
        }
        public TimedBar LastBar
        {
            get
            {
#if BarStruct
                return bars.LastValue;
#else
                return new TimedBar
                {
                    OpenTime = openTime.LastValue,
                    Open = open.LastValue,
                    High = high.LastValue,
                    Low = low.LastValue,
                    Close = close.LastValue,
                    Volume = tickVolume.LastValue,
                };
#endif
            }
        }

        public BarType this[int index]
        {
            get
            {
#if BarStruct
                return bars[index];
#else
                return new TimedBar
                {
                    OpenTime = openTime[index],
                    Open = open[index],
                    High = high[index],
                    Low = low[index],
                    Close = close[index],
                    Volume = tickVolume[index],
                };
#endif
            }
            set
            {
                openTime[index] = value.OpenTime;
                open[index] = value.Open;
                high[index] = value.High;
                low[index] = value.Low;
                close[index] = value.Close;
                tickVolume[index] = value.Volume;
            }
        }

        private IEnumerable<DoubleDataSeries> AllDataSeries
        {
            get
            {
                yield return open;
                yield return high;
                yield return low;
                yield return close;
                yield return tickVolume;
            }
        }


        #endregion

        #endregion

        #region Backtesting

        public HistoricalPlaybackState HistoricalPlaybackState { get; set; }

        #endregion

        #region Events

        public event Action<SymbolBar> Bar
        {
            add
            {
                bool changed = bar == null;
                bar += value;
                if (changed)
                {
                    BarHasObserversChanged?.Invoke(this, BarHasObservers);
                }
            }
            remove
            {
                bool changed = bar != null;
                bar -= value;
                if (bar == null)
                {
                    BarHasObserversChanged?.Invoke(this, BarHasObservers);
                }
            }
        }
        event Action<SymbolBar> bar;

        public bool BarHasObservers
        {
            get
            {
                return bar != null;
            }
        }

        public event Action<IMarketSeries, bool> BarHasObserversChanged;

        public bool LatestBarHasObservers { get { return barSubject.HasObservers; } }
        public IObservable<TimedBar> LatestBar
        {
            get
            {
                return barSubject;
            }
        }

        public int MinIndex { get { return OpenTime.MinIndex; } }

        BehaviorSubject<TimedBar> barSubject = new BehaviorSubject<BarType>(null);


        //public event Action<MarketSeries> BarReceived;
        public event Action<MarketSeries, double/*bid*/, double/*ask*/> TickReceived;
        //public event Action<TimedBar> BarFinished;
        //public event Action<TimedBar> InterimBarReceived;
        #endregion

        #region Methods

        /// <param name="time"></param>
        /// <param name="loadHistoricalData">If true, this may block for a long time!</param>
        /// <returns></returns>
        public int FindIndex(DateTime time, bool loadHistoricalData = false)
        {
            var result = openTime.FindIndex(time);
            if (result == -1 && loadHistoricalData)
            {
                var first = OpenTime.First();
                if (time < first)
                {
                    //var span = time - first;
                    //var estimatedBars = span.TotalMilliseconds / TimeFrame.TimeSpan.TotalMilliseconds;
                    EnsureDataAvailable(time, first).Wait(); // BLOCKING!
                }
                result = openTime.FindIndex(time);
            }
            //else
            //{

            //}
            return result;
        }
        #endregion

        //public IEnumerable<SymbolBar> GetBars(DateTime fromTimeExclusive, DateTime endTimeInclusive)
        //{
        //    return null;
        //    //SymbolBar bar = null;
        //    //bar.Yield();
        //}

        #region (Internal) Methods - Data input

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bar"></param>
        /// <param name="barOpenTime"></param>
        /// <param name="finishedBar">True if backtesting with data that has one bar per time step.</param>
        public void OnBar(TimedBar bar, bool finishedBar = false)
        {
            if (OpenTime.LastValue != bar.OpenTime)
            {
                if (FillMissingBars)
                {
                    // REVIEW 
                    Console.WriteLine("WARN: Bug exists when trying to fill from year 0 to present when FillMissingBars enabled.");
                    for (var nextTime = OpenTime.LastValue + TimeSpanIncrement; nextTime < bar.OpenTime; nextTime += TimeSpanIncrement)
                    {
                        AddDataPointAtTime(nextTime);
                    }
                }
                if (OpenTime.LastValue != bar.OpenTime)
                {
                    AddDataPointAtTime(bar.OpenTime);
                }
            }

            // Replace last values
            open.LastValue = bar.Open;
            high.LastValue = bar.High;
            low.LastValue = bar.Low;
            close.LastValue = bar.Close;
            tickVolume.LastValue = bar.Volume;

            if (finishedBar)
            {
                //BarReceived?.Invoke(this);
                this.barSubject.OnNext(bar);
                this.bar?.Invoke(new SymbolBar(SymbolCode, bar, bar.OpenTime));
            }
        }

        private void StartNewBar(DateTime? time = null)
        {
            if (open.Count > 0)
            {
                AddDataPointAtTime(openTime.LastValue + TimeSpanIncrement);
                open.LastValue = high.LastValue = low.LastValue = close.LastValue = close.Last(1);
            }
            else
            {
                if (!time.HasValue) throw new ArgumentNullException("value for time required when series is empty");

                var roundedTime = this.TimeFrame.Round(time.Value);
                AddDataPointAtTime(roundedTime);
                // bar = NaN
            }
            tickVolume.LastValue = 0;

        }
        public void OnTick(DateTime time, double bid, double ask)
        {
            if ((openTime.LastValue + TimeSpanIncrement) <= time)
            {
                StartNewBar(time);
            }
            tickVolume.LastValue++;
            TickReceived?.Invoke(this, bid, ask);
        }

        internal void AddDataPointAtTime(DateTime time)
        {
            OpenTime.Add(time);
            foreach (var ds in AllDataSeries)
            {
                ds.Add();
            }
        }

        public void Add(List<TimedBarStruct> bars, DateTime? startDate = null, DateTime? endDate = null)
        {
            // TODO: Fill range from startDate to endDate with "NoData" and if a range is loaded that creates a gap with existing ranges, fill that with "MissingData"

            if (bars.Count == 0) return;
            var resultsStartDate = bars[0].OpenTime;
            var resultsEndDate = bars[bars.Count - 1].OpenTime;
            if (!startDate.HasValue) startDate = resultsStartDate;
            if (!endDate.HasValue) endDate = resultsEndDate;

#if DEBUG_BARSCOPIED
            int barsCopied = 0;
#endif

            if (this.Count == 0)
            {
                foreach (var b in bars)
                {
                    Add(b.OpenTime, b.Open, b.High, b.Low, b.Close, b.Volume);
                }
            }
            else
            {
                var dataStartDate = this.OpenTime.First();
                var dataEndDate = this.OpenTime.Last();


                if (startDate <= dataStartDate) // prepending data
                {
                    int lastIndexToCopy;
                    for (lastIndexToCopy = bars.Count - 1; lastIndexToCopy >= 0 && bars[lastIndexToCopy].OpenTime >= dataStartDate; lastIndexToCopy--) ; // OPTIMIZE?

                    for (int dataIndex = OpenTime.MinIndex - 1; lastIndexToCopy >= 0; dataIndex--, lastIndexToCopy--)
                    {
                        this[dataIndex] = bars[lastIndexToCopy];
#if DEBUG_BARSCOPIED
                        barsCopied++;
#endif
                    }

                    if (endDate.Value < dataStartDate)
                    {
                        AddGap(endDate.Value + TimeFrame.TimeSpan, dataStartDate - TimeFrame.TimeSpan);
                    }
                }

                // Both above and below may get run

                if (endDate >= dataEndDate) // append data
                {
                    int lastIndexToCopy;
                    for (lastIndexToCopy = 0; lastIndexToCopy < bars.Count && bars[lastIndexToCopy].OpenTime <= dataEndDate; lastIndexToCopy++) ; // OPTIMIZE?

                    for (int dataIndex = OpenTime.LastIndex + 1; lastIndexToCopy < bars.Count; dataIndex++, lastIndexToCopy++)
                    {
                        this[dataIndex] = bars[lastIndexToCopy];
#if DEBUG_BARSCOPIED
                        barsCopied++;
#endif
                    }

                    if (startDate.Value > dataEndDate)
                    {
                        AddGap(dataEndDate + TimeFrame.TimeSpan, startDate.Value - TimeFrame.TimeSpan);
                    }
                }
            }
#if DEBUG_BARSCOPIED
            Debug.WriteLine($"{SymbolCode}-{TimeFrame.Name} Imported {barsCopied} bars");
#endif
            EraseGap(startDate.Value, endDate.Value);
        }
        private void AddGap(DateTime startDate, DateTime endDate)
        {
            if (HasGap(startDate, endDate))
            {
                //Gap in data would be created.  Fill with MissingValue
                throw new NotImplementedException("TODO: Resolve Overlapping gaps");
            }
            Debug.WriteLine($"UNTESTED - {this.ToString()} GAP: {startDate} - {endDate}");
            if (Gaps == null) { Gaps = new SortedDictionary<DateTime, DateTime>(); }
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
        private void EraseGap(DateTime startDate, DateTime endDate)
        {
            if (HasGap(startDate, endDate))
            {
                throw new NotImplementedException("TODO: Erase gap");
            }
        }
        public bool HasData(DateTime startDate, DateTime endDate)
        {
            if (LastBar.OpenTime >= endDate && FirstBar.OpenTime <= startDate) return true;

            foreach (var kvp in Gaps)
            {
                if (kvp.Key > endDate) break;
                if (kvp.Key < endDate && kvp.Value > startDate) return false;
            }
            return true;
        }
        public bool HasGap(DateTime startDate, DateTime endDate)
        {
            if (Gaps == null) { return false; }
            foreach (var kvp in Gaps)
            {
                if (kvp.Key > endDate) break;
                if (kvp.Key < endDate && kvp.Value > startDate) return true;
            }
            return false;
        }
        private SortedDictionary<DateTime, DateTime> Gaps;


        public void Add(DateTime time, double open, double high, double low, double close, double volume)
        {
            this.openTime.Add(time);
            this.open.Add(open);
            this.high.Add(high);
            this.low.Add(low);
            this.close.Add(close);
            this.tickVolume.Add(volume);
        }

        private void Add(DateTime[] time, double[] open, double[] high, double[] low, double[] close, double[] volume)
        {
            this.openTime.Add(time);
            this.open.Add(open);
            this.high.Add(high);
            this.low.Add(low);
            this.close.Add(close);
            this.tickVolume.Add(volume);
        }
        private void Add(DateTime[] time, double[] open, double[] high, double[] low, double[] close, double[] volume, int count)
        {
            for (int i = 0; i < count; i++)
            {
                this.openTime.Add(time[i]);
                this.open.Add(open[i]);
                this.high.Add(high[i]);
                this.low.Add(low[i]);
                this.close.Add(close[i]);
                this.tickVolume.Add(volume[i]);
            }
        }

        #endregion

        #region Methods - Import

        // MOVE to file importer

        public const char csvSeparator = ',';

        private DateTime ParseDate(string line /*, bool fastDate, bool hasSeconds*/)
        {
            var cells = line.Split(csvSeparator);
            var dateString = cells[0] + " " + cells[1];
            DateTime result;
            DateTime.TryParse(dateString, out result);
            return result;
        }

        public DateTime? GetFirstDateFromPosition(FileStream fs, long position)
        {
            fs.Position = position;

            byte[] buffer = new byte[256];
            fs.Read(buffer, 0, buffer.Length);
            var str = UTF8Encoding.UTF8.GetString(buffer);
            var lines = str.Split('\n');
            if (lines.Length < 2) return null;
            var dateTime = ParseDate(lines[1]);
            return dateTime;
        }
        public long FindPositionForDate(FileStream fs, DateTime seekDate, bool getPreviousBar = true)
        {
            int rewindAmount = 100000;

            DateTime firstDate;
            DateTime lastDate;
            string line;

            fs.Seek(0, SeekOrigin.Begin);
            using (var sr = new StreamReader(fs, UTF8Encoding.UTF8, true, 1024, true))
            {
                line = sr.ReadLine().Trim();

                if (line.Length == 0 || !char.IsNumber(line[0])) // Skip header row
                {
                    line = sr.ReadLine();
                }
                firstDate = ParseDate(line);
                //Console.WriteLine($"First date: {firstDate}");
            }

            fs.Seek(-512, SeekOrigin.End);
            List<string> lastLines = new List<string>();
            using (var sr = new StreamReader(fs, UTF8Encoding.UTF8, true, 1024, true))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    lastLines.Add(line);
                }
                int index = lastLines.Count - 1;
                while (lastLines[index].Trim().Length == 0)
                {
                    index--;
                }
                string lastLine = lastLines[index];
                lastDate = ParseDate(lastLine);
                //Console.WriteLine($"Last date: {lastDate}");
            }

            var fileTimeSpan = lastDate - firstDate;
            var seekTimeSpan = seekDate - firstDate;
            if (seekTimeSpan < TimeSpan.Zero) return -1;
            if (seekDate > lastDate)
            {
                return fs.Length;
            }

            var seekPercent = seekTimeSpan.TotalMilliseconds / fileTimeSpan.TotalMilliseconds;
            var seekBytes = Math.Max(0, (long)(fs.Length * (seekPercent - 0.005)) - 100);

            long bytesDelta;
            while (true)
            {
                fs.Seek(seekBytes, SeekOrigin.Begin);

                bytesDelta = 0;
                using (var sr = new StreamReader(fs, UTF8Encoding.UTF8, true, 1024, true))
                {
                    line = sr.ReadLine(); // Skip first line, probably partial.
                    bytesDelta += line.Length + 2;

                    DateTime date1;
                    DateTime date2;

                    line = sr.ReadLine();
                    bytesDelta += line.Length + 2;
                    date1 = ParseDate(line);

                    line = sr.ReadLine();
                    bytesDelta += line.Length + 2;
                    date2 = ParseDate(sr.ReadLine());

                    while (date2 < seekDate)
                    {
                        date1 = date2;
                        line = sr.ReadLine();
                        bytesDelta += line.Length + 2;
                        date2 = ParseDate(line);
                    }

                    if (date2 > seekDate && date1 > seekDate && seekBytes > 0)
                    {
                        seekBytes -= rewindAmount;
                        if (seekBytes < 0) seekBytes = 0;
                        //Console.WriteLine($"Overshot.  Rewinding by {rewindAmount} to {seekBytes}.");
                        continue;
                    }
                    else
                    {
                        if (getPreviousBar)
                        {
                            bytesDelta -= line.Length + 2;
                            if (bytesDelta < 0) bytesDelta = 0;
                        }
                        //Console.WriteLine($"date1 {date1}, seekDate {seekDate}, date2 {date2} at byte {seekBytes + bytesDelta}");
                        break;
                    }
                }
            }
            return Math.Max(0, seekBytes + bytesDelta - 2); // Minus 2 for \r\n -- first line is discarded.
        }


        public void ImportFromFile(string path, DateTime? startDate = null, DateTime? endDate = null, bool getPreviousBar = true)
        {
            int lines = 0;
            long totalBytes = new FileInfo(path).Length;
            long bytes = 0;
            long bytesToRead = totalBytes;
            long startPos = 0;
            long endPos = totalBytes;

            var sw = Stopwatch.StartNew();


            int bufferSize = 128;
            DateTime[] openTimeBuffer = new DateTime[bufferSize];
            double[] openBuffer = new double[bufferSize];
            double[] highBuffer = new double[bufferSize];
            double[] lowBuffer = new double[bufferSize];
            double[] closeBuffer = new double[bufferSize];
            double[] volumeBuffer = new double[bufferSize];
            int bufferIndex = 0;

            bool fastDate = false;
            bool hasSeconds = false;

            try
            {
                string line;

                FileStream fs;
                using (fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    if (startDate.HasValue)
                    {
                        startPos = Math.Max(0, FindPositionForDate(fs, startDate.Value));
                    }
                    if (endDate.HasValue)
                    {
                        endPos = FindPositionForDate(fs, endDate.Value);
                    }

                    fs.Seek(startPos, SeekOrigin.Begin);

                    bytesToRead = endPos - startPos;

                    Console.Write($"Reading {bytesToRead} bytes from '{Path.GetFileName(path)}'   00.0%");

                    using (var sr = new StreamReader(fs))
                    {
                        line = sr.ReadLine(); // Discard first line
                        if (line != null)
                        {
                            bytes += line.Length + 2;
                        }

                        bool isFirstLine = true;
                        for (string previousLine = null; (line = sr.ReadLine()) != null; previousLine = line)
                        {
                            bytes += line.Length + 2;
                            if (++lines % 32768 == 0 || bytes >= totalBytes)
                            {
                                Console.Write("\b\b\b\b\b" + (100.0 * bytes / bytesToRead).ToString("N1").PadLeft(4) + "%");
                            }

                            if (String.IsNullOrWhiteSpace(line)) continue;
                            var cells = line.Split(',');
                            if (isFirstLine)
                            {
                                isFirstLine = false;
                                var trimmed = line.TrimStart();
                                if (!char.IsNumber(trimmed[0]))
                                {
                                    continue; // Skip header line
                                }

                                if (cells[0][4] == '.')
                                {
                                    fastDate = true;
                                }
                                else
                                {
                                    Console.Write(" (slow parsing mode) ");
                                }
                                if (cells[1].Length > 6)
                                {
                                    hasSeconds = true;
                                }
                            }

                            DateTime time;
                            if (fastDate)
                            {
                                time = new DateTime(
                                        Convert.ToInt32(cells[0].Substring(0, 4)),
                                        Convert.ToInt32(cells[0].Substring(5, 2)),
                                        Convert.ToInt32(cells[0].Substring(8, 2)),
                                        Convert.ToInt32(cells[1].Substring(0, 2)),
                                        Convert.ToInt32(cells[1].Substring(3, 2)),
                                        hasSeconds ? Convert.ToInt32(cells[1].Substring(6, 2)) : 0);
                            }
                            else
                            {
                                time = DateTime.Parse(cells[0] + " " + cells[1]);
                            }

                            // FUTURE: option to get the previous bar before the startdate.  getPreviousBar
                            if (startDate.HasValue && time < startDate.Value) continue;


#if true
                            if (endDate.HasValue && time > endDate.Value)
                            {
                                this.Add(openTimeBuffer, openBuffer, highBuffer, lowBuffer, closeBuffer, volumeBuffer, bufferIndex);
                                break;
                            }

                            openTimeBuffer[bufferIndex] = time;
                            openBuffer[bufferIndex] = Convert.ToDouble(cells[2]);
                            highBuffer[bufferIndex] = Convert.ToDouble(cells[3]);
                            lowBuffer[bufferIndex] = Convert.ToDouble(cells[4]);
                            closeBuffer[bufferIndex] = Convert.ToDouble(cells[5]);
                            volumeBuffer[bufferIndex] = Convert.ToDouble(cells[6]);
                            if (++bufferIndex == bufferSize)
                            {
                                bufferIndex = 0;
                                this.Add(openTimeBuffer, openBuffer, highBuffer, lowBuffer, closeBuffer, volumeBuffer);
                            }
#else
                        if (endDate.HasValue && time > endDate.Value) break;

                        var Open = Convert.ToDouble(cells[2]);
                        var High = Convert.ToDouble(cells[3]);
                        var Low = Convert.ToDouble(cells[4]);
                        var Close = Convert.ToDouble(cells[5]);
                        var Volume = Convert.ToDouble(cells[6]);
                        this.Add(Time, Open, High, Low, Close, Volume);
#endif
                        }
                    }
                }

                var elapsed = Math.Max(1, sw.ElapsedMilliseconds);

                Console.WriteLine($"\b\b\b\b\b ...done. ({lines} lines in {elapsed / 1000}s, {lines * 1000 / elapsed } lines/sec)");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }



        public static MarketSeries ImportFromFile(string symbolCode, TimeFrame timeFrame, string path, DateTime? startDate = null, DateTime? endDate = null)
        {
            var series = new MarketSeries(null, symbolCode, timeFrame);
            series.ImportFromFile(path, startDate, endDate);
            GC.Collect();

            if (series != null && series.OpenTime.Count > 0)
            {
                Console.WriteLine($"Imported {timeFrame} {symbolCode} ({series.OpenTime.Count} data points from {series.OpenTime[0]} to {series.OpenTime.LastValue})");
            }
            else
            {
                Console.WriteLine($"Could not import {timeFrame} {symbolCode}");
            }
            return series;
        }

        #endregion

        public event Action<DateTime, DateTime> LoadHistoricalDataCompleted;
        public void RaiseLoadHistoricalDataCompleted(DateTime startDate, DateTime endDate)
        {
            LoadHistoricalDataCompleted?.Invoke(startDate, endDate);
        }
    }

    public static class MarketSeriesUtilities
    {
        public const char Delimiter = ';';

        public static string GetSeriesKey(this string symbol, TimeFrame timeFrame)
        {
            return symbol + Delimiter.ToString() + timeFrame.Name;
        }

        internal static void DecodeKey(string key, out string symbol, out TimeFrame timeFrame)
        {
            var chunks = key.Split(Delimiter);
            symbol = chunks[0];
            timeFrame = TimeFrame.TryParse(chunks[1]);
        }
    }
}
