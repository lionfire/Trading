using LionFire.Reflection;
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


    public string? Guid
    {
        get => guid;
        set
        {
            if (guid != default) throw new AlreadySetException();
            guid = value;
        }
    }
    private string? guid;

    #region Equality

    public override bool Equals(object? obj) => obj is OptimizationRunInfo other && Equals(other);

    public bool Equals(OptimizationRunInfo? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Guid == other.Guid && Guid != default;
    }
    public static bool operator ==(OptimizationRunInfo? left, OptimizationRunInfo? right) => left?.Equals(right) ?? right is null;
    public static bool operator !=(OptimizationRunInfo? left, OptimizationRunInfo? right) => !(left == right);
    public override int GetHashCode() => (Guid ?? "").GetHashCode();

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
