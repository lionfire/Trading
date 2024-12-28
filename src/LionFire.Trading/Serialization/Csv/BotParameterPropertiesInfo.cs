using LionFire.Trading;
using LionFire.Trading.Journal;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using System.Text.Json.Serialization;

namespace LionFire.Serialization.Csv;

public class BotParameterPropertiesInfo : BotParameterPropertiesInfoBase
{
    #region (static)

    public static BotParameterPropertiesInfo? SafeGet(Type? rootPropertyInstanceType) => rootPropertyInstanceType == null ? null : cache.GetOrAdd(rootPropertyInstanceType, t => new BotParameterPropertiesInfo(t));
    public static BotParameterPropertiesInfo Get(Type rootPropertyInstanceType) => rootPropertyInstanceType == null ? throw new ArgumentNullException() : cache.GetOrAdd(rootPropertyInstanceType, t => new BotParameterPropertiesInfo(t));
    private static ConcurrentDictionary<Type, BotParameterPropertiesInfo> cache = new();

    #endregion

    public Type ParametersType { get; }

    public BotParameterPropertiesInfo(Type type) : base(manualInit: true)
    {
        ParametersType = type;
        Init();
    }

    protected override List<HierarchicalPropertyInfo> GetDictionary()
    {
        var rootHierarchicalPropertyInfo = new HierarchicalPropertyInfo(ParametersType);
        List<HierarchicalPropertyInfo> results = new();
        RecurseProperties(rootHierarchicalPropertyInfo, ref results, propertyTypeOverride: rootHierarchicalPropertyInfo.RootPropertyType);
        return results;
    }
}

public abstract class BotParameterPropertiesInfoBase
{
    #region Properties

    public Dictionary<string, HierarchicalPropertyInfo> NameDictionary { get; protected set; } = null!;
    public Dictionary<string, HierarchicalPropertyInfo> PathDictionary 
        => pathDictionary ??= NameDictionary.Values.ToDictionary(hpi => hpi.Path);
    private Dictionary<string, HierarchicalPropertyInfo>? pathDictionary;


    #endregion

    #region Lifecycle

    public BotParameterPropertiesInfoBase(bool manualInit = false)
    {
        if (!manualInit) Init();

    }
    protected void Init()
    {
        var results = GetDictionary();
        SetKeys(results);
        NameDictionary = results.ToDictionary(hpi => hpi.Key);
    }

    #endregion

    protected abstract List<HierarchicalPropertyInfo> GetDictionary();

    #region Logic

    protected bool IsUsableProperty(PropertyInfo pi)
    {
        if (!pi.CanWrite) return false;
        if (pi.GetCustomAttribute<JsonIgnoreAttribute>() != null) return false;
        if (pi.GetCustomAttribute<JournalIgnoreAttribute>() != null) return false;
        //if (pi.GetCustomAttribute<ParameterAttribute>() == null) return false;
        return true;
    }

    protected bool IsSerializable(PropertyInfo subProp)
    {
        return subProp.PropertyType.IsPrimitive
                            || subProp.PropertyType.IsEnum
                            || subProp.PropertyType.IsAssignableTo(typeof(Type))
                            || (subProp.PropertyType.IsAssignableTo(typeof(ISerializableAsString))
                                    && subProp.PropertyType.GetCustomAttribute<SerializeAsSerializedAttribute>()?.SerializeAsSerialized != false)
                            || subProp.PropertyType.GetCustomAttribute<SerializeAsSerializedAttribute>()?.SerializeAsSerialized == true;
    }

    protected void RecurseProperties(HierarchicalPropertyInfo parentHierarchicalPropertyInfo, ref List<HierarchicalPropertyInfo> results, ImmutableList<PropertyInfo>? path = null, Type? propertyTypeOverride = null)
    {
        PropertyInfo? parentPropertyInfo = parentHierarchicalPropertyInfo.LastPropertyInfo;

        path ??= ImmutableList<PropertyInfo>.Empty;
        if (parentPropertyInfo != null)
        {
            path = path.Add(parentPropertyInfo);
        }

        foreach (var subProp in (propertyTypeOverride ?? parentPropertyInfo!.PropertyType).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(IsUsableProperty))
        {
            var info = new HierarchicalPropertyInfo(parentHierarchicalPropertyInfo, path.Add(subProp));
            if (IsSerializable(subProp))
            {
                results!.Add(info);
            }
            else
            {
                RecurseProperties(info, ref results, path);
            }
        }
    }

    protected static void SetKeys(List<HierarchicalPropertyInfo> results)
    {
        HashSet<string> uniqueNames = new();
        HashSet<string> duplicateNames = new();

        foreach (var r in results)
        {
            if (uniqueNames.Contains(r.Name))
            {
                duplicateNames.Add(r.Name);
                uniqueNames.Remove(r.Name);
            }
            else if (!duplicateNames.Contains(r.Name))
            {
                uniqueNames.Add(r.Name);
            }
        }

        // ENH: Attribute to always use path
        // ENH: Attribute to always use path: inherit from class

        foreach (var r in results)
        {
            r.Key = uniqueNames.Contains(r.Name) ? r.Name : r.Path;
        }
    }

    #endregion
}


