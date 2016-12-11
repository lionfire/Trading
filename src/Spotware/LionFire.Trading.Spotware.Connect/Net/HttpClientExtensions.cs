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
            return ((int)response.StatusCode) == 509;
        }

        public static async Task<HttpResponseMessage> GetAsyncWithRetries(this HttpClient client, string uri, Predicate<HttpResponseMessage> retryCondition = null, int retryDelayMilliseconds = 2000, int retryCount = 30)
        {
            if (retryCondition == null) { retryCondition = IsTimeError; }

            int failCount = 0;
            HttpResponseMessage response = null;
            for (int retry509 = retryCount; response == null || retry509 > 0 && retryCondition(response); retry509--)
            {
                response = await client.GetAsync(uri);
                if (retryCondition(response))
                {
                    Debug.WriteLine($"509 {uri}");
                    Thread.Sleep(retryDelayMilliseconds + 5000 * failCount);
                    failCount++;
                }
            }
            return response;
        }
    }
}