using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class SettlementInstructions : Message
    {
        public const string MsgType = "T";

        public SettlementInstructions():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public SettlementInstructions(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstID aSettlInstID,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstTransType aSettlInstTransType,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstRefID aSettlInstRefID,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstMode aSettlInstMode,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstSource aSettlInstSource,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAccount aAllocAccount,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.TransactTime aTransactTime)
               : this()
        {
            this.SettlInstID = aSettlInstID;
			this.SettlInstTransType = aSettlInstTransType;
			this.SettlInstRefID = aSettlInstRefID;
			this.SettlInstMode = aSettlInstMode;
			this.SettlInstSource = aSettlInstSource;
			this.AllocAccount = aAllocAccount;
			this.TransactTime = aTransactTime;
        }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstID SettlInstID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstID val) { this.SettlInstID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstID val) { return IsSetSettlInstID(); }

        public bool IsSetSettlInstID() { return IsSetField(Tags.SettlInstID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstTransType SettlInstTransType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstTransType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstTransType val) { this.SettlInstTransType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstTransType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstTransType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstTransType val) { return IsSetSettlInstTransType(); }

        public bool IsSetSettlInstTransType() { return IsSetField(Tags.SettlInstTransType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstRefID SettlInstRefID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstRefID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstRefID val) { this.SettlInstRefID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstRefID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstRefID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstRefID val) { return IsSetSettlInstRefID(); }

        public bool IsSetSettlInstRefID() { return IsSetField(Tags.SettlInstRefID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstMode SettlInstMode
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstMode();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstMode val) { this.SettlInstMode = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstMode Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstMode val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstMode val) { return IsSetSettlInstMode(); }

        public bool IsSetSettlInstMode() { return IsSetField(Tags.SettlInstMode); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstSource SettlInstSource
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstSource();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstSource val) { this.SettlInstSource = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstSource Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstSource val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstSource val) { return IsSetSettlInstSource(); }

        public bool IsSetSettlInstSource() { return IsSetField(Tags.SettlInstSource); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAccount AllocAccount
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAccount();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAccount val) { this.AllocAccount = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAccount Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAccount val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAccount val) { return IsSetAllocAccount(); }

        public bool IsSetAllocAccount() { return IsSetField(Tags.AllocAccount); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlLocation SettlLocation
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlLocation();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlLocation val) { this.SettlLocation = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlLocation Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlLocation val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlLocation val) { return IsSetSettlLocation(); }

        public bool IsSetSettlLocation() { return IsSetField(Tags.SettlLocation); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocID AllocID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocID val) { this.AllocID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocID val) { return IsSetAllocID(); }

        public bool IsSetAllocID() { return IsSetField(Tags.AllocID); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbType StandInstDbType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbType val) { this.StandInstDbType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbType val) { return IsSetStandInstDbType(); }

        public bool IsSetStandInstDbType() { return IsSetField(Tags.StandInstDbType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbName StandInstDbName
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbName();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbName val) { this.StandInstDbName = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbName Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbName val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbName val) { return IsSetStandInstDbName(); }

        public bool IsSetStandInstDbName() { return IsSetField(Tags.StandInstDbName); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbID StandInstDbID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbID val) { this.StandInstDbID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.StandInstDbID val) { return IsSetStandInstDbID(); }

        public bool IsSetStandInstDbID() { return IsSetField(Tags.StandInstDbID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlDeliveryType SettlDeliveryType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlDeliveryType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlDeliveryType val) { this.SettlDeliveryType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlDeliveryType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlDeliveryType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlDeliveryType val) { return IsSetSettlDeliveryType(); }

        public bool IsSetSettlDeliveryType() { return IsSetField(Tags.SettlDeliveryType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlDepositoryCode SettlDepositoryCode
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlDepositoryCode();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlDepositoryCode val) { this.SettlDepositoryCode = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlDepositoryCode Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlDepositoryCode val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlDepositoryCode val) { return IsSetSettlDepositoryCode(); }

        public bool IsSetSettlDepositoryCode() { return IsSetField(Tags.SettlDepositoryCode); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlBrkrCode SettlBrkrCode
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlBrkrCode();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlBrkrCode val) { this.SettlBrkrCode = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlBrkrCode Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlBrkrCode val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlBrkrCode val) { return IsSetSettlBrkrCode(); }

        public bool IsSetSettlBrkrCode() { return IsSetField(Tags.SettlBrkrCode); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstCode SettlInstCode
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstCode();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstCode val) { this.SettlInstCode = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstCode Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstCode val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstCode val) { return IsSetSettlInstCode(); }

        public bool IsSetSettlInstCode() { return IsSetField(Tags.SettlInstCode); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentName SecuritySettlAgentName
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentName();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentName val) { this.SecuritySettlAgentName = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentName Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentName val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentName val) { return IsSetSecuritySettlAgentName(); }

        public bool IsSetSecuritySettlAgentName() { return IsSetField(Tags.SecuritySettlAgentName); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentCode SecuritySettlAgentCode
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentCode();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentCode val) { this.SecuritySettlAgentCode = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentCode Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentCode val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentCode val) { return IsSetSecuritySettlAgentCode(); }

        public bool IsSetSecuritySettlAgentCode() { return IsSetField(Tags.SecuritySettlAgentCode); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentAcctNum SecuritySettlAgentAcctNum
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentAcctNum();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentAcctNum val) { this.SecuritySettlAgentAcctNum = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentAcctNum Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentAcctNum val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentAcctNum val) { return IsSetSecuritySettlAgentAcctNum(); }

        public bool IsSetSecuritySettlAgentAcctNum() { return IsSetField(Tags.SecuritySettlAgentAcctNum); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentAcctName SecuritySettlAgentAcctName
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentAcctName();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentAcctName val) { this.SecuritySettlAgentAcctName = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentAcctName Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentAcctName val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentAcctName val) { return IsSetSecuritySettlAgentAcctName(); }

        public bool IsSetSecuritySettlAgentAcctName() { return IsSetField(Tags.SecuritySettlAgentAcctName); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentContactName SecuritySettlAgentContactName
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentContactName();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentContactName val) { this.SecuritySettlAgentContactName = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentContactName Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentContactName val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentContactName val) { return IsSetSecuritySettlAgentContactName(); }

        public bool IsSetSecuritySettlAgentContactName() { return IsSetField(Tags.SecuritySettlAgentContactName); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentContactPhone SecuritySettlAgentContactPhone
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentContactPhone();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentContactPhone val) { this.SecuritySettlAgentContactPhone = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentContactPhone Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentContactPhone val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecuritySettlAgentContactPhone val) { return IsSetSecuritySettlAgentContactPhone(); }

        public bool IsSetSecuritySettlAgentContactPhone() { return IsSetField(Tags.SecuritySettlAgentContactPhone); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentName CashSettlAgentName
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentName();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentName val) { this.CashSettlAgentName = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentName Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentName val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentName val) { return IsSetCashSettlAgentName(); }

        public bool IsSetCashSettlAgentName() { return IsSetField(Tags.CashSettlAgentName); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentCode CashSettlAgentCode
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentCode();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentCode val) { this.CashSettlAgentCode = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentCode Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentCode val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentCode val) { return IsSetCashSettlAgentCode(); }

        public bool IsSetCashSettlAgentCode() { return IsSetField(Tags.CashSettlAgentCode); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentAcctNum CashSettlAgentAcctNum
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentAcctNum();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentAcctNum val) { this.CashSettlAgentAcctNum = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentAcctNum Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentAcctNum val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentAcctNum val) { return IsSetCashSettlAgentAcctNum(); }

        public bool IsSetCashSettlAgentAcctNum() { return IsSetField(Tags.CashSettlAgentAcctNum); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentAcctName CashSettlAgentAcctName
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentAcctName();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentAcctName val) { this.CashSettlAgentAcctName = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentAcctName Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentAcctName val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentAcctName val) { return IsSetCashSettlAgentAcctName(); }

        public bool IsSetCashSettlAgentAcctName() { return IsSetField(Tags.CashSettlAgentAcctName); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentContactName CashSettlAgentContactName
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentContactName();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentContactName val) { this.CashSettlAgentContactName = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentContactName Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentContactName val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentContactName val) { return IsSetCashSettlAgentContactName(); }

        public bool IsSetCashSettlAgentContactName() { return IsSetField(Tags.CashSettlAgentContactName); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentContactPhone CashSettlAgentContactPhone
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentContactPhone();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentContactPhone val) { this.CashSettlAgentContactPhone = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentContactPhone Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentContactPhone val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CashSettlAgentContactPhone val) { return IsSetCashSettlAgentContactPhone(); }

        public bool IsSetCashSettlAgentContactPhone() { return IsSetField(Tags.CashSettlAgentContactPhone); }


    }
}
