using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    // For cAlgo compatibility - TODO: Only compile for cAlgo
    public class Server
    {
        IAccount account;
        public Server(IAccount account)
        {
            this.account = account;
        }

        #region Time

        public DateTime Time
        {
            get { return account.ServerTime; }
            //set {
            //    time = value;
            //    LocalDelta = DateTime.UtcNow - value;
            //}
        }
        //private DateTime time;

        #endregion

    }
}
