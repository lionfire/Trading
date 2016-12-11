//#define Backtest
#define Live
#define Proprietary
#define Bots
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
using LionFire.Trading.Spotware.Connect;
using LionFire.Trading.Applications;
using LionFire.Execution;
using LionFire.Templating;
using LionFire.Trading.Bots;

namespace LionFire.Trading.Agent.Program
{

    public class FrameworkProgram
    {
        static void Main(string[] args)
        {
            
            try
            {
                throw new Exception("TODO: IsCommandLineEnabled = true");

                LionFire.Extensions.Logging.NLog.NLogConfig.LoadDefaultConfig();

                var a = new AppHost()

                #region Bootstrap
                    .AddJsonAssetProvider(@"c:\Trading")
                    .Bootstrap()
                #endregion

                #region Logging
                    .ConfigureServices(sc => sc.AddLogging())
                    .AddInit(app =>
                        app.ServiceProvider.GetService<ILoggerFactory>()
                            .AddNLog()
                        //.AddConsole()
                        )
                #endregion

                    //.AddSpotwareConnectClient("LionFire.Trading")  // TODO - use this one
                    .AddSpotwareConnectClient("LionProwl")
                    //.AddSpotwareConnectClient("LionFireDev")

#if Live
                    .AddTrading(TradingOptions.Auto, AccountMode.Live)
                    .Add<TCTraderAccount>("IC Markets.Live.Manual")
                    
#else
                    .AddTrading(TradingOptions.Auto, AccountMode.Demo)
                    .Add<TCTraderAccount>("IC Markets.Demo3")
#endif

#if Live
                    .Bootstrap()
#if Proprietary
                    //.Add(new TLionTrender("XAUUSD", "m1")
                    //{
                    //    Mode = BotModes.Live,
                    //    Log = true,
                    //    LogBacktest = true,
                    //    MinPositionSize = 1,
                    //    Indicator = new TLionTrending
                    //    {
                    //        Log = true,
                    //        OpenWindowMinutes = 55,
                    //        CloseWindowMinutes = 34,
                    //        PointsToOpenLong = 3.0,
                    //        PointsToOpenShort = 3.0,
                    //        PointsToCloseLong = 2.0,
                    //        PointsToCloseShort = 2.0,
                    //    }
                    //}.Create())
                    .Add<TLionTrender>("4bx2")
                    // Future: Add bot types
                    //.Add<TLionTrender>()
#endif

#endif

                    //.Add(TimeFrame.m1)
                    //.Add(TimeFrame.h1)
                    //.Add(new TSymbol("XAUUSD"))
                    //.Add(new TSymbol("EURUSD"))
                    //.Add(new TSymbol("USDJPY"))
                    //.Add(new TSymbol("USDCHF"))

                    //"spotware-lfdev";
                    //"spotware-lionprowl";

                    //.AddScanner<TLionTrender>("zt9f")

                    //.AddSpotwareConnectAccount()
                    //.AddBrokerAccount()
#if Backtest
                    .AddBacktest()
#endif
                    //.AddScanner()
                    //.AddShutdownOnConsoleExitCommand()
                    ;

                a.Run().Wait()
                ;
                //Console.WriteLine();
                //Console.WriteLine("Press any key to exit");
                //Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }

        }
    }

    public class TSymbol
    {
        public string Code { get; set; }
        public TSymbol(string code) { this.Code = code; }
    }



    public static class TradingContextExtensions
    {

        public static IAppHost AddBot<TBot>(this IAppHost app, string configName, AccountMode mode = AccountMode.Unspecified)
        {
            throw new NotImplementedException();

            //return app;
        }


    }


}
