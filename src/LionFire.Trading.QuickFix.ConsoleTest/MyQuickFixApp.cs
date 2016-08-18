using QuickFix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.QuickFixN.ConsoleTest
{
    public class MyQuickFixApp : MessageCracker, IApplication
    {
        public void FromApp(Message msg, SessionID sessionID) {
            Crack(msg, sessionID);
        }
        public void OnCreate(SessionID sessionID) { }
        public void OnLogout(SessionID sessionID) { }
        public void OnLogon(SessionID sessionID) { }
        public void FromAdmin(Message msg, SessionID sessionID) { }
        public void ToAdmin(Message msg, SessionID sessionID) { }
        public void ToApp(Message msg, SessionID sessionID) { }

    //    public void OnMessage(
    //QuickFix.FIX44.NewOrderSingle ord,
    //SessionID sessionID)
    //    {
    //        ProcessOrder(ord.Price, ord.OrderQty, ord.Account);
    //    }

    }
}
