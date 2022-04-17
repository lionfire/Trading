namespace LionFire.Trading.HistoricalData.Orleans_;

public class HistoricalDataChunkGrainKey
{
    public static string GetKey(TimeFrame timeFrame, string symbol, DateTime start, DateTime endExclusive, string exchange, string exchangeArea, Type type)
        => $"{exchange ?? throw new ArgumentNullException(nameof(exchange))}:{exchangeArea ?? throw new ArgumentNullException(nameof(exchangeArea))}:{symbol ?? throw new ArgumentNullException(nameof(symbol))}:{timeFrame?.Name ?? throw new ArgumentNullException(nameof(timeFrame))}:{start}:{endExclusive}:{type.FullName}";


    public TimeFrame TimeFrame;
    public string Symbol;
    public DateTime Start;
    public DateTime EndExclusive;
    public string Exchange;
    public string ExchangeArea;
    public Type Type;

    public static HistoricalDataChunkGrainKey Parse(string key)
    {
        var chunks = key.Split(':');

        if (chunks.Length != ) { throw new ArgumentException($"{nameof(key)} is in invalid format"); }

        return new HistoricalDataChunkGrainKey
        {
            TimeFrame = TimeFrame.Parse(chunks[0]),
            Symbol = chunks[1],
            Start = DateTime.FromBinary(long.Parse(chunks[2])),
            EndExclusive = DateTime.FromBinary(long.Parse(chunks[3])),
            Exchange = chunks[4],
            ExchangeArea = chunks[5],
            Type = Type.GetType(chunks[6]) ?? throw new ArgumentException($"Failed to resolve Type '{chunks[6]}'"),
        };
    }
}
