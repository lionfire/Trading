using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class Positions : List<PositionDouble>, IPositionsDouble
    {
        public event Action<PositionDoubleClosedEventArgs> Closed;

        public event Action<PositionDoubleOpenedEventArgs> Opened;

        public PositionDouble Find(string label, Symbol symbol)
        {
            return this.Where(p => p.Label == label && p.SymbolCode == symbol.Code).FirstOrDefault();
        }
    }
    public class Positions2 : List<IPosition>, IPositions
    {
        public event Action<PositionClosedEventArgs2> Closed;

        public event Action<PositionOpenedEventArgs2> Opened;

        public IPosition Find(string label, Symbol symbol)
        {
            return this.Where(p => p.Label == label && p.SymbolCode == symbol.Code).FirstOrDefault();
        }
    }
}
