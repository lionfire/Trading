using LionFire.ExtensionMethods;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Execution;

namespace LionFire.Trading.Data
{

    public class HistoricalDataCacheFileHeader
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime QueryDate { get; set; }
    }


    /*
     * TODO: Load from zip files to reduce filesystem footprint:
     * 
     * http://stackoverflow.com/a/36762855/208304
     * http://stackoverflow.com/a/14753848/208304
     * string zipFileFullPath = "{{TypeYourZipFileFullPathHere}}";
string targetFileName = "{{TypeYourTargetFileNameHere}}";
string text = new string(
            (new System.IO.StreamReader(
             System.IO.Compression.ZipFile.OpenRead(zipFileFullPath)
             .Entries.Where(x => x.Name.Equals(targetFileName,
                                          StringComparison.InvariantCulture))
             .FirstOrDefault()
             .Open(), Encoding.UTF8)
             .ReadToEnd())
             .ToArray());
     */
    public class HistoricalDataCacheFile
    {

        #region (Static) Utility Methods


        public static (DateTime chunkStartDate, DateTime chunkEndDate) GetChunkRange(TimeFrame tf, DateTime date, out DateTime chunkStartDate, out DateTime chunkEndDate) // TOC#7
        {
            switch (tf.TimeFrameUnit)
            {
                case TimeFrameUnit.Tick:
                    chunkStartDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0, DateTimeKind.Utc);
                    chunkEndDate = new DateTime(chunkStartDate.Year, chunkStartDate.Month, chunkStartDate.Day, chunkStartDate.Hour, 59, 59, 999, DateTimeKind.Utc);
                    break;
                case TimeFrameUnit.Minute:
                    chunkStartDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
                    chunkEndDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999, DateTimeKind.Utc);
                    break;
                case TimeFrameUnit.Hour:
                    chunkStartDate = new DateTime(date.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    chunkEndDate = new DateTime(date.Year, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc);
                    break;
                default:
                    throw new NotImplementedException($"TimeFrameUnit {tf.TimeFrameUnit} not supported yet");
            }
            return (chunkStartDate, chunkEndDate);
        }

        #endregion

        #region Configuration

        public string CacheRoot = @"d:\st\Investing-Data\MarketData"; // HARDPATH TODO

        #endregion

        #region Identity

        public static string GetKey(string brokerName, string symbolCode, TimeFrame tf, DateTime date, string brokerSubType = null)
        {
            var subType = brokerSubType == null ? "" : $" ({brokerSubType})";

            string dateStr;

            var tfUnit = tf.TimeFrameUnit;

            if (tfUnit == TimeFrameUnit.Minute && tf.TimeFrameValue > 60)
            {
                throw new NotImplementedException("TODO: tfUnit == TimeFrameUnit.Minute && tf.TimeFrameValue > 60");
            }

            switch (tf.TimeFrameUnit)
            {
                case TimeFrameUnit.Tick:
                    dateStr = $"{date.Year}\\{date.Month}\\{date.Day}\\{date.Hour}";
                    break;
                case TimeFrameUnit.Minute:
                    dateStr = $"{date.Year}\\{date.Month}\\{date.Day}";
                    break;
                case TimeFrameUnit.Hour:
                    dateStr = $"{date.Year}";
                    break;
                default:
                    throw new NotImplementedException($"TimeFrameUnit {tf.TimeFrameUnit} not supported yet");
            }

            return $"{brokerName}{subType}\\{symbolCode}\\{tf.Name}\\{dateStr}";
        }

        public string SymbolCode { get { return MarketSeriesBase.SymbolCode; } }
        public TimeFrame TimeFrame { get { return MarketSeriesBase.TimeFrame; } }
        public string BrokerSubType { get; set; }

        public MarketSeriesBase MarketSeriesBase { get; set; }
        public MarketSeries MarketSeries { get { return MarketSeriesBase as MarketSeries; } }
        public MarketTickSeries MarketTickSeries { get { return MarketSeriesBase as MarketTickSeries; } }

        #region StartDate

        public DateTime StartDate
        {
            get { return startDate; }
            set { startDate = value; }
        }
        private DateTime startDate;

        #endregion

        #region EndDate

        public DateTime EndDate
        {
            get { return endDate; }
            set { endDate = value; }
        }
        private DateTime endDate;

        #endregion
        public DateTime QueryDate { get; set; }

        public DateTime LastDataDate
        {
            get
            {
                if (Bars != null && Bars.Count > 0)
                {
                    return Bars[Bars.Count - 1].OpenTime;
                }
                if (Ticks != null && Ticks.Count > 0)
                {
                    return Ticks[Ticks.Count - 1].Time;
                }
                return default(DateTime);
            }
        }
        public string Key
        {
            get
            {
                return GetKey(Feed?.Template?.BrokerName, SymbolCode, TimeFrame, StartDate, BrokerSubType);
            }
        }

        #endregion

        public DataLoadResult DataLoadResult
        {
            get
            {
                return new DataLoadResult(MarketSeriesBase)
                {
                    Bars = this.Bars,
                    IsAvailable = this.IsAvailable,
                    IsPartial = this.IsPartial,
                    StartDate = this.StartDate,
                    EndDate = this.EndDate,
                    Ticks = this.Ticks,
                    QueryDate = this.QueryDate,
                };
            }
        }


        #region Construction

        public HistoricalDataCacheFile(MarketSeriesBase series, DateTime chunkDate)
        {
            this.MarketSeriesBase = series;
            this.Feed = series.Feed;
            GetChunkRange(TimeFrame, chunkDate, out startDate, out endDate);
            FilePath = GetFilePath();
        }

        public string GetFilePath()
        {
            var path = Path.Combine(LionFireEnvironment.AppProgramDataDir, "Data", Feed.Template.BrokerName, SymbolCode, TimeFrame.Name);

            switch (TimeFrame.Name)
            {
                case "t1":
                    path = Path.Combine(path, StartDate.Year.ToString(), StartDate.Month.ToString(), StartDate.Day.ToString(), StartDate.Hour.ToString());
                    break;
                case "m1":
                    path = Path.Combine(path, StartDate.Year.ToString(), StartDate.Month.ToString(), StartDate.Day.ToString());
                    break;
                case "h1":
                    path = Path.Combine(path, StartDate.Year.ToString());
                    break;
                default:
                    break;
            }

            path += ExtensionWithDot;
            return path;
        }

        public HistoricalDataCacheFile(MarketSeriesBase series, DateTime start, DateTime end, DateTime queryDate)
        {
            this.MarketSeriesBase = series;
            this.Feed = series.Feed;
            this.StartDate = start;
            this.EndDate = end;
            this.QueryDate = queryDate;
            this.FilePath = GetFilePath();
        }



        public static async Task<HistoricalDataCacheFile> GetCacheFile(MarketSeriesBase marketSeries, DateTime chunkDate)
        {
            HistoricalDataCacheFile cacheFile = cache.GetOrAdd(
                GetKey(marketSeries.Feed.Template.BrokerName, marketSeries.SymbolCode, marketSeries.TimeFrame, chunkDate),
                 _ => new HistoricalDataCacheFile(marketSeries, chunkDate)
             );

            await cacheFile.EnsureLoaded().ConfigureAwait(false);

            return cacheFile;
        }

        #endregion

        public async Task<bool> EnsureLoaded()
        {
            if (Ticks == null && Bars == null)
            {
                return await Load().ConfigureAwait(false);
            }
            return true;
        }
        #region Cache

        private static ConcurrentDictionary<string, HistoricalDataCacheFile> cache = new ConcurrentDictionary<string, HistoricalDataCacheFile>();

        public static void ClearCache()
        {
            cache.Clear();
        }

        #endregion

        #region Relationships

        public IFeed Feed { get; set; }

        #endregion

        #region Serialization

        public HistoricalDataCacheFileHeader Header
        {
            get
            {
                return new HistoricalDataCacheFileHeader()
                {
                    StartDate = this.StartDate,
                    EndDate = this.EndDate,
                    QueryDate = this.QueryDate,
                };
            }
            set
            {
                StartDate = value.StartDate;
                EndDate = value.EndDate;
                QueryDate = value.QueryDate;
            }
        }

        #endregion

        #region Properties

        public string FilePath { get; private set; }
        public string PartFilePath { get { return FilePath.Replace(ExtensionWithDot, ".part" + ExtensionWithDot); } }
        public string NoDataFilePath { get { return FilePath.Replace(ExtensionWithDot, ".unavailable" + ExtensionWithDot); } }
        public string ExtensionWithDot { get { return ".dat"; } }


        public bool HasData => (Bars != null && Bars.Count > 0) || (Ticks != null && Ticks.Count > 0);
        public bool IsPartial
        {
            get
            {
                if (isPartial.HasValue) return isPartial.Value;
                return QueryDate < EndDate + TimeSpan.FromMinutes(2); // HARDCODE
            }
            set { isPartial = value; }
        }
        bool? isPartial = null;
        public bool IsAvailable
        {
            get
            {
                if (isAvailable.HasValue) return isAvailable.Value;
                return Bars != null && Bars.Count > 0;
            }
            set { isAvailable = value; }
        }
        private bool? isAvailable = null;

        public bool EmptyData { get; set; }

        public bool IsPersisted { get; set; }

        public TimeSpan OutOfDateTimeSpan
        {
            get
            {
                if (!IsPartial) return TimeSpan.MinValue;
                return DateTime.UtcNow - new DateTime(Math.Max(LastDataDate.Ticks, QueryDate.Ticks));
            }
        }


        public List<Tick> Ticks { get; set; }
        public List<TimedBar> Bars { get; set; }

        #endregion

        #region Serialization

        public string CacheDir
        {
            get
            {
                return Path.Combine(CacheRoot, Feed.Template.BrokerName);
            }
        }

        public const bool KeepOldData = false;

        #region Save

        public static async Task SaveCacheFile(DataLoadResult result)
        {
            var cacheFile = new HistoricalDataCacheFile(result.MarketSeriesBase, result.StartDate, result.EndDate, result.QueryDate)
            {
                Bars = result.Bars,
                Ticks = result.Ticks,
            };

            cache.AddOrUpdate(cacheFile.Key, cacheFile, (k, f) => cacheFile);

            await cacheFile.Save().ConfigureAwait(false);
        }

        public async Task Save()
        {
            await Task.Factory.StartNew(() =>
            {
                // TODO: end time calculation
                // - only respect EndTime if current time is after end date by a certain amount:  
                // t1: 2min
                // m1: Last expected + 2,
                // h1: Last expected + 2h

                string path;
                if (IsPartial) path = PartFilePath;
                else if ((Bars != null && Bars.Count > 0) || (Ticks != null && Ticks.Count > 0)) path = FilePath;
                else path = NoDataFilePath;

                if (!Directory.Exists(System.IO.Path.GetDirectoryName(path))) { Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)); }

                if (File.Exists(path))
                {
                    if (!KeepOldData)
                    {
                        try
                        {
                            File.Delete(path);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Failed to delete old file: " + path + " with exception: " + ex);
                        }
                    }

                    if (File.Exists(path))
                    {

                        int i = 1;
                        do
                        {
                            var suffix = ".dat";
                            var oldSuffix = ".old";

                            var oldDatSuffix = oldSuffix + suffix;
                            var bakPath = path.TryRemoveFromEnd(suffix) + "-" + i++ + oldDatSuffix;
                            if (!File.Exists(bakPath))
                            {
                                File.Move(path, bakPath);
                                break;
                            }
                        }
                        while (true);
                    }
                }

                int versionNumber = 1;
                using (var sw = new BinaryWriter(new FileStream(path, FileMode.Create)))
                {
                    //var json = JsonConvert.SerializeObject(Header);
                    //sw.Write(json);
                    sw.Write(versionNumber);
                    sw.Write(Header.StartDate.ToBinary());
                    sw.Write(Header.EndDate.ToBinary());
                    sw.Write(Header.QueryDate.ToBinary());

                    if (TimeFrame == TimeFrame.t1)
                    {
                        if (Ticks != null)
                        {
                            foreach (var tick in Ticks)
                            {
                                sw.Write(tick.Time.ToBinary());
                                sw.Write(tick.Bid);
                                sw.Write(tick.Ask);
                            }
                        }
                        else
                        {
                            var series = MarketTickSeries;
                            int startIndex = series.FindIndex(StartDate);
                            int endIndex = series.FindIndex(EndDate);
                            for (int i = startIndex; i <= endIndex; i++)
                            {
                                var tick = series[i];
                                if (tick.Time < StartDate || tick.Time > EndDate) continue;
                                sw.Write(tick.Time.ToBinary());
                                sw.Write(tick.Bid);
                                sw.Write(tick.Ask);
                            }
                        }
                    }
                    else
                    {
                        if (Bars != null)
                        {
                            foreach (var bar in Bars)
                            {
                                sw.Write(bar.OpenTime.ToBinary());
                                sw.Write(bar.Open);
                                sw.Write(bar.High);
                                sw.Write(bar.Low);
                                sw.Write(bar.Close);
                                sw.Write(bar.Volume);
                            }
                        }
                        else
                        {
                            var series = MarketSeries;
                            int startIndex = series.FindIndex(StartDate);
                            int endIndex = series.FindIndex(EndDate);
                            for (int i = startIndex; i <= endIndex; i++)
                            {
                                var bar = series[i];
                                if (bar.OpenTime < StartDate || bar.OpenTime > EndDate) continue;
                                sw.Write(bar.OpenTime.ToBinary());
                                sw.Write(bar.Open);
                                sw.Write(bar.High);
                                sw.Write(bar.Low);
                                sw.Write(bar.Close);
                                sw.Write(bar.Volume);
                            }
                        }
                    }
                    Debug.WriteLine($"[{MarketSeriesBase} - cache saved]  - Info: {this.ToString()}");
                }

                if (!KeepOldData && !IsPartial)
                {
                    try
                    {
                        var partialPath = PartFilePath;
                        if (File.Exists(partialPath))
                        {
                            File.Delete(partialPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                }
                if (!KeepOldData && HasData)
                {
                    try
                    {
                        var noDataPath = NoDataFilePath;
                        if (File.Exists(noDataPath))
                        {
                            File.Delete(noDataPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                }

                IsPersisted = true;
            }).ConfigureAwait(false);
        }

        #endregion

        #region Load

        public bool EnforceFileMinimumSizes = true; // TEMP
        public int MinFileSize = 3000;
        public int MinPartialFileSize = 3000;

        public async Task<bool> Load(bool loadOnlyHeader = false)
        {
            string path = null;
            try
            {
                again:
                if (File.Exists(FilePath))
                {
                    if (EnforceFileMinimumSizes && new FileInfo(FilePath).Length < MinFileSize) { File.Delete(FilePath); goto again; }
                    IsPartial = false;
                    IsAvailable = true;
                    EmptyData = false;
                    path = FilePath;
                }
                else if (File.Exists(PartFilePath))
                {
                    if (EnforceFileMinimumSizes && new FileInfo(PartFilePath).Length < MinPartialFileSize) { File.Delete(PartFilePath); goto again; }
                    IsPartial = true;
                    IsAvailable = true;
                    EmptyData = false;
                    path = PartFilePath;
                }
                else if (File.Exists(NoDataFilePath))
                {
                    IsPartial = false;
                    IsAvailable = false;
                    EmptyData = true;
                    path = NoDataFilePath;
                }
                else
                {
                    IsAvailable = false;
                    return false;
                }

                // OPTIMIZE: Albeit, it might be considered dangerous) Instead of parsing EACH Int32, you could do them all at once using Buffer.BlockCopy()
                //http://stackoverflow.com/questions/17043631/using-stream-read-vs-binaryreader-read-to-process-binary-streams


                await new Func<Task<bool>>(() =>
                {
#if SanityChecks
                    if (!File.Exists(path)) throw new Exception("Unexpected: path doesn't exist: " + path);
#endif
                    //await Task.Run(() =>
                    //{
                    using (var br = new BinaryReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
                    {
                        try
                        {
                            //var deserializedHeader = JsonConvert.DeserializeObject<HistoricalDataCacheFileHeader>(br.ReadString());
                            //if (deserializedHeader.StartDate != StartDate)
                            //{
                            //    throw new Exception("deserializedHeader.StartDate != StartDate");
                            //}
                            //if (deserializedHeader.EndDate != EndDate)
                            //{
                            //    throw new Exception("deserializedHeader.EndDate != EndDate");
                            //}
                            //this.Header = deserializedHeader;// Overwrites local properties

                            int version = br.ReadInt32();
                            if (version != 1)
                            {
                                int zeroCount = 1;
                                if (version == 0)
                                {
                                    try
                                    {
                                        while (br.ReadInt32() == 0 && zeroCount < 10) { zeroCount++; }
                                    }
                                    catch { }
                                    if (zeroCount > 5)
                                    {
                                        // Assume file is corrupt and delete it.
                                        throw new DataCorruptException();
                                    }
                                }
                                throw new InvalidDataException("Cache file data versions supported: 1");
                            }

                            this.StartDate = DateTime.FromBinary(br.ReadInt64());
                            this.EndDate = DateTime.FromBinary(br.ReadInt64());
                            this.QueryDate = DateTime.FromBinary(br.ReadInt64());

                            if (!loadOnlyHeader)
                            {
                                if (TimeFrame == TimeFrame.t1)
                                {
                                    var ticks = new List<Tick>();
                                    while (br.BaseStream.Position < br.BaseStream.Length)
                                    {
                                        ticks.Add(new Tick(
                                        DateTime.FromBinary(br.ReadInt64())
                                        , br.ReadDouble()
                                        , br.ReadDouble()
                                        ));
                                    }
                                    this.Ticks = ticks;
                                }
                                else
                                {
                                    var bars = new List<TimedBar>();
                                    while (br.BaseStream.Position < br.BaseStream.Length)
                                    {
                                        bars.Add(new TimedBar(
                                        DateTime.FromBinary(br.ReadInt64())
                                        , br.ReadDouble()
                                        , br.ReadDouble()
                                        , br.ReadDouble()
                                        , br.ReadDouble()
                                        , br.ReadDouble()
                                        ));
                                    }
                                    this.Bars = bars;
                                }
                            }
                            IsPersisted = true;
                        }
                        catch (IOException ioe)
                        {
                            if (ioe.Message.Contains("end of stream")) throw new DataCorruptException("End of stream", ioe);
                        }
                    }

                    //Debug.WriteLine($"[{MarketSeriesBase} - cache loaded] Loaded {StartDate} - {EndDate} @ {QueryDate}, {Count} items");
                    //Debug.WriteLine($"[{MarketSeriesBase} - cache loaded]  - Info: {this.ToString()}");
                    //}
                    //).ConfigureAwait(false);
                    IsPersisted = true;

                    return Task.FromResult(IsPersisted);
                }).AutoRetry(allowException: e => e.GetType() != typeof(DataCorruptException)); // TOCONFIGUREAWAIT ?
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error reading cache file: " + ex);
                IsAvailable = false;
                if (DeleteCacheOnLoadError && path != null)
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch
                    {
                        Debug.WriteLine($"[data corrupt] Failed to delete corrupt cache file '{path}': " + ex); // TOLOG
                    }
                }
                IsPersisted = false;
                return IsPersisted;
            }
            return IsPersisted;
        }
        public static bool DeleteCacheOnLoadError = true; // TOCONFIG

        public int Count
        {
            get
            {
                if (Bars != null) { return Bars.Count; }
                if (Ticks != null) { return Ticks.Count; }
                return 0;
            }
        }
        #endregion

        #endregion

        //public TimeSpan CacheFileSize(TimeFrame tf)
        //{
        //    switch (tf.Name)
        //    {
        //        case "t1":
        //            return TimeSpan.FromHours(1);
        //        case "m1":
        //            return TimeSpan.FromDays(1);
        //        case "m5":
        //            return TimeSpan.FromDays(31); // Calendar month
        //        case "h1":
        //            return TimeSpan.FromDays(365); // Calendar year 
        //        default:
        //            throw new NotImplementedException();
        //    }
        //}


        public override string ToString()
        {
            return $"{{cache file {this.MarketSeriesBase} Start:{StartDate} End:{EndDate} Query:{QueryDate} IsAvailable:{IsAvailable} IsPartial:{IsPartial}}}";
        }
    }

}
