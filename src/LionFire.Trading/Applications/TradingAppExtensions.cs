using LionFire.Applications;
using LionFire.Applications.Hosting;
using LionFire.Execution;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using LionFire.Instantiating;
using LionFire.Structures;
using System.Reflection;
using LionFire.Types;
using LionFire.DependencyInjection;

namespace LionFire.Trading.Applications
{
    //public enum AssemblyFlags
    //{
    //    None = 0,

    //    /// <summary>
    //    /// Assembly contains data types used in serialization
    //    /// </summary>
    //    Data = 1 << 0,

    //    All = Data,
    //}

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
            // FUTURE: find another way to get this to app during ConfigureServices
            TypeNamingContext tnc = ManualSingleton<TypeNamingContext>.GuaranteedInstance;
            tnc.UseShortNamesForDataAssemblies = true;

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

                //InjectionContext.SetSingletonDefault<TypeNamingContext>(tnc);  RECENTCHANGE - should no lonber be needed now that the app's IServiceProvider is used as the default for InjectionContext.

                serviceCollection.AddSingleton<TypeNamingContext>(tnc);

            });
            

            AssetInstantiationStrategy.Enable();

            return host;
        }

    }
}
