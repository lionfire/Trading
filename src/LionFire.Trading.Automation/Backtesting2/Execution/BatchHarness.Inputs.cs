using LionFire.Trading.Automation.Bots;
using LionFire.Trading.Data;
using LionFire.Trading.Indicators.Inputs;
using Oakton.Descriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core.Tokens;

namespace LionFire.Trading.Automation;

public partial class BatchHarness<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{

    #region Input Parameters: Aggregate

    // Consolidate (de-dupe) using the max lookback for duplicates.

    /// <summary>
    /// Iterates through the input parameters of the PBot (with corresponding loopback values) and calls TryAddPInputEnumerator
    /// </summary>
    /// <param name="pMarketProcessor"></param>
    /// <param name="marketParticipantContext"></param>
    /// <param name="aggregatedPInputs"></param>
    /// <param name="marketParticipant">Optional market participant that implements IHasInputMappings (e.g., AccountMarketSim)</param>
    private void AggregatePInputsForPMarketParticipant(IPMarketProcessor? pMarketProcessor,
    MarketParticipantContext<TPrecision> marketParticipantContext,
    Dictionary<string, PInputToEnumerator> aggregatedPInputs,
    IHasInputMappings? marketParticipant = null)
    {
        ArgumentNullException.ThrowIfNull(pMarketProcessor);
        
        // Determine input mappings based on context type and participant type
        IEnumerable<InputParameterToValueMapping> inputMappings;
        List<PInputToMappingToValuesWindowProperty>? participantMappings = null;
        bool mappingsArePrePopulated = false;
        
        // First priority: Use context's InputMappings if it's an AccountMarketSimContext
        if (marketParticipantContext is AccountMarketSimContext<TPrecision> accountContext)
        {
            // AccountMarketSimContext manages its own InputMappings - they're already complete
            participantMappings = accountContext.InputMappings;
            inputMappings = participantMappings?.Select(m => m.Mapping) ?? Enumerable.Empty<InputParameterToValueMapping>();
            mappingsArePrePopulated = true; // These mappings are already complete, don't add to them
        }
        // Second priority: Use market participant's own InputMappings if provided
        else if (marketParticipant?.InputMappings != null)
        {
            participantMappings = marketParticipant.InputMappings;
            inputMappings = participantMappings.Select(m => m.Mapping);
            mappingsArePrePopulated = true; // These mappings are already complete, don't add to them
        }
        // Third priority: For bots, use BotInfo
        else if (pMarketProcessor is IPBot2 pBot)
        {
            // For bots, use BotInfo to get mappings
            var botType = pBot.MaterializedType;
            var botInfo = BotInfos.Get(pMarketProcessor.GetType(), botType);
            inputMappings = botInfo.InputParameterToValueMapping ?? Enumerable.Empty<InputParameterToValueMapping>();
            // For bots, we need to build the mappings list
            mappingsArePrePopulated = false;
        }
        else
        {
            // This case represents a PMarketProcessor that is neither a bot nor has proper context/mappings.
            // This is an unsupported scenario that would fail later with a TargetException when trying
            // to access properties, so we fail fast with a clear error message.
            throw new InvalidOperationException(
                $"Unable to determine InputMappings for {pMarketProcessor.GetType().Name}. " +
                $"The market processor must either have an AccountMarketSimContext, " +
                $"implement IHasInputMappings with valid InputMappings, " +
                $"or be an IPBot2 with BotInfo mappings.");
        }
        
        // If mappings are pre-populated (AccountMarketSim case), we just need to process them for aggregation
        // without modifying the original list
        if (mappingsArePrePopulated && participantMappings != null)
        {
            int index = 0;
            foreach (var mapping in participantMappings)
            {
                int lookback = pMarketProcessor.InputLookbacks == null || index >= pMarketProcessor.InputLookbacks.Length
                    ? 0
                    : pMarketProcessor.InputLookbacks[index];
                
                // Process the existing mapping for aggregation
                var key = mapping.PInput.Key;
                if (aggregatedPInputs.TryGetValue(key, out PInputToEnumerator? value))
                {
                    // Input already exists, so make sure the lookback is sufficient
                    value.Lookback = Math.Max(value.Lookback, lookback);
                }
                else
                {
                    // Input does not exist, so add it to the aggregated inputs
                    aggregatedPInputs.Add(key, new PInputToEnumerator
                    {
                        PInput = mapping.PInput,
                        Lookback = lookback,
                    });
                }
                index++;
            }
        }
        else
        {
            // Bots:  build the mappings list
            var targetMappings = marketParticipantContext.InputMappings ?? new List<PInputToMappingToValuesWindowProperty>();
            if (marketParticipantContext.InputMappings == null)
            {
                marketParticipantContext.InputMappings = targetMappings;
            }
            
            int inputEnumeratorIndex = -1;
            foreach (var inputInjectionMapping in inputMappings)
            {
                inputEnumeratorIndex++;

                int lookback = pMarketProcessor.InputLookbacks == null
                    ? 0
                    : pMarketProcessor.InputLookbacks[inputEnumeratorIndex];

                AggregatePInput(aggregatedPInputs, targetMappings, inputInjectionMapping, lookback, pMarketProcessor);
            }
        }

        //if(botContext.DefaultSimAccount != null)
        //{
        //    botContext.DefaultSimAccount.
        //}
#if BacktestAccountSlottedParameters
            if (backtest.Controller.Account is BacktestAccount2<TPrecision> bta)
            {
                    NewMethod(inputEnumerators, backtest, inputEnumeratorIndex, BacktestAccountSymbolInjectionInfo, 1, bta.Parameters);
            }
#endif

    }

    /// <summary>
    /// Invoked for each parameter.
    /// 
    /// 1. Hydrates unbound inputs to bound input
    /// 2. 
    /// </summary>
    /// <param name="aggregatedPInputs"></param>
    /// <param name="batchInputMappings"></param>
    /// <param name="inputEnumeratorIndex"></param>
    /// <param name="parameterToValueMapping"></param>
    /// <param name="lookback"></param>
    /// <param name="pMarketListener"></param>
    private static void AggregatePInput(Dictionary<string, PInputToEnumerator> aggregatedPInputs, List<PInputToMappingToValuesWindowProperty> batchInputMappings
        //, int inputEnumeratorIndex
        , InputParameterToValueMapping parameterToValueMapping, int lookback, IPMarketProcessor pMarketListener)
    {
        ArgumentNullException.ThrowIfNull(batchInputMappings);
        IPInput pHydratedInput;

        #region Hydrate unbound inputs to bound inputs
        {
            IPInput pInput = (IPInput)parameterToValueMapping.Parameter.GetValue(pMarketListener, null)!;

            while (pInput is IPInputThatSupportsUnboundInputs unboundInput) // Recurse
            {
                // Example:
                // unboundInput = ATR(3), no TF, no symbol
                // PBoundInput pInput = ATR(3), m1, BTCUSDT.P
                pInput = new PBoundInput(unboundInput, pMarketListener);
            }

            pHydratedInput = pInput;
        }
        #endregion

        var key = pHydratedInput.Key;

        // Example:
        // - pHydratedInput =
        //   - {HLCReference
        //       {
        //         Key = Binance.futures:BTCUSDT/m1#HLC,
        //         Exchange = Binance,
        //         ExchangeArea = futures,
        //         Symbol = BTCUSDT,
        //         TimeFrame = m1,
        //         ValueType = LionFire.Trading.HLC`1[System.Double]
        //       }
        //     }	
        // parameterToValueMapping = 
        //   {InputParameterToValueMapping
        //     {
        //       Parameter = (HLCReference<double>) PBarsBot<PATRBot<double>, double>.Bars,
        //       Values = (IReadOnlyValuesWindow<HLC<double>>) BarsBot.Bars
        //      }
        //    }
        batchInputMappings.Add(new PInputToMappingToValuesWindowProperty(pHydratedInput, parameterToValueMapping));

        //if (firstBarsInput == null && pHydratedInput.ValueType.IsAssignableTo(typeof(IKlineMarker)))
        //{
        //    firstBarsInput = pHydratedInput;
        //}

        if (aggregatedPInputs.TryGetValue(key, out PInputToEnumerator? value))
        {
            // Input already exists, so make sure the lookback is sufficient
            value.Lookback = Math.Max(value.Lookback, lookback);
        }
        else
        {
            // Input does not exist, so create a new IndexedInput
            aggregatedPInputs.Add(key, new PInputToEnumerator
            {
                PInput = pHydratedInput,
                //PInput = (IPInput)typeInputInfo.Parameter.GetValue(backtest.PBacktest.Bot, null)!,
                Lookback = lookback,
                //Index = inputEnumeratorIndex, // UNUSED
            });
        }
    }

    #endregion

    
}

public static class IMarketDataResolverX
{
    public static InputEnumeratorBase CreateInputEnumerator(this IMarketDataResolver marketDataResolver, IPInput pInput, int lookback)
    {
        IHistoricalTimeSeries series = marketDataResolver.Resolve(pInput);
        return CreateInputEnumerator(series, lookback);
    }

    public static InputEnumeratorBase CreateInputEnumerator(IHistoricalTimeSeries series, int lookback)
    {
        if (lookback == 0)
        {
            return (InputEnumeratorBase)typeof(SingleValueInputEnumerator<,>)
               .MakeGenericType(series.ValueType, series.PrecisionType)
               .GetConstructor([typeof(IHistoricalTimeSeries<>).MakeGenericType(series.ValueType)])!
               .Invoke([series]);
        }
        else if (lookback < 0) throw new ArgumentOutOfRangeException(nameof(lookback));
        else
        {
            return (InputEnumeratorBase)typeof(ChunkingInputEnumerator<,>)
                .MakeGenericType(series.ValueType, series.PrecisionType)
                .GetConstructor([typeof(IHistoricalTimeSeries<>).MakeGenericType(series.ValueType), typeof(int)])!
                .Invoke([series, lookback]);
        }
    }
}
