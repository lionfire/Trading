namespace LionFire.Trading.HistoricalData;

[Flags]
public enum HistoricalDataSourceKind
{
    Unspecified = 0,
    InMemory = 1 << 0,
    LocalDisk = 1 << 1,
    LocalNetwork = 1 << 2,
    Exchange = 1 << 3,

    Local = InMemory | LocalDisk | LocalNetwork,
    All = InMemory | LocalDisk | LocalNetwork | Exchange,
}


[Flags]
public enum HistoricalDataSourceKind2
{
    Unspecified = 0,
    Local = 1 << 0,
    FromSource = 1 << 1,
    Compound = 1 << 8,
}