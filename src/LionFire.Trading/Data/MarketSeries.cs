//#define DEBUG_BARSCOPIED
//#define BarStruct
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
    public class TMarketSeries : TMarketSeriesBase, ITemplate<MarketSeries>, IValidatesCreate
    {
        public ValidationContext ValidateCreate(ValidationContext context)
        {
            context.MemberNonNull(Feed, nameof(Feed));
            if (TimeFrame == "t1")
            {
                context.AddIssue(new ValidationIssue
                {
                    Message = "t1 not supported for TMarketSeries.  Use TMarketTickSeries instead.",
                    VariableName = nameof(TimeFrame),
                    Kind = ValidationIssueKind.InvalidConfiguration | ValidationIssueKind.ParameterOutOfRange,
                });
            }
            return context;
        }
    }

    public sealed class MarketSeries : MarketSeriesBase<TMarketSeries, TimedBar>, IMarketSeries, IMarketSeriesInternal, ITemplateInstance<TMarketSeries>
    {

        #region Configuration

        public static readonly bool FillMissingBars = false; // REVIEW - 
        // If true, each index represents an increment of the TimeFrame.  
        public static readonly bool UniformBars = false;

        #endregion

        #region Construction

        public MarketSeries() : base() { }
        public MarketSeries(IFeed account, string key) : base(account, key)
        {
        }
        public MarketSeries(IFeed market, string symbol, TimeFrame timeFrame) : base(market, symbol, timeFrame)
        {
        }

        #endregion

        #region Data

        public BarType this[DateTimeOffset time]
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
            get
            {
                if (bars == null)
                {
                    bars = new BarSeriesAdapter(this);
                }
                return bars;
            }
        }
        private BarSeriesAdapter bars; // TOOPTIMIZE: switch to BarSeries


        public DataSeries Open
        {
            get { return open; }
        }
        private DataSeries open = new DataSeries();

        public DataSeries High
        {
            get { return high; }
        }
        private DataSeries high = new DataSeries();

        public DataSeries Low
        {
            get { return low; }
        }
        private DataSeries low = new DataSeries();

        public DataSeries Close
        {
            get { return close; }
        }
        private DataSeries close = new DataSeries();

        public DataSeries TickVolume
        {
            get { return tickVolume; }
        }
        private DataSeries tickVolume = new DataSeries();

        #region Derived

        public override BarType this[int index]
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
        private IEnumerable<DataSeries> AllDataSeries
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

        public event Action<TimedBar> Bar
        {
            add
            {
                bool last = BarHasObservers;
                timedBar += value;
                if (last != BarHasObservers)
                {
                    BarHasObserversChanged?.Invoke(this, BarHasObservers);
                }
            }
            remove
            {
                bool last = BarHasObservers;
                timedBar -= value;
                if (last != BarHasObservers)
                {
                    BarHasObserversChanged?.Invoke(this, BarHasObservers);
                }
            }
        }
        event Action<TimedBar> timedBar;

        //public event Action<SymbolBar> SymbolBar
        //{
        //    add
        //    {
        //        bool last = BarHasObservers;
        //        symbolBar += value;
        //        if (last != BarHasObservers)
        //        {
        //            BarHasObserversChanged?.Invoke(this, BarHasObservers);
        //        }
        //    }
        //    remove
        //    {
        //        bool last = BarHasObservers;
        //        symbolBar -= value;
        //        if (last != BarHasObservers)
        //        {
        //            BarHasObserversChanged?.Invoke(this, BarHasObservers);
        //        }
        //    }
        //}
        //event Action<SymbolBar> symbolBar;
        private const object symbolBar = null;

        public bool BarHasObservers
        {
            get
            {
                return symbolBar != null || timedBar != null;
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

        BehaviorSubject<TimedBar> barSubject = new BehaviorSubject<BarType>(TimedBar.Invalid);

        //public event Action<MarketSeries> BarReceived;
        public event Action<MarketSeries, double/*bid*/, double/*ask*/> TickReceived;
        //public event Action<TimedBar> BarFinished;
        //public event Action<TimedBar> InterimBarReceived;
        #endregion


        #region Methods

        // OLD - this is also in base class
        ///// <param name="time"></param>
        ///// <param name="loadHistoricalData">If true, this may block for a long time!</param>
        ///// <returns></returns>
        //public int FindIndex(DateTime time, bool loadHistoricalData = false)
        //{
        //    var result = openTime.FindIndex(time);
        //    if (result == -1 && loadHistoricalData)
        //    {
        //        var first = OpenTime.First();
        //        if (time < first)
        //        {
        //            //var span = time - first;
        //            //var estimatedBars = span.TotalMilliseconds / TimeFrame.TimeSpan.TotalMilliseconds;
        //            EnsureDataAvailable(time, first).Wait(); // BLOCKING!
        //        }
        //        result = openTime.FindIndex(time);
        //    }
        //    //else
        //    //{

        //    //}
        //    return result;
        //}
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
                    for (var nextTime = OpenTime.LastValue + TimeFrameTimeSpan; nextTime < bar.OpenTime; nextTime += TimeFrameTimeSpan)
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
                //this.symbolBar?.Invoke(new SymbolBar(SymbolCode, bar));
                this.timedBar?.Invoke(bar);
            }
        }

        private void StartNewBar(DateTime? time = null)
        {
            if (open.Count > 0)
            {
                AddDataPointAtTime(openTime.LastValue + TimeFrameTimeSpan);
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
            if ((openTime.LastValue + TimeFrameTimeSpan) <= time)
            {
                StartNewBar(time);
            }
            tickVolume.LastValue++;
            TickReceived?.Invoke(this, bid, ask);
        }

        internal void AddDataPointAtTime(DateTimeOffset time)
        {
            OpenTime.Add(time);
            foreach (var ds in AllDataSeries)
            {
                ds.Add();
            }
        }
        /*
        public void Add(List<TimedBar> bars, DateTime? startDate = null, DateTime? endDate = null)
        {
            // TODO: Fill range from startDate to endDate with "NoData" and if a range is loaded that creates a gap with existing ranges, fill that with "MissingData"

            //if (bars.Count == 0) return;
            var resultsStartDate = bars.Count == 0 ? default(DateTime):bars[0].OpenTime;
            var resultsEndDate = bars.Count == 0 ? default(DateTime) : bars[bars.Count - 1].OpenTime;
            if (!startDate.HasValue) { startDate = resultsStartDate; Debug.WriteLine("WARN -!startDate.HasValue in MarketSeries.Add"); }
            if (!endDate.HasValue) { endDate = resultsEndDate; Debug.WriteLine("WARN -!endDate.HasValue in MarketSeries.Add"); }

#if DEBUG_BARSCOPIED
            int barsCopied = 0;
#endif

            if (this.Count == 0 && DataEndDate == default(DateTime) && DataStartDate == default(DateTime))
            {
                foreach (var b in bars)
                {
                    //Add(b.Time,b.Open,b.High,b.Low,b.Close,b.Volume);
                    this.openTime.Add(b.Time);
                    this.open.Add(b.Open);
                    this.high.Add(b.High);
                    this.low.Add(b.Low);
                    this.close.Add(b.Close);
                    this.tickVolume.Add(b.Volume);
                }
            }
            else
            {

                if (startDate <= DataStartDate) // prepending data
                {
                    int lastIndexToCopy;
                    for (lastIndexToCopy = bars.Count - 1; lastIndexToCopy >= 0 && bars[lastIndexToCopy].OpenTime >= DataStartDate; lastIndexToCopy--) ; // OPTIMIZE?

                    for (int dataIndex = OpenTime.MinIndex - 1; lastIndexToCopy >= 0; dataIndex--, lastIndexToCopy--)
                    {
                        //var bar = bars[lastIndexToCopy] as TimedBar;
                        //if (bar == null)
                        //{
                        //    bar = new TimedBar(bars[lastIndexToCopy]);
                        //}
                        this[dataIndex] = bars[lastIndexToCopy];
#if DEBUG_BARSCOPIED
                        barsCopied++;
#endif
                    }

                    if (DataStartDate != default(DateTime) && endDate.Value + TimeFrame.TimeSpan < DataStartDate)
                    {
                        Debug.WriteLine($"[DATA GAP] endDate: {endDate.Value}, DataStartDate: {DataStartDate}");
                        AddGap(endDate.Value + TimeFrame.TimeSpan, DataStartDate - TimeFrame.TimeSpan);
                    }

                }

                // Both above and below may get run

                if (endDate >= DataEndDate) // append data
                {
                    int lastIndexToCopy;
                    for (lastIndexToCopy = 0; lastIndexToCopy < bars.Count && bars[lastIndexToCopy].OpenTime <= DataEndDate; lastIndexToCopy++) ; // OPTIMIZE?

                    for (int dataIndex = OpenTime.LastIndex + 1; lastIndexToCopy < bars.Count; dataIndex++, lastIndexToCopy++)
                    {
                        //var bar = bars[lastIndexToCopy] as TimedBar;
                        //if (bar == null)
                        //{
                        //    bar = new TimedBar(bars[lastIndexToCopy]);
                        //}
                        this[dataIndex] = bars[lastIndexToCopy];                       
#if DEBUG_BARSCOPIED
                        barsCopied++;
#endif
                    }

                    if (DataEndDate != default(DateTime) && startDate.Value - TimeFrame.TimeSpan > DataEndDate)
                    {
                        Debug.WriteLine($"[DATA GAP] startDate: {startDate.Value}, DataEndDate: {DataEndDate}");
                        AddGap(DataEndDate + TimeFrame.TimeSpan, startDate.Value - TimeFrame.TimeSpan);
                    }
                }
            }

            var oldEnd = DataEndDate;
            var oldStart = DataStartDate;
            if (DataEndDate == default(DateTime) || startDate.Value - TimeFrame.TimeSpan <= DataEndDate && endDate.Value > DataEndDate)
            {
                DataEndDate = endDate.Value;
            }
            if (DataStartDate == default(DateTime) || endDate.Value + TimeFrame.TimeSpan >= DataStartDate && startDate.Value < DataStartDate)
            {
                DataStartDate = startDate.Value;
            }

            Debug.WriteLine($"[{this}] New data range: {DataStartDate} - {DataEndDate}  (was {oldStart} - {oldEnd})");
#if DEBUG_BARSCOPIED
            Debug.WriteLine($"{SymbolCode}-{TimeFrame.Name} Imported {barsCopied} bars");
#endif
            EraseGap(startDate.Value, endDate.Value);
        }
        */



        protected override void Add(TimedBar dataPoint)
        {
            var bar = dataPoint;
            // TODO: Don't split into 6 bidirectional arrays -- use one pair of arrays for TimedBar
            //this.bars.Add((TimedBar)dataPoint);
            this.openTime.Add(bar.OpenTime);
            this.open.Add(bar.Open);
            this.high.Add(bar.High);
            this.low.Add(bar.Low);
            this.close.Add(bar.Close);
            this.tickVolume.Add(bar.Volume);
        }

        public void Add(DateTimeOffset time, double open, double high, double low, double close, double volume)
        {
            this.openTime.Add(time);
            this.open.Add(open);
            this.high.Add(high);
            this.low.Add(low);
            this.close.Add(close);
            this.tickVolume.Add(volume);
        }

        private void Add(DateTimeOffset[] time, double[] open, double[] high, double[] low, double[] close, double[] volume)
        {
            this.openTime.Add(time);
            this.open.Add(open);
            this.high.Add(high);
            this.low.Add(low);
            this.close.Add(close);
            this.tickVolume.Add(volume);
        }
        private void Add(DateTimeOffset[] time, double[] open, double[] high, double[] low, double[] close, double[] volume, int count)
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
        public long FindPositionForDate(FileStream fs, DateTimeOffset seekDate, bool getPreviousBar = true)
        {
            int rewindAmount = 100000;

            DateTimeOffset firstDate;
            DateTimeOffset lastDate;
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

        public void ImportFromFile(string path, DateTimeOffset? startDate = null, DateTimeOffset? endDate = null, bool getPreviousBar = true)
        {
            int lines = 0;
            long totalBytes = new FileInfo(path).Length;
            long bytes = 0;
            long bytesToRead = totalBytes;
            long startPos = 0;
            long endPos = totalBytes;

            var sw = Stopwatch.StartNew();


            int bufferSize = 128;
            DateTimeOffset[] openTimeBuffer = new DateTimeOffset[bufferSize];
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



        #region Misc

        public override string DataPointName { get { return "bars"; } }

        #endregion
    }

}
