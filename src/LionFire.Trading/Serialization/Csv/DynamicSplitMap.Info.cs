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
    private class CustomBotParameterPropertiesInfo : BotParameterPropertiesInfoBase
    {
        #region (static)

        public static CustomBotParameterPropertiesInfo Get(Type rootPropertyInstanceType) => cache.GetOrAdd(rootPropertyInstanceType, t => new CustomBotParameterPropertiesInfo(t));
        private static ConcurrentDictionary<Type, CustomBotParameterPropertiesInfo> cache = new();

        public const string DefaultRootPropertyName = "Parameters";

        #endregion

        public Type ParametersType { get; }
        public string? RootPropertyName { get; }

        #region Lifecycle

        public CustomBotParameterPropertiesInfo(Type parametersType, string? rootPropertyName = null) : base(manualInit: true)
        {
            ParametersType = parametersType;
            RootPropertyName = rootPropertyName;
            Init();
        }

        protected override List<HierarchicalPropertyInfo> GetDictionary()
        {
            var pi = typeof(T).GetProperty(RootPropertyName ?? DefaultRootPropertyName) ?? throw new ArgumentException($"No property {RootPropertyName ?? DefaultRootPropertyName} on {typeof(T).FullName}");

            List<HierarchicalPropertyInfo> results = new();
            var rootHierarchicalPropertyInfo = new HierarchicalPropertyInfo(null, [pi]);
            Recurse(rootHierarchicalPropertyInfo, ref results, propertyTypeOverride: ParametersType);
            return results;
        }

        #endregion


    }
}

