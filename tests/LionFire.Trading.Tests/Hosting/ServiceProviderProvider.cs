using LionFire.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Binance.Net;
using LionFire.Trading.Journal;

public static class ServiceProviderProvider
{
    public static IServiceProvider ServiceProvider => serviceProvider ??= Init();

    private static IServiceProvider Init()
    {
        var Configuration = new ConfigurationManager();

        // Defaults
        Configuration.AddInMemoryCollection([
            new ("LionFire:Trading:HistoricalData:Windows:BaseDir", @"F:\st\Investing-HistoricalData\"), // HARDCODE HARDPATH
            new ("LionFire:Trading:Backtesting:Dir", @"z:\Trading\Backtesting") // HARDCODE HARDPATH
        ]);
        // DOTNET__LionFire__Trading__HistoricalData__Windows__BaseDir
        Configuration.AddEnvironmentVariables("DOTNET__"); // TODO: Change prefix to LIONFIRE__ or LF__

        IServiceCollection services = new ServiceCollection();
        services
            .AddOptions()
            .AddLogging(b => b.AddConsole())
            .AddSingleton<IConfiguration>(Configuration)
            .AddTradeJournal()
            .AddHistoricalBars(Configuration)
            .AddIndicators()
            .AddQuantConnectIndicators()

            .AddBinance()

            .AddAutomationRuntime()
            .Backtesting()
            .ConfigureBacktestingOptions(Configuration)
            ;

        return services.BuildServiceProvider();
    }
    private static IServiceProvider? serviceProvider;
}
