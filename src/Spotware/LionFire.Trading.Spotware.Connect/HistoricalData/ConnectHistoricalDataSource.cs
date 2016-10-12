using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Util;
using Flurl.Http;

namespace LionFire.Trading.Connect
{
    

    public class ConnectHistoricalDataSource
    {

        public string ConfigPath { get { return @"e:\Trading\Accounts\IC Markets.Demo1.token.json"; } }

        private const string baseUrl = "https://api.spotware.com/connect";
        private const string TradingUrl = baseUrl + "/tradingaccounts";

        public ConnectToken ConnectToken {
            get {
                if (connectToken == null)
                {
                    connectToken = JsonConvert.DeserializeObject<ConnectToken>(File.ReadAllText(ConfigPath));
                }
                return connectToken;
            }
        }
        ConnectToken connectToken;

#region Get historical bars

        public async Task<IEnumerable<TimedBar>> Get(string symbol, string timeFrame, DateTime from, DateTime to)
        {
            if (timeFrame != "m1" && timeFrame != "h1")
            {
                throw new ArgumentException("Only m1 and h1 supported for TimeFrame from this provider.");
            }

            var response = await TradingUrl
                .AppendPathSegment(ConnectToken.AccountId)
                .AppendPathSegment("symbols")
                .AppendPathSegment(symbol)
                .AppendPathSegment("trendbars")
                .AppendPathSegment(timeFrame)
                .SetQueryParam("access_token", ConnectToken.AccessToken)
                .SetQueryParam("from", from.ToString("yyyyMMddHHmmss"))
                .SetQueryParam("to", to.ToString("yyyyMMddHHmmss"))
                .GetJsonAsync<TrendbarsResponse>()
                ;

            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            
            return response.data.Select(tb => new TimedBar
            {
                OpenTime = epoch.AddSeconds(Convert.ToDouble(tb.timestamp) / 1000.0),
                Open = tb.open,
                High = tb.high,
                Low = tb.low,
                Close = tb.close,
                Volume = tb.volume,
            });
        }

private class TrendbarsResponse
    {
        public Trendbar[] data { get; set; }
    }
    private class Trendbar
    {

        public string timestamp { get; set; }
        public double high { get; set; }
        public double low { get; set; }
        public double open { get; set; }
        public double close { get; set; }
        public long volume { get; set; }
    }

#endregion


    }
}
