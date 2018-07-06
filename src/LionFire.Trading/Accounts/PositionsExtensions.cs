using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public static class PositionsExtensions
    {
        public static double GetNetVolume(this Positions positions)
        {
            double volume = 0;
            foreach (var p in positions)
            {
                if (p.TradeType == TradeType.Buy)
                {
                    volume += p.Volume;
                }
                else
                {
                    volume -= p.Volume;
                }
            }
            return volume;
        }
    }
}
