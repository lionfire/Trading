using LionFire.Trading.Automation.Journaling.Trades;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading.Journal;

public static class TradeJournalHostingX
{
    public static IServiceCollection AddTradeJournal(this IServiceCollection services, Action<TradeJournalOptions>? options = null)
    {
        if (options != null) { services.Configure<TradeJournalOptions>(options); }

        //services.AddTransient<IBacktestTradeJournal<double>, BacktestTradeJournal<double>>();
        //services.AddTransient<IBacktestTradeJournal<decimal>, BacktestTradeJournal<decimal>>();

        return services;
    }
}
