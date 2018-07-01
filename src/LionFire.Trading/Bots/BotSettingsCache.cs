using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading.Bots
{
    public class BotSettingsCache
    {
        public string SymbolCode;

        public DateTime LastStart;
        //public DateTime LastEnd;
        //public KeyValuePair<DateTime, TimeSpan>[] BacktestDurations;
        public static TimeSpan SettingsCacheExpire = TimeSpan.FromSeconds(30);

        public BotSettings Settings;
        public bool IsExpired
        {
            get
            {
                var now = DateTime.UtcNow;
                var delta = now - LastStart;

                if (delta > SettingsCacheExpire)
                {
                    return true;
                }
                return false;
            }
        }
    }
}
