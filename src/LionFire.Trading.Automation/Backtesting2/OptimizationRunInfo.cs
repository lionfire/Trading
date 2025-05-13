using Newtonsoft.Json;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;

namespace LionFire.Trading.Automation;

public class OptimizationRunInfo : IEquatable<OptimizationRunInfo>
{
    //public OptimizationRunReference Reference
    //{
    //    get
    //    {
    //        if (Guid == Guid.Empty) throw new InvalidOperationException("Guid is empty");
    //        return new OptimizationRunReference(
    //            BotAssemblyNameString, 

    //            Guid);
    //    }
    //}


    public Guid Guid { get; init; }

    #region Equality

    public override bool Equals(object? obj) => obj is OptimizationRunInfo other && Equals(other);

    public bool Equals(OptimizationRunInfo? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Guid == other.Guid && Guid != Guid.Empty;
    }
    public static bool operator ==(OptimizationRunInfo? left, OptimizationRunInfo? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(OptimizationRunInfo? left, OptimizationRunInfo? right) => !(left == right);
    public override int GetHashCode() => Guid.GetHashCode();

    #endregion

    public OptimizationRunInfo() { }

    [JsonIgnore]
    public TimeFrame? TimeFrame { get; set; }

    public string? TimeFrameString { get => TimeFrame?.ToShortString(); set => TimeFrame = value == null ? null : TimeFrame.Parse(value); }

    public DateTimeOffset Start { get; set; }
    public DateTimeOffset EndExclusive { get; set; }
    public string? Exchange { get; set; }
    public string? ExchangeArea { get; set; }
    public string? Symbol { get; set; }

    [JsonIgnore]
    public ExchangeSymbolTimeFrame? ExchangeSymbolTimeFrame
    {
        get => new ExchangeSymbolTimeFrame(Exchange, ExchangeArea, Symbol, TimeFrame);
        set => (Exchange, ExchangeArea, Symbol, TimeFrame) = value == null ? (null, null, null, null) : (value.Exchange, value.ExchangeArea, value.Symbol, value.TimeFrame);
    }

    public bool TicksEnabled { get; set; }

    public string? BotName { get; set; }
    public string? BotTypeName { get; set; }

    /// <summary>
    /// For the assembly containing the Parameters type (PBot), and probably also the runtime type.
    /// </summary>
    public string? BotAssemblyNameString { get; set; }

    #region (Derived)

    [Ignore]
    [System.Text.Json.Serialization.JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public AssemblyName? BotAssemblyName =>
        BotAssemblyNameString == null ? null : new AssemblyName(BotAssemblyNameString);

    #endregion

    public DateTime OptimizationExecutionDate { get; set; }
    public string? MachineName { get; set; }

    // TODO - new:

    //public BotVersion BotVersion { get; set; }
    public DateTime BotBuildDate { get; set; }

    /// <summary>
    /// Most recent version of all supporting first-party DLLs (LionFire namespace)
    /// </summary>
    public DateTime EngineBuildDate { get; set; }

    public void TryHydrateBuildDates(Type pBotType)
    {
        // ENH: fall back to GetAssemblyBuildDate?
        EngineBuildDate = AssemblyVersionUtils.GetLinkerTimestamp(this.GetType());
        BotBuildDate = AssemblyVersionUtils.GetLinkerTimestamp(pBotType);
    }
}



public static class AssemblyVersionUtils
{
    // Based on Grok response
    public static DateTime GetAssemblyBuildDate(Type type)
    {
        // Get the assembly containing the type
        Assembly assembly = type.Assembly;

        // Get the file path of the assembly
        string assemblyPath = new Uri(assembly.Location).LocalPath;

        // Get the last write time of the assembly file
        FileInfo fileInfo = new FileInfo(assemblyPath);
        return fileInfo.LastWriteTime;
    }

    // Based on Grok response
    public static DateTime GetLinkerTimestamp(Type type)
    {
        // Get the assembly containing the type
        Assembly assembly = type.Assembly;

        // Get the file path of the assembly
        string assemblyPath = new Uri(assembly.Location).LocalPath;

        try
        {
            // Read the PE header timestamp
            using (var stream = new FileStream(assemblyPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var reader = new BinaryReader(stream))
            {
                // Seek to the DOS header
                stream.Seek(0, SeekOrigin.Begin);

                // Read DOS header signature (MZ)
                if (reader.ReadUInt16() != 0x5A4D) // 'MZ'
                    throw new InvalidOperationException("Invalid PE file: Missing MZ signature.");

                // Seek to the PE header offset (stored at offset 0x3C)
                stream.Seek(0x3C, SeekOrigin.Begin);
                uint peHeaderOffset = reader.ReadUInt32();

                // Seek to the PE header
                stream.Seek(peHeaderOffset, SeekOrigin.Begin);

                // Read PE signature
                if (reader.ReadUInt32() != 0x00004550) // 'PE\0\0'
                    throw new InvalidOperationException("Invalid PE file: Missing PE signature.");

                // Skip COFF header (20 bytes) to reach the optional header
                stream.Seek(20, SeekOrigin.Current);

                // Read the linker timestamp (offset 8 in the optional header for both PE32 and PE32+)
                uint timestamp = reader.ReadUInt32();

                // Convert timestamp to DateTime (Unix epoch, seconds since 1970-01-01)
                DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                return epoch.AddSeconds(timestamp).ToLocalTime();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to read linker timestamp from PE header.", ex);
        }
    }
}
