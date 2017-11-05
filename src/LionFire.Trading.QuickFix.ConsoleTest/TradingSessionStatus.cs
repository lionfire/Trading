using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class TradingSessionStatus : Message
    {
        public const string MsgType = "h";

        public TradingSessionStatus():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public TradingSessionStatus(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradingSessionID aTradingSessionID,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesStatus aTradSesStatus)
               : this()
        {
            this.TradingSessionID = aTradingSessionID;
			this.TradSesStatus = aTradSesStatus;
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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnsolicitedIndicator UnsolicitedIndicator
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.UnsolicitedIndicator();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnsolicitedIndicator val) { this.UnsolicitedIndicator = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnsolicitedIndicator Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnsolicitedIndicator val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnsolicitedIndicator val) { return IsSetUnsolicitedIndicator(); }

        public bool IsSetUnsolicitedIndicator() { return IsSetField(Tags.UnsolicitedIndicator); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesStatus TradSesStatus
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesStatus();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesStatus val) { this.TradSesStatus = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesStatus Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesStatus val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesStatus val) { return IsSetTradSesStatus(); }

        public bool IsSetTradSesStatus() { return IsSetField(Tags.TradSesStatus); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesStartTime TradSesStartTime
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesStartTime();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesStartTime val) { this.TradSesStartTime = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesStartTime Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesStartTime val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesStartTime val) { return IsSetTradSesStartTime(); }

        public bool IsSetTradSesStartTime() { return IsSetField(Tags.TradSesStartTime); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesOpenTime TradSesOpenTime
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesOpenTime();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesOpenTime val) { this.TradSesOpenTime = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesOpenTime Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesOpenTime val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesOpenTime val) { return IsSetTradSesOpenTime(); }

        public bool IsSetTradSesOpenTime() { return IsSetField(Tags.TradSesOpenTime); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesPreCloseTime TradSesPreCloseTime
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesPreCloseTime();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesPreCloseTime val) { this.TradSesPreCloseTime = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesPreCloseTime Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesPreCloseTime val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesPreCloseTime val) { return IsSetTradSesPreCloseTime(); }

        public bool IsSetTradSesPreCloseTime() { return IsSetField(Tags.TradSesPreCloseTime); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesCloseTime TradSesCloseTime
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesCloseTime();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesCloseTime val) { this.TradSesCloseTime = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesCloseTime Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesCloseTime val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesCloseTime val) { return IsSetTradSesCloseTime(); }

        public bool IsSetTradSesCloseTime() { return IsSetField(Tags.TradSesCloseTime); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesEndTime TradSesEndTime
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesEndTime();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesEndTime val) { this.TradSesEndTime = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesEndTime Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesEndTime val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradSesEndTime val) { return IsSetTradSesEndTime(); }

        public bool IsSetTradSesEndTime() { return IsSetField(Tags.TradSesEndTime); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalVolumeTraded TotalVolumeTraded
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalVolumeTraded();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalVolumeTraded val) { this.TotalVolumeTraded = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalVolumeTraded Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalVolumeTraded val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalVolumeTraded val) { return IsSetTotalVolumeTraded(); }

        public bool IsSetTotalVolumeTraded() { return IsSetField(Tags.TotalVolumeTraded); }

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


    }
}
