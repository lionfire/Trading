using LionFire.ExtensionMethods.Validation;
using LionFire.Types;
using LionFire.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
//using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace LionFire.Trading.Automation;

public class BotHarnessFactory
{
    #region Dependencies

    public IServiceProvider ServiceProvider { get; }
    public BotTypeRegistry BotTypeRegistry { get; }
    public BotHarnessOptions Options { get; }
    public ILogger<BotHarnessFactory> Logger { get; }

    #endregion

    #region Lifecycle

    public BotHarnessFactory(IServiceProvider serviceProvider, BotTypeRegistry botTypeRegistry, IOptions<BotHarnessOptions> options, ILogger<BotHarnessFactory> logger)
    {
        ServiceProvider = serviceProvider;
        BotTypeRegistry = botTypeRegistry;
        Options = options.Value;
        Logger = logger;
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
    public IRealtimeBotHarness Create(BotEntity botEntity)
    {
        botEntity.ValidateOrThrow();

        if (botEntity.Parameters != null)
        {
            var (bot, numericType) = CreateBotFromParameters(botEntity);

            // Create BotHarness with the correct numeric type
            var harnessType = typeof(BotHarness<>).MakeGenericType(numericType);
            return (IRealtimeBotHarness)ActivatorUtilities.CreateInstance(ServiceProvider, harnessType, bot);
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

    private (IBot2 bot, Type numericType) CreateBotFromParameters(BotEntity botEntity)
    {
        var botType = BotTypeRegistry.BotRegistry.GetTypeFromNameOrThrow(botEntity.BotTypeName
            ?? throw new ArgumentNullException(nameof(botEntity.BotTypeName)));

        // Determine target numeric type based on override settings and bot context
        Type sourceNumericType = GetNumericTypeFromParameters(botEntity.Parameters);
        Type targetNumericType = DetermineTargetNumericType(botEntity, sourceNumericType);

        // If Bot type has 1 generic parameter (check for name TPrecision), use the target numeric type
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
                    // Use the target numeric type (possibly overridden)
                    botTypeParameters = [targetNumericType];
                    botType = botType.MakeGenericType(botTypeParameters);
                }
            }
            else
            {
                botType = botType.MakeGenericType(botTypeParameters);
                // Update targetNumericType based on the provided type parameters
                if (botTypeParameters.Length > 0 && IsNumericType(botTypeParameters[0]))
                {
                    targetNumericType = botTypeParameters[0];
                }
            }
        }

        var bot = (IBot2)ActivatorUtilities.CreateInstance(ServiceProvider, botType);

        // Handle parameter assignment with type conversion if needed
        var botParametersType = botType.GetProperty(nameof(IBot2.Parameters))!.PropertyType;
        if (botEntity.Parameters != null)
        {
            if (botParametersType.IsInstanceOfType(botEntity.Parameters) && sourceNumericType == targetNumericType)
            {
                // No conversion needed
                bot.Parameters = botEntity.Parameters;
            }
            else
            {
                // Type conversion needed
                if (Options.LogTypeConversions)
                {
                    Logger.LogInformation("Converting bot parameters from {SourceType} to {TargetType} for bot {BotName}", 
                        sourceNumericType.Name, targetNumericType.Name, botEntity.Name ?? botEntity.BotTypeName);
                }

                var newParams = Activator.CreateInstance(botParametersType)!;
                CopyPropertiesWithTypeConversion(botEntity.Parameters, newParams, sourceNumericType, targetNumericType);
                bot.Parameters = (IPBot2)newParams;
            }
        }

        return (bot, targetNumericType);
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(decimal) || type == typeof(double) || type == typeof(float) ||
               type == typeof(int) || type == typeof(long) || type == typeof(short) ||
               type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) ||
               type == typeof(byte) || type == typeof(sbyte);
    }

    private Type GetNumericTypeFromParameters(IPBot2? parameters)
    {
        if (parameters == null) return typeof(decimal);

        var parametersType = parameters.GetType();
        if (parametersType.IsGenericType)
        {
            var genericArgs = parametersType.GetGenericArguments();
            // Look for numeric types in the generic arguments
            foreach (var arg in genericArgs)
            {
                if (IsNumericType(arg))
                {
                    return arg;
                }
            }
        }
        return typeof(decimal);
    }

    private Type DetermineTargetNumericType(BotEntity botEntity, Type sourceNumericType)
    {
        // Priority: Bot-specific override > Global override > Source type

        // 1. Check bot-specific override
        if (botEntity.LiveNumericTypeOverride != null && IsNumericType(botEntity.LiveNumericTypeOverride))
        {
            CheckForPrecisionLoss(sourceNumericType, botEntity.LiveNumericTypeOverride, botEntity.Name ?? botEntity.BotTypeName);
            return botEntity.LiveNumericTypeOverride;
        }

        // 2. Check global override based on live/backtest context
        if (botEntity.Live)
        {
            CheckForPrecisionLoss(sourceNumericType, Options.DefaultLiveNumericType, botEntity.Name ?? botEntity.BotTypeName);
            return Options.DefaultLiveNumericType;
        }
        else if (Options.DefaultBacktestNumericType != null)
        {
            CheckForPrecisionLoss(sourceNumericType, Options.DefaultBacktestNumericType, botEntity.Name ?? botEntity.BotTypeName);
            return Options.DefaultBacktestNumericType;
        }

        // 3. Use source type as fallback
        return sourceNumericType;
    }

    private void CheckForPrecisionLoss(Type sourceType, Type targetType, string? botName)
    {
        if (!Options.WarnOnPrecisionLoss) return;

        bool precisionLoss = (sourceType == typeof(decimal) && (targetType == typeof(double) || targetType == typeof(float))) ||
                            (sourceType == typeof(double) && targetType == typeof(float));

        if (precisionLoss)
        {
            Logger.LogWarning("Potential precision loss converting from {SourceType} to {TargetType} for bot {BotName}", 
                sourceType.Name, targetType.Name, botName);
        }
    }

    private void CopyPropertiesWithTypeConversion(object source, object target, Type sourceNumericType, Type targetNumericType)
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
                else if (IsNumericType(prop.PropertyType) && IsNumericType(targetProp.PropertyType))
                {
                    // Convert numeric values
                    var convertedValue = ConvertNumericValue(value, targetProp.PropertyType);
                    targetProp.SetValue(target, convertedValue);
                }
                else if (!targetProp.PropertyType.IsPrimitive && targetProp.PropertyType != typeof(string))
                {
                    // Recursively copy for complex types with potential type conversion
                    var nestedTarget = Activator.CreateInstance(targetProp.PropertyType);
                    CopyPropertiesWithTypeConversion(value, nestedTarget, sourceNumericType, targetNumericType);
                    targetProp.SetValue(target, nestedTarget);
                }
                // else: types are not compatible, skip
            }
        }
    }

    private static object ConvertNumericValue(object value, Type targetType)
    {
        return targetType switch
        {
            _ when targetType == typeof(decimal) => Convert.ToDecimal(value),
            _ when targetType == typeof(double) => Convert.ToDouble(value),
            _ when targetType == typeof(float) => Convert.ToSingle(value),
            _ when targetType == typeof(int) => Convert.ToInt32(value),
            _ when targetType == typeof(long) => Convert.ToInt64(value),
            _ when targetType == typeof(short) => Convert.ToInt16(value),
            _ when targetType == typeof(uint) => Convert.ToUInt32(value),
            _ when targetType == typeof(ulong) => Convert.ToUInt64(value),
            _ when targetType == typeof(ushort) => Convert.ToUInt16(value),
            _ when targetType == typeof(byte) => Convert.ToByte(value),
            _ when targetType == typeof(sbyte) => Convert.ToSByte(value),
            _ => value
        };
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

    private IRealtimeBotHarness Create_FromOptimizationRunReference(BotEntity botEntity)
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
