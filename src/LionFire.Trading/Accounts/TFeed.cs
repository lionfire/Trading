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
        //public string AccountName { get; set; }

        public string BrokerName { get; set; }

        public List<ITemplate> Children { get; set; }

        public ExecutionState DesiredExecutionState { get; set; }
    }
}
