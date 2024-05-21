namespace LionFire.Trading.Automation;

public interface IPMarketProcessor
{
    object[]? Inputs { get; }
    int[]? InputLookbacks { get; set; }

    Type InstanceType { get; }

}

// TODO: Also have indicators derive from this?
public abstract class PMarketProcessor : IPMarketProcessor
{
    public object[]? Inputs { get; set; }
    public virtual object[] DerivedInputs => [];

    public int[]? InputLookbacks { get; set; }

    public abstract Type InstanceType { get; }
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
