using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading.Bots
{
    public static class BotSettingsCache
    {
        //public string SymbolCode;

        public static DateTime LastStart;
        //public DateTime LastEnd;
        //public KeyValuePair<DateTime, TimeSpan>[] BacktestDurations;
        public static TimeSpan SettingsCacheExpire => Settings== null ? TimeSpan.FromSeconds(180) : TimeSpan.FromSeconds(Settings.SettingsCacheTimeout);

        public static BotSettings Settings;
        public static bool IsExpired
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
