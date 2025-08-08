using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation;

public abstract class MarketParticipantBase<TPrecision> : IMarketParticipant<TPrecision>
        where TPrecision : struct, INumber<TPrecision>
{
}
