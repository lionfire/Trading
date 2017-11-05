using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class MarketDataRequestReject : Message
    {
        public const string MsgType = "Y";

        public MarketDataRequestReject():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public MarketDataRequestReject(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqID aMDReqID)
               : this()
        {
            this.MDReqID = aMDReqID;
        }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqID MDReqID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqID val) { this.MDReqID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqID val) { return IsSetMDReqID(); }

        public bool IsSetMDReqID() { return IsSetField(Tags.MDReqID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqRejReason MDReqRejReason
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqRejReason();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqRejReason val) { this.MDReqRejReason = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqRejReason Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqRejReason val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqRejReason val) { return IsSetMDReqRejReason(); }

        public bool IsSetMDReqRejReason() { return IsSetField(Tags.MDReqRejReason); }

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
