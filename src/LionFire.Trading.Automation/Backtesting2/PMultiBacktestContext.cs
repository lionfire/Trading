using LionFire.Trading.Automation.Optimization;

namespace LionFire.Trading.Automation;

public class PMultiBacktestContext
{
    #region Lifecycle

    public PMultiBacktestContext(POptimization pOptimization)
    {
        OptimizationOptions = pOptimization;
        ExchangeSymbol = pOptimization.ExchangeSymbol;
        PBotType = pOptimization.PBotType;
        BotType = TryGetBotType(PBotType);
    }

    public PMultiBacktestContext(Type pBotType, ExchangeSymbol? exchangeSymbol = null)
    {
        PBotType = pBotType;
        BotType = TryGetBotType(PBotType);
        ExchangeSymbol = ExchangeSymbol ?? ExchangeSymbol.Unknown;
    }
    public PMultiBacktestContext(IEnumerable<PBacktestTask2> pBacktestTask2)
    {
        var first = pBacktestTask2.First();
        PBotType = first.PBot!.GetType();
        BotType = TryGetBotType(PBotType);
        ExchangeSymbol = first.ExchangeSymbol ?? ExchangeSymbol.Unknown;
    }

    private static Type TryGetBotType(Type pBotType)
    {
        Type? botType;
        if (pBotType.IsAssignableTo(typeof(IPBot2Static)))
        {
            botType = (Type)pBotType.GetProperty(nameof(IPBot2Static.StaticMaterializedType))!.GetValue(null)!;
        }
        else
        {
            throw new ArgumentException($"Provide {nameof(botType)} or a {nameof(pBotType)} of type IPBot2Static");
        }

        return botType;
    }

    #endregion

    public Type PBotType { get; }
    public Type BotType { get; }

    public ExchangeSymbol ExchangeSymbol { get; set; }

    public POptimization? OptimizationOptions { get; set; }
}

