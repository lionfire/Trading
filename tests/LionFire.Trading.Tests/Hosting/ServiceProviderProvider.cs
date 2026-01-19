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

        // Defaults - using Windows/Unix bifurcation pattern
        // These can be overridden via environment variables: DOTNET__Trading__Backtesting__Windows__Dir
        Configuration.AddInMemoryCollection([
            new ("Trading:HistoricalData:Windows:BaseDir", @"F:\st\Investing-HistoricalData\"),
            new ("Trading:HistoricalData:Unix:BaseDir", @"/st/Investing-HistoricalData/"),
            new ("Trading:Backtesting:Windows:Dir", @"F:\st\Investing-Output\.local\Backtests\"),
            new ("Trading:Backtesting:Unix:Dir", @"/st/Investing-Output/.local/Backtests/"),
        ]);
        Configuration.AddEnvironmentVariables("DOTNET__");

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

            .Backtesting()
            ;

        throw new NotImplementedException(".ConfigureBacktestingOptions(Configuration)");
            
        return services.BuildServiceProvider();
    }
    private static IServiceProvider? serviceProvider;
}
