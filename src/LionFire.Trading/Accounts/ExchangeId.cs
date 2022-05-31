namespace LionFire.Trading
{
    public class ExchangeId
    {
        public const char Separator = '.';

        public ExchangeId() { }
        public ExchangeId(string exchange, string exchangeArea)
        {
            Exchange = exchange;
            ExchangeArea = exchangeArea;
        }

        public string Exchange { get; set; }
        public string ExchangeArea { get; set; }
        public string Id => ExchangeArea == null ? Exchange : $"{Exchange}{Separator}{ExchangeArea}";

        public static implicit operator ExchangeId(string exchangeId)
        {
            var x = exchangeId.Split(Separator, 2);
            return new ExchangeId { Exchange = x[0], ExchangeArea = x.Length == 2 ? x[1] : null };
        }

        public override string ToString() => Id;
    }
}
