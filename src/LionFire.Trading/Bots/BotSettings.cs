using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading.Bots
{
    public class BotSettings
    {
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }

        public string BacktestApi { get; set; }
        public string BacktestTable { get; set; }
        public string BacktestTableKey { get; set; }
        public string MonitoringApi { get; set; }

        public double MinFitness { get; set; }
        
        public bool Debug { get; set; }

        public bool Link { get; set; }
        public string LinkApi { get; set; }

    }
}
