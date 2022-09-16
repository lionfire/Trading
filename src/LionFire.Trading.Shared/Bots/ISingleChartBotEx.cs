using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    public interface ISingleChartBotEx : ISingleChartBot // TEMP Interface for some uncategorized advanced features
    {

        double LowChannel { get; }
        double MidChannel { get; }
        double HighChannel { get; }


        int ClosePointsBackoffFreezeBars { get; }
        double ClosePointsBackoff { get; }

        double LastEffectiveOpenLongPoints { get; }
        double EffectiveOpenLongPoints { get; }
        double LastEffectiveOpenShortPoints { get; }
        double EffectiveOpenShortPoints { get; }

        //RangedNumber TakeProfitNearChannel { get; }

        #region Configuration

        bool UseTakeProfit { get; set; }

        #endregion

    }
}
