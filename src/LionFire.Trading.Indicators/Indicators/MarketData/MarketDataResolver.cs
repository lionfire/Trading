using LionFire.Orleans_.ObserverGrains;
using LionFire.Trading.Data;
using Microsoft.Extensions.DependencyInjection;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Execution;
namespace LionFire.Trading.Indicators.Inputs;


public interface IMarketDataResolver
{
    IHistoricalTimeSeries Resolve(object reference)
    {
        var result = TryResolve(reference);
        if (result == null) throw new ArgumentException("Failed to resolve: " + reference);
        return result;
    }
    IHistoricalTimeSeries? TryResolve(object reference);
}

public class MarketDataResolver : IMarketDataResolver
{
    public IServiceProvider ServiceProvider { get; }

    public MarketDataResolver(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IHistoricalTimeSeries? TryResolve(object reference)
    {
        // FUTURE ENH: IInputResolver<T> where T is SymbolValueAspect, etc.

        if (reference is SymbolValueAspect sva)
        {
            ExchangeSymbolTimeFrame exchangeSymbolTimeFrame = sva;
            DataPointAspect aspect = sva.Aspect;
            return ActivatorUtilities.CreateInstance<BarAspectSeries<decimal>>(ServiceProvider, exchangeSymbolTimeFrame, aspect);
        }
        else if(reference is ExchangeSymbolTimeFrame exchangeSymbolTimeFrame)
        {
            return ActivatorUtilities.CreateInstance<BarSeries>(ServiceProvider, exchangeSymbolTimeFrame);
        }

        return null;
    }
}

