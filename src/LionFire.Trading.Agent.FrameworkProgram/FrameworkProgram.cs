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

namespace LionFire.Trading.Agent.Program
{
    public class TSymbol
    {
        public string Code { get; set; }
        public TSymbol(string code) { this.Code = code; }
    }
    

    class FrameworkProgram
    {
        static void Main(string[] args)
        {
#if truex
            //LionFire.Trading.Spotware.Connect
            TradingApiTest.Main(args);
#else
            try
            {

                LionFire.Extensions.Logging.NLog.NLogConfig.LoadDefaultConfig();

                new AppHost()

                #region Bootstrap
                    .AddJsonAssetProvider(@"E:\Trading")
                    .Bootstrap()
                #endregion

                #region Logging
                    .AddConfig(app =>
                        app.ServiceCollection
                            .AddLogging()
                        )
                    .AddInit(app =>
                        app.ServiceProvider.GetService<ILoggerFactory>()
                            .AddNLog()
                        //.AddConsole()
                        )
                #endregion

                    .AddTrading(TradingOptions.Auto)
                    //.AddSpotwareConnectClient("lionprowl") // TODO
                    .Add<TCTraderAccount>("spotware-lionprowl") // TODO: Change to "IC Markets.Demo1";
                    //.Add<TLionTrender>()

                    //.Add(TimeFrame.m1)
                    //.Add(TimeFrame.h1)
                    //.Add(new TSymbol("XAUUSD"))
                    //.Add(new TSymbol("EURUSD"))
                    //.Add(new TSymbol("USDJPY"))
                    //.Add(new TSymbol("USDCHF"))


                    //"spotware-lfdev";
                    //"spotware-lionprowl";
                    //.AddBot<LionTrender>()


                    //.AddSpotwareConnectAccount()
                    //.AddBrokerAccount()
                    //.AddBacktest()
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
#endif
        }
    }




}
