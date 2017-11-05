using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class Reject : Message
    {
        public const string MsgType = "3";

        public Reject():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public Reject(LionFire.Trading.QuickFix.ConsoleTest.Fields.RefSeqNum aRefSeqNum)
               : this()
        {
            this.RefSeqNum = aRefSeqNum;
        }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RefSeqNum RefSeqNum
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.RefSeqNum();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.RefSeqNum val) { this.RefSeqNum = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RefSeqNum Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.RefSeqNum val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.RefSeqNum val) { return IsSetRefSeqNum(); }

        public bool IsSetRefSeqNum() { return IsSetField(Tags.RefSeqNum); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RefTagID RefTagID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.RefTagID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.RefTagID val) { this.RefTagID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RefTagID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.RefTagID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.RefTagID val) { return IsSetRefTagID(); }

        public bool IsSetRefTagID() { return IsSetField(Tags.RefTagID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RefMsgType RefMsgType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.RefMsgType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.RefMsgType val) { this.RefMsgType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RefMsgType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.RefMsgType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.RefMsgType val) { return IsSetRefMsgType(); }

        public bool IsSetRefMsgType() { return IsSetField(Tags.RefMsgType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SessionRejectReason SessionRejectReason
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SessionRejectReason();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SessionRejectReason val) { this.SessionRejectReason = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SessionRejectReason Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SessionRejectReason val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SessionRejectReason val) { return IsSetSessionRejectReason(); }

        public bool IsSetSessionRejectReason() { return IsSetField(Tags.SessionRejectReason); }

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
