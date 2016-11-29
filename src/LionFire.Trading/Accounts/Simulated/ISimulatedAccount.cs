using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface ISimulatedAccount  : IAccount
    {
        GetFitnessArgs GetFitnessArgs();

        TimeFrame SimulationTimeStep { get; set; }
        //event Action SimulationTickFinished;
    }
}
