using System.Collections.Concurrent;
using System.Reflection;

namespace LionFire.Trading.DataFlow; // TODO: Move to .DataFlow namespace

public interface IHasSignalInfo
{
    IReadOnlyList<SignalInfo> GetSignalInfos();
}

public static class SignalInfoReflection
{
    public static IReadOnlyList<SignalInfo> GetSignalInfos(Type type)
    {
        if(type is IHasSignalInfo hasSignalInfo) { return hasSignalInfo.GetSignalInfos(); }

        return signalInfos.GetOrAdd(type, t =>
        {
            var list = new List<SignalInfo>();

            foreach (var pi in type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => InputSlotsReflection.IsInputSlotType(p.PropertyType)))
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

}


