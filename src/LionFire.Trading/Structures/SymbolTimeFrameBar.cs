using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class SymbolTimeFrameBar : SymbolBar
    {
        #region Construction

        public SymbolTimeFrameBar() { }
        public SymbolTimeFrameBar(string code, Bar bar, DateTime time, TimeFrame timeFrame) : base(code, bar, time)
        {
            this.TimeFrame = timeFrame;
        }

        #endregion

        public TimeFrame TimeFrame { get; set; }

        public override string ToString()
        {
            var vol = Bar.Volume <= 0 ? "" : $" [v:{ Bar.Volume}]";
            return $"{Time} {Code} ({TimeFrame.Name}) o:{Bar.Open} h:{Bar.High} l:{Bar.Low} c:{Bar.Close}{vol}";
        }
    }
}
