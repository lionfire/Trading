using LionFire.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Workspaces
{
    public class ColorDefaults
    {
        public static ColorDefaults Instance { get { return LionFire.Structures.Singleton<ColorDefaults>.Instance; } }

        public Dictionary<BotMode, string[]> FGColors { get; private set; } = new Dictionary<BotMode, string[]>();
        public Dictionary<BotMode, string[]> BGColors { get; private set; } = new Dictionary<BotMode, string[]>();

        public ColorDefaults()
        {
            BGColors.Add(BotMode.Scanner, new string[] { "D059FF" });
            FGColors.Add(BotMode.Scanner, new string[] { "FFFFFF" });

            BGColors.Add(BotMode.Live, new string[] { "6D70FF" });
            FGColors.Add(BotMode.Live, new string[] { "FFFFFF" });

            BGColors.Add(BotMode.Demo, new string[] { "87F7FF" });
            FGColors.Add(BotMode.Demo, new string[] { "FFFFFF" });

            BGColors.Add(BotMode.Paper, new string[] { "6D70FF" });
            FGColors.Add(BotMode.Paper, new string[] { "A0A5A3" });
        }
        public string[] GetDefaultBGColors(BotMode mode)
        {
            return BGColors.TryGetValue(mode);
        }
        public string[] GetDefaultFGColors(BotMode mode)
        {
            return FGColors.TryGetValue(mode);
        }
    }
}
