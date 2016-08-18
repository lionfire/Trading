using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    [Flags]
    public enum ValueCondition
    {
        Unspecified = 0,
        Equal = 1 << 1,
        Greater = 1 << 0,
        Lower = 1 << 1,
        GreaterOrEqual = Greater | Equal,
        LowerOrEqual = Lower | Equal,
    }
}
