using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace LionFire.Trading.DataFlow; // TODO: Move to .DataFlow namespace

public readonly record struct SignalInfo(PropertyInfo PropertyInfo);

public static class InputSlotsReflection
{

    public static IReadOnlyList<SignalInfo> GetSignalInfos(Type type)
    {
        return signalInfos.GetOrAdd(type, t =>
        {
            var list = new List<SignalInfo>();

            foreach (var pi in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => IsInputSlotType(p.PropertyType)))
            {
                list.Add(new SignalInfo(pi));
            }

            // FUTURE: Order the inputs based on Order attribute
            // FUTURE: Augment SignalInfo with info from some sort of attribute
            // FUTURE: If type implements static interface, use that to get the signals

            return list;
        });
    }
    private static ConcurrentDictionary<Type, IReadOnlyList<SignalInfo>> signalInfos = new();

    public static IReadOnlyList<InputSlot> GetInputSlots(Type type)
    {
        return inputSlots.GetOrAdd(type, t =>
        {
            var list = new List<InputSlot>();

            foreach (var pi in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => IsInputSlotType(p.PropertyType)))
            {
                var inputSlot = new InputSlot
                {
                    Name = pi.Name,
                    ValueType = IReferenceToX.GetTypeOfReferenced(pi.PropertyType),
                };
                list.Add(inputSlot);
            }

            // FUTURE: Order the inputs based on Order attribute
            // FUTURE: Augment InputSlot with info from some sort of attribute
            // FUTURE: If type implements static interface, use that to get the slots

            return list;
        });
    }
    private static ConcurrentDictionary<Type, IReadOnlyList<InputSlot>> inputSlots = new();

    public static bool IsInputSlotType(Type type) => IsMarketData(type);

    public static bool IsMarketData(Type type)
    {
        //if (type.IsAssignableTo(typeof(IPInput))) return true; // TODO REVIEW - Uncomment this?

        while (type.IsConstructedGenericType) { type = type.GetGenericTypeDefinition(); }

        if (type == typeof(HLCReference<>)) return true;
        if (type == typeof(SymbolValueAspect<>)) return true;
        
        return false;
    }
}


