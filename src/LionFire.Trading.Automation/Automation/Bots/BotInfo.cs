using LionFire.Trading.ValueWindows;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation.Bots;

public readonly record struct TypeInputInfo(PropertyInfo Parameter, PropertyInfo Values);

public class BotInfo
{
    public List<TypeInputInfo>? TypeInputInfos { get; set; }
}

public static class BotInfos
{
    static ConcurrentDictionary<Type, BotInfo> dict = new();
    public static BotInfo Get(Type parameterType, Type botType)
    {
        return dict.GetOrAdd(parameterType, t =>
        {

            if (!parameterType.IsAssignableTo(typeof(IPBot2))) throw new ArgumentException($"parameterType must be assignable to {typeof(IPBot2).FullName}.  parameterType: {parameterType.FullName}");

            var result = new BotInfo();
            result.TypeInputInfos = new();

            var botProperties = botType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(pi => pi.PropertyType.IsAssignableTo(typeof(IReadOnlyValuesWindow)));

            foreach (var pi in botProperties)
            {
                var parameterProperty = parameterType.GetProperty(pi.Name);

                if (parameterProperty == null)
                {
                    throw new ArgumentException($"Could not find matching Property {pi.Name} on {parameterType.FullName}");
                }

                result.TypeInputInfos.Add(new TypeInputInfo(parameterProperty, pi));
            }
            return result;
        });
    }
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
