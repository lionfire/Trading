using Binance.Net.Interfaces;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Objects.Models.Spot;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Logging;
using Orleans.Runtime;
using Orleans.Timers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LionFire.Trading.Binance_.UsdFutures24HStatsG;


namespace LionFire.Trading.Binance_;

public interface IUsdFuturesInfoG : IGrainWithStringKey, IRemindable
{

    ValueTask AutoRefreshInterval(TimeSpan? timeSpan);
    Task<StatsState> LastDayStats();

    Task<StatsState> RetrieveLastDayStats();
}

public class UsdFutures24HStatsG : IGrainBase, IUsdFuturesInfoG
{
    private static readonly string ReminderName = typeof(UsdFutures24HStatsG).Name;

    public IPersistentState<StatsState> State { get; }
    public ILogger<UsdFutures24HStatsG> Logger { get; }
    public IReminderRegistry ReminderRegistry { get; }
    public IGrainContext GrainContext { get; }
    public IBinanceRestClient BinanceRestClient { get; }

    IGrainReminder? reminder;

    public UsdFutures24HStatsG(
        [PersistentState("binance-futures-usd-stats-24h-state", "Trading")] IPersistentState<StatsState> state,
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

    public Task<StatsState> LastDayStats()
    {
        return Task.FromResult(State.State);
    }

    public async Task<StatsState> RetrieveLastDayStats()
    {
        Logger.LogInformation("Retrieving 24h stats");

        var info = await BinanceRestClient.UsdFuturesApi.ExchangeData.GetTickersAsync(); // WEIGHT: 80

        var statsState = new StatsState
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

    [Alias("binance-futures-usd-stats-24h")]
    [GenerateSerializer]
    public class StatsState
    {
        [Id(0)]
        public DateTimeOffset RetrievedOn { get; set; }
        [Id(1)]
        public List<Binance24HPriceStats> List { get; set; }
    }

    //[Alias("binance-futures-usd-symbols")]
    //public class SymbolsState
    //{

    //}
    //public class SymbolInfo
    //{

    //}
}

[Immutable]
[GenerateSerializer]
public class Binance24HPriceStats
{
    [Id(0)]
    public string Symbol { get; set; }
    [Id(1)]
    public decimal QuoteVolume { get; set; }


    /// <summary>
    /// The actual price change in the last 24 hours
    /// </summary>
    [Id(2)]
    public decimal PriceChange { get; set; }

    /// <summary>
    /// The price change in percentage in the last 24 hours
    /// </summary>
    [Id(3)]
    public decimal PriceChangePercent { get; set; }

    /// <summary>
    /// The weighted average price in the last 24 hours
    /// </summary>
    [Id(4)]
    public decimal WeightedAveragePrice { get; set; }

    /// <summary>
    /// The most recent trade quantity
    /// </summary>
    [Id(5)]
    public decimal LastQuantity { get; set; }

    /// <summary>
    /// Time at which this 24 hours opened
    /// </summary>
    [Id(6)]
    public DateTime OpenTime { get; set; }

    /// <summary>
    /// Time at which this 24 hours closed
    /// </summary>
    [Id(7)]
    public DateTime CloseTime { get; set; }

    /// <summary>
    /// The first trade ID in the last 24 hours
    /// </summary>
    [Id(8)]
    public long FirstTradeId { get; set; }

    /// <summary>
    /// The last trade ID in the last 24 hours
    /// </summary>
    [Id(9)]
    public long LastTradeId { get; set; }

    /// <summary>
    /// The amount of trades made in the last 24 hours
    /// </summary>
    [Id(10)]
    public long TotalTrades { get; set; }

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
