using LionFire.Structures;

namespace LionFire.Trading.Automation;

// TODO: Also have indicators derive from this?
// TODO: Subclass for unbound inputs?
public abstract class PMarketProcessor : IPMarketProcessor
{
    //public IPInput[]? Inputs { get;  }
    public virtual IKeyed<string>[] DerivedInputs => [];

    public int[]? InputLookbacks { get; set; }

    public abstract Type InstanceType { get; }
    public abstract Type MaterializedType { get; }


}
