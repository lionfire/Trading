using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class TradingSessionStatusRequest : Message
    {
        public const string MsgType = "g";

        public TradingSessionStatusRequest():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public TradingSessionStatusRequest(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesReqID aTradSesReqID,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.SubscriptionRequestType aSubscriptionRequestType)
               : this()
        {
            this.TradSesReqID = aTradSesReqID;
			this.SubscriptionRequestType = aSubscriptionRequestType;
        }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesReqID TradSesReqID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesReqID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesReqID val) { this.TradSesReqID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesReqID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesReqID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesReqID val) { return IsSetTradSesReqID(); }

        public bool IsSetTradSesReqID() { return IsSetField(Tags.TradSesReqID); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesMethod TradSesMethod
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesMethod();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesMethod val) { this.TradSesMethod = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesMethod Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesMethod val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesMethod val) { return IsSetTradSesMethod(); }

        public bool IsSetTradSesMethod() { return IsSetField(Tags.TradSesMethod); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesMode TradSesMode
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesMode();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesMode val) { this.TradSesMode = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesMode Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesMode val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesMode val) { return IsSetTradSesMode(); }

        public bool IsSetTradSesMode() { return IsSetField(Tags.TradSesMode); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SubscriptionRequestType SubscriptionRequestType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SubscriptionRequestType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SubscriptionRequestType val) { this.SubscriptionRequestType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SubscriptionRequestType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SubscriptionRequestType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SubscriptionRequestType val) { return IsSetSubscriptionRequestType(); }

        public bool IsSetSubscriptionRequestType() { return IsSetField(Tags.SubscriptionRequestType); }


    }
}
