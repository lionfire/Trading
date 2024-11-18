using LionFire.Trading.Automation.Optimization;

namespace LionFire.Trading.Automation;

public class PMultiBacktestContext
{
    #region Lifecycle

    public PMultiBacktestContext(POptimization pOptimization)
    {
        OptimizationOptions = pOptimization;
    }

    public PMultiBacktestContext(Type pBotType, ExchangeSymbol? exchangeSymbol = null, DateTimeOffset? start = null, DateTimeOffset? endExclusive = null)
    {
        OptimizationOptions = new POptimization(pBotType, ExchangeSymbol)
        {
            CommonBacktestParameters = new()
            {
                Start = start ?? default,
                EndExclusive = endExclusive ?? default
            }
        };
    }

    public PMultiBacktestContext(IEnumerable<PBacktestTask2> pBacktestTask2, DateTimeOffset? start, DateTimeOffset? endExclusive)
    {
        var first = pBacktestTask2.First();
        var pBotType = first.PBot!.GetType();
        var exchangeSymbol = first.ExchangeSymbol ?? ExchangeSymbol.Unknown;

        OptimizationOptions = new POptimization(pBotType, exchangeSymbol)
        {
            CommonBacktestParameters = new()
            {
                Start = start ?? default,
                EndExclusive = endExclusive ?? default
            }
        };
    }

    #endregion

    public POptimization OptimizationOptions { get; set; }

    #region Derived

    #region Convenience

    public Type PBotType => OptimizationOptions.PBotType;
    public ExchangeSymbol ExchangeSymbol => OptimizationOptions.ExchangeSymbol;

    #endregion

    public Type BotType => botType ??= TryGetBotType(PBotType);
    private Type? botType;
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
}

