using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Objects.Models.Spot;
using LionFire.DependencyMachines;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Timers;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace LionFire.Trading.Binance_;

public class UsdFutures24HStatsG : IGrainBase, IUsdFuturesInfoG, IRemindable
{
    private static readonly string ReminderName = typeof(UsdFutures24HStatsG).Name;

    public IPersistentState<Binance24HStatsState> State { get; }
    private static Binance24HStatsState? LastState { get; set; }
    public ILogger<UsdFutures24HStatsG> Logger { get; }
    public IReminderRegistry ReminderRegistry { get; }
    public IGrainContext GrainContext { get; }
    public IBinanceRestClient BinanceRestClient { get; }

    IGrainReminder? reminder;

    #region Metrics

    static Meter Meter = new("LionFire.Trading.Binance_.UsdFutures24HStatsG", "1.0.0");
    static Counter<int> Retrieves = Meter.CreateCounter<int>("Retrieves");
    static ObservableGauge<int> SymbolCount = Meter.CreateObservableGauge<int>("SymbolCount", () =>  LastState?.List?.Count ?? 0);
    static int x = 0;
    static Histogram<long> RetrieveTime = Meter.CreateHistogram<long>("RetrieveTime");

    #endregion

    #region Lifecycle

    public UsdFutures24HStatsG(
        [PersistentState("binance-futures-usd-stats-24h-state", "Trading")] IPersistentState<Binance24HStatsState> state,
        ILogger<UsdFutures24HStatsG> logger,
        IReminderRegistry reminderRegistry,
        IGrainContext grainContext,
        IBinanceRestClient binanceRestClient)
    {
        State = state;
        Logger = logger;
        ReminderRegistry = reminderRegistry;
        GrainContext = grainContext;
        BinanceRestClient = binanceRestClient;
    }

    public ValueTask OnActivateAsync(CancellationToken cancellationToken)
    {
        LastState = State.State;
        return default;
    }

    #endregion


    public async ValueTask AutoRefreshInterval(TimeSpan? timeSpan)
    {
        if (!timeSpan.HasValue)
        {
            await ReminderRegistry.UnregisterReminder(GrainContext.GrainId, reminder);
        }
        else
        {
            var nextRetrieve = timeSpan.Value - (DateTimeOffset.UtcNow - State.State.RetrievedOn);
            if (nextRetrieve < TimeSpan.Zero)
            {
                nextRetrieve = TimeSpan.Zero;
            }
            Logger.LogInformation("Performing first retrieve in {minutes} minutes", nextRetrieve.TotalMinutes);

            reminder = await ReminderRegistry.RegisterOrUpdateReminder(
             callingGrainId: GrainContext.GrainId,
             reminderName: ReminderName,
             dueTime: nextRetrieve,
             period: timeSpan.Value);
        }
    }

    async Task IRemindable.ReceiveReminder(string reminderName, TickStatus status)
    {
        await RetrieveLastDayStats();
    }

    public Task<Binance24HStatsState> LastDayStats()
    {
        return Task.FromResult(State.State);
    }

    public async Task<Binance24HStatsState> RetrieveLastDayStats()
    {
        Logger.LogInformation("Retrieving 24h stats");
        Retrieves.Add(1);

        CryptoExchange.Net.Objects.WebCallResult<IBinance24HPrice[]> info;

        try
        {
            using var _ = new DisposableStopwatch(s => RetrieveTime.Record(s.ElapsedMilliseconds));
            info = await BinanceRestClient.UsdFuturesApi.ExchangeData.GetTickersAsync(); // WEIGHT: 80
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to get 24H stats", ex);
        }

        var statsState = new Binance24HStatsState
        {
            RetrievedOn = DateTimeOffset.UtcNow,

            List = info.Data
            //.Where(d => d.QuoteVolume > 0
            //&& d.Symbol.Contains("USD") && !d.Symbol.StartsWith("USD")
            //)
            .OrderByDescending(d => d.QuoteVolume)
            .Select(a =>
            {
                return new Binance24HPriceStats
                {
                    Symbol = a.Symbol,
                    QuoteVolume = a.QuoteVolume,
                    PriceChange = a.PriceChange,
                    PriceChangePercent = a.PriceChangePercent,
                    WeightedAveragePrice = a.WeightedAveragePrice,
                    LastQuantity = a.LastQuantity,
                    OpenTime = a.OpenTime,
                    CloseTime = a.CloseTime,
                    FirstTradeId = a.FirstTradeId,
                    LastTradeId = a.LastTradeId,
                    TotalTrades = a.TotalTrades,
                };
            }).ToList()
        };
        if (statsState != null) { LastState = statsState; }
        State.State = statsState;
        await State.WriteStateAsync();

        //HashSet<string> quoteCurrencyWhitelist = new HashSet<string>
        //{
        //    "USDT",
        //    "USDC",
        //    "TUSD",
        //    "BUSD",
        //    "USD",
        //};

        var sb = new StringBuilder();
        //foreach (var a in info.Data
        //    .Where(d => d.QuoteVolume > 0
        //    //&& d.Symbol.Contains("USD") && !d.Symbol.StartsWith("USD")
        //    )
        //    .OrderByDescending(d => d.QuoteVolume)
        //    .Select(a =>
        //    {
        //        return new SymbolStats
        //        {
        //            Name = a.Symbol,
        //            QuoteVolume = a.QuoteVolume,
        //        };
        //    })
        //    )

        sb.AppendLine($"Retrieved {State.State.List.Count} symbols");
        foreach (var a in State.State.List
               .Where(d => d.QuoteVolume > 0
               && d.Symbol.Contains("USD") && !d.Symbol.StartsWith("USD")
               ).Take(10))
        {
            sb.Append(a.Symbol.PadLeft(14));
            sb.Append($" {a.QuoteVolume.ToString("N0").PadLeft(14)}");
            sb.AppendLine();
        }
        Logger.LogInformation(sb.ToString());
        return State.State;
    }



    //[Alias("binance-futures-usd-symbols")]
    //public class SymbolsState
    //{

    //}
    //public class SymbolInfo
    //{

    //}
}

//[RegisterConverter]
//public sealed class Binance24HPriceStatsConverter :
//    IConverter<IBinance24HPrice, Binance24HPriceStats>
//{
//    public IBinance24HPrice ConvertFromSurrogate(
//        in Binance24HPriceStats surrogate) =>
//        new
//        {

//        }(surrogate.Num, surrogate.String, surrogate.DateTimeOffset);

//    public Binance24HPriceStats ConvertToSurrogate(
//        in IBinance24HPrice value) =>
//        new()
//        {
//            Num = value.Num,
//            String = value.String,
//            DateTimeOffset = value.DateTimeOffset
//        };
//}
