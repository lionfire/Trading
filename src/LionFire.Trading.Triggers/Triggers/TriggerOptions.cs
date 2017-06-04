using System;

namespace LionFire.Triggers
{
    public class TriggerOptions
    {
        public bool Rearm { get; set; }

        /// <summary>
        /// Zero: instant rearm
        /// Null: default value
        /// </summary>
        public TimeSpan? RearmDelay { get; set; }
    }
}
