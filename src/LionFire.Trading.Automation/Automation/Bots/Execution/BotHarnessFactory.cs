using LionFire.ExtensionMethods.Validation;
using LionFire.Types;
using LionFire.Validation;
using Microsoft.Extensions.DependencyInjection;
//using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace LionFire.Trading.Automation;

public class BotHarnessFactory
{
    #region Dependencies

    public IServiceProvider ServiceProvider { get; }
    public BotTypeRegistry BotTypeRegistry { get; }

    #endregion

    #region Lifecycle

    public BotHarnessFactory(IServiceProvider serviceProvider, BotTypeRegistry botTypeRegistry)
    {
        ServiceProvider = serviceProvider;
        BotTypeRegistry = botTypeRegistry;
    }

    #endregion

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
        botEntity.ValidateOrThrow();

        if (botEntity.Parameters != null)
        {
            var bot = CreateBotFromParameters(botEntity);

            return ActivatorUtilities.CreateInstance<BotHarness<decimal>>(ServiceProvider, bot);
        }
        //else if (botEntity.PBotHarness != null)
        //{
        //    var p = botEntity.PBotHarness;
        //    //Type type = Type.GetType(botEntity.PBotHarness);
        //    throw new NotImplementedException();
        //}
        else if (botEntity.BacktestReference?.OptimizationRunReference != null)
        {
            return Create_FromOptimizationRunReference(botEntity);
        }
        else
        {
            throw new BotFaultException($"{nameof(botEntity)} - does not contain enough information to create a bot.");
        }
    }

    private IBot2 CreateBotFromParameters(BotEntity botEntity)
    {
        var botType = BotTypeRegistry.BotRegistry.GetTypeFromNameOrThrow(botEntity.BotTypeName
            ?? throw new ArgumentNullException(nameof(botEntity.BotTypeName)));

        // If Bot type has 1 generic parameter (check for name TPrecision), default to decimal precision for live bots
        var botTypeParameters = botEntity.BotTypeParameters;
        if (botType.IsGenericTypeDefinition)
        {
            if (botTypeParameters == null)
            {
                var genericArgs = botType.GetGenericArguments();
                if (genericArgs.Length == 1
                    //&& genericArgs[0].Name == "TPrecision"
                    )
                {
                    // Use decimal for live bots by default
                    botTypeParameters = [typeof(decimal)];
                    botType = botType.MakeGenericType(botTypeParameters);
                }
            }
        }

        var bot = (IBot2)ActivatorUtilities.CreateInstance(ServiceProvider, botType);

        // If there's a mismatch with the parameter types, instantiate the proper parameter type, and do a copy from the botEntity.PMultiSim to the new one.
        var botParametersType = botType.GetProperty(nameof(IBot2.Parameters))!.PropertyType;
        if (botEntity.Parameters != null)
        {
            if (botEntity.Parameters != null && botParametersType.IsInstanceOfType(botEntity.Parameters))
            {
                bot.Parameters = botEntity.Parameters;
            }
            else
            {
                var newParams = Activator.CreateInstance(botParametersType)!;
                CopyPropertiesRecursive(botEntity.Parameters, newParams);
                bot.Parameters = (IPBot2)newParams;
            }
        }

        return bot;
    }

    private static void CopyPropertiesRecursive(object source, object target)
    {
        if (source == null || target == null) return;
        var sourceType = source.GetType();
        var targetType = target.GetType();
        foreach (var prop in sourceType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            if (!prop.CanRead) continue;
            var targetProp = targetType.GetProperty(prop.Name);
            if (targetProp != null && targetProp.CanWrite)
            {
                var value = prop.GetValue(source);
                if (value == null)
                {
                    targetProp.SetValue(target, null);
                }
                else if (targetProp.PropertyType.IsAssignableFrom(prop.PropertyType))
                {
                    targetProp.SetValue(target, value);
                }
                else if (!targetProp.PropertyType.IsPrimitive && targetProp.PropertyType != typeof(string))
                {
                    // Recursively copy for complex types
                    var nestedTarget = Activator.CreateInstance(targetProp.PropertyType);
                    CopyPropertiesRecursive(value, nestedTarget);
                    targetProp.SetValue(target, nestedTarget);
                }
                // else: types are not compatible, skip or handle as needed
            }
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
