using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading
{

    public class TradeResult
    {
        public ErrorCode? Error { get; set; }
        public bool IsSuccessful { get; set; }
        public PendingOrder PendingOrder { get; set; }
        public Position Position { get; set; }

        public string Message { get; set; }
        public override string ToString()
        {
            return Message ?? Error?.ToString() ?? "(no message)";
        }

        public static TradeResult LimitedByConfig {
            get {
                return limitedByConfig;
            }
        }
        private static TradeResult limitedByConfig = new TradeResult
        {
            IsSuccessful = false,
            Message = "Prevented by configuration and the current state",
        };

        public static TradeResult NotImplemented
        {
            get
            {
                return notImplemented;
            }
        }
        private static TradeResult notImplemented = new TradeResult
        {
            IsSuccessful = false,
            Message = "Not implemented",
        };
    }
}
