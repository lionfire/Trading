using LionFire.Trading.Feeds;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.BroadcastChannel;

namespace LionFire.Trading.Binance_;

public interface ILiveBarTranscriberG : IGrainWithStringKey
{
}

//[ImplicitChannelSubscription(BinanceBroadcastChannelNames.ConfirmedBars)]
//[ImplicitChannelSubscription(BinanceBroadcastChannelNames.TentativeBars)]
//[ImplicitChannelSubscription(BarsBroadcastChannelNames.RevisionBars)]
//[ImplicitChannelSubscription()]
public sealed class LiveBarTranscriberG(ILogger<LiveBarTranscriberG> Logger) :
    Grain,
    ILiveBarTranscriberG,
    IOnBroadcastChannelSubscribed
{
    //List<BarEnvelope> confirmedBars = new();

    #region Lifecycle (IOnBroadcastChannelSubscribed)

    public async Task OnSubscribed(IBroadcastChannelSubscription subscription)
    {
        Logger.LogInformation("{grainId} subscribed to BroadcastChannel {broadcastChannelId} (provider: {broadcastChannelProvider})", this.GetPrimaryKeyString(), subscription.ChannelId, subscription.ProviderName);
        //await subscription.Attach<BarEnvelope[]>(e => OnBar(), OnError);
        await subscription.Attach<BarEnvelope[]>(OnBarFactory(subscription.ChannelId), OnError);
    }

    #endregion

    #region OnBar

    private Func<BarEnvelope[], Task> OnBarFactory(ChannelId channelId)
    {
        return envelopes => OnBar(envelopes, channelId);
    }
    private Task OnBar(BarEnvelope[] envelopes) => OnBar(envelopes, null);
    private Task OnBar(BarEnvelope[] envelopes, ChannelId? channelId)
    {
        Logger.Log(envelopes.Length > 0 ? LogLevel.Debug : LogLevel.Trace, "{id} Received {count} envelopes on namespace: {ns} and key: {key}", this.GetPrimaryKeyString(), envelopes.Length, channelId?.GetNamespace(), channelId?.GetKeyAsString());

        foreach (var envelope in envelopes)
        {
            Logger.LogInformation("{ns}/{id}: {kline} (status: {status}) ", (channelId?.GetNamespace()??"").PadLeft(14), this.GetPrimaryKeyString().PadRight(14), envelope.Kline, envelope.Status.ToString());

            var k = envelope.Kline;

            if (IsLastForHistoricalDataChunk(k.CloseTime, envelope.TimeFrame))
            {
                Logger.LogInformation($"Last bar for chunk: {envelope.Kline} (status: {envelope.Status})");
            }
        }

        return Task.CompletedTask;
    }

    #endregion

    private Task OnError(Exception ex)
    {
        Logger.LogError(ex, $"An error occurred: {ex}");

        return Task.CompletedTask;
    }

    #region Utility

    public static bool IsLastForHistoricalDataChunk(DateTime closeTime, TimeFrame timeFrame)
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

    #endregion
}
