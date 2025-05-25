using LionFire.Types;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace LionFire.Trading.Automation;

public class LiveBotHarnessFactory
{
    public IServiceProvider ServiceProvider { get; }
    public BotTypeRegistry BotTypeRegistry { get; }

    public LiveBotHarnessFactory(IServiceProvider serviceProvider, BotTypeRegistry botTypeRegistry)
    {
        ServiceProvider = serviceProvider;
        BotTypeRegistry = botTypeRegistry;
    }

    /// <summary>
    /// Precedence:
    /// - PBotHarness
    /// - BacktestReference
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="botEntity"></param>
    /// <returns></returns>
    public ILiveBotHarness Create(BotEntity botEntity)
    {
        if (botEntity.PBotHarness != null)
        {
            var p = botEntity.PBotHarness;
            //Type type = Type.GetType(botEntity.PBotHarness);
            throw new NotImplementedException();
        }
        else if (botEntity.BacktestReference?.OptimizationRunReference != null)
        {
            return Create_FromOptimizationRunReference(botEntity);
        }
        else
        {
            throw new BotFaultException($"{nameof(botEntity)} - does not contain enough information to create a bot.");
        }
    }


    private ILiveBotHarness Create_FromOptimizationRunReference(BotEntity botEntity)
    {
        #region Validating and deducing arguments

        var orr = botEntity.BacktestReference!.OptimizationRunReference!;
        var botType = ServiceProvider.GetTypeFromName<IBot2>(orr.Bot);

        if (botType == null) { throw new NotFoundException($"Unknown {typeof(IBot2).Name} type: {orr.Bot}"); }

        Type? pBotType = BotTypeRegistry.GetPBotForBot(botType);
        if (pBotType == null) { throw new NotFoundException($"No PBot type found for {botType.Name}"); }

        #endregion

        var pBot = ActivatorUtilities.CreateInstance(ServiceProvider, pBotType!) as IPBot2;
        //BatchId;
        //BacktestId;



        throw new NotImplementedException();
        //var bot = ...;

        //return new LiveBotHarness<decimal>();
    }

    //public IPBot2 GetPBot2FromBacktestReference()
    //{

    //}

}

public class BotFaultException : Exception
{
    public BotFaultException(string? message) : base(message) { }
}