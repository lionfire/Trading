//#if NET462
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using LionFire.Trading.Spotware.Connect;
//using Microsoft.Extensions.DependencyInjection;

//namespace LionFire.Applications.Hosting
//{
//    public class SpotwareConnectTask : AppTask
//        //, IAppConfigurer
//    {
//        CTraderAccount market = new CTraderAccount();

//        public string ConfigName { get; set; }

//        public SpotwareConnectTask(string configName = null) : base()
//        {
//            ConfigName = configName;
            
//            TryInitializeAction = _TryInitialize;
//        }

//        private bool _TryInitialize()
//        {
//            market.ConfigName = ConfigName;
//            if (!market.TryInitialize()) return false;

//            return true;
//        }

//        //public void Config(IAppHost app)
//        //{            
//        //}
//    }
//}
//#endif