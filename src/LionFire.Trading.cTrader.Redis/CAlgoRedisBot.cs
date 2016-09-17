using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;
using LionFire.Extensions.Logging;
using Microsoft.Extensions.Logging;
using System.Threading;
using Newtonsoft.Json;
using LionFire.Applications;

namespace LionFire.Trading.cTrader.Redis
{
    [Flags]
    public enum AccountMode
    {
        Unspecified = 0,
        Demo = 1 << 0,
        Live = 1 << 1,
        Any = Demo | Live,
    }

    public class BrokerAccountConfig
    {
        public AccountMode AccountMode { get; set; }

        /// <summary>
        /// Set to "*" to work with any account id
        /// </summary>
        public string AccountId { get; set; }

        /// <summary>
        /// Set to "*" to work with any broker.
        /// </summary>
        public string BrokerName { get; set; }
    }

    public class TCAlgoRedisBot : BrokerAccountConfig
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 6379;

        public string PublishChannel { get; set; } = "test_in";
        public string ReceiveChannel { get; set; } = "test_out";
        public string QuoteChannel { get; set; } = "tw_quote";

    }

    public class CAlgoRedisBot : AppTask
    {
        public TCAlgoRedisBot Config { get; set; } = new TCAlgoRedisBot();

        ILogger logger;

        public class RedisCmd
        {
            public string cmd;
            public string _callbackId;
            public object payload;
        }

        protected override void Run()
        {
            logger = this.GetLogger();

            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");

            var subscriber = redis.GetSubscriber();

            int cmdNum = 1;
            ConcurrentDictionary<int, Action<string>> callbacks = new ConcurrentDictionary<int, Action<string>>();

            var callbackId = (cmdNum++).GetHashCode();

            Action<string> callback = msg =>
            {
                logger.LogInformation("[TEST] Got response: " + msg);
            };
            callbacks.TryAdd(callbackId, callback);

            var cmd = new RedisCmd
            {
                cmd = "get_time",
                _callbackId = callbackId.ToString(),
            };

            // TODO: Verify vs config:
            // - demo /live
            // - broker name
            // - account id

            //string ping = redis.Ping();
            //logger.LogInformation("Ping: " + redis.Ping());
            //logger.LogInformation("Echo: " + redis.Echo("hello world"));
            //logger.LogInformation("Redis time: " + redis.Time());

            Task.Factory.StartNew(() =>
            {
                    subscriber.Subscribe(new RedisChannel(Config.ReceiveChannel, RedisChannel.PatternMode.Literal), (c, v) =>
                {
                    logger.LogInformation($"[redis:{Config.ReceiveChannel}] " + v.ToString());
                });
                subscriber.Subscribe(new RedisChannel(Config.QuoteChannel, RedisChannel.PatternMode.Literal), (c, v) =>
                {
                    logger.LogInformation($"[redis:{Config.QuoteChannel}] " + v.ToString());
                });

                logger.LogInformation($"[{Config.ReceiveChannel}] (Subscribed)");
                logger.LogInformation($"[{Config.QuoteChannel}] (Subscribed)");

            });

            while (!CancellationToken.HasValue || !CancellationToken.Value.IsCancellationRequested)
            {
                logger.LogInformation("CAlgoRedisBot waiting for termination");
                subscriber.Publish(Config.PublishChannel, JsonConvert.SerializeObject(cmd));

                CancellationToken.Value.WaitHandle.WaitOne(4000);
            }
            logger.LogInformation("CAlgoRedisBot terminating");
        }
        public int pingCount = 0;
    }
}
