using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class SymbolBar
    {
        #region Construction

        public SymbolBar() { }
        public SymbolBar(string code, Bar bar, DateTime time)
        {
            this.Code = code;
            this.Bar = bar;
            this.Time = time;
        }
        public SymbolBar(string code, TimedBar bar)
        {
            this.Code = code;
            this.Bar = new Bar
            {
                Open = bar.Open,
                High = bar.High,
                Low = bar.Low,
                Close = bar.Close,
                Volume = bar.Volume,
            };
            this.Time = bar.OpenTime;
        }

        #endregion

        #region Properties

        public string Code { get; set; }
        public Bar Bar { get; set; }
        public DateTimeOffset Time { get; set; }

        #endregion

        #region Misc

        public override string ToString()
        {
            var vol = Bar.Volume <= 0 ? "" : $" [v:{ Bar.Volume}]";
            return $"{Time} {Code} o:{Bar.Open} h:{Bar.High} l:{Bar.Low} c:{Bar.Close}{vol}";
        }
        
        #endregion
    }

    
}
