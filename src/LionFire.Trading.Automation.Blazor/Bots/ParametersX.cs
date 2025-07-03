using LionFire.Trading.DataFlow;

namespace LionFire.Trading.Automation;

public static class ParametersX
{
    public static Dictionary<string, object?> ToParametersDictionary(this IPBot2? parameters)
    {
        Dictionary<string, object?> dict = new();

        if (parameters != null)
        {
            foreach (var pi in parameters.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
            {
                dict.Add(pi.Name, pi.GetValue(parameters));
            }
        }
        return dict;
    }

    public static T FromParametersDictionary<T>(this Dictionary<string, object>? parametersDict)
        where T : IPBot2
    {
        return (T)FromParametersDictionary(parametersDict, typeof(T));
    }

    public static object FromParametersDictionary(this Dictionary<string, object>? parametersDict, Type parametersType)
    {
        var obj = Activator.CreateInstance(parametersType)!;

        if (parametersDict != null)
        {
            foreach (var kvp in parametersDict)
            {
                parametersType.GetProperty(kvp.Key, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)?.SetValue(obj, kvp.Value);
            }
        }
        return obj;
    }
}