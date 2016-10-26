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

        public long AccountId { get; set; }
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

            var client = new HttpClient();
            
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.8,en-CA;q=0.6");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch, br");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("Host", "api.spotware.com");
            client.DefaultRequestHeaders.Add("Cookie", "_ga=GA1.2.1217132727.1477434575");
            


            client.BaseAddress = new Uri(SpotwareAccountApi.GetRoot(apiInfo.IsSandbox));

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
                .Replace("{oauth_token}", AccessToken)
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
