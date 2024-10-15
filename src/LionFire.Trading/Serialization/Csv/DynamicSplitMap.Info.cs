using LionFire.Trading;
using LionFire.Trading.Journal;
using ReactiveUI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LionFire.Serialization.Csv;

public partial class DynamicSplitMap<T>
{

    /// <summary>
    /// A dictionary of all Bot Parameters
    /// - Path is path to the Property
    /// - Name is the name of the Property
    /// - Key will be Name if it is unique, otherwise Path
    /// 
    /// </summary>
    private class BotParameterPropertiesInfo
    {
        #region (static)

        static ConcurrentDictionary<Type, BotParameterPropertiesInfo> cache = new();
        public static BotParameterPropertiesInfo Get(Type rootPropertyInstanceType) => cache.GetOrAdd(rootPropertyInstanceType, t => new BotParameterPropertiesInfo(t));

        public const string DefaultRootPropertyName = "Parameters";

        #endregion

        #region Lifecycle

        public BotParameterPropertiesInfo(Type parametersType, string? rootPropertyName = null)
        {
            var pi = typeof(T).GetProperty(rootPropertyName ?? DefaultRootPropertyName) ?? throw new ArgumentException($"No property {rootPropertyName ?? DefaultRootPropertyName} on {typeof(T).FullName}");

            List<HierarchicalPropertyInfo> results = new();

            var rootHierarchicalPropertyInfo = new HierarchicalPropertyInfo(null, [pi]);

            Recurse(rootHierarchicalPropertyInfo, ref results, typeOverride: parametersType);

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

            Dictionary = results.ToDictionary(hpi => hpi.Key);
        }

        #endregion

        #region Properties

        public Dictionary<string, HierarchicalPropertyInfo> Dictionary { get; }

        #endregion

        #region Logic

        private bool IsUsableProperty(PropertyInfo pi)
        {
            if (!pi.CanWrite) return false;
            if (pi.GetCustomAttribute<JsonIgnoreAttribute>() != null) return false;
            if (pi.GetCustomAttribute<JournalIgnoreAttribute>() != null) return false;
            return true;
        }

        private static bool IsSerializable(PropertyInfo subProp)
        {
            return subProp.PropertyType.IsPrimitive
                                || subProp.PropertyType.IsEnum
                                || subProp.PropertyType.IsAssignableTo(typeof(Type))
                                || (subProp.PropertyType.IsAssignableTo(typeof(ISerializableAsString))
                                        && subProp.PropertyType.GetCustomAttribute<SerializeAsSerializedAttribute>()?.SerializeAsSerialized != false)
                                || subProp.PropertyType.GetCustomAttribute<SerializeAsSerializedAttribute>()?.SerializeAsSerialized == true;
        }

        private void Recurse(HierarchicalPropertyInfo parentHierarchicalPropertyInfo, ref List<HierarchicalPropertyInfo> results, ImmutableList<PropertyInfo>? path = null, Type? typeOverride = null)
        {
            PropertyInfo parentPropertyInfo = parentHierarchicalPropertyInfo.LastPropertyInfo;

            path ??= ImmutableList<PropertyInfo>.Empty;
            path = path.Add(parentPropertyInfo);

            foreach (var subProp in (typeOverride ?? parentPropertyInfo.PropertyType).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(IsUsableProperty))
            {
                var info = new HierarchicalPropertyInfo(parentHierarchicalPropertyInfo, path.Add(subProp));
                if (IsSerializable(subProp))
                {
                    results!.Add(info);
                }
                else
                {
                    Recurse(info, ref results, path);
                }
            }
        }
        #endregion
    }
}

