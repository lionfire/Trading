
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.ExtensionMethods
{
    public static class HttpUtils
    {
        public static bool IsTimeError(HttpResponseMessage response)
        {
            var code = ((int)response.StatusCode);
            return code == 509 || code == 429;
        }
        public static string GetTimeErrorString(HttpResponseMessage response)
        {
            var code = ((int)response.StatusCode);
            switch (code)
            {
                case 509:
                    return "Bandwidth limit exceeded";
                case 429:
                    return "Too many requests";
                default:
                    return "";
            }
        }

        public static int RetryIncreaseMilliseconds = 5000;
        public const int DefaultRetryDelayMilliseconds = 2000;
        public const int DefaultRetryCount = 30;

        public static bool DebugHttpSuccess = true;

        public static async Task<HttpResponseMessage> GetAsyncWithRetries(this HttpClient client, string uri, Predicate<HttpResponseMessage> retryCondition = null, int retryDelayMilliseconds = DefaultRetryDelayMilliseconds, int retryCount = DefaultRetryCount)
        {
            if (retryCondition == null) { retryCondition = IsTimeError; }

            int failCount = 0;
            HttpResponseMessage response = null;
            for (int retriesRemaining = retryCount; response == null || retriesRemaining > 0 && retryCondition(response); retriesRemaining--)
            {
                response = await client.GetAsync(uri);
                if (retryCondition(response))
                {
                    var msg = GetTimeErrorString(response);

                    Debug.WriteLine($"{response.StatusCode} {msg} - {uri}");
                    Thread.Sleep(retryDelayMilliseconds + RetryIncreaseMilliseconds * failCount);
                    failCount++;
                }
                if(DebugHttpSuccess) Debug.WriteLine($"[http] {response.StatusCode} {uri}");
            }
            return response;
        }
    }
}