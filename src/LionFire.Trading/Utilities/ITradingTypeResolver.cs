using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface ITradingTypeResolver
    {
        Type GetTemplateType(string type);
        Type GetType(string type);
        IFeed CreateAccount(string name);
    }
}
