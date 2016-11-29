using LionFire.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IMarketParticipant 
    {
        IAccount Account { get; set; }
    }

}
