using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Applications;
using LionFire.Applications.Hosting;
using Microsoft.Extensions.DependencyInjection;
using LionFire.Trading.Proprietary.Indicators;
using LionFire.Trading.Proprietary.Bots;
using LionFire.Trading.Backtesting;
using Microsoft.Extensions.Logging;
using LionFire.Extensions.Logging;
using System.Threading;
using LionFire.Trading.Connect;
using LionFire.Trading.Bots;

namespace LionFire.Trading.Agent.Program
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            try
            {
                LionFire.Extensions.Logging.NLog.NLogConfig.LoadDefaultConfig();

                new AppHost()
                    .AddSpotwareConnectAccount("IC Markets.Demo1")
                    .AddConfig(app => 
                        app.ServiceCollection
                            .AddLogging()
                        )
                    .AddInit(app => app.ServiceProvider.GetService<ILoggerFactory>()
                        .AddNLog()
                    //.AddConsole()
                    )
                    //.AddCAlgoRedisAccount()
                    //.AddBacktest()
                    .AddBot<LionTrender>()
                    //.AddCommandLineDispatcher<AgentDispatcher>(args)
                    .AddShutdownOnConsoleExitCommand()
                    //.Run(TestHistoricalDataSource)
                    ;

                //Console.WriteLine();
                //Console.WriteLine("Press any key to exit");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }


        private async static Task TestHistoricalDataSource()
        {
            var s = new ConnectHistoricalDataSource();
            var bars = await s.Get("XAUUSD", "h1", new DateTime(2016, 01, 01), new DateTime(2016, 01, 10));
            foreach (var bar in bars)
            {
                Console.WriteLine(bar);
            }
        }
    }

    public class BotTask : AppTask
    {
        protected override void Run()
        {
            base.Run();
#warning TODO
            //this.app
            //ManualSingleton<IServiceCollection>

        }
    }

    public static class MarketParticipantExtensions
    {
        public static IAppHost AddBot<T>(this IAppHost host)
            where T : IBot
        {
            //host.ServiceCollection.Add(new ServiceDescriptor(typeof(T), null, ServiceLifetime.Scoped



            return host;
        }
    }
    
}

