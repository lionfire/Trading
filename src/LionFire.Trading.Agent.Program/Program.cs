using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Trading.cTrader.Redis;
using LionFire.Applications;
using LionFire.Applications.Hosting;
using Microsoft.Extensions.DependencyInjection;
using LionFire.Trading.Proprietary.Indicators;
using LionFire.Trading.Proprietary.Bots;
using LionFire.Trading.Backtesting;
using Microsoft.Extensions.Logging;
using LionFire.Extensions.Logging;

namespace LionFire.Trading.Agent.Program
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                LionFire.Extensions.Logging.NLog.NLogConfig.LoadDefaultConfig();

                new ApplicationHost()
                    .AddConfig(app => 
                        app.ServiceCollection
                            .AddLogging()
                        )
                    .AddInit(app => app.ServiceProvider.GetService<ILoggerFactory>()
                        .AddNLog()
                        .AddConsole()
                    )
                    //.AddBrokerAccount()
                    .AddBacktest()
                    .AddShutdownOnConsoleExitCommand()
                    .Run();

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

    
}

