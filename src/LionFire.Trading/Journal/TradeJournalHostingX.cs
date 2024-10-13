namespace LionFire.Trading.Journal;

public static class TradeJournalHostingX
{
    public static IServiceCollection AddTradeJournal(this IServiceCollection services, Action<TradeJournalOptions>? options = null)
    {
        if (options != null) { services.Configure<TradeJournalOptions>(options); }

        services.AddTransient<ITradeJournal<double>, TradeJournal<double>>();
        services.AddTransient<ITradeJournal<decimal>, TradeJournal<decimal>>();

        return services;
    }
}
