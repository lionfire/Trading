using LionFire.Hosting;
using LionFire.Trading.Binance_;
using LionFire.Trading.Indicators;
using LionFire.Trading.Automation.Blazor.Optimization;
//using LionFire.Trading.Automation.Orleans.Hosting;
using LionFire.Logging;
using Microsoft.Extensions.Logging;

//var customLoggerProvider = new CustomLoggerProvider();

Host.CreateApplicationBuilder(args)
    .LionFire(9850, lf => lf
        .ForIHostApplicationBuilder(builder => builder
             //.Trading()
             .UseOrleansClient_LF()
             //.If(lf.Configuration["OrleansClient:Enable"], lf=> lf
             //   .UseOrleansClient_LF())
        )
        //.FireLynxApp()
        .If(Convert.ToBoolean(lf.Configuration["Orleans:Enable"]), lf => lf.Silo((context, silo) =>
        {
            silo.AddOptimizationQueueGrains();
        }))
        .WebHost<TradingWorkerStartup>(w => w.Http().BlazorInteractiveServer())
        .ConfigureServices(services => services

        #region Data
            .AddTradingData()
            .AddHistoricalBars(lf.Configuration)
        #endregion

        #region Indicators
            .AddSingleton<IndicatorProvider>()
            .AddIndicators()
        #endregion

        #region Exchanges (TODO: move this upstream)
            .AddSingleton<BinanceClientProvider>()
        #endregion

        #region Logging
            .AddLogging(b =>
            {
                b.AddFile("logs/app-{Date}.log", minimumLevel: LogLevel.Information);
            })
            //.AddSingleton(customLoggerProvider)
            //.AddLogging(b =>
            //{
            //    b.ClearProviders(); // TEMP - clear console logger in LF DLLs?
            //    b.AddProvider(customLoggerProvider);
            //})
        #endregion

            #region UI

            //    .VosMount("/output".ToVobReference(), @"f:\st\Investing-Output\.local".ToFileReference())
            .AddUIComponents()
            .AddMudBlazorComponents()
            .AddMvvm()
            .AddTradingUI()
            //.AddTransient<OneShotOptimizeVM>()
            .AddSingleton<OneShotOptimizeVM>()
            .AddTransient<LionFire.Trading.Automation.Blazor.Optimization.Queue.OptimizationQueueVM>()
        
            #endregion

            .AutomationModel(lf.Configuration)
            .Automation(lf.Configuration)
            .AddOptimizationQueue() // Add Orleans queue services
        )
    )
    .Build()
    .Run();

