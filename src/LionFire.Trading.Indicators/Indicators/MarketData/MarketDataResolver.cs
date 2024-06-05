using LionFire.Orleans_.ObserverGrains;
using LionFire.Trading.Data;
using Microsoft.Extensions.DependencyInjection;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Execution;
using LionFire.Trading.Indicators.Harnesses;
using LionFire.Trading.DataFlow;
namespace LionFire.Trading.Indicators.Inputs;


public interface IMarketDataResolver
{
    IHistoricalTimeSeries Resolve(object reference, Slot? slot = null)
    {
        var result = TryResolve(reference, slot);
        if (result == null) throw new ArgumentException("Failed to resolve: " + reference);
        return result;
    }
    IHistoricalTimeSeries? TryResolve(object reference, Slot? slot = null);
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

    public IHistoricalTimeSeries? TryResolve(object reference, Slot? slot = null)
    {
        IHistoricalTimeSeries _Preliminary(object reference)
        {
            // FUTURE ENH: IInputResolver<T> where T is SymbolValueAspect, etc.

            if (reference is SymbolValueAspect sva)
            {
                ExchangeSymbolTimeFrame exchangeSymbolTimeFrame = sva;
#error NEXT: Change valueType parameter to some sort of IValueCoerce, with CoerceKline providing its own DataPointAspect, Volume rules, etc.  Also do this for ExchangeSymbolTimeFrame and perhaps elsewhere.
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
            else if (reference is PBoundInput pBoundInput) // TODO - should test for IIndicatorParameters here, in a pass-through fashion, without having to worry about PBoundInput
            {
                if (pBoundInput.PUnboundInput is IIndicatorParameters pIndicator)
                {
                    var slots = pIndicator.Slots;
                    int slotIndex = 0;
                    var signals = new List<IHistoricalTimeSeries>();
                    foreach (var signal in pBoundInput.Signals)
                    {
                        var resolved = ((IMarketDataResolver)this).Resolve(signal, slots[slotIndex++]);
                        signals.Add(resolved);
                    }

                    if (signals.Count != pIndicator.InputCount)
                    {
                        //#error NEXT: provide the inputs
                        throw new ArgumentException($"Indicator {pIndicator.GetType().Name}.{pIndicator.InputCount} = {pIndicator.InputCount}, but {signals.Count} Inputs were provided.");
                    }

                    // We handle the memory in the iterator instead of the harness.  REVIEW - Is or can Memory be obsolete here?
                    //var outputExecutionOptions = new OutputComponentOptions()
                    //{
                    //    Memory = pIndicator.Memory,
                    //};

                    if (pBoundInput.PUnboundInput.TimeFrame != null && pBoundInput.PUnboundInput.TimeFrame != pBoundInput.TimeFrame)
                    {
                        // FUTURE:
                        //  - PUnboundInput is more granular: either
                        //     - roll-up bars from a more granular indicator (pointless/impossible?), or
                        //     - just use the same TimeFrame
                        // - PUnboundInput is more coarse
                        //   - repeat the same value for all granular bars within the coarse bar
                        throw new NotImplementedException("Not implemented yet: PUnboundInput.TimeFrame that is different than PBoundInput.TimeFrame");
                    }
                    else
                    {
                        return HistoricalIndicatorHarness.Create(pIndicator, pBoundInput.TimeFrame, signals);

                        //var timeSeries = (IHistoricalTimeSeries)Activator.CreateInstance(typeof(HistoricalIndicatorHarness<,,,>).MakeGenericType(
                        //                    pIndicator.IndicatorType,
                        //                    pIndicator.GetType(),
                        //                    pIndicator.InputType,
                        //                    pIndicator.OutputType
                        //                ),
                        //                pIndicator,
                        //                pBoundInput.TimeFrame,
                        //                (IReadOnlyList<IHistoricalTimeSeries>)signals,
                        //                null // outputExecutionOptions
                        //                )!;
                        //return timeSeries;
                    }
                }

                typeof(HistoricalIndicatorHarness<,,,>).MakeGenericType(pBoundInput.PUnboundInput.ValueType);
            }
            else if (reference is IIndicatorHarnessOptions indicatorHarnessOptions)
            {
                // OLD
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
            else if (reference is IPUnboundInput _)
            {
                throw new ArgumentException($"{typeof(IPUnboundInput).FullName} must be bound before resolving");
            }

            return null;
        }
        var hts = _Preliminary(reference);
        if(valueType != null && valueType != hts.ValueType)
        {
            throw new NotImplementedException("NEXT: convert");
        }
        return hts;
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
            Values = valuesResult.Values.ToArray() ?? [],
            IsSuccess = valuesResult.IsSuccess,
            FailReason = valuesResult.FailReason,
        };
        return result;
    }

    #endregion

}
