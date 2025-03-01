using LionFire.Structures;

namespace LionFire.Trading;

public interface IOptimizationProjectItem :  IKeyable<string>
{
    bool IsUpToDate { get; set; }

     bool IsBuilding { get; set; }
}
