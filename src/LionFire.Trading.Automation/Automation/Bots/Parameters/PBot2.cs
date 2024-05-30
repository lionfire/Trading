using LionFire.Metadata;
using LionFire.Structures;
using System.Collections.Concurrent;
using System.Reflection;
using LionFire.Trading.DataFlow;

namespace LionFire.Trading.Automation;
public class PMarketProcessorInfo
{

    #region (static)

    public static PMarketProcessorInfo Get(Type type) => dict.GetOrAdd(type, t => new PMarketProcessorInfo(t));
    static ConcurrentDictionary<Type, PMarketProcessorInfo> dict = new();

    #endregion


    public PMarketProcessorInfo(Type type)
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

// TODO: Also have indicators derive from this?
public abstract class PMarketProcessor : IPMarketProcessor
{
    //public IPInput[]? Inputs { get;  }
    public virtual IKeyed<string>[] DerivedInputs => [];

    public int[]? InputLookbacks { get; set; }

    public abstract Type InstanceType { get; }
    public abstract Type MaterializedType { get; }
}

public abstract class PBot2<TConcrete> : PMarketProcessor, IPBotHierarchical2
{


    //public static IReadOnlyList<InputSlot> InputSlots { get; private set; }

    //public static IReadOnlyList<InputSlot> InputSlots()
    //=> [new InputSlot() {
    //                Name = "ATR",
    //                Type = typeof(AverageTrueRange),
    //            }];

    public IEnumerable<IPBot2> Children => getChildren(this);

    #region (static)

    static PBot2()
    {
        var propertyInfos = typeof(TConcrete).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(p => p.PropertyType.IsAssignableTo(typeof(IPBotHierarchical2)));

        getChildren = pBot =>
        {
            return propertyInfos.Select(pi => pi.GetValue(pBot) as IPBotHierarchical2).Where(p => p != null)!;
        };

        #region InputSlots

#if MAYBE
        List<InputSlot> inputSlots = new();

        foreach (var pi in typeof(TConcrete)
            .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
            .Where(pi => pi.PropertyType.IsAssignableTo(typeof(IIndicatorParameters)))
            )
        {
            inputSlots.Add(new InputSlot()
            {
                Name = pi.Name,
                Type = pi.PropertyType, // IPBot2
            });
        }

#endif
        #endregion
    }
    #endregion
    static Func<PBot2<TConcrete>, IEnumerable<IPBotHierarchical2>> getChildren;

}
