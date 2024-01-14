//using Binance.Net.Interfaces;
using Orleans;

namespace LionFire.Trading;

[GenerateSerializer]
public class BarEnvelope(IKline kline, DateTimeOffset retrieveTime = default, BarStatus status = default)
{
    public TimeFrame TimeFrame => TimeFrame.FromTimeSpan(Kline.CloseTime - Kline.OpenTime);

    [Id(0)]
    public BarStatus Status { get; set; } = status;
    [Id(1)]
    public DateTimeOffset RetrieveTime { get; set; } = retrieveTime;

    public double Progress => (RetrieveTime - Kline.OpenTime).TotalSeconds / (Kline.CloseTime - Kline.OpenTime).TotalSeconds;

    [Id(2)]
    public IKline Kline { get; set; } = kline;
}
