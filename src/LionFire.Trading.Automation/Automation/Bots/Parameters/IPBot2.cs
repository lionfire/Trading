namespace LionFire.Trading.Automation;

public interface IPBot2 : IPMarketProcessor
{
}

public interface IPBot2Static // TODO: Merge into IPBot2
{
    static abstract Type StaticMaterializedType { get; }
}
