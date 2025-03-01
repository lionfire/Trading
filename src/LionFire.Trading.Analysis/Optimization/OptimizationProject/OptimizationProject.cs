using DynamicData;
using LionFire.Base;
using LionFire.Mvvm;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading;

/// <summary>
/// Optimization project document
/// - 1 bot type
/// - snapshot of analysis
/// - via DI, OptimizationViewModel can retrieve all available Optimization Runs
///   - some runs may no longer be available
/// </summary>
public partial class OptimizationProject : ReactiveObject
{

    #region Lifecycle

    public OptimizationProject()
    {
        BotTypesList = [];
    }

    #endregion

    #region Properties

    public SourceCache<BotTypeReference, string> BotTypes { get; private set; }
    public List<BotTypeReference> BotTypesList { get => BotTypes?.Items ?? []; set => BotTypes = new SourceCache<BotTypeReference, string>(x => x.Key); }

    //public SourceCache<string, string> Assemblies { get; private set; } // FUTURE

    public SourceCache<IOptimizationProjectItem, string> Items { get; set; }

    #endregion

    #region (static)

    public static OptimizationProject CreateSample()
    {
        var project = new OptimizationProject();

        //project.Items.AddOrUpdate("test1");

        return project;
    }
    
    #endregion

}

public class BotAnalysisSnapshot
{
}
