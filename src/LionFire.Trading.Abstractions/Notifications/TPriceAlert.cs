namespace LionFire.Trading.Notifications
{
    public class TPriceAlert
    {
        public string Symbol { get; set; }
        public string Exchange { get; set; }
        public double Price { get; set; }
        public string Operator { get; set; }
        public string Profile { get; set; }

        public string Key => $"{Symbol}|{Exchange}|{Price}";
        //public string SymbolKey => $"{Exchange}:{Symbol}";

        public override string ToString() => $"{Symbol} {Exchange} {Price} {Profile}";
    }
}
