using LionFire.Execution;
using LionFire.Instantiating;
using System;
using System.Collections.Generic;
using System.Text;

namespace LionFire.Trading.Accounts
{
    /// <summary>
    /// Read-only feed of market data.
    /// </summary>
    public abstract class TFeed : IHierarchicalTemplate, ITemplate
    {
        public string AccountId { get; set; }
        public string AccessToken { get; set; }

        /// <summary>
        /// E.g. CTrader, MT4 ECN, MT4 Pro, ...
        /// </summary>
        public string AccountType { get; internal set; }
        //public string AccountName { get; set; }

        public string BrokerName { get; set; }

        public string Key => BrokerName + "^" + AccountId;

        public List<ITemplate> Children { get; set; }

        public ExecutionState DesiredExecutionState { get; set; }
    }
}
