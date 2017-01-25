//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using LionFire.Trading.Bots;
//using LionFire.Instantiating;
//using LionFire.Assets;

//namespace LionFire.Trading.Backtesting
//{

//    public class BacktestMarket : SimulatedAccountBase<TBacktestMarket>, IAccount
//    {
//        //#region Relationships

//        //public new BacktestAccount Account {
//        //    get {
//        //        return (BacktestAccount) base.Accounts.SingleOrDefault();
//        //    }
//        //    set {
//        //        base.Accounts.Clear();
//        //        base.Accounts.Add(value);
//        //    }
//        //}

//        //public TAccount TAccount { get; protected set; }

//        //#endregion

//        #region IAccount Implementation

//        public override bool IsBacktesting { get { return true; } }

//        #endregion

//        #region Initialization

//        bool isInitialized = false;
//        public override void Initialize()
//        {
//            if (isInitialized) return;

//            this.TAccount = Template.AccountName.Load<TAccount>();
//            this.TimeFrame = Template.TimeFrame;
//            this.StartDate = Template.StartDate;
//            this.EndDate = Template.EndDate;
                        
//            this.Account = new BacktestAccount(this);

//            base.Initialize();

//            isInitialized = true;
            
//        }

//        #endregion

//        #region Event Handling

//        protected override void OnLastExecutedTimeChanged()
//        {
//#if SanityChecks
//            if (LastExecutedTime != default(DateTime)  && LastExecutedTime != Template.StartDate && double.IsNaN(Account.Balance) )
//            {
//                throw new InvalidOperationException("Backtest in progress while Account Balance is NaN");
//            }
//#endif
//            base.OnLastExecutedTimeChanged(); // Does nothing at the moment
//            this.Account.UpdatePositions();
//        }

//        #endregion
//    }
//}
