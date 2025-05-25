using LionFire.TypeRegistration;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace LionFire.Trading.Automation;

public class BotTypeRegistry
{
    public TypeRegistry PBotRegistry { get; }
    public TypeRegistry BotRegistry { get; }

    public BotTypeRegistry(IServiceProvider serviceProvider)
    {
        PBotRegistry = serviceProvider.GetRequiredKeyedService<TypeRegistry>(typeof(IPBot2));
        BotRegistry = serviceProvider.GetRequiredKeyedService<TypeRegistry>(typeof(IBot2));
    }

    public Type GetPBotForBot(Type botType)
    {
        Type? pBotType = null;

        var pBotTypeMethodInfo = botType.GetProperty(nameof(IBot2.ParametersType), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);


        if (pBotTypeMethodInfo == null && !BotTypeRegistry.TryGetBotParameterFromConvention(botType, out pBotType))
        {
            throw new NotFoundException($"No {nameof(IBot2.ParametersType)} property on {botType.Name}");
        }

        if (pBotType == null)
        {
            pBotType = pBotTypeMethodInfo!.GetValue(null) as Type;
            if (pBotType == null && !BotTypeRegistry.TryGetBotParameterFromConvention(botType, out pBotType))
            {
                throw new NotFoundException($"No {nameof(IBot2.ParametersType)} property returned null");
            }
        }

        // OPTIMIZE: cache result in a dictionary
        return pBotType;
    }

    public static bool TryGetBotParameterFromConvention(Type botType, [MaybeNullWhen(false)] out Type pBotType)
    {
        pBotType = botType.Assembly.GetType(botType.Namespace + ".P" + botType.Name);
        return pBotType != null;
    }

    public string GetBotName(Type botType)
    {
#if DEBUG
        if (!botType.IsAssignableTo(typeof(IBot2))) throw new ArgumentException("Only intended for IBot2");
#endif
        string result = botType.Name;

        if (botType.IsGenericType)
        {
            int i = result.IndexOf('`');
            if (i >= 0) { result = result[..i]; }
        }
        return result;
    }
}
