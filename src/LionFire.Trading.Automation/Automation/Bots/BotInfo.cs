using LionFire.Trading.ValueWindows;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation.Bots;

public class BotInfo
{
    #region Lifecycle

    public BotInfo() { }
    public BotInfo(Type pBotType, Type botType)
    {
        #region Validation

        if (!pBotType.IsAssignableTo(typeof(IPMarketProcessor))) throw new ArgumentException($"parameterType must be assignable to {typeof(IPMarketProcessor).FullName}.  parameterType: {pBotType.FullName}");
        if (!botType.IsAssignableTo(typeof(IBot2))) throw new ArgumentException($"parameterType must be assignable to {typeof(IBot2).FullName}.  parameterType: {pBotType.FullName}");

        #endregion

        InputParameterToValueMapping = BotInfo.GetInputParameterToValueMapping(pBotType, botType);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Ordered list of mapping of Bot Input Parameter property to Bot Input Values property.
    /// E.g.: Map "BTCUSDT HLC m5, lookback window of 5 bars" to a circular buffer with actual values.
    /// </summary>
    public List<InputParameterToValueMapping>? InputParameterToValueMapping { get; set; }

    private static List<InputParameterToValueMapping> GetInputParameterToValueMapping(Type pBotType, Type botType)
    {
        List<InputParameterToValueMapping> result = [];

        var botSignals = botType
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(pi => pi.PropertyType.IsAssignableTo(typeof(IReadOnlyValuesWindow)))
                        .OrderBy(pi => pi.GetCustomAttribute<SignalAttribute>()?.Index ?? 0)
                        .ThenBy(pi => pi.Name)
                        ;

        foreach (var propertyInfo in botSignals)
        {
            var parameterProperty = pBotType.GetProperty(propertyInfo.Name);

            if (parameterProperty == null)
            {
                throw new ArgumentException($"Could not find matching Property {propertyInfo.Name} on {pBotType.FullName}");
            }

            result.Add(new InputParameterToValueMapping(parameterProperty, propertyInfo));
        }

        return result;
    }

    #endregion
}

public static class BotInfos
{
    #region State

    static ConcurrentDictionary<Type, BotInfo> dict = new();

    #endregion

    #region Methods

    public static BotInfo Get(Type pBotType, Type botType) => dict.GetOrAdd(pBotType, t => new BotInfo(pBotType, botType));

    #endregion

#if UNUSED // OLD From BotBatchBacktestControllerBase
    public partial class BotBatchControllerBase
    {

#if UNUSED
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


        #region Inputs

        private void ResolveIndicatorInputs(IIndicatorHarnessOptions harnessOptions, IIndicatorParameters parameters)
        {
            //Inputs = new[] {
            //            new ExchangeSymbolTimeFrame("Binance", "futures", "BTCUSDT", TimeFrame.h1)
            //             } // OPTIMIZE - Aspect: HLC

            throw new NotImplementedException();
        }

        #endregion
#endif

        // OLD UNUSED
        //protected async void InitBot(IBot2 bot)
        //{
        //    var info = BotInitializationInfo.GetFor(bot);

        //    if (info.BotWindowsToParameterIndicators != null)
        //    {
        //        foreach (var kvp in info.BotWindowsToParameterIndicators)
        //        {
        //            InitBotIndicator(bot, kvp.Key, kvp.Value);
        //        }
        //    }

        //    List<Task>? tasks = null;
        //    foreach (var kvp in InputEnumerators)
        //    {
        //        var inputEnumerator = kvp.Value;
        //        if (inputEnumerator is IChunkingInputEnumerator chunking && chunking.LookbackRequired > 0)
        //        {
        //            tasks ??= new();
        //            var preloadStart = TimeFrame.AddBars(Start, -chunking.LookbackRequired);
        //            var preloadEndExclusive = Start;

        //            tasks.Add(Task.Run(async () =>
        //            {
        //                await inputEnumerator.PreloadRange(preloadStart, preloadEndExclusive).ConfigureAwait(false);
        //                inputEnumerator.MoveNext(chunking.LookbackRequired);
        //            }));
        //        }
        //    }
        //    if (tasks != null) { await tasks.WhenAll().ConfigureAwait(false); }

        //}

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

            IEnumerable<PropertyInfo> GetReverseValuesWindowsPropertyInfos(Type type) => type
                    .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                    .Where(pi => pi.PropertyType.IsAssignableTo(typeof(IReadOnlyValuesWindow)));

            public BotInitializationInfo(IBot2 bot)
            {
                Type parametersType = bot.Parameters.GetType();

                // TODO: Determine the sources for these windows
                // Eg:
                // - Window property name: "ATR"
                // - Bot parameter name: "ATR" or (FUTURE:) Bot parameter attribute [RuntimeProperty("ATR")]

                foreach (var window in GetReverseValuesWindowsPropertyInfos(bot.GetType()))
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
                        throw new ArgumentException($"{nameof(IReadOnlyValuesWindow)} '{window.Name}' in bot has a matching property in PBacktests, but wiring up ValueType of {p.PropertyType.FullName} is not supported.");
                    }
                }
            }

            // Input could be 
            public Dictionary<PropertyInfo, PropertyInfo>? BotWindowsToParameterIndicators { get; private set; }
            //public IEnumerable<PropertyInfo> ReverseValuesWindow { get; private init; }

            //public (int source, PropertyInfo) Indicators { get; set; }
        }
    }
#endif
}

public class PBotInfo
{
    public PropertyInfo? Bars { get; set; }

}

public static class PBotInfos
{
    static ConcurrentDictionary<Type, PBotInfo> dict = new();
    public static PBotInfo Get(Type parameterType)
    {
        return dict.GetOrAdd(parameterType, t =>
        {
            if (!parameterType.IsAssignableTo(typeof(IPBot2))) throw new ArgumentException($"parameterType must be assignable to {typeof(IPBot2).FullName}.  parameterType: {parameterType.FullName}");

            var result = new PBotInfo();

            result.Bars = parameterType.GetProperty("Bars");

            return result;
        });
    }
}
