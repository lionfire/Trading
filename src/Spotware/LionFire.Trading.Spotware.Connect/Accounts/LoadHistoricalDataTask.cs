using LionFire.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using LionFire.Trading.Spotware.Connect.AccountApi;

namespace LionFire.Trading.Spotware.Connect
{

    public class LoadHistoricalDataTask : ProgressiveTask
    {

        int maxBarsPerRequest = 500; // TODO MOVE TOCONFIG

        public string Symbol;
        public TimeFrame TimeFrame;

        public DateTime EndTime = DateTime.UtcNow;
        public int MinBars = TradingOptions.DefaultHistoricalDataBarsDefault;

        public string AccountId { get; set; }
        public string AccessToken { get; set; }

        #region Derived

        string requestTimeFrame;
        DateTime startTime;

        #endregion

        #region Construction

        public LoadHistoricalDataTask() { }
        public LoadHistoricalDataTask(string symbol, TimeFrame timeFrame) { this.Symbol = symbol; this.TimeFrame = timeFrame; }

        #endregion

        public async Task Run()
        {
            await Execute();
        }

        // ENH: max request size, and progress reporting
        private async Task Execute()
        {
            if (MinBars == 0)
            {
                UpdateProgress(0.1, "Done.  (No action since MinBars == 0)");
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

            if (requestTimeFrame == "h1")
            {
                startTime = EndTime - TimeSpan.FromHours(requestBars);
                daysPageSize = Math.Max(1, maxBarsPerRequest / 24);
            }
            else
            {
                startTime = EndTime - TimeSpan.FromMinutes(requestBars);
                daysPageSize = Math.Max(1, maxBarsPerRequest / (24 * 60));
            }

            #endregion

            var timeSpan = EndTime - startTime;
            if (timeSpan.TotalDays < 0)
            {
                throw new ArgumentException("timespan is negative");
            }

            if (timeSpan.TotalDays > daysPageSize)
            {
                Console.WriteLine("WARNING TODO: download historical trendbars - timeSpan.TotalDays > daysPageSize.  TimeSpan: " + timeSpan);
            }

            //var prefix = "{ \"data\":[";

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

            UpdateProgress(0.11, "Sending request");
            var response = await client.GetAsync(uri);

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

            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<SpotwareTrendbarsResult>(json);

            UpdateProgress(0.98, "Processing data");

            Result = new List<TimedBarStruct>();
            foreach (var b in data.data)
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
            UpdateProgress(1, "Done");
        }

        public List<TimedBarStruct> Result { get; set; }


    }


}
