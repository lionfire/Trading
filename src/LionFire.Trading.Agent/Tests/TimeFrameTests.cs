using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Tests
{
    public static class TimeFrameTests
    {
        public static void TimeFrame_MoreGranular_Test()
        {
            Console.WriteLine("TimeFrame MoreGranular test:");
            // FUTURE: 
            for (var tf = TimeFrame.h4; tf != null; tf = tf.MoreGranular())
            {
                Console.WriteLine($" - {tf}");
            }
        }
    }
}
