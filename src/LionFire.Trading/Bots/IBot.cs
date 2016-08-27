#if cAlgo
using cAlgo.API;
using cAlgo.API.Internals;
#endif
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    public interface IBot
    {
        BotConfig BotConfig { get; set; }


        TradeResult ClosePosition(Position position);
        TradeResult ModifyPosition(Position position, double? StopLoss, double? TakeProfit);

        Microsoft.Extensions.Logging.ILogger Logger { get; }
    }
}
