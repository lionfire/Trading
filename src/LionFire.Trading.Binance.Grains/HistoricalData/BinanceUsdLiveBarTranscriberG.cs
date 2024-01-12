using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LionFire.Trading.HistoricalData.Retrieval;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.BroadcastChannel;

namespace LionFire.Trading.Binance_;

public interface IBinanceUsdLiveBarTranscriberG : IGrainWithStringKey
{
}

[ImplicitChannelSubscription(BinanceBroadcastChannelNames.ConfirmedBars)]
[ImplicitChannelSubscription()]
public sealed class BinanceUsdLiveBarTranscriberG(ILogger<BinanceUsdLiveBarTranscriberG> Logger) :
    Grain,
    IBinanceUsdLiveBarTranscriberG,
    IOnBroadcastChannelSubscribed
{
    //List<BinanceBarEnvelope> confirmedBars = new();

    public Task OnSubscribed(IBroadcastChannelSubscription subscription) =>
        subscription.Attach<BinanceBarEnvelope>(OnBar, OnError);

    private Task OnBar(BinanceBarEnvelope envelope)
    {
        Logger.LogInformation($"Received bar (via BroadcastChannel): {envelope.Kline} (status: {envelope.Status})");

        var k = envelope.Kline;

        if (IsLastForHistoricalDataChunk(k.CloseTime, envelope.TimeFrame))
        {
            Logger.LogInformation($"Last bar for chunk: {envelope.Kline} (status: {envelope.Status})");
        }

        return Task.CompletedTask;
    }

    public bool IsLastForHistoricalDataChunk(DateTime closeTime, TimeFrame timeFrame)
    {
        switch (timeFrame.ToShortString())
        {
            case "m1":
                return closeTime.Hour == 23 && closeTime.Minute == 59;
            case "h1":
                return closeTime.Hour == 23;
            default:
                return true;
        }
    }

    private Task OnError(Exception ex)
    {
        Logger.LogError(ex, $"An error occurred: {ex}");

        return Task.CompletedTask;
    }
}
