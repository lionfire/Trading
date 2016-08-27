//#define BarStruct
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
#if BarStruct
using BarType = LionFire.Trading.TimedBarStruct;
#else
using BarType = LionFire.Trading.TimedBar;
#endif
using System.Threading.Tasks;

namespace LionFire.Trading
{
    

    public sealed class MarketSeries : IMarketSeries, IMarketSeriesInternal
    {
        #region Identity

        #region Derived

        public string Key { get { return key; } }
        private readonly string key;

        #endregion

        public string SymbolCode {
            get; private set;
        }
        public TimeFrame TimeFrame {
            get; private set;
        }

        #endregion

        #region Relationships

        public IDataSource Source { get; set; }

        #endregion

        #region Configuration

        public static readonly bool FillMissingBars = false; // REVIEW - 
        // If true, each index represents an increment of the TimeFrame.  
        public static readonly bool UniformBars = false;

        #region Derived

        private TimeSpan TimeSpanIncrement {
            get {
                var timeSpan = TimeFrame.TimeSpan;
                if (timeSpan != TimeSpan.Zero) return timeSpan;

                switch (TimeFrame.TimeFrameUnit)
                {
                    case TimeFrameUnit.Tick:
                        return TimeSpan.Zero;
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

        public MarketSeries(string key)
        {
            this.key = key;

            string symbol;
            TimeFrame timeFrame;
            MarketSeriesUtilities.DecodeKey(key, out symbol, out timeFrame);
            this.SymbolCode = symbol;
            this.TimeFrame = timeFrame;
        }
        public MarketSeries(string symbol, TimeFrame timeFrame)
        {
            this.key = symbol.GetSeriesKey(timeFrame);
            this.SymbolCode = symbol;
            this.TimeFrame = timeFrame;
        }

        #endregion

        #region Data

        public IBarSeries Bars {
            get { return bars; }
        }
        private BarSeries bars = new BarSeries();

        public TimeSeries OpenTime {
            get { return openTime; }
        }
        private TimeSeries openTime = new TimeSeries();

        public IDataSeries Open {
            get { return open; }
        }
        private DoubleDataSeries open = new DoubleDataSeries();

        public IDataSeries High {
            get { return high; }
        }
        private DoubleDataSeries high = new DoubleDataSeries();

        public IDataSeries Low {
            get { return low; }
        }
        private DoubleDataSeries low = new DoubleDataSeries();

        public IDataSeries Close {
            get { return close; }
        }
        private DoubleDataSeries close = new DoubleDataSeries();

        public IDataSeries TickVolume {
            get { return tickVolume; }
        }
        private DoubleDataSeries tickVolume = new DoubleDataSeries();

        #region Derived

        public int Count {
            get {
#if BarStruct
                return bars.Count;
#else
                return OpenTime.Count;
#endif
            }
        }

        public TimedBar LastBar {
            get {
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

        public BarType this[int index] {
            get {
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
        }

        private IEnumerable<DoubleDataSeries> AllDataSeries {
            get {
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

        public event Action<MarketSeries> BarReceived;
        public event Action<MarketSeries, double/*bid*/, double/*ask*/> TickReceived;
        //public event Action<TimedBar> BarFinished;
        //public event Action<TimedBar> InterimBarReceived;
        #endregion

        public int FindIndex(DateTime time)
        {
            return openTime.FindIndex(time);
        }

        public BarType this[DateTime time] {
            get {
                var index = FindIndex(time);
                if (index < 0) return default(BarType);
                return this[index];
            }
        }

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

            BarReceived?.Invoke(this);
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

        internal void Add(DateTime time, double open, double high, double low, double close, double volume)
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
            var series = new MarketSeries(symbolCode, timeFrame);
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
