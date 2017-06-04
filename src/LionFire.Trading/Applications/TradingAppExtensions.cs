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
using LionFire.Composables;

namespace LionFire.Trading
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

    //public class TradingPackage : IConfigures<IServiceCollection>, IAdding
    //{
    //    public void Configure(IServiceCollection serviceCollection)
    //    {
    //    }
    //    public bool OnAdding<T>(IComposable<T> parent)
    //    {
    //        return false;
    //    }
    //}

    public static class TradingAppExtensions
    {
        /// <summary>
        /// Used if no options are provided to the IAppHost.AddTrading() extension method.
        /// </summary>
        public static TradingOptions DefaultTradingOptions
        {
            get
            {
                return new TradingOptions
                {
                    Features = TradingFeatures.All & ~TradingFeatures.AllLive,
                };
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appHost"></param>
        /// <param name="defaultOptions"></param>
        /// <param name="accountModesAllowed">Override options with this mode (if not unspecified)</param>
        /// <returns></returns>
        public static IAppHost AddTrading(this IAppHost appHost, TradingOptions defaultOptions = null, AccountMode accountModesAllowed = AccountMode.Unspecified)
        {
            if (defaultOptions == null) { defaultOptions = DefaultTradingOptions; }

            // FUTURE: find another way to get this to app during ConfigureServices
            TypeNamingContext tnc = ManualSingleton<TypeNamingContext>.GuaranteedInstance;
            tnc.UseShortNamesForDataAssemblies = true;

            PrioritizeAndThrottleDataJobs();

            appHost.ConfigureServices((Action<IServiceCollection>)(serviceCollection =>
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

                var tradingContext = new TradingContext((TradingOptions)tradingOptions);

                //InjectionContext.Current.AddSingleton(tradingOptions);
                //InjectionContext.Current.AddSingleton(tradingContext);
                serviceCollection.AddSingleton((TradingContext)tradingContext);
                serviceCollection.AddSingleton((TradingOptions)tradingContext.Options);

                //InjectionContext.SetSingletonDefault<TypeNamingContext>(tnc);  RECENTCHANGE - should no lonber be needed now that the app's IServiceProvider is used as the default for InjectionContext.

                serviceCollection.AddSingleton(tnc);
            }));


            AssetInstantiationStrategy.Enable();

            return appHost;
        }

        public static void PrioritizeAndThrottleDataJobs(int maxJobs = 5)
        {
            var jm = Defaults.Get<JobManager>();

            //jm.AddQueueWithPrioritizer<HistoricalDataJobPrioritizer>();
            jm.AddQueue(new JobQueue
            {
                MaxConcurrentJobs = 5,
                Prioritizer = new HistoricalDataJobPrioritizer(),
            });
        }

    }
}
