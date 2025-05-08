using Newtonsoft.Json;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Xml.Linq;

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

    public TimeFrame? TimeFrame { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset EndExclusive { get; set; }
    public ExchangeSymbol ExchangeSymbol { get; set; }
    public bool TicksEnabled { get; set; }

    public string? BotName { get; set; }
    
    /// <summary>
    /// For the assembly containing the Parameters type (PBot), and probably also the runtime type.
    /// </summary>
    public string? BotAssemblyNameString { get; set; }

    #region (Derived)

    [Ignore]
    [Newtonsoft.Json.JsonIgnore]
    public AssemblyName? BotAssemblyName =>
        BotAssemblyNameString == null ? null : new AssemblyName(BotAssemblyNameString);

    #endregion

    public DateTime BacktestExecutionDate { get; set; }
    public string? MachineName { get; set; }

    // TODO - new:

    //public BotVersion BotVersion { get; set; }
    public DateTime BotBuildDate { get; set; }

    /// <summary>
    /// Most recent version of all supporting first-party DLLs (LionFire namespace)
    /// </summary>
    public DateTime EngineBuildDate { get; set; }

}

