using LionFire.Orleans_.ObserverGrains;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading.Indicators.Inputs;


public interface IMarketDataResolver
{
    ITimeSeriesSource? TryResolve(object reference);
}

public class MarketDataResolver : IMarketDataResolver
{
    public IServiceProvider ServiceProvider { get; }

    public MarketDataResolver(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public ITimeSeriesSource? TryResolve(object reference)
    {
        // FUTURE ENH: IInputResolver<T> where T is SymbolValueAspect, etc.

        if (reference is SymbolValueAspect sva)
        {
            return ActivatorUtilities.CreateInstance<SymbolValueAspectInput<decimal>>(ServiceProvider, sva);
        }

        return null;
    }
}

