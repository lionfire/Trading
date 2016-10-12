using LionFire.Applications;
using LionFire.Applications.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Applications
{
    public static class TradingAppExtensions
    {
        public static IAppHost AddTrading(this IAppHost host, TradingOptions options)
        {
            host.AddConfig(app =>
            {
                //app.ServiceCollection.AddSingleton<IAccountProvider, AccountProvider>(); FUTURE
                app.ServiceCollection.AddSingleton<ITradingContext>(new TradingContext(options));
            });

            return host;
        }
    }
}
