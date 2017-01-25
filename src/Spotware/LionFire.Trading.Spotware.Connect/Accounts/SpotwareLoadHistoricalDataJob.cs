using LionFire.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LionFire.Trading.Spotware.Connect.AccountApi;
using System.Diagnostics;
using System.Threading;
using LionFire.ExtensionMethods;
using LionFire.Instantiating;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using LionFire.Trading.Data;


namespace LionFire.Trading.Spotware.Connect
{

    //public interface ITLoadHistoricalDataJob : ITemplate<ILoadHistoricalDataJob>
    //{
    //}

    public class SpotwareConnectLoadHistoricalDataProvider : IHistoricalDataProvider
    {
        public IAccount Account { get { return account; } }
        private CTraderAccount account;
        public SpotwareConnectLoadHistoricalDataProvider(CTraderAccount account)
        {
            this.account = account;
        }

        public const int MaxConcurrentRequests = 5;
        private static SemaphoreSlim retrieveSemaphore = new SemaphoreSlim(MaxConcurrentRequests, MaxConcurrentRequests);

        public async Task<DataLoadResult> RetrieveDataForChunk(MarketSeriesBase series, DateTime date, bool cacheOnly = false, bool writeCache = true, TimeSpan? maxOutOfDate = null)
        {
            var result = new DataLoadResult(series);

            DateTime chunkStart;
            DateTime chunkEnd;
            HistoricalDataCacheFile.GetChunkRange(series.TimeFrame, date, out chunkStart, out chunkEnd);

            result.QueryDate = DateTime.UtcNow;
            var job = new SpotwareLoadHistoricalDataJob(series)
            {
                StartTime = chunkStart,
                EndTime = chunkEnd,
                WriteCache = writeCache,
                LinkWithAccountData = !cacheOnly,
            };

            try
            {
                await retrieveSemaphore.WaitAsync();
                await job.Run();
            }
            finally
            {
                retrieveSemaphore.Release();
            }

            if (job.ResultCount > 0)
            {
                result.IsAvailable = true;
            }

            result.Bars = job.ResultBars;
            result.Ticks = job.ResultTicks;
            result.StartDate = chunkStart;
            result.EndDate = chunkEnd;

            if (result.QueryDate < chunkEnd)
            {
                result.IsPartial = true;
            }

            return result;
        }
    }

    public class SpotwareLoadHistoricalDataJob : ProgressiveJob, IJob
    {

        #region Parameters

        public bool WriteCache { get; set; } = false;

        public string Symbol;
        public TimeFrame TimeFrame;

        /// <summary>
        /// Will be changed to UTC Now if past now.
        /// </summary>
        public DateTime EndTime = DateTime.UtcNow;
        //public DateTime EffectiveEndTime
        //{
        //    get
        //    {
        //        return GetEffectiveEndTime(EndTime);
        //    }
        //}
        public DateTime? StartTime = null;
        //public int MinBars = TradingOptions.DefaultHistoricalDataBarsDefault;
        public int MinItems = 0;

        public DateTime GetEffectiveEndTime(DateTime endTime)
        {
            var now = DateTime.UtcNow + TimeSpan.FromMinutes(5); // HARDCODE
            if (now < endTime)
            {
                return now;
            }
            return endTime;
        }

        #endregion

        #region Configuration

        /// <summary>
        /// If true and Account is set, data will be loaded from the account, and any missing data will be placed into the Account's data in memory.
        /// </summary>
        public bool LinkWithAccountData { get; set; } = true;

        public CTraderAccount Account { get; set; }

        #region MarketSeriesBase

        public MarketSeriesBase MarketSeriesBase
        {
            get { return marketSeriesBase; }
            set { marketSeriesBase = value; }
        }
        private MarketSeriesBase marketSeriesBase;

        #endregion


        public MarketSeries MarketSeries { get { return MarketSeriesBase as MarketSeries; } }
        public MarketTickSeries MarketTickSeries { get { return MarketSeriesBase as MarketTickSeries; } }

        public string AccountId { get { if (accountId != null) { return accountId; } return Account?.Template.AccountId; } set { this.accountId = value; } }
        private string accountId;

        public string AccessToken { get { if (accessToken != null) { return accessToken; } return Account?.Template.AccessToken; } set { this.accessToken = value; } }
        private string accessToken;

        #endregion

        #region Performance Tweaks

        int maxBarsPerRequest = 4000; // TODO MOVE TOCONFIG  // REVIEW - what is the spotware max? Did I see 5000?

        #endregion

        #region Derived

        string requestTimeFrame;
        DateTime startTime;

        #endregion

        #region Construction

        public SpotwareLoadHistoricalDataJob() { }
        public SpotwareLoadHistoricalDataJob(MarketSeriesBase series)
        {
            if (series == null) throw new ArgumentNullException(nameof(series));
            this.MarketSeriesBase = series;
            this.Symbol = series.SymbolCode;
            this.TimeFrame = series.TimeFrame;
            this.Account = (CTraderAccount)series.Account;
        }
        //public SpotwareLoadHistoricalDataJob(string symbol, TimeFrame timeFrame) { this.Symbol = symbol; this.TimeFrame = timeFrame; }

        #endregion

        #region Equality

        public override int GetHashCode()
        {
            int hash = 0;
            if (Symbol != null) { hash ^= Symbol.GetHashCode(); }
            if (TimeFrame != null) { hash ^= TimeFrame.Name.GetHashCode(); }
            hash ^= EndTime.GetHashCode();
            if (StartTime.HasValue) hash ^= StartTime.Value.GetHashCode();
            hash ^= MinItems.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SpotwareLoadHistoricalDataJob;
            if (other == null) return false;

            return Symbol == other.Symbol && TimeFrame?.Name == other.TimeFrame?.Name && EndTime == other.EndTime && StartTime == other.StartTime && MinItems == other.MinItems;
        }

        #endregion

        public override Task Start()
        {
            if (state.Value == ExecutionState.Ready)
            {
                state.OnNext(ExecutionState.Started);
                RunTask = Execute();
                RunTask.ContinueWith(_ => state.OnNext(ExecutionState.Finished));
            }
            return Task.CompletedTask;
        }

        public async Task Run()
        {
            await Start();
            await RunTask;
        }

        public static int MaxTryAgainCount { get { return 4 * (24 / TryAgainRewindHours); } } // 4 days
        public static int TryAgainRewindHours = 8;

        //private void J(ref DateTime startDate, ref DateTime endDate)
        //{
        //    var cacheFiles = HistoricalDataCacheFile.GetCacheFiles(Account, Symbol, TimeFrame, this.startTime, this.EndTime);
        //}

        private async Task Execute()
        {
            await _Execute();
        }

        public static class MarketDateUtils
        {
            public static int MarketOpenHour = 21;
            public static int MarketCloseHour = 22;

            public static int GetMarketHourDays(DateTime startDate, DateTime? endDate)
            {
                int result = 0;
                if (!endDate.HasValue) endDate = startDate;

                for (DateTime date = startDate; date <= endDate; date += TimeSpan.FromDays(1))
                {
                    if (date.DayOfWeek == DayOfWeek.Saturday) continue;

                    if (date.Month == 12)
                    {
                        if (date.Day == 25 || date.Day == 31) continue;
                    }
                    if (date.Month == 1)
                    {
                        if (date.Day == 1 || date.Day == 2) continue;
                    }
                    result++;
                }
                return result;
            }
        }


        // ENH: max request size, and progress reporting
        private async Task _Execute()
        {
            //MarketSeriesBase = MarketSeries ?? (MarketSeriesBase)MarketTickSeries;
            bool useTicks = TimeFrame.Name == "t1";
            if (useTicks)
            {
                ResultTicks = new List<Tick>();
            }
            else
            {
                ResultBars = new List<TimedBar>();
            }

            if (
                MinItems == 0 &&
                !StartTime.HasValue)
            {
                UpdateProgress(0.1, "Done.  (No action since MinBars == 0 && !StartTime.HasValue)");

                return;
            }
            UpdateProgress(0.1, "Starting");

            var apiInfo = Defaults.Get<ISpotwareConnectAppInfo>();

            var client = SpotwareAccountApi.NewHttpClient();

            #region Calculate req

            double multiplier;

            if (TimeFrame.TimeSpan.TotalHours % 1.0 == 0.0)
            {
                requestTimeFrame = "h1";
                multiplier = TimeFrame.TimeSpan.TotalHours;

            }
            else if (TimeFrame.TimeFrameUnit == TimeFrameUnit.Tick)
            {
                requestTimeFrame = "m1";
                multiplier = TimeFrame.TimeSpan.TotalMinutes / 2.0; // Estimation
            }
            else
            {
                requestTimeFrame = "m1";
                multiplier = TimeFrame.TimeSpan.TotalMinutes;
            }

            int daysPageSize;

            int requestBars = (int)(MinItems * multiplier);

            //var prefix = "{ \"data\":[";

#if AllowRewind

            bool rewind = false;
            rewind = DateUtils.IsMarketDate(StartTime, EndTime);
            // TODO: Subtract days
            //int MarketOpenHour = 21; // 
            //int MarketCloseHour = 22; // 
            //switch (EndTime.DayOfWeek)
            //{
            //    case DayOfWeek.Friday:
            //        break;
            //    case DayOfWeek.Saturday:
            //        EndTime = EndTime - TimeSpan.FromDays(1);
            //        rewind = true;
            //        break;
            //    case DayOfWeek.Sunday:
            //        if (EndTime.Hour < MarketOpenHour)
            //        {
            //            EndTime = EndTime - TimeSpan.FromDays(2);
            //            rewind = true;
            //        }
            //        break;
            //    default:
            //        break;
            //}
            if (rewind)
            {
                EndTime = new DateTime(EndTime.Year, EndTime.Month, EndTime.Day, MarketCloseHour, 0, 0);
            }
#endif

            if (StartTime.HasValue)
            {
                startTime = StartTime.Value;
                // TODO: maxBarsPerRequest?
            }
            else if (requestBars > 0)
            {
                if (requestTimeFrame == "h1")
                {
                    startTime = EndTime - TimeSpan.FromHours(requestBars);
                }
                else
                {
                    startTime = EndTime - TimeSpan.FromMinutes(requestBars);
                }
            }
            else
            {
                throw new Exception("!StartTime.HasValue && requestBars <= 0");
            }

            if (requestTimeFrame == "h1")
            {
                daysPageSize = Math.Max(1, maxBarsPerRequest / 24);
            }
            else
            {
                daysPageSize = Math.Max(1, maxBarsPerRequest / (24 * 60));
            }

            #endregion

            var InitialEndTime = EndTime;
            var endTimeIterator = EndTime;

            var timeSpan = endTimeIterator - startTime;
            if (timeSpan.TotalDays < 0)
            {
                throw new ArgumentException("timespan is negative");
            }

            if (timeSpan.TotalDays > daysPageSize)
            {
                Debug.WriteLine("WARNING TODO: download historical trendbars - timeSpan.TotalDays > daysPageSize.  TimeSpan: " + timeSpan);
            }

            var downloadedTickSets = new Stack<List<Tick>>();
            var downloadedBarSets = new Stack<SpotwareTrendbar[]>();

            int totalItemsDownloaded = 0;
            int tryAgainCount = 0;
            tryagain:

            if (startTime == default(DateTime))
            {
                throw new ArgumentException("startTime == default(DateTime)");
            }

            string from, to, date;
            if (useTicks)
            {
                from = startTime.ToSpotwareTimeUriParameter();
                to = GetEffectiveEndTime(endTimeIterator).ToSpotwareTimeUriParameter();
                date = startTime.Date.ToString("yyyyMMdd");

                if (startTime.Date != EndTime.Date)
                {
                    throw new NotSupportedException("startTime.Date != EndTime.Date for ticks");
                }
            }
            else
            {
                from = startTime.ToSpotwareUriParameter();
                to = GetEffectiveEndTime(endTimeIterator).ToSpotwareUriParameter();
                date = "";
                //bidOrAsk = "";
            }


            var uri = useTicks ? SpotwareAccountApi.TicksUri : SpotwareAccountApi.TrendBarsUri;
            uri = uri
                .Replace("{symbolName}", Symbol)
                .Replace("{requestTimeFrame}", requestTimeFrame)
                .Replace("{id}", AccountId.ToString())
                .Replace("{oauth_token}", System.Uri.EscapeDataString(AccessToken))
                .Replace("{from}", from)
                .Replace("{to}", to)
                .Replace("{date}", date)
                ;

            // Read from stream: see http://stackoverflow.com/questions/26601594/what-is-the-correct-way-to-use-json-net-to-parse-stream-of-json-objects

            DateTime queryDate = DateTime.UtcNow;
            UpdateProgress(0.11, $"Sending request: {from}-{to}");

            var response1 = await client.GetAsyncWithRetries(uri.Replace("{bidOrAsk}", "bid"), retryDelayMilliseconds: 10000);
            HttpResponseMessage response2 = null;
            if (useTicks)
            {
                response2 = await client.GetAsyncWithRetries(uri.Replace("{bidOrAsk}", "ask"), retryDelayMilliseconds: 10000);
            }

            UpdateProgress(0.12, "Receiving response");
            string json = null;
            {
                var receiveStream = await response1.Content.ReadAsStreamAsync();
                System.IO.StreamReader readStream = new System.IO.StreamReader(receiveStream, System.Text.Encoding.UTF8);
                json = readStream.ReadToEnd();
            }

            string askJson = null;
            if (useTicks)
            {
                var receiveStream = await response2.Content.ReadAsStreamAsync();
                System.IO.StreamReader readStream = new System.IO.StreamReader(receiveStream, System.Text.Encoding.UTF8);
                askJson = readStream.ReadToEnd();
            }


            UpdateProgress(0.95, "Deserializing");
            var error = Newtonsoft.Json.JsonConvert.DeserializeObject<SpotwareErrorContainer>(json);
            if (error?.error != null)
            {
                throw new Exception($"API returned error: {error.error.errorCode} - '{error.error.description}'");
            }
            if (String.IsNullOrWhiteSpace(json))
            {
                throw new Exception($"API returned empty response.  StatusCode:  {response1.StatusCode}");
            }

            //ISpotwareItemsResult data2;


            if (useTicks)
            {
                var bidData = Newtonsoft.Json.JsonConvert.DeserializeObject<SpotwareTicksResult>(json);
                var askData = Newtonsoft.Json.JsonConvert.DeserializeObject<SpotwareTicksResult>(askJson);

                if (bidData.data == null)
                {
                    throw new Exception($"API returned no bid data.  StatusCode:  {response1.StatusCode}");
                }
                if (askData.data == null)
                {
                    throw new Exception($"API returned no ask data.  StatusCode:  {response2.StatusCode}");
                }
                if (bidData.data.Length >= 4999)
                {
                    throw new Exception("probably didn't load all from Spotware.  Max: 5000.  TODO: repeat request, asking for next set of ticks.");
                }
                if (askData.data.Length >= 4999)
                {
                    throw new Exception("probably didn't load all from Spotware.  Max: 5000.  TODO: repeat request, asking for next set of ticks.");
                }

                var ticks = AggregateTicks(bidData, askData);
                downloadedTickSets.Push(ticks);
                totalItemsDownloaded += ticks.Count;
            }
            else
            {
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<SpotwareTrendbarsResult>(json);
                if (data.data == null)
                {
                    throw new Exception($"API returned no data.  StatusCode:  {response1.StatusCode}");
                }

                downloadedBarSets.Push(data.data);
                totalItemsDownloaded += data.data.Length;
            }

            if (MinItems > 0 && totalItemsDownloaded < MinItems)
            {
                //int lessThanExpectedAmount = MinItems - data2.Count; // OLD
                int lessThanExpectedAmount = MinItems - totalItemsDownloaded;

                if (tryAgainCount > MaxTryAgainCount)
                {
                    throw new Exception($"Didn't get the requested {MinItems} minimum bars.  Tried rewinding to {endTimeIterator}.  If this rewind is not enough, increase MaxTryAgainCount.");
                }
                switch (requestTimeFrame)
                {
                    case "t1":
                        {
                            var amount = TimeSpan.FromMinutes(Math.Min(TryAgainRewindHours * 60 * 60, lessThanExpectedAmount));
                            endTimeIterator = startTime - TimeSpan.FromMilliseconds(1); // REVIEW: can two ticks happen at the same millisecond and would the data supplier only give me one?
                            startTime -= amount;
                            break;
                        }
                    case "m1":
                        {
                            var amount = TimeSpan.FromMinutes(Math.Min(TryAgainRewindHours * 60, lessThanExpectedAmount));
                            endTimeIterator = startTime - TimeSpan.FromMinutes(1);
                            startTime -= amount;
                            break;
                        }
                    case "h1":
                        {
                            var amount = TimeSpan.FromHours(Math.Min(TryAgainRewindHours, lessThanExpectedAmount));
                            endTimeIterator = startTime - TimeSpan.FromHours(1);
                            startTime -= amount;
                            break;
                        }
                    default:
                        throw new NotImplementedException();
                }
                tryAgainCount++;
                goto tryagain;
            }

            UpdateProgress(0.96, "Processing data");

            DateTime firstDownloaded = new DateTime(9999, 12, 31, 23, 59, 59, 999, DateTimeKind.Utc);
            DateTime lastDownloaded = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

            if (useTicks)
            {
                var sets = downloadedTickSets;
                while (sets.Count > 0)
                {
                    var set = sets.Pop();

                    if (set.Count > 0)
                    {
                        Debug.WriteLine($"[data] {Symbol} {TimeFrame.Name} Loading {set.Count} bars {set[0].Time.ToString(DateFormat)} to {set[set.Count - 1].Time.ToString(DateFormat)}");

                        firstDownloaded = firstDownloaded.Min(set[0].Time);
                        lastDownloaded = lastDownloaded.Max(set[set.Count - 1].Time);

                        ResultTicks.AddRange(set);
                    }
                }
            }
            else
            {
                var sets = downloadedBarSets;

                while (sets.Count > 0)
                {
                    var set = sets.Pop();

                    // TOSANITYCHECK: verify contiguous
                    if (set.Length > 0)
                    {
                        //Debug.WriteLine($"[{Symbol}-{TimeFrame.Name} Loading bars {set[0].timestamp.ToDateTime().ToString(DateFormat)} to {set[set.Length - 1].timestamp.ToDateTime().ToString(DateFormat)}");

                        firstDownloaded = firstDownloaded.Min(set[0].timestamp.ToDateTime());
                        lastDownloaded = lastDownloaded.Max(set[set.Length - 1].timestamp.ToDateTime());

                        foreach (var b in set)
                        {
                            ResultBars.Add(new TimedBar()
                            {
                                OpenTime = b.timestamp.ToDateTime(),
                                Open = b.open,
                                High = b.high,
                                Low = b.low,
                                Close = b.close,
                                Volume = b.volume,
                            });
                        }
                    }
                }
            }

            Debug.WriteLine($"[{Symbol}-{TimeFrame.Name} - DOWNLOADED] {firstDownloaded} to {lastDownloaded}");

            UpdateProgress(1, "Done");
        }

        private List<Tick> AggregateTicks(SpotwareTicksResult bidData, SpotwareTicksResult askData)
        {
            List<Tick> ticks = new List<Tick>();
            int bidIndex = bidData.data.Length - 1;
            int askIndex = askData.data.Length - 1;
            for (; true;)
            {
                var bid = bidIndex < 0 ? SpotwareTick.Invalid : bidData.data[bidIndex];
                var ask = askIndex < 0 ? SpotwareTick.Invalid : askData.data[askIndex];

                if (!bid.IsValid && !ask.IsValid) break;

                if (bid.timestamp < ask.timestamp)
                {
                    ticks.Add(new Tick(bid.timestamp.ToDateTime(), bid: bid.tick));
                    bidIndex--;

                }
                else if (bid.timestamp > ask.timestamp)
                {
                    ticks.Add(new Tick(ask.timestamp.ToDateTime(), ask: ask.tick));
                    askIndex--;
                }
                else
                {
                    ticks.Add(new Tick(bid.timestamp.ToDateTime(), bid: bid.tick, ask: ask.tick));
                    bidIndex--;
                    askIndex--;
                }
            }
            return ticks;
        }

        public const string DateFormat = "yyyy-MM-dd HH:mm:ss";
        public List<TimedBar> ResultBars { get; set; }
        public List<Tick> ResultTicks { get; set; }
        public int ResultCount
        {
            get
            {
                if (ResultBars != null) return ResultBars.Count;
                if (ResultTicks != null) return ResultTicks.Count;
                return 0;
            }
        }
        public DateTime LastOpenTime
        {
            get
            {
                if (ResultBars != null) return ResultBars.Last().OpenTime;
                if (ResultTicks != null) return ResultTicks.Last().Time;
                return default(DateTime);
            }
        }

        #region Misc

        public override string ToString()
        {
            return $"LoadHistoricalData({Symbol}, {TimeFrame.Name} {this.startTime} - {this.EndTime}  minbars: {MinItems}  hash: {GetHashCode()})";
        }
        #endregion
    }


    public static class DateTimeExtensions
    {
        public static DateTime Min(this DateTime v1, DateTime v2)
        {
            if (v1.Kind != v2.Kind) throw new ArgumentException("DateTime.Kind must match");
            return new DateTime(Math.Min(v1.Ticks, v2.Ticks), v1.Kind);
        }
        public static DateTime Max(this DateTime v1, DateTime v2)
        {
            if (v1.Kind != v2.Kind) throw new ArgumentException("DateTime.Kind must match");
            return new DateTime(Math.Max(v1.Ticks, v2.Ticks), v1.Kind);
        }
    }
}
