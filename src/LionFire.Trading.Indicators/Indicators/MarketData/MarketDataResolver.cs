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
    IHistoricalTimeSeries? TryResolve(object reference, Type? valueType = null);
    IHistoricalTimeSeries Resolve(Type valueType, object reference)
        => (IHistoricalTimeSeries)(this.GetType().GetMethod(nameof(Resolve))!.MakeGenericMethod(valueType).Invoke(this, [reference])!);

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
        else if (reference is IIndicatorHarnessOptions indicatorHarnessOptions)
        {

            var timeSeries = (IHistoricalTimeSeries)Activator.CreateInstance(typeof(HistoricalTimeSeriesFromIndicatorHarness<,,,>).MakeGenericType(
                    indicatorHarnessOptions.IndicatorParameters.IndicatorType,
                    indicatorHarnessOptions.IndicatorParameters.GetType(),
                    indicatorHarnessOptions.IndicatorParameters.InputType,
                    indicatorHarnessOptions.IndicatorParameters.OutputType
                ), 
                ServiceProvider,
                indicatorHarnessOptions // implicit cast to IndicatorHarnessOptions<TParameters>
                )!;
            return timeSeries;
        }

        return null;
    }
}

public class HistoricalTimeSeriesFromIndicatorHarness<TIndicator, TParameters, TInput, TOutput> : IHistoricalTimeSeries<TOutput>
    where TIndicator : IIndicator2<TParameters, TInput, TOutput>
  where TParameters : IIndicatorParameters
{
    #region Dependencies

    public IServiceProvider ServiceProvider { get; }

    #endregion

    #region Parameters

    public Type ValueType => typeof(TOutput);
    public TimeFrame TimeFrame => indicatorHarness.TimeFrame;

    #endregion

    #region Lifecycle

    public HistoricalTimeSeriesFromIndicatorHarness(IServiceProvider serviceProvider, IndicatorHarnessOptions<TParameters> indicatorHarnessOptions)
    {
        ServiceProvider = serviceProvider;
        TParameters parameters = (TParameters)indicatorHarnessOptions.IndicatorParameters;

        indicatorHarness = new HistoricalIndicatorHarness<TIndicator, TParameters, TInput, TOutput>(ServiceProvider, indicatorHarnessOptions);
    }

    #endregion

    TOutput[]? outputBuffer;
    IIndicatorHarness<TParameters, TInput, TOutput> indicatorHarness;

    #region Methods

    public async ValueTask<HistoricalDataResult<TOutput>> Get(DateTimeOffset start, DateTimeOffset endExclusive)
    {
        var valuesResult = await indicatorHarness.TryGetValues(start, endExclusive, ref outputBuffer);

        var result = new HistoricalDataResult<TOutput>
        {
            Items = valuesResult.Values?.ToArray() ?? [],
            IsSuccess = valuesResult.IsSuccess,
            FailReason = valuesResult.FailReason,
        };
        return result;
    }

    #endregion

}
