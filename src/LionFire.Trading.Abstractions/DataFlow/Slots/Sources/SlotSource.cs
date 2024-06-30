namespace LionFire.Trading.DataFlow;

// TODO REVIEW - not implemented, needs work
public readonly record struct SlotSource(IPInput? Input)
{
    public readonly int? SourceIndex;
    public readonly string? SourceName;

    public bool IsSet => Input != null;
}

