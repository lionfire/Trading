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
using LionFire.Threading.Tasks;
using LionFire.Execution.Jobs;

namespace LionFire.Trading.Spotware.Connect
{
    
    public class SpotwareDataProvider : IHistoricalDataProvider
    {
        public IAccount Account { get { return account; } }
        private CTraderAccount account;
        public SpotwareDataProvider(CTraderAccount account)
        {
            this.account = account;
        }

        ConcurrentDictionary<string, SpotwareDataJob> jobs = new ConcurrentDictionary<string, SpotwareDataJob>();

        public async Task<DataLoadResult> RetrieveDataForChunk(MarketSeriesBase series, DateTime date, bool cacheOnly = false, bool writeCache = true, TimeSpan? maxOutOfDate = null)
        {
            var result = new DataLoadResult(series);

            DateTime chunkStart;
            DateTime chunkEnd;
            HistoricalDataCacheFile.GetChunkRange(series.TimeFrame, date, out chunkStart, out chunkEnd);

            result.QueryDate = DateTime.UtcNow;
            var key = $"{series.ToString()};{date.ToString()};{cacheOnly};{writeCache};{maxOutOfDate}";
            bool created = false;

            tryagain:
            var job = jobs.GetOrAdd(key, k=> {
                created = true;
                return new SpotwareDataJob(series)
                {
                    StartTime = chunkStart,
                    EndTime = chunkEnd,
                    WriteCache = writeCache,
                    LinkWithAccountData = !cacheOnly,
                };
            });
            if (!created)
            {
                if (DateTime.UtcNow - job.CreateDate > TimeSpan.FromMinutes(1))
                {
                    if (jobs.TryRemove(key, out job)) goto tryagain;
                }
                else
                {
                    await job.Wait().ConfigureAwait(false);
                }
            }


            await job.Start().ConfigureAwait(false);
            await job.Wait().ConfigureAwait(false);


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

}
