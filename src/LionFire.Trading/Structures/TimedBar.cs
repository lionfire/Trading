using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public class TimeFrameBar : TimedBar
    {
        public string TimeFrame { get; set; }
    }

    public class TimedBar : Bar
    {
        public TimedBar() { }
        public TimedBar(DateTime date, double open, double high, double low, double close, double volume)
        {
            this.OpenTime = date;
            this.Open = open;
            this.High = high;
            this.Low = low;
            this.Close = close;
            this.Volume = volume;
        }

        public TimedBar(SymbolBar b)
        {
            this.OpenTime = b.Time;
            this.High = b.Bar.High;
            this.Low = b.Bar.Low;
            this.Open = b.Bar.Open;
            this.Close = b.Bar.Close;
            this.Volume = b.Bar.Volume;
        }
        public TimedBar(TimedBarStruct b)
        {
            this.OpenTime = b.OpenTime;
            this.High = b.High;
            this.Low = b.Low;
            this.Open = b.Open;
            this.Close = b.Close;
            this.Volume = b.Volume;
        }
        public static implicit operator TimedBar(TimedBarStruct b)
        {
            return new TimedBar(b);
        }

        public DateTime OpenTime { get; set; }

        public override string ToString()
        {
            var date = OpenTime.ToDefaultString();
            return $"{date} {base.ToString()}";
        }


        public void Merge(TimedBar next)
        {

            var current = this;
            if (next != null)
            {
                High = Math.Max(current.High, next.High);
                Low = Math.Min(current.Low, next.Low);
                Close = next.Close;
                Volume = current.Volume + next.Volume;
            }

            //return new TimedBar
            //{
            //    OpenTime = current.OpenTime,
            //    Open = current.Open,
            //    Close = next.Close,
            //    High = Math.Max(current.High, next.High),
            //    Low = Math.Min(current.Low, next.Low),
            //    Volume = current.Volume + next.Volume
            //};
        }

        public TimedBar Clone()
        {
            return new TimedBar
            {
                OpenTime = this.OpenTime,
                Open = this.Open,
                High = this.High,
                Low = this.Low,
                Close = this.Close,
                Volume = this.Volume,
            };
        }
    }
}
