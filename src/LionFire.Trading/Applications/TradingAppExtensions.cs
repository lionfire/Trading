using LionFire.Applications;
using LionFire.Applications.Hosting;
using LionFire.Execution;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LionFire.Trading.Applications
{
    public static class TradingAppExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        /// <param name="options"></param>
        /// <param name="accountModes">Override options with this mode (if not unspecified)</param>
        /// <returns></returns>
        public static IAppHost AddTrading(this IAppHost host, TradingOptions options, AccountMode accountModes = AccountMode.Unspecified)
        {
            host.ConfigureServices(serviceCollection =>
            {
                //var Configuration = LionFire.Structures.ManualSingleton<IConfigurationRoot>.Instance;
                //serviceCollection.Configure<TradingOptions>(opt => Configuration.GetSection("Trading").Bind(opt));

                if (accountModes != AccountMode.Unspecified)
                {
                    options.AccountModes = accountModes;
                }
                //app.ServiceCollection.AddSingleton<IAccountProvider, AccountProvider>(); FUTURE
                serviceCollection.AddSingleton<ITradingContext>(new TradingContext(options));
            });

            return host;
        }
    }
}
