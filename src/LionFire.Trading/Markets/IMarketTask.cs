using LionFire.Trading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Applications.Trading
{
    public interface IMarketTask
    {
        IAccount_Old Account { get; }
    }
}
