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
using LionFire.Persistence;
using LionFire.Assets;
using LionFire.Execution.Jobs;
using LionFire.Trading.Data;

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
        /// <param name="defaultOptions"></param>
        /// <param name="accountModesAllowed">Override options with this mode (if not unspecified)</param>
        /// <returns></returns>
        public static IAppHost AddTrading(this IAppHost host, TradingOptions defaultOptions, AccountMode accountModesAllowed = AccountMode.Unspecified)
        {
            // FUTURE: find another way to get this to app during ConfigureServices
            TypeNamingContext tnc = ManualSingleton<TypeNamingContext>.GuaranteedInstance;
            tnc.UseShortNamesForDataAssemblies = true;

            var jm = Defaults.Get<JobManager>();

            //jm.AddQueueWithPrioritizer<HistoricalDataJobPrioritizer>();
            jm.AddQueue(new JobQueue
            {
                MaxConcurrentJobs = 5,
                Prioritizer = new HistoricalDataJobPrioritizer(),
            });

            host.ConfigureServices(serviceCollection =>
            {
                //var Configuration = LionFire.Structures.ManualSingleton<IConfigurationRoot>.Instance;
                //serviceCollection.Configure<TradingOptions>(opt => Configuration.GetSection("Trading").Bind(opt));
                
                //app.ServiceCollection.AddSingleton<IAccountProvider, AccountProvider>(); FUTURE
                var tradingOptions = "Default".Load<TradingOptions>();
                if (tradingOptions == null)
                {
                    tradingOptions = defaultOptions;
                }
                if (accountModesAllowed != AccountMode.Unspecified)
                {
                    defaultOptions.AccountModes &= accountModesAllowed;
                }
                //tradingOptions.EnableAutoSave();

                var tradingContext = new TradingContext(tradingOptions);
                InjectionContext.Current.AddSingleton(tradingOptions);
                InjectionContext.Current.AddSingleton(tradingContext);
                serviceCollection.AddSingleton(tradingContext);
                serviceCollection.AddSingleton(tradingContext.Options);

                //InjectionContext.SetSingletonDefault<TypeNamingContext>(tnc);  RECENTCHANGE - should no lonber be needed now that the app's IServiceProvider is used as the default for InjectionContext.

                serviceCollection.AddSingleton<TypeNamingContext>(tnc);

            });


            AssetInstantiationStrategy.Enable();

            return host;
        }

    }
}
