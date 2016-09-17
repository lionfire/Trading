using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LionFire.Extensions.Logging;
using System.Reflection;
using LionFire.Structures;

namespace LionFire.Trading.Bots
{
    // TODO: Rename BotBase to SingleSeriesBotBase  and make a new BotBase that is more generic

    public partial class BotBase<TConfig> : IBot
        where TConfig : BotConfig, new()
    {
        #region Configuration

        BotConfig IBot.BotConfig { get { return Config; } set { Config =(TConfig) value; } }

        public TConfig Config { get; set; } = new TConfig();

        public LosingTradeLimiterConfig LosingTradeLimiterConfig { get; set; } = new LosingTradeLimiterConfig();


#endregion

#if cAlgo
        protected virtual void OnStarting()
#else
        protected override void OnStarting()
#endif
        {
            
            logger = this.GetLogger(this.ToString().Replace(' ', '.'), Config.Log);
            

            logger.LogInformation($"------- START {this} -------");
            
        }
        partial void OnStarting_();

        protected virtual void OnStopping()
        {
            logger.LogInformation($"------- STOP {this} -------");
        }

        protected virtual void OnNewBar()
        {
        }

#region Misc

        public virtual string Label {
            get {
                return label ?? this.GetType().Name;
            }
            set {
                label = value;
            }
        }
        private string label = null;

        public Microsoft.Extensions.Logging.ILogger Logger { get { return logger; } }
        protected Microsoft.Extensions.Logging.ILogger logger;
        //public Microsoft.Extensions.Logging.ILogger BacktestLogger { get; protected set; }

        #endregion
    }


}
