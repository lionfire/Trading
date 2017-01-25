using LionFire.Assets;
using LionFire.Reactive;
using LionFire.Reactive.Subjects;
using LionFire.Instantiating;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Accounts
{
    public abstract class LiveAccountBase<TTemplate> : AccountBase<TTemplate>, IAccount
        where TTemplate : TAccount
    {

        #region State

        public override double Equity { get; protected set; }
        public override string Currency { get; }


        #region Balance

        public override double Balance
        {
            get { return balance; }
            protected set
            {
                if (balance == value) return;
                balance = value;
                BalanceChanged?.Invoke();
            }
        }
        private double balance = double.NaN;

        public event Action BalanceChanged;

        #endregion

        #endregion
        

        #region Server State

        #region Time

        public override DateTime ServerTime
        {
            get
            {
                return serverTime;
            }
            protected set
            {
                serverTime = value;
                var delta = DateTime.UtcNow - value;
                if (Math.Abs(delta.TotalMinutes) < 5.0) // REVIEW
                {
                    LocalDelta = DateTime.UtcNow - value;
                }
            }
        }
        protected DateTime serverTime = default(DateTime);
        public void AdvanceServerTime(DateTime time)
        {
            if (time > serverTime)
            {
                ServerTime = time;
            }
        }

        public override DateTime ExtrapolatedServerTime
        {
            get
            {
                return DateTime.UtcNow + LocalDelta; // REVIEW
            }
        }

        /// <summary>
        /// Positive delta: local clock is ahead of server.
        /// Negative delta: server clock is ahead.
        /// </summary>
        public TimeSpan LocalDelta
        {
            get; set;
        }

        #endregion

        #endregion

        #region Informational Properties


        public override bool IsBacktesting { get { return false; } }

        public override bool IsSimulation
        {
            get
            {
                return false;
            }
        }

        #region Derived

        public override bool IsRealMoney
        {
            get
            {
                return !IsDemo;
            }
        }

        #endregion

        #region Symbol Information

        public override IEnumerable<string> SymbolsAvailable
        {
            get
            {
                return BrokerInfoUtils.GetSymbolsAvailable(Template?.BrokerName);
            }
        }

        public IEnumerable<string> GetSymbolTimeFramesAvailable(string symbol)
        {
            logger.LogWarning("NOTIMPLEMENTED GetSymbolTimeFramesAvailable");
            return Enumerable.Empty<string>();
        }

        #endregion

        #endregion
    }
}
