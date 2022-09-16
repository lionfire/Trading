namespace LionFire.Trading
{
    public struct NativeSymbolTick
    {
        public DateTime DateTime { get; set; }
        public string Symbol { get; set; }
        public PriceKind Kind { get; set; }
        public decimal Price { get; set; }
    }
}
