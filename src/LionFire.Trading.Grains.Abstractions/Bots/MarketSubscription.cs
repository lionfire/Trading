using System.Security.Cryptography;
using System.Text;
using Orleans;

namespace LionFire.Trading.Grains.Bots;

/// <summary>
/// Defines a market subscription for a realtime bot harness, specifying which exchange,
/// trading area, symbol, and timeframe to subscribe to.
/// </summary>
/// <remarks>
/// This class provides two key generation methods for Orleans channel subscriptions:
///
/// - ToChannelKey(): Returns a human-readable string key in format "exchange.area:symbol timeframe"
///   Use this when you need readable channel names for logging and debugging.
///
/// - ToChannelGuid(): Returns a deterministic Guid derived from MD5 hash of the channel key.
///   Use this for Orleans BroadcastChannel subscriptions where Guid-based channels are preferred.
///   The same market parameters always produce the same Guid, enabling consistent subscriptions.
///
/// The format must exactly match the publisher's channel key format.
/// </remarks>
[GenerateSerializer]
[Alias("market-subscription")]
public class MarketSubscription
{
    /// <summary>
    /// Exchange name (e.g., "binance", "phemex"). 
    /// </summary>
    [Id(0)]
    public string Exchange { get; set; } 

    /// <summary>
    /// Exchange trading area (e.g., "spot", "futures"). Defaults to "futures".
    /// </summary>
    [Id(1)]
    public string ExchangeArea { get; set; } = "futures";

    /// <summary>
    /// Trading symbol (e.g., "BTCUSD", "ETHUSD").
    /// </summary>
    [Id(2)]
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Timeframe for bars (e.g., "m1", "m5", "h1").
    /// </summary>
    [Id(3)]
    public string TimeFrame { get; set; } = string.Empty;

    /// <summary>
    /// Generates the channel key in the format "exchange.area:symbol timeframe".
    /// </summary>
    /// <returns>Channel key string suitable for Orleans BroadcastChannel string-based subscriptions</returns>
    /// <example>
    /// For Exchange="binance", ExchangeArea="futures", Symbol="BTCUSD", TimeFrame="m1":
    /// Returns "binance.futures:BTCUSD m1"
    /// </example>
    public string ToChannelKey()
    {
        return $"{Exchange}.{ExchangeArea}:{Symbol} {TimeFrame}";
    }

    /// <summary>
    /// Generates a deterministic Guid from the channel key using MD5 hashing.
    /// </summary>
    /// <returns>Guid that is consistent for the same market parameters</returns>
    /// <remarks>
    /// Uses MD5 hash of the uppercase channel key to generate a Guid. This ensures:
    /// - Same market parameters always produce the same Guid
    /// - Low collision rate even with 500+ different markets
    /// - Fast computation
    /// - 128-bit MD5 hash maps directly to Guid structure
    ///
    /// This is useful for Orleans BroadcastChannel Guid-based subscriptions where you need
    /// a stable identifier that can be reconstructed from market parameters.
    /// </remarks>
    public Guid ToChannelGuid()
    {
        var channelKey = ToChannelKey().ToUpperInvariant();
        var hash = MD5.HashData(Encoding.UTF8.GetBytes(channelKey));
        return new Guid(hash);
    }
}
