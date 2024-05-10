namespace LionFire.Trading.Automation;

public interface IPMarketProcessor
{
    object[]? Inputs { get; }
    int[]? InputMemories { get; set; }

    Type InstanceType { get; }

}

// TODO: Also have indicators derive from this?
public abstract class PMarketProcessor : IPMarketProcessor
{
    public object[]? Inputs { get; set; }
    public int[]? InputMemories { get; set; }

    public abstract Type InstanceType { get; }
}

public abstract class PBot2<TConcrete> : PMarketProcessor, IPBotHierarchical2
{
    public IEnumerable<IPBot2> Children => getChildren(this);

    #region (static)

    static PBot2()
    {
        var propertyInfos = typeof(TConcrete).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(p => p.PropertyType.IsAssignableTo(typeof(IPBotHierarchical2)));

        getChildren = pBot =>
        {
            return propertyInfos.Select(pi => pi.GetValue(pBot) as IPBotHierarchical2).Where(p => p != null)!;
        };
    }
    static Func<PBot2<TConcrete>, IEnumerable<IPBotHierarchical2>> getChildren;

    #endregion

}
