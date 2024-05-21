using LionFire.ExtensionMethods;
using LionFire.Trading.Data;
using LionFire.Trading.Indicators.Harnesses;
using LionFire.Trading.Indicators.Inputs;
using LionFire.Trading.ValueWindows;
using Microsoft.Extensions.DependencyInjection;
using Nito.AsyncEx;
using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LionFire.Trading.Automation;

//public class BotControllerBotState
//{
//    // Harnesses 
//}

/// <summary>
/// Batch processing of multiple bots:
/// - InputEnumerators are enumerated in lock step
/// </summary>
/// <remarks>
/// </remarks>
public abstract class BatchBotControllerBase : IBotController
{
    #region Identity

    public abstract BotExecutionMode BotExecutionMode { get; }

    #endregion

    #region Dependencies

    public IServiceProvider ServiceProvider { get; }

    #endregion

    #region Parameters

    public IPBacktestTask2 Parameters { get; }

    #region Derived

    public TimeFrame TimeFrame => Parameters.TimeFrame;

    #endregion

    #endregion

    #region Lifecycle

    public BatchBotControllerBase(IServiceProvider serviceProvider, IPBacktestTask2 parameters)
    {
        ServiceProvider = serviceProvider;
        Parameters = parameters;
    }

    #endregion

    #region State

    #region Bots

    public List<IBot2>? Bots { get; private set; }

    #endregion

    #region Enumerators

    public Dictionary<string, InputEnumeratorBase> InputEnumerators { get; } = new();

    #endregion

    #endregion

    #region Init

    public void Init(params IBot2[] bots)
    {
        if (Bots != null) throw new AlreadyException();
        Bots = bots.ToList();

        foreach (var bot in bots)
        {
            InitBot(bot);
        }
    }

    public IHistoricalTimeSeries Resolve(Type outputType, object source) => ServiceProvider.GetRequiredService<IMarketDataResolver>().Resolve(outputType, source);

    private void InitBotIndicator(IBot2 bot, PropertyInfo botWindowProperty, PropertyInfo indicatorParametersProperty)
    {
        var parameters = (IIndicatorParameters)(indicatorParametersProperty.GetValue(bot.Parameters)
                       ?? throw new ArgumentNullException($"Bot parameters of type {indicatorParametersProperty.PropertyType.FullName} is null for bot of type {bot.GetType().FullName}"));

        var indicatorHarnessOptions = (IIndicatorHarnessOptions)typeof(IndicatorHarnessOptions<>).MakeGenericType(parameters.GetType()).GetConstructor([parameters.GetType()])!.Invoke([parameters]);
        indicatorHarnessOptions.TimeFrame = TimeFrame.h1;

        ResolveIndicatorInputs(indicatorHarnessOptions, parameters);

        var historicalTimeSeries = () => Resolve(parameters.OutputType, indicatorHarnessOptions);

        int lookback = parameters.Lookback;

        var inputEnumeratorKey = "";
        var inputEnumerator = InputEnumerators.TryGetValue(inputEnumeratorKey);

        if (InputEnumeratorFactory.CreateOrGrow(botWindowProperty.PropertyType, historicalTimeSeries, lookback, ref inputEnumerator))
        {
            InputEnumerators[inputEnumeratorKey] = inputEnumerator!;
        }

        switch (BotExecutionMode)
        {
            case BotExecutionMode.Backtest:
                // Create Historical Indicator Harness
                break;
            case BotExecutionMode.Live:
                // Create Live Indicator Harness
                break;
            default:
                throw new UnreachableCodeException();
        }

        botWindowProperty.SetValue(bot, inputEnumerator);
    }

    private void ResolveIndicatorInputs(IIndicatorHarnessOptions harnessOptions, IIndicatorParameters parameters)
    {
        //Inputs = new[] {
        //            new ExchangeSymbolTimeFrame("Binance", "futures", "BTCUSDT", TimeFrame.h1)
        //             } // OPTIMIZE - Aspect: HLC

        throw new NotImplementedException();
    }

    protected async void InitBot(IBot2 bot)
    {
        var info = BotInitializationInfo.GetFor(bot);

        if (info.BotWindowsToParameterIndicators != null)
        {
            foreach (var kvp in info.BotWindowsToParameterIndicators)
            {
                InitBotIndicator(bot, kvp.Key, kvp.Value);
            }
        }

        List<Task>? tasks = null;
        foreach (var kvp in InputEnumerators)
        {
            var inputEnumerator = kvp.Value;
            if (inputEnumerator is IChunkingInputEnumerator chunking && chunking.LookbackRequired > 0)
            {
                tasks ??= new();
                var preloadStart = TimeFrame.AddBars(Parameters.Start, -chunking.LookbackRequired);
                var preloadEndExclusive = Parameters.Start;

                tasks.Add(Task.Run(async () =>
                {
                    await inputEnumerator.PreloadRange(preloadStart, preloadEndExclusive).ConfigureAwait(false);
                    inputEnumerator.MoveNext(chunking.LookbackRequired);
                }));
            }
        }
        if (tasks != null) { await tasks.WhenAll().ConfigureAwait(false); }

    }


    private class BotInitializationInfo
    {
        #region (static)

        internal static BotInitializationInfo GetFor(IBot2 bot)
        {
            return initInfos.GetOrAdd(bot.GetType(), type =>
            {
                var info = new BotInitializationInfo(bot);
                return info;
            });
        }

        static readonly ConcurrentDictionary<Type, BotInitializationInfo> initInfos = new();

        #endregion

        public BotInitializationInfo(IBot2 bot)
        {
            Type type = bot.GetType();
            Type parametersType = bot.Parameters.GetType();

            var ReverseValuesWindow = type
                .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                .Where(pi => pi.PropertyType.IsAssignableTo(typeof(IReadOnlyValuesWindow)))
                ;

            // TODO: Determine the sources for these windows
            // Eg:
            // - Window property name: "ATR"
            // - Bot parameter name: "ATR" or (FUTURE:) Bot parameter attribute [RuntimeProperty("ATR")]

            foreach (var window in ReverseValuesWindow)
            {
                var p = parametersType.GetProperty(window.Name);
                if (p == null)
                {
                    throw new ArgumentException($"No parameter found for window {window.Name}");
                }

                if (p.PropertyType.IsAssignableTo(typeof(IIndicatorParameters)))
                {
                    BotWindowsToParameterIndicators ??= new();
                    BotWindowsToParameterIndicators.Add(window, p);
                }
                else
                {
                    throw new ArgumentException($"{nameof(IReadOnlyValuesWindow)} '{window.Name}' in bot has a matching property in Parameters, but wiring up Type of {p.PropertyType.FullName} is not supported.");
                }
            }
        }

        // Input could be 
        public Dictionary<PropertyInfo, PropertyInfo>? BotWindowsToParameterIndicators { get; private set; }
        //public IEnumerable<PropertyInfo> ReverseValuesWindow { get; private init; }

        //public (int source, PropertyInfo) Indicators { get; set; }
    }

    #endregion
}


