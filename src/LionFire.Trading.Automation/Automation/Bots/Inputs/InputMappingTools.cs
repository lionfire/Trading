using LionFire.Trading.Automation.Bots;

namespace LionFire.Trading.Automation;

public static class InputMappingTools
{


    /// <summary>
    /// Multiple bots may reuse the same input enumerators -- the lookback is guaranteed to be at least the amount requested but it could be more.
    /// </summary>
    /// <param name="marketListener"></param>
    /// <param name="inputEnumerators"></param>
    /// <param name="inputMappings"></param>
    internal static void HydrateValueWindowsOnMarketListener(IMarketListener marketListener, Dictionary<string, InputEnumeratorBase> inputEnumerators, List<PInputToMappingToValuesWindowProperty> inputMappings)
    {
        foreach (var inputMapping in inputMappings)
        {
            inputMapping.Mapping.Values.SetValue(marketListener, inputEnumerators[inputMapping.PInput.Key].Values);
        }
    }
}



