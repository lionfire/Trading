using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Spotware.Connect.AccountApi
{
    public static class SpotwareAccountApi
    {
        public static string SandboxUriRoot { get; } = "https://sandbox-api.spotware.com/connect/";
        public static string UriRoot { get; } = "https://api.spotware.com/connect/";

        public static string GetRoot(bool isSandbox) { return isSandbox ? SandboxUriRoot : UriRoot; }

        // requestTimeFrame: h1 or m1
        public const string TrendBarsUri = @"/connect/tradingaccounts/{id}/symbols/{symbolName}/trendbars/{requestTimeFrame}?oauth_token={oauth_token}&from={from}&to={to}";

        public static string ToSpotwareUriParameter(this DateTime time)
        {
            return time.ToString("yyyyMMddhhmmss");
        }

        public static DateTime ToDateTime(this long timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc) + TimeSpan.FromMilliseconds(timestamp);
        }
    }
}
