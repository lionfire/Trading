//#define Backtest
#define Live
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
using LionFire.Trading.Spotware.Connect;
using LionFire.Trading.Applications;
using LionFire.Execution;
using LionFire.Templating;

namespace LionFire.Trading.Agent.Program
{
    public class FrameworkProgram
    {
        static void Main(string[] args)
        {
            try
            {
                LionFire.Extensions.Logging.NLog.NLogConfig.LoadDefaultConfig();

                new AppHost()

                #region Bootstrap
                    .AddJsonAssetProvider(@"E:\Trading")
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

                    .AddTrading(TradingOptions.Auto, AccountMode.Live)
#if Live
                    .AddSpotwareConnectClient("lionprowl")
                    .Add<TCTraderAccount>("spotware-lionprowl") // TODO: Change to "IC Markets.Demo1";
                                                                //.Add<TLionTrender>()
                                                                //.Add<TLionTrender>("4bx2")
#if Proprietary
                    .Add(new TLionTrender("XAUUSD", "m1")
                    {
                        Log = true,
                        LogBacktest = true,
                        MinPositionSize = 1,
                        Indicator = new TLionTrending
                        {
                            Log = true,
                            OpenWindowPeriods = 55,
                            CloseWindowPeriods = 34,
                            PointsToOpenLong = 3.0,
                            PointsToOpenShort = 3.0,
                            PointsToCloseLong = 2.0,
                            PointsToCloseShort = 2.0,
                        }
                    }.Create())
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
                    .Run().Wait();

                //Console.WriteLine();
                //Console.WriteLine("Press any key to exit");
                //Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
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
