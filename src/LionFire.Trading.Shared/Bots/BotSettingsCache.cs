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
        public static TimeSpan SettingsCacheExpire
            => Settings?.SettingsCacheTimeout.HasValue == true ? TimeSpan.FromSeconds(Settings.SettingsCacheTimeout.Value) : TimeSpan.FromSeconds(180);

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
