using System;
using System.Collections.Generic;

namespace LionFire.Trading
{
    public interface IPositions : IReadOnlyList<IPosition>
    {
        IPosition Find(string label, Symbol symbol);

        event Action<PositionClosedEventArgs2> Closed;

        event Action<PositionOpenedEventArgs2> Opened;
    }

    public class PositionClosedEventArgs2
    {
        public PositionClosedEventArgs2(IPosition position, PositionCloseReason reason)
        {
            this.Position = position;
            this.Reason = reason;
        }

        public IPosition Position { get; set; }
        public PositionCloseReason Reason { get; set; }
    }
    public class PositionOpenedEventArgs2
    {
        public IPosition Position { get; set; }
    }

}
