#if TODO
using LionFire.Trading.HistoricalData.Retrieval;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.HistoricalData.Indicators;



public interface IIndicators
{
    Task<IValuesResult<T>?> GetShortChunk<T>(string indicatorName, SymbolBarsRange range, bool fallbackToLongChunkSource = true, QueryOptions? options = null);

    Task<IValuesResult<T>?> GetLongChunk<T>(string indicatorName, SymbolBarsRange range, QueryOptions? options = null);
}

public class IndicatorsDataService : IIndicators
{
    public IndicatorsComputationService IndicatorsComputationService { get; }

    public IndicatorsDataService(IndicatorsComputationService indicatorsComputationService)
    {
        IndicatorsComputationService = indicatorsComputationService;
    }

    public Task<IValuesResult<T>?> GetShortChunk<T>(string indicatorName, SymbolBarsRange range, bool fallbackToLongChunk = true, QueryOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public Task<IValuesResult<T>?> GetLongChunk<T>(string indicatorName, SymbolBarsRange range, QueryOptions? options = null)
    {
        throw new NotImplementedException();
    }
}

public class IndicatorsComputationService : IIndicators
{
    public IndicatorsComputationService(IBars bars)
    {

    }

    public Task<IValuesResult<T>?> GetShortChunk<T>(string indicatorName, SymbolBarsRange range, bool fallbackToLongChunk = true, QueryOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public Task<IValuesResult<T>?> GetLongChunk<T>(string indicatorName, SymbolBarsRange range, QueryOptions? options = null)
    {
        throw new NotImplementedException();
    }

}

#endif