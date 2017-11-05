using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class ExecutionReport : Message
    {
        public const string MsgType = "8";

        public ExecutionReport():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public ExecutionReport(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderID aOrderID,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecID aExecID,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecTransType aExecTransType,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecType aExecType,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdStatus aOrdStatus,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.Symbol aSymbol,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.Side aSide,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.LeavesQty aLeavesQty,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.CumQty aCumQty,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPx aAvgPx)
               : this()
        {
            this.OrderID = aOrderID;
			this.ExecID = aExecID;
			this.ExecTransType = aExecTransType;
			this.ExecType = aExecType;
			this.OrdStatus = aOrdStatus;
			this.Symbol = aSymbol;
			this.Side = aSide;
			this.LeavesQty = aLeavesQty;
			this.CumQty = aCumQty;
			this.AvgPx = aAvgPx;
        }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderID OrderID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderID val) { this.OrderID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderID val) { return IsSetOrderID(); }

        public bool IsSetOrderID() { return IsSetField(Tags.OrderID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecondaryOrderID SecondaryOrderID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SecondaryOrderID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecondaryOrderID val) { this.SecondaryOrderID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecondaryOrderID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecondaryOrderID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecondaryOrderID val) { return IsSetSecondaryOrderID(); }

        public bool IsSetSecondaryOrderID() { return IsSetField(Tags.SecondaryOrderID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ClOrdID ClOrdID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ClOrdID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClOrdID val) { this.ClOrdID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ClOrdID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClOrdID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClOrdID val) { return IsSetClOrdID(); }

        public bool IsSetClOrdID() { return IsSetField(Tags.ClOrdID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigClOrdID OrigClOrdID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigClOrdID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigClOrdID val) { this.OrigClOrdID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigClOrdID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigClOrdID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigClOrdID val) { return IsSetOrigClOrdID(); }

        public bool IsSetOrigClOrdID() { return IsSetField(Tags.OrigClOrdID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ClientID ClientID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ClientID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClientID val) { this.ClientID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ClientID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClientID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClientID val) { return IsSetClientID(); }

        public bool IsSetClientID() { return IsSetField(Tags.ClientID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecBroker ExecBroker
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecBroker();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecBroker val) { this.ExecBroker = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecBroker Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecBroker val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecBroker val) { return IsSetExecBroker(); }

        public bool IsSetExecBroker() { return IsSetField(Tags.ExecBroker); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoContraBrokers NoContraBrokers
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NoContraBrokers();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoContraBrokers val) { this.NoContraBrokers = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoContraBrokers Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoContraBrokers val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoContraBrokers val) { return IsSetNoContraBrokers(); }

        public bool IsSetNoContraBrokers() { return IsSetField(Tags.NoContraBrokers); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ListID ListID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ListID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListID val) { this.ListID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ListID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListID val) { return IsSetListID(); }

        public bool IsSetListID() { return IsSetField(Tags.ListID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecID ExecID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecID val) { this.ExecID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecID val) { return IsSetExecID(); }

        public bool IsSetExecID() { return IsSetField(Tags.ExecID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecTransType ExecTransType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecTransType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecTransType val) { this.ExecTransType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecTransType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecTransType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecTransType val) { return IsSetExecTransType(); }

        public bool IsSetExecTransType() { return IsSetField(Tags.ExecTransType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecRefID ExecRefID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecRefID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecRefID val) { this.ExecRefID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecRefID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecRefID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecRefID val) { return IsSetExecRefID(); }

        public bool IsSetExecRefID() { return IsSetField(Tags.ExecRefID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecType ExecType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecType val) { this.ExecType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecType val) { return IsSetExecType(); }

        public bool IsSetExecType() { return IsSetField(Tags.ExecType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdStatus OrdStatus
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdStatus();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdStatus val) { this.OrdStatus = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdStatus Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdStatus val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdStatus val) { return IsSetOrdStatus(); }

        public bool IsSetOrdStatus() { return IsSetField(Tags.OrdStatus); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdRejReason OrdRejReason
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdRejReason();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdRejReason val) { this.OrdRejReason = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdRejReason Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdRejReason val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdRejReason val) { return IsSetOrdRejReason(); }

        public bool IsSetOrdRejReason() { return IsSetField(Tags.OrdRejReason); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecRestatementReason ExecRestatementReason
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecRestatementReason();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecRestatementReason val) { this.ExecRestatementReason = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecRestatementReason Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecRestatementReason val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecRestatementReason val) { return IsSetExecRestatementReason(); }

        public bool IsSetExecRestatementReason() { return IsSetField(Tags.ExecRestatementReason); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Account Account
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Account();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Account val) { this.Account = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Account Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Account val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Account val) { return IsSetAccount(); }

        public bool IsSetAccount() { return IsSetField(Tags.Account); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlmntTyp SettlmntTyp
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlmntTyp();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlmntTyp val) { this.SettlmntTyp = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlmntTyp Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlmntTyp val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlmntTyp val) { return IsSetSettlmntTyp(); }

        public bool IsSetSettlmntTyp() { return IsSetField(Tags.SettlmntTyp); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate FutSettDate
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate val) { this.FutSettDate = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate val) { return IsSetFutSettDate(); }

        public bool IsSetFutSettDate() { return IsSetField(Tags.FutSettDate); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Symbol Symbol
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Symbol();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Symbol val) { this.Symbol = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Symbol Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Symbol val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Symbol val) { return IsSetSymbol(); }

        public bool IsSetSymbol() { return IsSetField(Tags.Symbol); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SymbolSfx SymbolSfx
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SymbolSfx();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SymbolSfx val) { this.SymbolSfx = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SymbolSfx Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SymbolSfx val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SymbolSfx val) { return IsSetSymbolSfx(); }

        public bool IsSetSymbolSfx() { return IsSetField(Tags.SymbolSfx); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityID SecurityID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityID val) { this.SecurityID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityID val) { return IsSetSecurityID(); }

        public bool IsSetSecurityID() { return IsSetField(Tags.SecurityID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IDSource IDSource
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.IDSource();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.IDSource val) { this.IDSource = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IDSource Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.IDSource val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.IDSource val) { return IsSetIDSource(); }

        public bool IsSetIDSource() { return IsSetField(Tags.IDSource); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityType SecurityType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityType val) { this.SecurityType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityType val) { return IsSetSecurityType(); }

        public bool IsSetSecurityType() { return IsSetField(Tags.SecurityType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityMonthYear MaturityMonthYear
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityMonthYear();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityMonthYear val) { this.MaturityMonthYear = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityMonthYear Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityMonthYear val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityMonthYear val) { return IsSetMaturityMonthYear(); }

        public bool IsSetMaturityMonthYear() { return IsSetField(Tags.MaturityMonthYear); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityDay MaturityDay
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityDay();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityDay val) { this.MaturityDay = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityDay Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityDay val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityDay val) { return IsSetMaturityDay(); }

        public bool IsSetMaturityDay() { return IsSetField(Tags.MaturityDay); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.PutOrCall PutOrCall
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.PutOrCall();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.PutOrCall val) { this.PutOrCall = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.PutOrCall Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.PutOrCall val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.PutOrCall val) { return IsSetPutOrCall(); }

        public bool IsSetPutOrCall() { return IsSetField(Tags.PutOrCall); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikePrice StrikePrice
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikePrice();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikePrice val) { this.StrikePrice = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikePrice Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikePrice val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikePrice val) { return IsSetStrikePrice(); }

        public bool IsSetStrikePrice() { return IsSetField(Tags.StrikePrice); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OptAttribute OptAttribute
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OptAttribute();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OptAttribute val) { this.OptAttribute = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OptAttribute Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OptAttribute val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OptAttribute val) { return IsSetOptAttribute(); }

        public bool IsSetOptAttribute() { return IsSetField(Tags.OptAttribute); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ContractMultiplier ContractMultiplier
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ContractMultiplier();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContractMultiplier val) { this.ContractMultiplier = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ContractMultiplier Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContractMultiplier val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContractMultiplier val) { return IsSetContractMultiplier(); }

        public bool IsSetContractMultiplier() { return IsSetField(Tags.ContractMultiplier); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CouponRate CouponRate
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CouponRate();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CouponRate val) { this.CouponRate = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CouponRate Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CouponRate val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CouponRate val) { return IsSetCouponRate(); }

        public bool IsSetCouponRate() { return IsSetField(Tags.CouponRate); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityExchange SecurityExchange
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityExchange();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityExchange val) { this.SecurityExchange = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityExchange Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityExchange val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityExchange val) { return IsSetSecurityExchange(); }

        public bool IsSetSecurityExchange() { return IsSetField(Tags.SecurityExchange); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Issuer Issuer
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Issuer();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Issuer val) { this.Issuer = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Issuer Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Issuer val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Issuer val) { return IsSetIssuer(); }

        public bool IsSetIssuer() { return IsSetField(Tags.Issuer); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuerLen EncodedIssuerLen
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuerLen();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuerLen val) { this.EncodedIssuerLen = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuerLen Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuerLen val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuerLen val) { return IsSetEncodedIssuerLen(); }

        public bool IsSetEncodedIssuerLen() { return IsSetField(Tags.EncodedIssuerLen); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuer EncodedIssuer
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuer();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuer val) { this.EncodedIssuer = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuer Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuer val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuer val) { return IsSetEncodedIssuer(); }

        public bool IsSetEncodedIssuer() { return IsSetField(Tags.EncodedIssuer); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityDesc SecurityDesc
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityDesc();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityDesc val) { this.SecurityDesc = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityDesc Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityDesc val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityDesc val) { return IsSetSecurityDesc(); }

        public bool IsSetSecurityDesc() { return IsSetField(Tags.SecurityDesc); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDescLen EncodedSecurityDescLen
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDescLen();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDescLen val) { this.EncodedSecurityDescLen = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDescLen Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDescLen val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDescLen val) { return IsSetEncodedSecurityDescLen(); }

        public bool IsSetEncodedSecurityDescLen() { return IsSetField(Tags.EncodedSecurityDescLen); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDesc EncodedSecurityDesc
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDesc();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDesc val) { this.EncodedSecurityDesc = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDesc Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDesc val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDesc val) { return IsSetEncodedSecurityDesc(); }

        public bool IsSetEncodedSecurityDesc() { return IsSetField(Tags.EncodedSecurityDesc); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Side Side
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Side();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Side val) { this.Side = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Side Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Side val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Side val) { return IsSetSide(); }

        public bool IsSetSide() { return IsSetField(Tags.Side); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty OrderQty
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty val) { this.OrderQty = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty val) { return IsSetOrderQty(); }

        public bool IsSetOrderQty() { return IsSetField(Tags.OrderQty); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CashOrderQty CashOrderQty
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CashOrderQty();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashOrderQty val) { this.CashOrderQty = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CashOrderQty Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashOrderQty val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashOrderQty val) { return IsSetCashOrderQty(); }

        public bool IsSetCashOrderQty() { return IsSetField(Tags.CashOrderQty); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdType OrdType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdType val) { this.OrdType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdType val) { return IsSetOrdType(); }

        public bool IsSetOrdType() { return IsSetField(Tags.OrdType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Price Price
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Price();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Price val) { this.Price = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Price Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Price val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Price val) { return IsSetPrice(); }

        public bool IsSetPrice() { return IsSetField(Tags.Price); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.StopPx StopPx
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.StopPx();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.StopPx val) { this.StopPx = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.StopPx Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.StopPx val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.StopPx val) { return IsSetStopPx(); }

        public bool IsSetStopPx() { return IsSetField(Tags.StopPx); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.PegDifference PegDifference
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.PegDifference();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.PegDifference val) { this.PegDifference = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.PegDifference Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.PegDifference val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.PegDifference val) { return IsSetPegDifference(); }

        public bool IsSetPegDifference() { return IsSetField(Tags.PegDifference); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.DiscretionInst DiscretionInst
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.DiscretionInst();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.DiscretionInst val) { this.DiscretionInst = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.DiscretionInst Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.DiscretionInst val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.DiscretionInst val) { return IsSetDiscretionInst(); }

        public bool IsSetDiscretionInst() { return IsSetField(Tags.DiscretionInst); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.DiscretionOffset DiscretionOffset
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.DiscretionOffset();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.DiscretionOffset val) { this.DiscretionOffset = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.DiscretionOffset Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.DiscretionOffset val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.DiscretionOffset val) { return IsSetDiscretionOffset(); }

        public bool IsSetDiscretionOffset() { return IsSetField(Tags.DiscretionOffset); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Currency Currency
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Currency();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Currency val) { this.Currency = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Currency Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Currency val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Currency val) { return IsSetCurrency(); }

        public bool IsSetCurrency() { return IsSetField(Tags.Currency); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ComplianceID ComplianceID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ComplianceID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ComplianceID val) { this.ComplianceID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ComplianceID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ComplianceID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ComplianceID val) { return IsSetComplianceID(); }

        public bool IsSetComplianceID() { return IsSetField(Tags.ComplianceID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SolicitedFlag SolicitedFlag
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SolicitedFlag();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SolicitedFlag val) { this.SolicitedFlag = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SolicitedFlag Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SolicitedFlag val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SolicitedFlag val) { return IsSetSolicitedFlag(); }

        public bool IsSetSolicitedFlag() { return IsSetField(Tags.SolicitedFlag); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TimeInForce TimeInForce
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TimeInForce();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TimeInForce val) { this.TimeInForce = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TimeInForce Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TimeInForce val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TimeInForce val) { return IsSetTimeInForce(); }

        public bool IsSetTimeInForce() { return IsSetField(Tags.TimeInForce); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EffectiveTime EffectiveTime
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EffectiveTime();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EffectiveTime val) { this.EffectiveTime = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EffectiveTime Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EffectiveTime val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EffectiveTime val) { return IsSetEffectiveTime(); }

        public bool IsSetEffectiveTime() { return IsSetField(Tags.EffectiveTime); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireDate ExpireDate
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireDate();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireDate val) { this.ExpireDate = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireDate Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireDate val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireDate val) { return IsSetExpireDate(); }

        public bool IsSetExpireDate() { return IsSetField(Tags.ExpireDate); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireTime ExpireTime
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireTime();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireTime val) { this.ExpireTime = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireTime Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireTime val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireTime val) { return IsSetExpireTime(); }

        public bool IsSetExpireTime() { return IsSetField(Tags.ExpireTime); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecInst ExecInst
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecInst();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecInst val) { this.ExecInst = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecInst Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecInst val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecInst val) { return IsSetExecInst(); }

        public bool IsSetExecInst() { return IsSetField(Tags.ExecInst); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Rule80A Rule80A
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Rule80A();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Rule80A val) { this.Rule80A = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Rule80A Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Rule80A val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Rule80A val) { return IsSetRule80A(); }

        public bool IsSetRule80A() { return IsSetField(Tags.Rule80A); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastShares LastShares
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LastShares();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastShares val) { this.LastShares = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastShares Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastShares val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastShares val) { return IsSetLastShares(); }

        public bool IsSetLastShares() { return IsSetField(Tags.LastShares); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastPx LastPx
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LastPx();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastPx val) { this.LastPx = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastPx Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastPx val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastPx val) { return IsSetLastPx(); }

        public bool IsSetLastPx() { return IsSetField(Tags.LastPx); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastSpotRate LastSpotRate
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LastSpotRate();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastSpotRate val) { this.LastSpotRate = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastSpotRate Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastSpotRate val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastSpotRate val) { return IsSetLastSpotRate(); }

        public bool IsSetLastSpotRate() { return IsSetField(Tags.LastSpotRate); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastForwardPoints LastForwardPoints
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LastForwardPoints();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastForwardPoints val) { this.LastForwardPoints = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastForwardPoints Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastForwardPoints val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastForwardPoints val) { return IsSetLastForwardPoints(); }

        public bool IsSetLastForwardPoints() { return IsSetField(Tags.LastForwardPoints); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastMkt LastMkt
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LastMkt();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastMkt val) { this.LastMkt = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastMkt Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastMkt val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastMkt val) { return IsSetLastMkt(); }

        public bool IsSetLastMkt() { return IsSetField(Tags.LastMkt); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradingSessionID TradingSessionID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TradingSessionID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradingSessionID val) { this.TradingSessionID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradingSessionID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradingSessionID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradingSessionID val) { return IsSetTradingSessionID(); }

        public bool IsSetTradingSessionID() { return IsSetField(Tags.TradingSessionID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastCapacity LastCapacity
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LastCapacity();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastCapacity val) { this.LastCapacity = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastCapacity Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastCapacity val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastCapacity val) { return IsSetLastCapacity(); }

        public bool IsSetLastCapacity() { return IsSetField(Tags.LastCapacity); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LeavesQty LeavesQty
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LeavesQty();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LeavesQty val) { this.LeavesQty = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LeavesQty Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LeavesQty val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LeavesQty val) { return IsSetLeavesQty(); }

        public bool IsSetLeavesQty() { return IsSetField(Tags.LeavesQty); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CumQty CumQty
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CumQty();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CumQty val) { this.CumQty = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CumQty Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CumQty val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CumQty val) { return IsSetCumQty(); }

        public bool IsSetCumQty() { return IsSetField(Tags.CumQty); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPx AvgPx
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPx();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPx val) { this.AvgPx = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPx Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPx val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPx val) { return IsSetAvgPx(); }

        public bool IsSetAvgPx() { return IsSetField(Tags.AvgPx); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.DayOrderQty DayOrderQty
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.DayOrderQty();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.DayOrderQty val) { this.DayOrderQty = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.DayOrderQty Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.DayOrderQty val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.DayOrderQty val) { return IsSetDayOrderQty(); }

        public bool IsSetDayOrderQty() { return IsSetField(Tags.DayOrderQty); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.DayCumQty DayCumQty
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.DayCumQty();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.DayCumQty val) { this.DayCumQty = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.DayCumQty Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.DayCumQty val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.DayCumQty val) { return IsSetDayCumQty(); }

        public bool IsSetDayCumQty() { return IsSetField(Tags.DayCumQty); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.DayAvgPx DayAvgPx
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.DayAvgPx();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.DayAvgPx val) { this.DayAvgPx = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.DayAvgPx Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.DayAvgPx val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.DayAvgPx val) { return IsSetDayAvgPx(); }

        public bool IsSetDayAvgPx() { return IsSetField(Tags.DayAvgPx); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.GTBookingInst GTBookingInst
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.GTBookingInst();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.GTBookingInst val) { this.GTBookingInst = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.GTBookingInst Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.GTBookingInst val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.GTBookingInst val) { return IsSetGTBookingInst(); }

        public bool IsSetGTBookingInst() { return IsSetField(Tags.GTBookingInst); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeDate TradeDate
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeDate();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeDate val) { this.TradeDate = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeDate Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeDate val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeDate val) { return IsSetTradeDate(); }

        public bool IsSetTradeDate() { return IsSetField(Tags.TradeDate); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TransactTime TransactTime
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TransactTime();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TransactTime val) { this.TransactTime = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TransactTime Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TransactTime val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TransactTime val) { return IsSetTransactTime(); }

        public bool IsSetTransactTime() { return IsSetField(Tags.TransactTime); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ReportToExch ReportToExch
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ReportToExch();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ReportToExch val) { this.ReportToExch = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ReportToExch Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ReportToExch val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ReportToExch val) { return IsSetReportToExch(); }

        public bool IsSetReportToExch() { return IsSetField(Tags.ReportToExch); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Commission Commission
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Commission();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Commission val) { this.Commission = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Commission Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Commission val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Commission val) { return IsSetCommission(); }

        public bool IsSetCommission() { return IsSetField(Tags.Commission); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CommType CommType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CommType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CommType val) { this.CommType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CommType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CommType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CommType val) { return IsSetCommType(); }

        public bool IsSetCommType() { return IsSetField(Tags.CommType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.GrossTradeAmt GrossTradeAmt
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.GrossTradeAmt();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.GrossTradeAmt val) { this.GrossTradeAmt = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.GrossTradeAmt Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.GrossTradeAmt val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.GrossTradeAmt val) { return IsSetGrossTradeAmt(); }

        public bool IsSetGrossTradeAmt() { return IsSetField(Tags.GrossTradeAmt); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrAmt SettlCurrAmt
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrAmt();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrAmt val) { this.SettlCurrAmt = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrAmt Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrAmt val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrAmt val) { return IsSetSettlCurrAmt(); }

        public bool IsSetSettlCurrAmt() { return IsSetField(Tags.SettlCurrAmt); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrency SettlCurrency
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrency();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrency val) { this.SettlCurrency = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrency Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrency val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrency val) { return IsSetSettlCurrency(); }

        public bool IsSetSettlCurrency() { return IsSetField(Tags.SettlCurrency); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRate SettlCurrFxRate
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRate();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRate val) { this.SettlCurrFxRate = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRate Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRate val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRate val) { return IsSetSettlCurrFxRate(); }

        public bool IsSetSettlCurrFxRate() { return IsSetField(Tags.SettlCurrFxRate); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRateCalc SettlCurrFxRateCalc
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRateCalc();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRateCalc val) { this.SettlCurrFxRateCalc = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRateCalc Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRateCalc val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRateCalc val) { return IsSetSettlCurrFxRateCalc(); }

        public bool IsSetSettlCurrFxRateCalc() { return IsSetField(Tags.SettlCurrFxRateCalc); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.HandlInst HandlInst
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.HandlInst();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.HandlInst val) { this.HandlInst = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.HandlInst Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.HandlInst val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.HandlInst val) { return IsSetHandlInst(); }

        public bool IsSetHandlInst() { return IsSetField(Tags.HandlInst); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MinQty MinQty
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MinQty();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MinQty val) { this.MinQty = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MinQty Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MinQty val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MinQty val) { return IsSetMinQty(); }

        public bool IsSetMinQty() { return IsSetField(Tags.MinQty); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxFloor MaxFloor
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxFloor();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxFloor val) { this.MaxFloor = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxFloor Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxFloor val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxFloor val) { return IsSetMaxFloor(); }

        public bool IsSetMaxFloor() { return IsSetField(Tags.MaxFloor); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenClose OpenClose
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenClose();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenClose val) { this.OpenClose = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenClose Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenClose val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenClose val) { return IsSetOpenClose(); }

        public bool IsSetOpenClose() { return IsSetField(Tags.OpenClose); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxShow MaxShow
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxShow();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxShow val) { this.MaxShow = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxShow Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxShow val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxShow val) { return IsSetMaxShow(); }

        public bool IsSetMaxShow() { return IsSetField(Tags.MaxShow); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Text Text
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Text();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Text val) { this.Text = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Text Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Text val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Text val) { return IsSetText(); }

        public bool IsSetText() { return IsSetField(Tags.Text); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedTextLen EncodedTextLen
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedTextLen();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedTextLen val) { this.EncodedTextLen = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedTextLen Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedTextLen val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedTextLen val) { return IsSetEncodedTextLen(); }

        public bool IsSetEncodedTextLen() { return IsSetField(Tags.EncodedTextLen); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedText EncodedText
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedText();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedText val) { this.EncodedText = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedText Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedText val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedText val) { return IsSetEncodedText(); }

        public bool IsSetEncodedText() { return IsSetField(Tags.EncodedText); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate2 FutSettDate2
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate2();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate2 val) { this.FutSettDate2 = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate2 Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate2 val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate2 val) { return IsSetFutSettDate2(); }

        public bool IsSetFutSettDate2() { return IsSetField(Tags.FutSettDate2); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty2 OrderQty2
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty2();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty2 val) { this.OrderQty2 = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty2 Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty2 val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty2 val) { return IsSetOrderQty2(); }

        public bool IsSetOrderQty2() { return IsSetField(Tags.OrderQty2); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ClearingFirm ClearingFirm
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ClearingFirm();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClearingFirm val) { this.ClearingFirm = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ClearingFirm Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClearingFirm val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClearingFirm val) { return IsSetClearingFirm(); }

        public bool IsSetClearingFirm() { return IsSetField(Tags.ClearingFirm); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ClearingAccount ClearingAccount
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ClearingAccount();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClearingAccount val) { this.ClearingAccount = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ClearingAccount Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClearingAccount val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClearingAccount val) { return IsSetClearingAccount(); }

        public bool IsSetClearingAccount() { return IsSetField(Tags.ClearingAccount); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MultiLegReportingType MultiLegReportingType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MultiLegReportingType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MultiLegReportingType val) { this.MultiLegReportingType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MultiLegReportingType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MultiLegReportingType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MultiLegReportingType val) { return IsSetMultiLegReportingType(); }

        public bool IsSetMultiLegReportingType() { return IsSetField(Tags.MultiLegReportingType); }


        public class NoContraBrokersGroup : Group
        {
            public static int[] fieldOrder = {Tags.ContraBroker, Tags.ContraTrader, Tags.ContraTradeQty, Tags.ContraTradeTime, 0};

            public NoContraBrokersGroup() : base(Tags.NoContraBrokers, Tags.ContraBroker, fieldOrder)
            {
            }

            public override Group Clone()
            {
                var clone = new NoContraBrokersGroup();
                clone.CopyStateFrom(this);
                return clone;
            }
        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraBroker ContraBroker
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraBroker();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraBroker val) { this.ContraBroker = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraBroker Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraBroker val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraBroker val) { return IsSetContraBroker(); }

        public bool IsSetContraBroker() { return IsSetField(Tags.ContraBroker); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTrader ContraTrader
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTrader();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTrader val) { this.ContraTrader = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTrader Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTrader val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTrader val) { return IsSetContraTrader(); }

        public bool IsSetContraTrader() { return IsSetField(Tags.ContraTrader); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTradeQty ContraTradeQty
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTradeQty();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTradeQty val) { this.ContraTradeQty = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTradeQty Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTradeQty val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTradeQty val) { return IsSetContraTradeQty(); }

        public bool IsSetContraTradeQty() { return IsSetField(Tags.ContraTradeQty); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTradeTime ContraTradeTime
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTradeTime();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTradeTime val) { this.ContraTradeTime = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTradeTime Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTradeTime val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContraTradeTime val) { return IsSetContraTradeTime(); }

        public bool IsSetContraTradeTime() { return IsSetField(Tags.ContraTradeTime); }


        }


    }
}
