using LionFire.Applications.Hosting;
using LionFire.Serialization.Json.Newtonsoft;
using System;
using LionFire.Trading.Applications;
using LionFire.Trading.Data;
using LionFire.Trading.Workspaces;
using LionFire.Extensions.Logging;
using System.Collections.Generic;
using LionFire.Assets;
using LionFire.Instantiating;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using LionFire.Composables;
using LionFire.Trading.Spotware.Connect;
using LionFire.DependencyInjection;
using System.Threading.Tasks;
using LionFire.Execution;
using LionFire.Execution.Jobs;
using LionFire.Applications;

namespace LionFire.Trading
{
    //public interface IMarketPrioritizer
    //{
    //    double SymbolPriority(string symbol);
    //    double CurrencyPriority(string symbol);
    //}
    //public class DefaultMarketPrioritizer : IMarketPrioritizer
    //{
    //    public double CurrencyPriority(string symbol)
    //    {

    //    }

    //    public double SymbolPriority(string symbol)
    //    {
    //        throw new NotImplementedException();
    //   }
    //}

    /// <summary>
    /// Default usage: look for new data in all workspaces in all accounts and quit.
    /// With start flag (todo): do the same and keep running.  For cTrader, grab m1 bars as they come in.
    /// </summary>
    class Program
    {
        private static void Main(string[] args)
        {

            LionFire.Extensions.Logging.NLog.NLogConfig.LoadDefaultConfig(console:
                LogLevel.Debug.ToString());

            var app = new AppHost()
                      .Add(new AppInfo
                      {
                          CompanyName = "LionFire",
                          //AppDataDirName = "Trading", // REVIEW - ok to replace with below?
                          CustomAppDataDirName = "Trading",
                          ProgramName = "Data Cache Service",
                      })
                      .AddLogging(factory => factory.AddNLog())
                //   .ConfigureServices(sc => sc.AddLogging())
                //   .AddInit(a => a
                //    .ServiceProvider.GetService<ILoggerFactory>()
                //    .AddNLog()
                ////.AddConsole()
                //)
                .AddJsonAssetProvider()
                .Add<NewtonsoftJsonSerialization>()
                      //.ConfigureServices<ObjectLogProvider>()
                      .ConfigureServices(c => c.AddSingleton<ITradingTypeResolver>(new TradingTypeResolverTemp2()))
                .Initialize()
                .AddTrading(new TradingOptions
                {
                    Features = TradingFeatures.HistoricalData,
                    AutoAttachToAccounts = true,
                    AccountModes = AccountMode.Demo,
                    //ForceReretrieveEmptyData = true,
                    //HistoricalDataStart = new DateTime(1997, 1, 1),
                    //HistoricalDataStart = new DateTime(2017, 5, 31), // TEMP
                    //HistoricalDataEnd = new DateTime (2016,12,31),
                    HistoricalDataStart = new DateTime(2017, 6, 1),
                    HistoricalDataEnd = new DateTime(2017, 6, 3),
                    HistoricalDataTimeFrames = new List<TimeFrame> {
                        TimeFrame.m1,
                        //TimeFrame.h1,
                    },
                    SymbolsWhiteList = new List<string>
                    {
                            "EURUSD",
                                "USDJPY",
                        //        "AUDUSD",
                        //        "USDCAD",
                        //    "XAUUSD",
                        //    //"XAGUSD",
                        //    //"NZDUSD",
                        }
                })

                // FUTURE: Get accounts and interested symbols by loading one or more workspaces:
                .AddAsset<TradingWorkspace>("Default")
                //.AddAllAssets<TWorkspace>()
                .Add<DataCacheService>()
                //.Add(new SeriesCacheService // FUTURE: Check run dir on local machine -- Don't start multiple services
                //{
                //    Account = "",
                //    Broker = "IC Markets",
                //})
                .RunNowAndWait(async () =>
                {
                    var s = App.GetComponent<SDataCacheService>();

                    while (s.IsStarted() || s.CompletionCount <= 0)
                    {
                        Console.WriteLine($"Waiting for SDataCacheService...");
                        await Task.Delay(3000);
                    }

                    while (JobManager.Default.Jobs.Count > 0)
                    {
                        Console.WriteLine($"Waiting for {JobManager.Default.Jobs.Count} jobs...");
                        await Task.Delay(5000);
                    }
                    Console.WriteLine("------------------------------- DONE -----------------------------");
                    Console.WriteLine($"Jobs remaining: {JobManager.Default.Jobs.Count}");
                    Console.ReadLine();
                }).Result
                ;
        }
    }

    public class TradingTypeResolverTemp2 : ITradingTypeResolver
    {
        public IFeed CreateAccount(string name) // TEMP REFACTOR
        {
            var tAccount = name.Load<TCTraderAccount>();
            if (tAccount == null) return null;
            var account = tAccount.Create();
            return account;
        }

        public Type GetTemplateType(string type)
        {
            throw new NotImplementedException();
        }

        public Type GetType(string type)
        {
            throw new NotImplementedException();
        }
    }
}