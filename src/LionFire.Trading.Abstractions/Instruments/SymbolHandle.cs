using LionFire.Trading.Instruments;

namespace LionFire.Trading.Instruments
{
    public class SymbolHandle
    {
        public SymbolHandle(string symbol)
        {
            Symbol = symbol;
            (LongAsset, ShortAsset) = AssetNameResolver.ResolvePair(symbol);
            //if (LongAsset == null || ShortAsset == null)
            //{
            //}
        }

        public string Symbol { get; }

        public string LongAsset { get; }
        public string ShortAsset { get; }

    }
}
