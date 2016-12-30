//#define Live
#define Proprietary
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
using LionFire.Execution;
using LionFire.Trading.Spotware.Connect;
using LionFire.Assets;
using LionFire.Trading.Applications;
using LionFire.Extensions.Options;
using LionFire.Templating;

namespace LionFire.Trading.Agent.Program
{
    public class Program
    {
        public static void Main(string[] args)
        {

            try
            {
                var basePath = LionFireEnvironment.ProgramDataDir;

                LionFire.Extensions.Logging.NLog.NLogConfig.LoadDefaultConfig();

                new AppHost()
                    //.AddOptions(basePath)

                #region Bootstrap
                    .AddJsonAssetProvider(LionFireEnvironment.ProgramDataDir)
                    .Bootstrap()
                #endregion

                #region Logging
                    .ConfigureServices(sc => sc.AddLogging())
                    .AddInit(app =>
                       app.ServiceProvider.GetService<ILoggerFactory>()
                           .AddNLog()
                       .AddConsole()
                       )
                #endregion

                    //.AddSpotwareConnectClient("LionFire.Trading") // TODO - use this one
                    .AddSpotwareConnectClient("LionProwl")
                    //.AddSpotwareConnectClient("LionFireDev")

#if Live
                    .AddTrading(TradingOptions.Auto, AccountMode.Live)
                    .Add<TCTraderAccount>("IC Markets.Live.Manual")
#else
                    .AddTrading(TradingOptions.Auto, AccountMode.Demo)
                    .Add<TCTraderAccount>("IC Markets.Demo")
#endif
                   //.AddConfig(app => 
                   //    app.ServiceCollection
                   //        .AddLogging()
                   //    )

                   //.AddCAlgoRedisAccount()
                   //.AddBacktest("cTrader/IC Markets.Live.Manual".Load<TAccount>())
                   .AddBacktest(new TBacktestAccount()
                   {

                       SimulateAccount = @"cTrader\IC Markets.Live.Manual.USD-Backtest",
                       BrokerName = "IC Markets",
                       StartDate = new DateTime(2016, 1, 1),
                       EndDate = new DateTime(2016, 11, 23),
                       TimeFrame = TimeFrame.h1,

                       //Symbols = new List<string> { "XAUUSD", "EURUSD" }, // UNUSED

                       Children = new List<ITemplate>
                       {
#if Proprietary
                    new TLionTrender("XAUUSD", "h1")
                    {
                        Log=false,
                        //LogBacktestThreshold = 2.0,
                        MinPositionSize = 1,
                        Indicator = new TLionTrending
                        {
                            Log = false,
                            OpenWindowMinutes = 55*60,
                            CloseWindowMinutes = 34*60,
                            PointsToOpenLong = 3.0,
                            PointsToOpenShort = 3.0,
                            PointsToCloseLong = 2.0,
                            PointsToCloseShort = 2.0,
                        }
                    }
#endif
                       }
                   })
                   //.AddBot<LionTrender>()
                   //.AddCommandLineDispatcher<AgentDispatcher>(args)
                   //.AddShutdownOnConsoleExitCommand()
                   .RunAndWait()
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

//    public class BotTask : AppTask
//    {
//        protected override void Run()
//        {
//            base.Run();
//#warning TODO
//            //this.app
//            //ManualSingleton<IServiceCollection>

//        }
//    }

    public static class AccountParticipantExtensions
    {
        public static IAppHost AddBot<T>(this IAppHost host)
            where T : IBot
        {
            //host.ServiceCollection.Add(new ServiceDescriptor(typeof(T), null, ServiceLifetime.Scoped



            return host;
        }
    }

}

