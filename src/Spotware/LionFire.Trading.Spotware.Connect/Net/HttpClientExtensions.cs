
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

        public const int ProbeCanContinuePredicateMillisecondsDelay = 200;

        public static bool DebugHttpSuccess = true;

        public static async Task<HttpResponseMessage> GetAsyncWithRetries(this HttpClient client, string uri, Predicate<HttpResponseMessage> failureDetector = null, int retryDelayMilliseconds = DefaultRetryDelayMilliseconds, int retryCount = DefaultRetryCount, Action<HttpResponseMessage> onFail = null, Func<bool> canContinue = null, CancellationToken? cancellationToken = null)
        {
            HttpResponseMessage response = null;
            try
            {
                if (failureDetector == null) { failureDetector = IsTimeError; }

                int failCount = 0;
                for (int retriesRemaining = retryCount; (response == null || retriesRemaining > 0 && failureDetector(response)); retriesRemaining--)
                {
                    if ((cancellationToken != null && cancellationToken.Value.IsCancellationRequested)) throw new OperationCanceledException(cancellationToken.Value);
                    response = await client.GetAsync(uri).ConfigureAwait(false);
                    if (failureDetector(response))
                    {
                        onFail?.Invoke(response);

                        //var msg = GetTimeErrorString(response);

                        if (canContinue != null)
                        {
                            do
                            {
                                await Task.Delay(ProbeCanContinuePredicateMillisecondsDelay);
                                //Debug.WriteLine($"{response.StatusCode} {msg} - {uri}");
                            }
                            while (!canContinue());
                        }
                        else
                        {
                            await Task.Delay(retryDelayMilliseconds + (RetryIncreaseMilliseconds * failCount));
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