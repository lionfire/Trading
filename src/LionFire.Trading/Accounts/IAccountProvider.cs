using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Applications
{
    public interface IAccountProvider
    {
        IAccount GetAccount(string configName);
    }
    
}
