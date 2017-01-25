using LionFire.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IAccountParticipant
#if !cAlgo
        : IHasExecutionStateFlags, IAcceptsExecutionStateFlags
#endif
    {
#if !cAlgo
        IAccount Account { get; set; }
#endif
    }

}
