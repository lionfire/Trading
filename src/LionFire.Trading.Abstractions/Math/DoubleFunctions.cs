using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class DoubleFunctions
    {
        public static Func<double, double> Linear = x => x;

        public static double Lerp(double @from, double to, double minX, double maxX, double x)
        {
            var xAbsProgress = x - minX;
            var totalProgressPossible = maxX - minX;
            var progressPercent = xAbsProgress / totalProgressPossible;
            return Lerp(@from, to, progressPercent);
        }
        public static double Lerp(double @from, double to, double progressPercent)
        {
            progressPercent = Math.Max(0, progressPercent);
            progressPercent = Math.Min(1, progressPercent);
            return @from * (1 - progressPercent) + to * progressPercent;
        }
    }
}
