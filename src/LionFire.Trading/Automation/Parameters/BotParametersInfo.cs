using LionFire.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation.Bots.Parameters;

public class BotParameterInfo
{
    public BotParameterInfo(KeyValuePair<string, PropertyInfo> pi)
    {
        Key = pi.Key;
        PropertyInfo = pi.Value;
    }

    public string Key { get; }
    public PropertyInfo PropertyInfo { get; }
}

public class BotParametersInfo
{
    #region (static)

    public static List<Type> ExlcudeAttributes = [typeof(JsonIgnoreAttribute)];

    public static ConcurrentDictionary<string, BotParametersInfo> ParametersInfos { get; } = new();

    public static BotParametersInfo Get(Type pType)
        => ParametersInfos.GetOrAdd(pType.FullName, type =>
        {
            var result = new BotParametersInfo(pType);


            return result;
        });

    #endregion

    #region Identity

    public Type pType { get; }

    #endregion

    #region Lifecycle

    public BotParametersInfo(Type pType)
    {
        this.pType = pType;
        Parameters = PropertyFlattener.GetFlattenedProperties(pType).Select(kvp => new BotParameterInfo(kvp)).ToDictionary(bpi => bpi.Key);
    }

    #endregion

    #region Properties

    public Dictionary<string, BotParameterInfo> Parameters { get; } = new();
    //public Dictionary<string, BotParameterInfo> ArrayParameters { get; set; } // FUTURE: handle parameters that are arrays, since the keys may be infinitely virtual: A.B[0].C, A.B[1].C, ...

    #endregion

}
