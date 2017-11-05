using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class BusinessMessageReject : Message
    {
        public const string MsgType = "j";

        public BusinessMessageReject():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public BusinessMessageReject(LionFire.Trading.QuickFix.ConsoleTest.Fields.RefMsgType aRefMsgType,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.BusinessRejectReason aBusinessRejectReason)
               : this()
        {
            this.RefMsgType = aRefMsgType;
			this.BusinessRejectReason = aBusinessRejectReason;
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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BusinessRejectRefID BusinessRejectRefID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.BusinessRejectRefID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.BusinessRejectRefID val) { this.BusinessRejectRefID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BusinessRejectRefID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.BusinessRejectRefID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.BusinessRejectRefID val) { return IsSetBusinessRejectRefID(); }

        public bool IsSetBusinessRejectRefID() { return IsSetField(Tags.BusinessRejectRefID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BusinessRejectReason BusinessRejectReason
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.BusinessRejectReason();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.BusinessRejectReason val) { this.BusinessRejectReason = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BusinessRejectReason Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.BusinessRejectReason val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.BusinessRejectReason val) { return IsSetBusinessRejectReason(); }

        public bool IsSetBusinessRejectReason() { return IsSetField(Tags.BusinessRejectReason); }

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
