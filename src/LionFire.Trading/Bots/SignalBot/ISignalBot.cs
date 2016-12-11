using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LionFire.Trading.Indicators;

namespace LionFire.Trading.Bots
{
    public interface ISignalBot : IBot
    {
        ISignalIndicator Indicator { get; }
        event Action Evaluated;
    }
}
