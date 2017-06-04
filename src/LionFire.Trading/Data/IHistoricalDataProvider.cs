using LionFire.Extensions.Logging;
using LionFire.Logging.Null;
using LionFire.Threading.Tasks;
using LionFire.Trading.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IHistoricalDataProvider
    {
        IAccount Account { get; }
        Task<DataLoadResult> RetrieveDataForChunk(MarketSeriesBase marketSeries, DateTime date, bool cacheOnly = false, bool writeCache = true, TimeSpan? maxOutOfDate = null, CancellationToken? cancellationToken = null);
    }

    public static class IHistoricalDataProviderExtensions
    {
        public static async Task<IEnumerable<DataLoadResult>> GetData(this IHistoricalDataProvider provider, MarketSeriesBase series, DateTime? startDate, DateTime endDate, bool cacheOnly = false, bool writeCache = true, TimeSpan? maxOutOfDate = null, int totalDesiredBars = 0, bool forceRetrieve = false, bool forceReretrieveEmptyData = false, CancellationToken? cancellationToken = null)
        {
            
            logger.LogTrace($"[GetData] {series} {startDate} - {endDate}");
            await series.DataLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!startDate.HasValue) { startDate = endDate; }
                if (!maxOutOfDate.HasValue)
                {
                    maxOutOfDate = HistoricalDataUtils.GetDefaultMaxOutOfDate(series);
                }

                List<DataLoadResult> results = new List<DataLoadResult>();

                DateTime nextChunkDate;
                if (series.HasAccount && series.Account.IsBacktesting)
                {
                    nextChunkDate = DateTime.FromBinary(Math.Min(series.Account.BacktestEndDate.ToBinary(), series.OpenTime.First().ToBinary()));
                }
                else
                {
                    if (cacheOnly)
                    {
                        nextChunkDate = endDate;
                        if (nextChunkDate == default(DateTime))
                        {
                            nextChunkDate = startDate.Value;
                        }
                    }
                    else
                    {
                        nextChunkDate = DateTime.UtcNow + TimeSpan.FromMinutes(1);
                    }
                }

                DataLoadResult dataLoadResult = null;

                int giveUpAfterNoData = GetGiveUpAfterNoData(series.TimeFrame);
                do
                {
                    if (nextChunkDate == default(DateTime)) break;

                    DateTime chunkStart;
                    DateTime chunkEnd;
                    HistoricalDataCacheFile.GetChunkRange(series.TimeFrame, nextChunkDate, out chunkStart, out chunkEnd);

                    dataLoadResult = await provider.GetDataForChunk(series, chunkStart, chunkEnd, cacheOnly, writeCache, forceReretrieveEmptyData: forceReretrieveEmptyData, cancellationToken: cancellationToken).ConfigureAwait(false);
                    results.Add(dataLoadResult);

                    nextChunkDate = chunkStart - TimeSpan.FromMinutes(1);

                    if (!dataLoadResult.IsAvailable)
                    {
                        giveUpAfterNoData--;
                    }
                    else
                    {
                        giveUpAfterNoData = GetGiveUpAfterNoData(series.TimeFrame);
                    }

                    //Debug.WriteLine($"[data {series}]  {dataLoadResult}" + (cacheOnly?" (CACHE ONLY)":""));
                } while (giveUpAfterNoData > 0 &&
                ((nextChunkDate != default(DateTime) && nextChunkDate > startDate.Value)
                 || series.Count < totalDesiredBars)
                );

                return results;

            }
            finally
            {
                series.DataLock.Release();
            }
        }

        private static int GetGiveUpAfterNoData(string timeFrame)
        {
            int giveUpAfterNoData = -1;
            var tf = TimeFrame.TryParse(timeFrame);
            switch (tf.TimeFrameUnit)
            {
                //case TimeFrameUnit.Unspecified:
                //    break;
                case TimeFrameUnit.Tick:
                    giveUpAfterNoData = 5 * 24; // hours
                    break;
                //case TimeFrameUnit.Second:
                //    break;
                case TimeFrameUnit.Minute:
                    giveUpAfterNoData = 5; // days
                    break;
                case TimeFrameUnit.Hour:
                    giveUpAfterNoData = 1; // Years
                    break;
                //case TimeFrameUnit.Day:
                //    break;
                //case TimeFrameUnit.Week:
                //    break;
                //case TimeFrameUnit.Month:
                //    break;
                case TimeFrameUnit.Year:
                    break;
                default:
                    throw new ArgumentException("Timeframe not supported");
            }

            return giveUpAfterNoData;
        }



        /// <summary>
        /// Load data from cache if available, or else retrieve from source.  
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="series"></param>
        /// <param name="chunkStart"></param>
        /// <param name="chunkEnd"></param>
        /// <param name="cacheOnly"></param>
        /// <param name="writeCache">If true, write any retrieved data to cache</param>
        /// <param name="maxOutOfDate"></param>
        /// <param name="forceRetrieve"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<DataLoadResult> GetDataForChunk(this IHistoricalDataProvider provider, MarketSeriesBase series, DateTime chunkStart, DateTime chunkEnd, bool cacheOnly = false, bool writeCache = true, TimeSpan? maxOutOfDate = null, bool forceRetrieve = false, bool forceReretrieveEmptyData = false, CancellationToken? cancellationToken = null)
        {
            logger.LogTrace($"[GetDataForChunk] {series} {chunkStart}");
            if (!maxOutOfDate.HasValue)
            {
                maxOutOfDate = HistoricalDataUtils.GetDefaultMaxOutOfDate(series);
            }

            if (chunkStart >= series.DataStartDate && chunkEnd <= series.DataEndDate)
            {
                var chunkEndPastLast = chunkEnd - series.OpenTime.LastValue;

                if (chunkEndPastLast <= TimeSpan.Zero)
                {
                    //logger.LogTrace($"[{series}] Already contains data for {chunkStart} - {chunkEnd}");
                    return DataLoadResult.AlreadyLoaded;
                }

                var nowPastLast = DateTime.UtcNow - series.OpenTime.LastValue;

                if (!forceRetrieve && nowPastLast < maxOutOfDate)
                {
                    //logger.LogTrace($"[{series}] Already contains data for {chunkStart} - {chunkEnd} (Only slightly out of date: {nowPastLast})");
                    return DataLoadResult.AlreadyLoaded;
                }
            }

            var cacheFile = await HistoricalDataCacheFile.GetCacheFile(series, chunkStart).ConfigureAwait(false);
            DataLoadResult result = cacheFile.DataLoadResult;

            bool isUpToDateAfterClose = series.CloseTime.HasValue && cacheFile.QueryDate > series.CloseTime.Value;

            if (!forceRetrieve && (!forceReretrieveEmptyData || !cacheFile.EmptyData) && cacheFile.IsPersisted && (isUpToDateAfterClose || cacheFile.OutOfDateTimeSpan < maxOutOfDate.Value)
                //&& (chunkEnd > DateTime.UtcNow || !result.IsPartial)
                )
            {

                //Debug.WriteLine($"[cache available {series}] Chunk {chunkStart}");
                if (!cacheOnly)
                {
                    if (result.Bars == null)
                    {
                        result.Bars = new List<TimedBar>();
                    }
                    ImportData(series, result);

                    //if (series is MarketSeries) // TOC#7 OLD
                    //{
                    //    var s = series as MarketSeries;
                    //    if (s != null) { s.Add(cacheFile.Bars, result.StartDate, result.EndDate); }
                    //}
                    //else if (series is MarketSeries) // TOC#7
                    //{
                    //    var s = series as MarketTickSeries;
                    //    if (s != null) { s.Add(cacheFile.Ticks, result.StartDate, result.EndDate); }
                    //}
                }
            }
            else
            {
                var str = forceRetrieve ? "FORCE RETRIEVE" : (forceReretrieveEmptyData && cacheFile.EmptyData) ? "RERETRIEVE EMPTY DATA" : "CACHE MISS";
                logger.LogInformation($"[{series} {str}] Retrieving chunk {chunkStart} ");
#if DEBUG
                logger.LogTrace($"<<<cache debug>>> cacheFile.IsPersisted {cacheFile.IsPersisted}, isUpToDateAfterClose {isUpToDateAfterClose}, cacheFile.OutOfDateTimeSpan < maxOutOfDate.Value {cacheFile.OutOfDateTimeSpan < maxOutOfDate.Value} ");
#endif
                var loader = new object();
                try
                {
                    series.OnLoadingStarted(loader);
                    result = await provider.RetrieveDataForChunk(series, chunkStart, cacheOnly, writeCache, cancellationToken: cancellationToken).ConfigureAwait(false);
                    if (result.Faulted) throw new Exception("Retrieve data for chunk failed");
                    //if (LinkWithAccountData && Account != null && ResultBars.Count > 0)

                    if (!cacheOnly)
                    {
                        ImportData(series, result);
                    }
                }
                finally
                {
                    series.OnLoadingFinished(loader);
                }
                if (writeCache )
                {
                    if (!result.Faulted)
                    {
                        //UpdateProgress(0.98, "Writing data into cache"); // FUTURE: Async?
                        TaskManager.OnFireAndForget(HistoricalDataCacheFile.SaveCacheFile(result), name: $"Save cache for {series.SymbolCode} from {chunkStart} - {chunkEnd}");
                    }
                }
                else
                {
                    logger.LogTrace("WRITECACHE OFF");
                }

            }
            return result;
        }

        private static void ImportData(MarketSeriesBase series, DataLoadResult result)
        {
            //UpdateProgress(0.97, "Loading data into memory");
            if (series.TimeFrame == "t1")
            {
                var tickSeries = (MarketTickSeries)series;
                tickSeries.Add(result.Ticks, result.StartDate, result.EndDate);
            }
            else
            {
                var barSeries = (MarketSeries)series;
                barSeries.Add(result.Bars, result.StartDate, result.EndDate);
            }
            series.RaiseLoadHistoricalDataCompleted(result.StartDate, result.EndDate);
            //Debug.WriteLine($"[{series} - data imported]  {result} with {result.Count} items: now has {series.Count} items total");
        }

        private static ILogger logger => StaticLogger<IHistoricalDataProvider>.Logger;

    }
    public static class StaticLogger<T>
    {
        public static ILogger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = typeof(T).GetLogger();
                }
                return _logger ?? NullLogger.Instance;
            }
        }
        private static ILogger _logger;
    }
}
