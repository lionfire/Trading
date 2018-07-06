using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class Positions : List<Position>, IPositions
    {
        public event Action<PositionClosedEventArgs> Closed;

        public event Action<PositionOpenedEventArgs> Opened;

        public Position Find(string label, Symbol symbol)
        {
            return this.Where(p => p.Label == label && p.SymbolCode == symbol.Code).FirstOrDefault();
        }
    }
}
