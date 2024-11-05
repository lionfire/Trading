using LionFire.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using FireLynx.Blazor;
using LionFire.Persistence.Filesystem;
using LionFire.Vos;
using System;
using Microsoft.Extensions.Configuration;
using LionFire.Mvvm;
using LionFire.Types;
using LionFire.Trading.Worker.Components.Pages.Optimization;
using LionFire.Trading.Binance_;
using LionFire.Trading.Indicators;


new HostApplicationBuilder(args)
    .LionFire(9850, lf => lf
        //.FireLynxApp()
        .If(Convert.ToBoolean(lf.Configuration["Orleans:Enable"]), lf => lf.Silo())
        .WebHost<TradingWorkerStartup, TradingWorkerWebHostConfig>(w => w.Http().Https().BlazorServer())
        .ConfigureServices(services => services
            .AddHistoricalBars(lf.Configuration)
            .AddSingleton<BinanceClientProvider>()
            .AddSingleton<IndicatorProvider>()
            .AddIndicators()
        )
    )
    .ConfigureServices(services => services
    //    .VosMount("/output".ToVobReference(), @"f:\st\Investing-Output\.local".ToFileReference())
            .AddUIComponents()
            .AddMudBlazorComponents()
            .AddMvvm()
            .AddTradingUI()
            .AddTransient<OneShotOptimizeVM>()
            .AddAutomation()
        )
    //.I(i => i.Configuration.AddUserSecrets<FireLynxAppStartup>(optional: true, reloadOnChange: true)) // TODO SECURITY - disable in Production
    .Build()
    .Run();


//using LionFire.Trading.Worker.Components;

