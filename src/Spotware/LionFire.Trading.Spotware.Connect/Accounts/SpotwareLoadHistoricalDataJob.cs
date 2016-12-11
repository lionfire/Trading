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
using LionFire.Templating;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using LionFire.Trading.Data;

namespace LionFire.Trading.Spotware.Connect
{

    //public interface ITLoadHistoricalDataJob : ITemplate<ILoadHistoricalDataJob>
    //{
    //}

    public class DataLoadResult
    {
        public DateTime StartDate;
        public DateTime EndDate;
    }

    public class SpotwareLoadHistoricalDataJob : ProgressiveJob, IJob
    {

        #region Parameters

        public bool WriteCache { get; set; } = true;

        public string Symbol;
        public TimeFrame TimeFrame;

        public DateTime EndTime = DateTime.UtcNow;
        public DateTime? StartTime = null;
        public int MinBars = TradingOptions.DefaultHistoricalDataBarsDefault;

        #endregion

        #region Configuration

        /// <summary>
        /// If true and Account is set, data will be loaded from the account, and any missing data will be placed into the Account's data in memory.
        /// </summary>
        public bool LinkWithAccountData { get; set; } = true;

        public CTraderAccount Account { get; set; }
        protected MarketSeriesBase MarketSeriesBase { get; private set; }
        public MarketSeries MarketSeries { get; set; }
        public MarketTickSeries MarketTickSeries { get; set; }

        public string AccountId { get { if (accountId != null) { return accountId; } return Account?.Template.AccountId; } set { this.accountId = value; } }
        private string accountId;

        public string AccessToken { get { if (accessToken != null) { return accessToken; } return Account?.Template.AccessToken; } set { this.accessToken = value; } }
        private string accessToken;

        #endregion

        #region Performance Tweaks

        int maxBarsPerRequest = 500; // TODO MOVE TOCONFIG

        #endregion

        #region Derived

        string requestTimeFrame;
        DateTime startTime;

        #endregion

        #region Construction

        public SpotwareLoadHistoricalDataJob() { }
        public SpotwareLoadHistoricalDataJob(string symbol, TimeFrame timeFrame) { this.Symbol = symbol; this.TimeFrame = timeFrame; }

        #endregion

        #region Equality

        public override int GetHashCode()
        {
            int hash = 0;
            if (Symbol != null) { hash ^= Symbol.GetHashCode(); }
            if (TimeFrame != null) { hash ^= TimeFrame.Name.GetHashCode(); }
            hash ^= EndTime.GetHashCode();
            if (StartTime.HasValue) hash ^= StartTime.Value.GetHashCode();
            hash ^= MinBars.GetHashCode();
            return hash;
        }

        public override bool Equals(object obj)
        {
            var other = obj as SpotwareLoadHistoricalDataJob;
            if (other == null) return false;

            return Symbol == other.Symbol && TimeFrame?.Name == other.TimeFrame?.Name && EndTime == other.EndTime && StartTime == other.StartTime && MinBars == other.MinBars;
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

        private async Task<DataLoadResult> LoadDataForChunk(DateTime date, bool cacheOnly = false, bool writeCache = true)
        {
            var result = new DataLoadResult();

            HistoricalDataCacheFile.GetChunkRange(TimeFrame, date, out result.StartDate, out result.EndDate);

            var cacheFile = HistoricalDataCacheFile.GetCacheFile(this.Account, this.Symbol, TimeFrame, date);
            await cacheFile.EnsureLoaded();

            return result;
        }

        // ENH: max request size, and progress reporting
        private async Task _Execute()
        {
            MarketSeriesBase = MarketSeries ?? (MarketSeriesBase)MarketTickSeries;

            if (MinBars == 0 && !StartTime.HasValue)
            {
                UpdateProgress(0.1, "Done.  (No action since MinBars == 0 && !StartTime.HasValue)");
                Result = new List<TimedBarStruct>();
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

            int requestBars = (int)(MinBars * multiplier);

            //var prefix = "{ \"data\":[";

            bool rewind = false;
            int MarketOpenHour = 21; // 
            int MarketCloseHour = 22; // 
            switch (EndTime.DayOfWeek)
            {
                case DayOfWeek.Friday:
                    break;
                case DayOfWeek.Saturday:
                    EndTime = EndTime - TimeSpan.FromDays(1);
                    rewind = true;
                    break;
                case DayOfWeek.Sunday:
                    if (EndTime.Hour < MarketOpenHour)
                    {
                        EndTime = EndTime - TimeSpan.FromDays(2);
                        rewind = true;
                    }
                    break;
                default:
                    break;
            }
            if (rewind)
            {
                EndTime = new DateTime(EndTime.Year, EndTime.Month, EndTime.Day, MarketCloseHour, 0, 0);
            }

            if (StartTime.HasValue)
            {
                startTime = StartTime.Value;
                // TODO: maxBarsPerRequest?
            }
            else
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

            var timeSpan = EndTime - startTime;
            if (timeSpan.TotalDays < 0)
            {
                throw new ArgumentException("timespan is negative");
            }

            if (timeSpan.TotalDays > daysPageSize)
            {
                Console.WriteLine("WARNING TODO: download historical trendbars - timeSpan.TotalDays > daysPageSize.  TimeSpan: " + timeSpan);
            }

            var downloadedBarSets = new Stack<SpotwareTrendbar[]>();
            int totalBarsDownloaded = 0;
            int tryAgainCount = 0;
            tryagain:

            if (startTime == default(DateTime))
            {
                throw new ArgumentException("startTime == default(DateTime)");
            }

            var from = startTime.ToSpotwareUriParameter();
            var to = EndTime.ToSpotwareUriParameter();

            var uri = SpotwareAccountApi.TrendBarsUri;
            uri = uri
                .Replace("{symbolName}", Symbol)
                .Replace("{requestTimeFrame}", requestTimeFrame)
                .Replace("{id}", AccountId.ToString())
                .Replace("{oauth_token}", System.Uri.EscapeDataString(AccessToken))
                .Replace("{from}", from)
                .Replace("{to}", to)
                ;

            // Read from stream: see http://stackoverflow.com/questions/26601594/what-is-the-correct-way-to-use-json-net-to-parse-stream-of-json-objects

            UpdateProgress(0.11, $"Sending request: {from}-{to}");

            var response = await client.GetAsyncWithRetries(uri, retryDelayMilliseconds: 10000);

            UpdateProgress(0.12, "Receiving response");
            var receiveStream = await response.Content.ReadAsStreamAsync();
            System.IO.StreamReader readStream = new System.IO.StreamReader(receiveStream, System.Text.Encoding.UTF8);
            var json = readStream.ReadToEnd();

            UpdateProgress(0.95, "Deserializing");
            var error = Newtonsoft.Json.JsonConvert.DeserializeObject<SpotwareErrorContainer>(json);
            if (error?.error != null)
            {
                throw new Exception($"API returned error: {error.error.errorCode} - '{error.error.description}'");
            }
            if (String.IsNullOrWhiteSpace(json))
            {
                throw new Exception($"API returned empty response.  StatusCode:  {response.StatusCode}");
            }

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<SpotwareTrendbarsResult>(json);

            if (data.data == null)
            {
                throw new Exception($"API returned no data.  StatusCode:  {response.StatusCode}");
            }

            downloadedBarSets.Push(data.data);
            totalBarsDownloaded += data.data.Length;

            if (totalBarsDownloaded < MinBars)
            {
                int lessThanExpectedAmount = MinBars - data.data.Length;

                if (tryAgainCount > MaxTryAgainCount)
                {
                    throw new Exception($"Didn't get the requested {MinBars} minimum bars.  Tried rewinding to {EndTime}.  If this rewind is not enough, increase MaxTryAgainCount.");
                }
                switch (requestTimeFrame)
                {
                    case "m1":
                        {
                            var amount = TimeSpan.FromMinutes(Math.Min(TryAgainRewindHours * 60, lessThanExpectedAmount));
                            EndTime = startTime - TimeSpan.FromMinutes(1);
                            startTime -= amount;
                            break;
                        }
                    case "h1":
                        {
                            var amount = TimeSpan.FromHours(Math.Min(TryAgainRewindHours, lessThanExpectedAmount));
                            EndTime = startTime - TimeSpan.FromHours(1);
                            startTime -= amount;
                            break;
                        }
                    default:
                        throw new NotImplementedException();
                }
                tryAgainCount++;
                goto tryagain;
            }

            UpdateProgress(0.98, "Processing data");

            Result = new List<TimedBarStruct>();
            while (downloadedBarSets.Count > 0)
            {
                var set = downloadedBarSets.Pop();
                //foreach (var set in downloadedBarSets)
                {
                    // TOSANITYCHECK: verify contiguous
                    if (set.Length > 0)
                    {
                        Debug.WriteLine($"[data] {Symbol} {TimeFrame.Name} Loading bars {set[0].timestamp.ToDateTime().ToString(DateFormat)} to {set[set.Length - 1].timestamp.ToDateTime().ToString(DateFormat)}");
                        foreach (var b in set)
                        {
                            Result.Add(new TimedBarStruct()
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

            if (LinkWithAccountData && Account != null && Result.Count > 0)
            {
                UpdateProgress(0.99, "Loading data into memory");
                if (MarketSeries != null)
                {
                    //var series = (MarketSeries)Account.GetMarketSeries(this.Symbol, this.TimeFrame);
                    MarketSeries.Add(Result, startTime, InitialEndTime);
                    MarketSeries.RaiseLoadHistoricalDataCompleted(startTime, EndTime);
                }
            }

            UpdateProgress(1, "Done");
        }

        public const string DateFormat = "yyyy-MM-dd HH:mm:ss";
        public List<TimedBarStruct> Result { get; set; }


        #region Misc

        public override string ToString()
        {
            return $"LoadHistoricalData({Symbol}, {TimeFrame.Name} {this.startTime} - {this.EndTime}  minbars: {MinBars}  hash: {GetHashCode()})";
        }
        #endregion
    }


}
