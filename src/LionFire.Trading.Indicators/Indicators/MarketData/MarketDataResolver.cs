using LionFire.Orleans_.ObserverGrains;
using LionFire.Trading.Data;
using Microsoft.Extensions.DependencyInjection;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Execution;
using LionFire.Trading.Indicators.Harnesses;
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
    IHistoricalTimeSeries<TValue> Resolve<TValue>(object reference)
    {
        var result = TryResolve<TValue>(reference);
        if (result == null) throw new ArgumentException("Failed to resolve: " + reference);
        return result;
    }
    IHistoricalTimeSeries<TValue>? TryResolve<TValue>(object reference);
}

public class MarketDataResolver : IMarketDataResolver
{
    public IServiceProvider ServiceProvider { get; }

    public MarketDataResolver(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
    }

    public IHistoricalTimeSeries<TValue>? TryResolve<TValue>(object reference)
    {
        return (IHistoricalTimeSeries<TValue>?)TryResolve(reference, typeof(TValue));
    }

    public IHistoricalTimeSeries? TryResolve(object reference, Type? valueType = null)
    {
        // FUTURE ENH: IInputResolver<T> where T is SymbolValueAspect, etc.

        if (reference is SymbolValueAspect sva)
        {
            ExchangeSymbolTimeFrame exchangeSymbolTimeFrame = sva;
            DataPointAspect aspect = sva.Aspect;

            if (sva is IValueType vt)
            {
                return (IHistoricalTimeSeries)ActivatorUtilities.CreateInstance(ServiceProvider,
                    typeof(BarAspectSeries<>).MakeGenericType(vt.ValueType),
                    exchangeSymbolTimeFrame, aspect);

            }
            else
            {
                return ActivatorUtilities.CreateInstance<BarAspectSeries<decimal>>(ServiceProvider, exchangeSymbolTimeFrame, aspect);
            }
        }
        else if (reference is ExchangeSymbolTimeFrame exchangeSymbolTimeFrame)
        {
            return ActivatorUtilities.CreateInstance<BarSeries>(ServiceProvider, exchangeSymbolTimeFrame);
        }
        else if (reference is IIndicatorHarnessOptions indicatorOptions)
        {

        }

        return null;
    }
}

