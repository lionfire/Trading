using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Data
{

    public class HistoricalDataCacheFileHeader
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class HistoricalDataCacheFile
    {
        #region Configuration

        public string CacheRoot = @"d:\st\Investing-Data\MarketData"; // HARDPATH

        #endregion

        #region Identity

        public static string GetKey(string brokerName, string symbolCode, TimeFrame tf, DateTime date, string brokerSubType = null)
        {
            var subType = brokerSubType == null ? "" : $" ({brokerSubType})";

            string dateStr;

            switch (tf.Name)
            {
                case "t1":
                    dateStr = $"{date.Year}\\{date.Month}\\{date.Day}\\{date.Hour}";
                    break;
                case "m1":
                    dateStr = $"{date.Year}\\{date.Month}\\{date.Day}";
                    break;
                case "h1":
                    dateStr = $"{date.Year}";
                    break;
                default:
                    throw new NotImplementedException();
            }

            return $"{brokerName}{subType}\\{symbolCode}\\{tf.Name}\\{dateStr}";
        }

        public string SymbolCode { get; set; }
        public TimeFrame TimeFrame { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string BrokerSubType { get; set; }

        public string Key
        {
            get
            {
                return GetKey(Account?.Template?.BrokerName, SymbolCode, TimeFrame, StartDate, BrokerSubType);
            }
        }

        #endregion

        #region Construction

        public HistoricalDataCacheFile(IAccount account, string symbol, TimeFrame timeFrame, DateTime startDate)
        {
            this.Account = account;
            this.SymbolCode = symbol;
            this.TimeFrame = timeFrame;
            this.StartDate = startDate;
        }

        public static void GetChunkRange(TimeFrame tf, DateTime date, out DateTime chunkStartDate, out DateTime chunkEndDate)
        {

            switch (tf.Name)
            {
                case "t1":
                    chunkStartDate = new DateTime(date.Year, date.Month, date.Day, date.Hour, 0, 0, DateTimeKind.Utc);
                    chunkEndDate = new DateTime(chunkStartDate.Year, chunkStartDate.Month, chunkStartDate.Day, chunkStartDate.Hour, 59, 59, 999, DateTimeKind.Utc);
                    break;
                case "m1":
                    chunkStartDate = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, DateTimeKind.Utc);
                    chunkEndDate = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999, DateTimeKind.Utc);
                    break;
                case "h1":
                    chunkStartDate = new DateTime(date.Year, 0, 0, 0, 0, 0, DateTimeKind.Utc);
                    chunkEndDate = new DateTime(date.Year, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public static HistoricalDataCacheFile GetCacheFile(IAccount account, string symbolCode, TimeFrame tf, DateTime date)
        {
            DateTime chunkStartDate, chunkEndDate;
            GetChunkRange(tf, date, out chunkStartDate, out chunkEndDate);

            var key = GetKey(account.Template.BrokerName, symbolCode, tf, date) + ".dat";

            HistoricalDataCacheFile cacheFile = cache.GetOrAdd(key, _ =>
                new HistoricalDataCacheFile(account, symbolCode, tf, chunkStartDate)
             );

            cacheFile.EnsureLoaded();
            return cacheFile;
        }

        #endregion

        public async Task EnsureLoaded()
        {
            if (Ticks == null && Bars == null)
            {
                await Load();
            }
        }
        #region Cache

        private static ConcurrentDictionary<string, HistoricalDataCacheFile> cache = new ConcurrentDictionary<string, HistoricalDataCacheFile>();

        public void ClearCache()
        {
            cache.Clear();
        }

        #endregion

        #region Relationships

        public IAccount Account { get; set; }

        #endregion

        #region Serialization

        public HistoricalDataCacheFileHeader Header { get; set; } = new HistoricalDataCacheFileHeader();

        #endregion

        #region Properties

        public string FilePath { get; private set; }
        public string PartFilePath { get { return FilePath.Replace(".dat", ".part.dat"); } }

        public List<Tick> Ticks { get; set; }
        public List<TimedBar> Bars { get; set; }


        #endregion

        #region Serialization

        public string CacheDir
        {
            get
            {
                return Path.Combine(CacheRoot, Account.Template.BrokerName);
            }
        }

        public static List<HistoricalDataCacheFile> GetCacheFiles(IAccount account, string symbol, TimeFrame timeFrame, DateTime startTime, DateTime endTime)
        {
            var results = new List<HistoricalDataCacheFile>();

            while (true)
            {
                var file = GetCacheFile(account, symbol, timeFrame, startTime);
            }

        }

        #region Save

        public void Save()
        {
            // TODO: end time calculation
            // - only respect EndTime if current time is after end date by a certain amount:  
            // t1: 2min
            // m1: Last expected + 2,
            // h1: Last expected + 2h

            if (!Directory.Exists(System.IO.Path.GetDirectoryName(FilePath))) { Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FilePath)); }

            if (File.Exists(FilePath))
            {
                do
                {
                    int i = 1;
                    var bakPath = FilePath + "-" + i + ".old.dat";
                    if (!File.Exists(bakPath))
                    {
                        File.Move(FilePath, bakPath);
                        break;
                    }
                }
                while (true);
            }

            using (var sw = new BinaryWriter(new FileStream(FilePath, FileMode.Create)))
            {
                var json = JsonConvert.SerializeObject(Header);
                sw.Write(json);

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
            }
        }

        #endregion

        #region Load

        public bool IsPartial { get; set; }
        public async Task<bool> Load()
        {
            string path;
            if (File.Exists(FilePath))
            {
                IsPartial = false;
                path = FilePath;
            }
            else if (File.Exists(PartFilePath))
            {
                IsPartial = true;
                path = PartFilePath;
            }
            else
            {
                return false;
            }

            // OPTIMIZE: Albeit, it might be considered dangerous) Instead of parsing EACH Int32, you could do them all at once using Buffer.BlockCopy()
            //http://stackoverflow.com/questions/17043631/using-stream-read-vs-binaryreader-read-to-process-binary-streams

            await Task.Factory.StartNew(() =>
            {
                using (var br = new BinaryReader(new FileStream(path, FileMode.Open)))
                {
                    this.Header = JsonConvert.DeserializeObject<HistoricalDataCacheFileHeader>(br.ReadString());

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
                    }
                }
            });

            return true;
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


    }

}
