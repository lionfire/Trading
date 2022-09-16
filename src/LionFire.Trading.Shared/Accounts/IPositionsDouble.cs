using System;
using System.Collections.Generic;
#if cAlgo
using PositionDouble = cAlgo.API.Position;
using Symbol = cAlgo.API.Internals.Symbol;
#endif
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{

    public class PositionDoubleClosedEventArgs
    {
        public PositionDoubleClosedEventArgs(PositionDouble position, PositionCloseReason reason)
        {
            this.Position = position;
            this.Reason = reason;
        }

        public PositionDouble Position { get; set; }
        public PositionCloseReason Reason { get; set; }
    }
    

    public class PositionDoubleOpenedEventArgs
    {
        public PositionDouble Position { get; set; }
    }
    

    public interface IPositionsDouble : IReadOnlyList<PositionDouble>
    {
        PositionDouble Find(string label, Symbol symbol);

         event Action<PositionDoubleClosedEventArgs> Closed;

         event Action<PositionDoubleOpenedEventArgs> Opened;
    }
}
