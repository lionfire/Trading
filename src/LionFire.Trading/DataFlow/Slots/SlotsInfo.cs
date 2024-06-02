using System.Collections.Concurrent;

namespace LionFire.Trading.DataFlow;

public class SlotsInfo
{
    #region (static)

    public static SourcesInfo GetSlotsInfo(Type type) => dict.GetOrAdd(type, t => new SourcesInfo(t));

    static ConcurrentDictionary<Type, SourcesInfo> dict = new();

    #endregion

    public SlotsInfo(Type type)
    {
    }
}
