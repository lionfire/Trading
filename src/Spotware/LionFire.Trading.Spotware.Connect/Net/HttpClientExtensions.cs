
using LionFire.Messaging;
using LionFire.Net;
using LionFire.Net.Messages;
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

        public static async Task<HttpResponseMessage> GetAsyncWithRetries(this HttpClient client, string uri, Predicate<HttpResponseMessage> retryCondition = null, int retryDelayMilliseconds = DefaultRetryDelayMilliseconds, int retryCount = DefaultRetryCount, Action<HttpResponseMessage> onFail = null, Func<bool> canContinue = null)
        {
            HttpResponseMessage response = null;
            try
            {
                if (retryCondition == null) { retryCondition = IsTimeError; }

                int failCount = 0;
                for (int retriesRemaining = retryCount; response == null || retriesRemaining > 0 && retryCondition(response); retriesRemaining--)
                {
                    response = await client.GetAsync(uri).ConfigureAwait(false);
                    if (retryCondition(response))
                    {
                        if (onFail != null) { onFail(response); }
                        var msg = GetTimeErrorString(response);

                        if (canContinue != null)
                        {
                            do
                            {
                                if (canContinue != null)
                                {
                                    await Task.Delay(200);
                                }
                                else
                                {
                                    await Task.Delay(retryDelayMilliseconds + RetryIncreaseMilliseconds * failCount);
                                }
                                //Debug.WriteLine($"{response.StatusCode} {msg} - {uri}");
                            }
                            while (canContinue != null && !canContinue());
                        }

                        failCount++;
                    }
                    if (DebugHttpSuccess) Debug.WriteLine($"[http] {response.StatusCode} {uri}");
                }
                return response;
            }
            catch (HttpRequestException hrex)
            {
                new MInternetFailure { Exception = hrex, Url = uri, Message = response }.Publish();
                return response;
            }
        }
    }
}