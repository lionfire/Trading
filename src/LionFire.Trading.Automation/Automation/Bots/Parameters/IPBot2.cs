namespace LionFire.Trading.Automation;

public interface IPBot2 : IPMarketProcessor
{
    Type MaterializedType { get; }
    //static abstract Type MaterializedTypeStatic { get; } // TODO: Replace non-static version with this?
}

public interface IPBot2Static // TODO: Merge into IPBot2
{
    static abstract Type StaticMaterializedType { get; }
}
