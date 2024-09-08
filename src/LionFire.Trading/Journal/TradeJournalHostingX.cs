namespace LionFire.Trading.Journal;

public static class TradeJournalHostingX
{
    public static IServiceCollection AddTradeJournal(this IServiceCollection services, Action<TradeJournalOptions>? options = null)
    {
        if (options != null) { services.Configure<TradeJournalOptions>(options); }

        services.AddSingleton<ITradeJournal<double>, TradeJournal<double>>();
        services.AddSingleton<ITradeJournal<decimal>, TradeJournal<decimal>>();

        return services;
    }
}
