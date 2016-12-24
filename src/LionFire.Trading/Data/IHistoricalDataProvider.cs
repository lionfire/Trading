using LionFire.Trading.Data;
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
        Task<DataLoadResult> RetrieveDataForChunk(MarketSeriesBase marketSeries, DateTime date, bool cacheOnly = false, bool writeCache = true, TimeSpan? maxOutOfDate = null);
    }

    public static class IHistoricalDataProviderExtensions
    {
        

        public static async Task<IEnumerable<DataLoadResult>> GetData(this IHistoricalDataProvider provider, MarketSeriesBase series, DateTime? startDate, DateTime endDate, bool cacheOnly = false, bool writeCache = true, TimeSpan? maxOutOfDate = null, int totalDesiredBars = 0, bool forceRetrieve = false)
        {
            await series.DataLock.WaitAsync();
            try
            {
                if (!startDate.HasValue) { startDate = endDate; }
                if (!maxOutOfDate.HasValue)
                {
                    maxOutOfDate = HistoricalDataUtils.GetDefaultMaxOutOfDate(series.TimeFrame);
                }

                List<DataLoadResult> results = new List<DataLoadResult>();

                DateTime nextChunkDate;
                if (series.Account.IsBacktesting)
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

                    dataLoadResult = await provider.GetDataForChunk(series, chunkStart, chunkEnd, cacheOnly, writeCache);
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
                } while ( giveUpAfterNoData > 0 &&
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
            int giveUpAfterNoData;
            if (timeFrame == "h1")
            {
                giveUpAfterNoData = 1; // years
            }
            else if (timeFrame == "m1")
            {
                giveUpAfterNoData = 5; // days
            }
            else if (timeFrame == "t1")
            {
                giveUpAfterNoData = 5*24; // hours
            }
            else
            {
                throw new ArgumentException("Timeframe not supported");
            }
            return giveUpAfterNoData;
        }

        public static async Task<DataLoadResult> GetDataForChunk(this IHistoricalDataProvider provider, MarketSeriesBase series, DateTime chunkStart, DateTime chunkEnd, bool cacheOnly = false, bool writeCache = true, TimeSpan? maxOutOfDate = null, bool forceRetrieve = false)
        {
            if (!maxOutOfDate.HasValue)
            {
                maxOutOfDate = HistoricalDataUtils.GetDefaultMaxOutOfDate(series.TimeFrame);
            }

            if (chunkStart >= series.DataStartDate && chunkEnd <= series.DataEndDate)
            {
                var chunkEndPastLast = chunkEnd - series.OpenTime.LastValue;

                if (chunkEndPastLast <= TimeSpan.Zero)
                {
                    Debug.WriteLine($"[{series}] Already contains data for {chunkStart} - {chunkEnd}");
                    return DataLoadResult.AlreadyLoaded; ;
                }

                var nowPastLast = DateTime.UtcNow - series.OpenTime.LastValue;

                if (!forceRetrieve && nowPastLast < maxOutOfDate)
                {
                    Debug.WriteLine($"[{series}] Already contains data for {chunkStart} - {chunkEnd} (Only slightly out of date: {nowPastLast})");
                    return DataLoadResult.AlreadyLoaded;
                }
            }

            var cacheFile = await HistoricalDataCacheFile.GetCacheFile(series, chunkStart);
            DataLoadResult result = cacheFile.DataLoadResult;

            if (!forceRetrieve && cacheFile.IsPersisted && cacheFile.OutOfDateTimeSpan < maxOutOfDate.Value)
            {
                Debug.WriteLine($"[cache available {series}] Chunk {chunkStart}");
                if (!cacheOnly)
                {
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
#if DEBUG
                var str = forceRetrieve ? "FORCE RETRIEVE" : "CACHE MISS";
                Debug.WriteLine($"[{series} {str}] Chunk {chunkStart}");
#endif
                var loader = new object();
                try
                {
                    series.OnLoadingStarted(loader);
                    result = await provider.RetrieveDataForChunk(series, chunkStart, cacheOnly, writeCache);

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
                if (writeCache)
                {
                    //UpdateProgress(0.98, "Writing data into cache"); // FUTURE: Async?
                    HistoricalDataCacheFile.SaveCacheFile(result);
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
            Debug.WriteLine($"[{series} - data imported]  {result} with {result.Count} items: now has {series.Count} items total");
        }
    }
}
