using LionFire.Structures;

namespace LionFire.Trading;

public class BotTypeReference : IKeyed<string>
{
    public string? TypeName { get; set; }
    public string? AssemblyName { get; set; }
    public string? AssemblyVersion { get; set; }

    /// <summary>
    /// Based on TypeName, AssemblyName, AssemblyVersion
    /// </summary>
    public string Key =>
        $"{TypeName};{AssemblyName};{AssemblyVersion}".Trim().TrimEnd(';');
}
