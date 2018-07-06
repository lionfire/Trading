using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public enum PositionCloseReason
    {
        Closed = 0,
        StopLoss = 1,
        TakeProfit = 2,
        StopOut = 3,
    }

    public class PositionClosedEventArgs
    {
        public PositionClosedEventArgs(Position position, PositionCloseReason reason)
        {
            this.Position = position;
            this.Reason = reason;
        }

        public Position Position { get; set; }
        public PositionCloseReason Reason { get; set; }
    }

    public class PositionOpenedEventArgs
    {
        public Position Position { get; set; }
    }

    public interface IPositions : IReadOnlyList<Position>
    {
        Position Find(string label, Symbol symbol);

         event Action<PositionClosedEventArgs> Closed;

         event Action<PositionOpenedEventArgs> Opened;
    }
}
