using LionFire.Metadata;
using System.Collections.Concurrent;
using System.Reflection;

namespace LionFire.Trading.DataFlow;

public class SourcesInfo
{
    #region (static)

    public static SourcesInfo GetSourcesInfo(Type type) => dict.GetOrAdd(type, t => new SourcesInfo(t));

    static ConcurrentDictionary<Type, SourcesInfo> dict = new();

    #endregion

    public SourcesInfo(Type type)
    {
        List<PropertyInfo>? unorderedProperties = null;
        SortedList<float, PropertyInfo>? orderedProperties = null;

        foreach (var pi in type
            .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .Where(p =>
                p.PropertyType.IsAssignableTo(typeof(IPInput))
                && !p.PropertyType.IsAssignableTo(typeof(IPUnboundInput))
                )
            )
        {
            var attr = pi.GetCustomAttribute<OrderAttribute>();

            if (attr != null) { (orderedProperties ??= new()).Add(attr.Order, pi); }
            else (unorderedProperties ??= new()).Add(pi);
        }

        var sources = new List<PropertyInfo>();

        if (orderedProperties != null)
        {
            sources.AddRange(orderedProperties.TakeWhile(p => p.Key < 0f).Select(p => p.Value));
        }
        if (unorderedProperties != null)
        {
            sources.AddRange(unorderedProperties.OrderBy(p => p.Name));
        }
        if (orderedProperties != null)
        {
            sources.AddRange(orderedProperties.SkipWhile(p => p.Key < 0f).Select(p => p.Value));
        }

        Sources = sources;
    }

    public IReadOnlyList<PropertyInfo> Sources { get; }
}
