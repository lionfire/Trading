using System;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LionFire.Extensions.Logging;

namespace LionFire.Trading.Indicators
{

    public abstract partial class IndicatorBase<TConfig> : IIndicator
        where TConfig : IIndicatorConfig
    {
        public IndicatorBase() { }

        public IndicatorBase(TConfig config) { this.Config = config; }

        //public DateTime StartTime { get; set; }
        //public TimeFrame TimeFrame { get; set; }

        //protected SortedList<KeyValuePair<DateTime, TimeSpan>, int> indexOffsets = new SortedList<KeyValuePair<DateTime, TimeSpan>, int>();

        ////private DateTime

        //private void Initialize()
        //{
        //}

        //protected void Calculate(int index, DateTime indexStartTime, TimeSpan indexInterval)
        //{
        //}



        #region Configuration

        public TConfig Config { get; set; }

        IIndicatorConfig IIndicator.Config { get{return this.Config;} set { this.Config = (TConfig)value; } }

        #endregion

        protected virtual void Init()
        {
            InitLog();            
        }

        public virtual void OnBotStart()
        {
            _OnBotStart();
        }

        partial void _OnBotStart();


        public virtual void InitLog()
        {
            try
            {
                l = this.GetLogger(this.ToString().Replace(' ', '.'), (bool)Config.Log);

                l.LogInformation($"....... START {this.ToStringDescription()} .......");
            }
            catch (TypeInitializationException tie)
            {
                throw tie.InnerException.InnerException;
            }
        }

        #region Misc

        public virtual string ToStringDescription()
        {
            return this.ToString();
        }

        protected Microsoft.Extensions.Logging.ILogger l;

        #endregion

        public virtual void CalculateToTime(DateTime openTime)
        {
            throw new NotImplementedException();
        }

        
    }
}
