using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    public partial class SignalBotBase<TIndicator, TConfig> : BotBase<TConfig>
    {
        public MarketSeries MarketSeries { get; set; } // TODO - Single series bot?
       

        

    }
}
