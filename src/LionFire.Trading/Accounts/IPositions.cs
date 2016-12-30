using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IPositions : IReadOnlyList<Position>
    {
        Position Find(string label, Symbol symbol);
    }
}
