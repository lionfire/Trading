using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.BroadcastChannel;

namespace LionFire.Trading.Binance_;

public interface IBinanceUsdLiveBarTranscriberG : IGrainWithStringKey
{
}

[ImplicitChannelSubscription(BinanceBroadcastChannelNames.ConfirmedBars)]
public sealed class BinanceUsdLiveBarTranscriberG(ILogger<BinanceUsdLiveBarTranscriberG> Logger) :
    Grain,
    IBinanceUsdLiveBarTranscriberG,
    IOnBroadcastChannelSubscribed
{
    //public ValueTask<Stock> GetStock(StockSymbol symbol) =>
    //    _stockCache.TryGetValue(symbol, out Stock? stock) is false
    //        ? new ValueTask<Stock>(Task.FromException<Stock>(new KeyNotFoundException()))
    //        : new ValueTask<Stock>(stock);

    public Task OnSubscribed(IBroadcastChannelSubscription subscription) =>
        subscription.Attach<BinanceBarEnvelope>(OnBar, OnError);

    private Task OnBar(BinanceBarEnvelope envelope)
    {
        Logger.LogInformation($"Received bar: {envelope.Bar}");

        switch (envelope.TimeFrame.ToShortString())
        {
            case "m1":
                break;
            case "h1":
                break;
            default:
                break;
        }

        return Task.CompletedTask;
    }

    private Task OnError(Exception ex)
    {
        Logger.LogError(ex, $"An error occurred: {ex}");

        return Task.CompletedTask;
    }
}
