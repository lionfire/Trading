using Newtonsoft.Json;
using System.Reflection;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Automation;

public class BacktestBatchInfo
{

    public BacktestBatchInfo() { }

    public TimeFrame? TimeFrame { get; set; }
    public DateTimeOffset Start { get; set; }
    public DateTimeOffset EndExclusive { get; set; }
    public bool TicksEnabled { get; set; }

    /// <summary>
    /// For the assembly containing the Parameters type (PBot), and probably also the runtime type.
    /// </summary>
    public string BotAssemblyNameString { get; set; }

    #region (Derived)

    [Ignore]
    [Newtonsoft.Json.JsonIgnore]
    public AssemblyName BotAssemblyName =>
        new AssemblyName(BotAssemblyNameString);

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

