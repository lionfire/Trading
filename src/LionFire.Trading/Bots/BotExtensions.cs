//#if cAlgo
//using cAlgo.API.Internals;
//using BotType = cAlgo.API.Robot;
//#else
//using LionFire.Trading.Bots;
//    using BotType = LionFire.Trading.Bots.IBot;
//#endif
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace LionFire.Trading
//{
//    public static class BotExtensions
//    {
//        public static DateTime GetExtrapoloatedServerTime(this BotType bot)
//        {
//#if cAlgo
//                return bot.Server.Time;
//#else
//            return bot.Account.ExtrapolatedServerTime;
//#endif
//        }
//    }
//}
