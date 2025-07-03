using LionFire.Trading.Automation.Bots;

namespace LionFire.Trading.Automation;

public static class InputMappingTools
{
    internal static void HydrateInputMappings(Dictionary<string, IndexedInput> inputEnumerators, IHasInputMappings hasInputMappings)
    {
        foreach (var inputMapping in hasInputMappings.InputMappings)
        {
            var inputEnumerator = inputEnumerators[inputMapping.PInput.Key];
            inputMapping.Mapping.Values.SetValue(hasInputMappings.Instance, inputEnumerator.Enumerator!.Values);
        }
    }

    internal static void InitInputsForBacktest(Dictionary<string, IndexedInput> inputEnumerators, IHasInputMappings hasInputMappings, int inputEnumeratorIndex, InputParameterToValueMapping parameterToValueMapping, int lookback, IPTimeFrameMarketProcessor instanceObject)
    {
        IPInput pHydratedInput;

        #region Hydrate unbound inputs to bound inputs
        {
            IPInput pInput = (IPInput)parameterToValueMapping.Parameter.GetValue(instanceObject, null)!;

            while (pInput is IPInputThatSupportsUnboundInputs unboundInput)
            {
                pInput = new PBoundInput(unboundInput, instanceObject);
            }

            pHydratedInput = pInput;
        }
        #endregion

        var key = pHydratedInput.Key;

        hasInputMappings.InputMappings.Add(new InputMapping(pHydratedInput, parameterToValueMapping));

        //if (firstBarsInput == null && pHydratedInput.ValueType.IsAssignableTo(typeof(IKlineMarker)))
        //{
        //    firstBarsInput = pHydratedInput;
        //}

        if (inputEnumerators.TryGetValue(key, out IndexedInput? value))
        {
            value.Lookback = Math.Max(value.Lookback, lookback);
        }
        else
        {
            inputEnumerators.Add(key, new IndexedInput
            {
                PInput = pHydratedInput,
                //PInput = (IPInput)typeInputInfo.Parameter.GetValue(backtest.PBacktest.Bot, null)!,
                Lookback = lookback,
                Index = inputEnumeratorIndex,
            });
        }
    }

}



