using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace LionFire.Trading.Agent
{
    public class NLogConfig
    {
        internal static void Init()
        {
            var config = new NLog.Config.LoggingConfiguration();
            {
                var targ = new NLog.Targets.NetworkTarget
                {
                    //Layout = "${message}",
                    Address = "tcp://localhost:4505",
                };

                config.AddTarget("tcp", targ);
                config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, targ);
            }
            {
                var targ = new NLog.Targets.NetworkTarget
                {
                    Address = "udp://localhost:9999",
                    //Layout = "${log4jxmlevent}"
                    //Layout = "${message}",
                };

                config.AddTarget("udp", targ);
                config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, targ);
            }

            {
                var targ = new NLog.Targets.FileTarget
                {
                    FileName = @"e:\temp\Trading.Agent.log",
                    Layout = "${message}"
                };

                config.AddTarget("file", targ);
                config.AddRule(NLog.LogLevel.Debug, NLog.LogLevel.Fatal, targ);
            }


            {


                // Step 2. Create targets and add them to the configuration 
                var consoleTarget = new ColoredConsoleTarget();
                config.AddTarget("console", consoleTarget);

                var fileTarget = new FileTarget();
                config.AddTarget("file", fileTarget);

                // Step 3. Set target properties 
                consoleTarget.Layout = @"${date:format=HH\:mm\:ss} ${logger} ${message}";
                fileTarget.FileName = "${basedir}/file.txt";
                fileTarget.Layout = "${message}";

                // Step 4. Define rules
                var rule1 = new LoggingRule("*", LogLevel.Debug, consoleTarget);
                config.LoggingRules.Add(rule1);

                var rule2 = new LoggingRule("*", LogLevel.Debug, fileTarget);
                config.LoggingRules.Add(rule2);




            }
            LogManager.Configuration = config;

            {
                // Example usage
                Logger logger = LogManager.GetLogger("Example");
                logger.Trace("trace log message");
                logger.Debug("debug log message");
                logger.Info("info log message");
                logger.Warn("warn log message");
                logger.Error("error log message");
                logger.Fatal("fatal log message");
            }

        }
    }
}
