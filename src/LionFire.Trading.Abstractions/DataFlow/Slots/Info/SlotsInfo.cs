using LionFire.Metadata;
using System.Collections.Concurrent;
using System.Reflection;

namespace LionFire.Trading.DataFlow;

public class SlotsInfo
{
    #region (static)

    public static SlotsInfo GetSlotsInfo(Type type) => dict.GetOrAdd(type, t => new SlotsInfo(t));

    static ConcurrentDictionary<Type, SlotsInfo> dict = new();

    #endregion

    #region Lifecycle

    public SlotsInfo(Type parametersType)
    {
        var instanceType = parametersType.IsParametersFor() ?? throw new ArgumentException("Type must implement IParametersFor<T>.");

        List<PropertyInfo>? unorderedProperties = null;
        SortedList<float, PropertyInfo>? orderedProperties = null;

        foreach (var pi in parametersType
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

        var slotsByName = new Dictionary<string, SlotInfo>();
        var slots = new List<SlotInfo>();

        void addRange(IEnumerable<PropertyInfo> propertyInfos)
        {
            foreach (var pi in propertyInfos)
            {
                var sourceProperty = parametersType.GetProperty(pi.Name + "Source");
                if (sourceProperty != null)
                {
                    if (sourceProperty.PropertyType != typeof(SlotSource))
                    {
                        sourceProperty = null;
                    }
                }

                var instanceProperty = ProcessorValuesWindow.TryGetInstanceProperty(instanceType, pi.Name);

                var slotInfo = new SlotInfo
                {
                    ParameterProperty = pi,
                    SourceProperty = sourceProperty,
                    ProcessorValuesWindowProperty = instanceProperty,
                };
                slots.Add(slotInfo);
                slotsByName.Add(pi.Name, slotInfo);
            }
        }

        if (orderedProperties != null)
        {
            addRange(orderedProperties.TakeWhile(p => p.Key < 0f).Select(kvp => kvp.Value));
        }
        if (unorderedProperties != null)
        {
            addRange(unorderedProperties.OrderBy(p => p.Name));
        }
        if (orderedProperties != null)
        {
            addRange(orderedProperties.SkipWhile(p => p.Key < 0f).Select(kvp => kvp.Value));
        }

        Slots = slots;
        SlotsByName = slotsByName;
    }

    #endregion

    public Dictionary<string, SlotInfo> SlotsByName { get; }
    public List<SlotInfo> Slots { get; }
}
