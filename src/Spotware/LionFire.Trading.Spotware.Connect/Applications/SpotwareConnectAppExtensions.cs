using LionFire.Applications.Hosting;
using LionFire.Structures;
using LionFire.Trading.Spotware.Connect;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Assets;

namespace LionFire.Applications.Hosting
{

    public static class SpotwareConnectAppExtensions
    {
        //        public static IAppHost AddSpotwareConnectAccount(this IAppHost app, string accountName = null)
        //        {
        //#if NET462
        //            var a = new SpotwareConnectTask(accountName);
        //            app.Add(a);
        //            return app;
        //#else
        //            throw new Exception("SpotwareConnectAccount only supported in .NET Framework");
        //#endif
        //        }

        //public static IAppHost AddSpotwareConnect(this IAppHost host)
        //{
        //    throw new NotImplementedException("TODO: Find a way to add named instances that implement an interface.");
        //    //host.AddConfig(app => app.ServiceCollection.AddSingleton<IMarketProvider, MarketProvider>());
        //    //return host;
        //}

        public static IAppHost AddSpotwareConnectClient(this IAppHost host, string clientConfigName)
        {

            var info = clientConfigName.Load<SpotwareConnectAppInfo>();

            Defaults.Set<ISpotwareConnectAppInfo>((ISpotwareConnectAppInfo)info);

            return host;
        }
    }
}
