namespace LionFire.Trading.Automation;

public class PBot2<TConcrete> : IPBotHierarchical2
{
    public IEnumerable<IPBot2> Children => getChildren(this);


    static PBot2()
    {
        var propertyInfos = typeof(TConcrete).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Where(p => p.PropertyType.IsAssignableTo(typeof(IPBotHierarchical2)));

        getChildren = pBot =>
        {
            return propertyInfos.Select(pi => pi.GetValue(pBot) as IPBotHierarchical2).Where(p => p != null)!;
        };
    }
    static Func<PBot2<TConcrete>, IEnumerable<IPBotHierarchical2>> getChildren;
}
