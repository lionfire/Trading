using LionFire.Assets;
using LionFire.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{

    //http://stackoverflow.com/a/19271062/208304
    public static class StaticRandom
    {
        static int seed = Environment.TickCount;

        static readonly ThreadLocal<Random> random =
            new ThreadLocal<Random>(() => new Random(Interlocked.Increment(ref seed)));

        public static int Next(int minValue, int maxValue)
        {
            return random.Value.Next(minValue, maxValue);
        }
    }

    public static class IdUtils
    {
        public static int DefaultIdLength = 12;


        public static string GenerateId(int length = 0)
        {
            if (length == 0) length = DefaultIdLength;



            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                var n = StaticRandom.Next(0, 36);
                if (n <= 25)
                {
                    chars[i] = (char)((int)'a' + n);
                }
                else
                {
                    chars[i] = (char)((int)'0' + n - 26);
                }
            }
            return new string(chars);
        }
    }

    [Flags]
    public enum BotMode
    {
        None = 0,
        Live = 1 << 0,
        Demo = 1 << 1,
        Scanner = 1 << 2,
        Paper = 1 << 3,
    }

    [AssetPath("Algos")]
    public class TBot : ITemplate
    {
        public string Id { get; set; } = IdUtils.GenerateId();

        public string Account { get; set; }

        //[Ignore]
        public string Symbol
        {
            get { return Symbols?.FirstOrDefault(); }
            set { Symbols = new List<string> { value }; }
        }
        public List<string> Symbols { get; set; }

        //[Ignore]
        public string TimeFrame
        {
            get { return TimeFrames?.FirstOrDefault(); }
            set { TimeFrames = new List<string> { value }; }
        }
        public List<string> TimeFrames { get; set; }


        public bool Log { get; set; } = false;

        public string LogFile { get; set; } = "e:/temp/Trading.cAlgo.log";

        [Parameter("Log Backtest Fitness Min", DefaultValue = 2.0)]
        public double LogBacktestThreshold { get; set; }

        //[Parameter("Log Backtest", DefaultValue = true)]
        //public bool LogBacktest { get; set; }

        [Parameter("Log Backtest Trades", DefaultValue = false)]
        public bool LogBacktestTrades { get; set; }

        public bool UseTicks { get; set; } = false;

        /// <summary>
        /// In units of quantity, not volume
        /// </summary>
        public long MinPositionSize { get; set; }

        public bool ScalePositionSizeWithEquity { get; set; } = true;

        ///// <summary>
        ///// Uses Equity balance but Symbol pricing -- may use different currencies!!
        ///// </summary>
        //public double PositionRiskPricePercent { get; set; }

        public double PositionRiskPercent { get; set; }

        public double PositionPercentOfEquity { get; set; }

        public bool AllowLong { get; set; } = true;
        public bool AllowShort { get; set; } = true;

        /// <summary>
        /// 0 or MaxValue means no limit
        /// </summary>
        public int MaxOpenPositions { get; set; } = int.MaxValue;
        public int MaxLongPositions { get; set; } = 1;
        public int MaxShortPositions { get; set; } = 1;

        public double BacktestProfitTPMultiplierOnSL { get; set; } = 0.0;
        public double BacktestMinTradesPerMonth { get; set; } = 0;


        //public double MaxEquityExposurePercent { get; set; } = 0.0;

        //public bool UseTakeProfit {
        //    get {
        //        throw new NotImplementedException();
        //    }

        //    set {
        //        throw new NotImplementedException();
        //    }
        //}

    }
}
