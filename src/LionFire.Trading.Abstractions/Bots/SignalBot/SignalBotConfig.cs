using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    public interface ISymbolTimeFrameCode // MOVE to separate file, link from cTrader project
    {
        string SymbolCode { get; set; }
        string TimeFrameCode { get; set; }
    }

    public class SingleSeriesSignalBotConfig<TIndicatorConfig> : SignalBotConfig<TIndicatorConfig>, ISymbolTimeFrameCode // MOVE to separate file, link from cTrader project
        where TIndicatorConfig : class
    {
        public SingleSeriesSignalBotConfig() { }
        public SingleSeriesSignalBotConfig(string symbolCode, string timeFrameCode) {
            this.SymbolCode = symbolCode;
            this.TimeFrameCode = timeFrameCode;
        }

        public string SymbolCode { get; set; }
        public string TimeFrameCode { get; set; }
    }

    public class SignalBotConfig<TIndicatorConfig> : BotConfig
        where TIndicatorConfig : class
    {
        
        public double PointsToLong { get; set; } = 1.0;
        public double PointsToShort { get; set; } = 1.0;


        public TIndicatorConfig Indicator { get; set; }



    }
}
