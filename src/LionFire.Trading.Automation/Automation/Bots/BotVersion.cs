namespace LionFire.Trading.Automation;

public struct BotVersion
{
    public ushort Major { get; set; }
    public ushort Minor { get; set; }
    public ushort Micro { get; set; }
    public ushort Patch { get; set; }
    public string? Suffix { get; set; }

    public bool IsSet => Major != 0 || Minor != 0 || Micro != 0 || Patch != 0;
}

