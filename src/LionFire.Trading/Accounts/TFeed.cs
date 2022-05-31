﻿#nullable enable

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
        public string? AccountId { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

        /// <summary>
        /// E.g. CTrader, MT4 ECN, MT4 Pro, ...  RENAME to Platform
        /// </summary>
        public string? AccountType { get; internal set; }

        // TODO: Set this from filename during load
        public string? AccountName { get; set; }

        public string? AssetSubPath => Exchange + "." + AccountName;

        public string? Exchange { get; set; }
        public string? ExchangeMarketName
        {
            get
            {
                if (exchangeMarketName == null)
                {
                    exchangeMarketName = Exchange;
                    if (!String.IsNullOrWhiteSpace(ExchangeAreaKind)) { exchangeMarketName += "." + ExchangeAreaKind; }
                    if (!String.IsNullOrWhiteSpace(ExchangeArea)) { exchangeMarketName += "." + ExchangeArea; }
                }
                return exchangeMarketName;
            }
            set { exchangeMarketName = value; }
        }
        public string? exchangeMarketName;
        public string? ExchangeMarketDisplayName => ExchangeMarketName?.Replace(".", " ");

        public virtual string ExchangeAreaKind => "";

        public string ExchangeArea
        {
            get => exchangeArea;
            set
            {
                if (OnlyValidExchangeArea != null && value != OnlyValidExchangeArea)
                {
                    throw new ArgumentException($"ExchangeArea must be {OnlyValidExchangeArea}");
                }
                exchangeArea = value;
            }
        }
        private string exchangeArea = "";
        protected virtual string? OnlyValidExchangeArea => null;

        public string? BrokerDisplayName { get => brokerDisplayName ?? Exchange?.Replace(".", " "); set { brokerDisplayName = value; } }
        public string? brokerDisplayName;

        public virtual string Key => Exchange + "^" + (string.IsNullOrWhiteSpace(ExchangeArea) ? "" : $"^{ExchangeArea}") + AccountId;

        public InstantiationCollection? Children { get; set; }
        IInstantiationCollection? IHierarchicalTemplate.Children => Children;

        // MOVE this to Workspace, don't put startup preferences here.
        public ExecutionStateEx DesiredExecutionState { get; set; }

    }
}
