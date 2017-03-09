using LionFire.Instantiating;
using LionFire.Trading.Bots;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Bots
{
    public class BotConfigRepository
    {
        public static string ConfigDir { get { return Path.Combine(LionFireEnvironment.AppProgramDataDir, @"Algos"); } } 

        public static async Task<string> SaveConfig(IBot bot)
        {
            var dir = ConfigDir;
            //dir = Path.Combine(dir, bot.GetType().Name);
            //var version = bot.Version.GetMinorCompatibilityVersion();
            //dir = Path.Combine(dir, version);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var config = bot.Template;
            if (config.Id == null)
            {
                config.Id = IdUtils.GenerateId();
            }

            var filename = config.Id + ".json";

            var path = Path.Combine(dir, filename);

            var tr = new TemplateReference(bot);

            //var json = JsonConvert.SerializeObject(bot.Template);
            var json = JsonConvert.SerializeObject(tr);

            using (var sw = new StreamWriter(new FileStream(path, FileMode.Create)))
            {
                await sw.WriteAsync(json).ConfigureAwait(false);
            }

            return config.Id;
        }
    }
}
