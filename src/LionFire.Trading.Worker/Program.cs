using LionFire.Hosting;
using LionFire.Trading.Binance_;
using LionFire.Trading.Indicators;
using LionFire.Trading.Automation.Blazor.Optimization;
using LionFire.Logging;

var wab = WebApplication.CreateBuilder(args);
var clp = new CustomLoggerProvider();

Host.CreateApplicationBuilder(args)
    .LionFire(9850, lf => lf
        //.FireLynxApp()
        .If(Convert.ToBoolean(lf.Configuration["Orleans:Enable"]), lf => lf.Silo())
        .WebHost<TradingWorkerStartup>(w => w.Http().BlazorServer())
        .ConfigureServices(services => services
            .AddHistoricalBars(lf.Configuration)
            .AddSingleton<BinanceClientProvider>()
            .AddSingleton<IndicatorProvider>()
            .AddIndicators()
            .Backtesting(lf.Configuration)
            .AddSingleton(clp)
            .AddLogging(b =>
            {
                b.ClearProviders(); // TEMP - clear console logger in LF DLLs?
                b.AddProvider(clp);
            })
            //    .VosMount("/output".ToVobReference(), @"f:\st\Investing-Output\.local".ToFileReference())
            .AddUIComponents()
            .AddMudBlazorComponents()
            .AddMvvm()
            .AddTradingUI()
            //.AddTransient<OneShotOptimizeVM>()
            .AddSingleton<OneShotOptimizeVM>()
            .AddAutomation()
        )
    )
    //.I(i => i.Configuration.AddUserSecrets<FireLynxAppStartup>(optional: true, reloadOnChange: true)) // TODO SECURITY - disable in Production
    .Build()
    .Run();

