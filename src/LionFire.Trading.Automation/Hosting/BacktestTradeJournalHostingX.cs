using LionFire.Trading.Automation.Journaling.Trades;
using Microsoft.Extensions.DependencyInjection;

namespace LionFire.Trading.Journal;

public static class TradeJournalHostingX
{
    public static IServiceCollection AddTradeJournal(this IServiceCollection services, Action<TradeJournalOptions>? options = null)
    {
        if (options != null) { services.Configure<TradeJournalOptions>(options); }

        //services.AddTransient<ISimulationTradeJournal<double>, SimulationTradeJournal<double>>();
        //services.AddTransient<ISimulationTradeJournal<decimal>, SimulationTradeJournal<decimal>>();

        return services;
    }
}
