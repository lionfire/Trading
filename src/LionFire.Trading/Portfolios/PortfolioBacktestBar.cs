using System;

namespace LionFire.Trading.Portfolios
{
    public class PortfolioBacktestBar
    {
        public PortfolioBacktestBar(DateTime openTime, double price)
        {
            OpenTime = openTime;
            High = Low = Open = close = price;
            TradesClosed = TradesOpened = 0;
        }

        public DateTime OpenTime;
        public double High;
        public double Low;
        public double Open;
        public double Close
        {
            get => close;
            set
            {
                close = value;
                if (value > High) High = value;
                if (value < Low) Low = value;
            }
        }
        private double close;
        public double TradesOpened;
        public double TradesClosed;

        #region ToString

        public override string ToString() => ToString(3);

        public string ToString(int decimalPlaces)
        {
            var date = OpenTime.ToDefaultString();

            var padChar = ' ';
            //var padChar = '0';
            //var vol = Volume > 0 ? $" [v:{Volume.ToString().PadLeft(decimalPlaces)}]" : "";
            return $"{date} o:{Open.ToString().PadRight(decimalPlaces, padChar)} h:{High.ToString().PadRight(decimalPlaces, padChar)} l:{Low.ToString().PadRight(decimalPlaces, padChar)} c:{Close.ToString().PadRight(decimalPlaces, padChar)}"; // {vol}
        }

        #endregion
    }
}
