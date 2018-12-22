namespace LionFire.Trading
{
    public class TradingPath
    {
        public static string LastTickRedisPath(string symbol, string exchange) => $"t:{exchange}:{symbol}";
    }
}
