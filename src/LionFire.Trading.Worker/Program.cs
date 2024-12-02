using LionFire.Hosting;
using LionFire.Trading.Binance_;
using LionFire.Trading.Indicators;
using LionFire.Trading.Automation.Blazor.Optimization;
using LionFire.Logging;

var customLoggerProvider = new CustomLoggerProvider();

Host.CreateApplicationBuilder(args)
    .LionFire(9850, lf => lf
        .ForIHostApplicationBuilder(builder => builder
             //.Trading()
            .UseOrleansClient_LF()
        )
        //.FireLynxApp()
        .If(Convert.ToBoolean(lf.Configuration["Orleans:Enable"]), lf => lf.Silo())
        .WebHost<TradingWorkerStartup>(w => w.Http().BlazorServer())
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
            .AddSingleton(customLoggerProvider)
            .AddLogging(b =>
            {
                b.ClearProviders(); // TEMP - clear console logger in LF DLLs?
                b.AddProvider(customLoggerProvider);
            })
        #endregion

            #region UI

            //    .VosMount("/output".ToVobReference(), @"f:\st\Investing-Output\.local".ToFileReference())
            .AddUIComponents()
            .AddMudBlazorComponents()
            .AddMvvm()
            .AddTradingUI()
            //.AddTransient<OneShotOptimizeVM>()
            .AddSingleton<OneShotOptimizeVM>()
        
            #endregion

            .Backtesting(lf.Configuration)
            .AddAutomation()
        )
    )
    .Build()
    .Run();

