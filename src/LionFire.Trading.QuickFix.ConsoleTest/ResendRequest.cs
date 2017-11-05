using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class ResendRequest : Message
    {
        public const string MsgType = "2";

        public ResendRequest():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public ResendRequest(LionFire.Trading.QuickFix.ConsoleTest.Fields.BeginSeqNo aBeginSeqNo,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.EndSeqNo aEndSeqNo)
               : this()
        {
            this.BeginSeqNo = aBeginSeqNo;
			this.EndSeqNo = aEndSeqNo;
        }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BeginSeqNo BeginSeqNo
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.BeginSeqNo();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.BeginSeqNo val) { this.BeginSeqNo = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BeginSeqNo Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.BeginSeqNo val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.BeginSeqNo val) { return IsSetBeginSeqNo(); }

        public bool IsSetBeginSeqNo() { return IsSetField(Tags.BeginSeqNo); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EndSeqNo EndSeqNo
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EndSeqNo();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EndSeqNo val) { this.EndSeqNo = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EndSeqNo Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EndSeqNo val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EndSeqNo val) { return IsSetEndSeqNo(); }

        public bool IsSetEndSeqNo() { return IsSetField(Tags.EndSeqNo); }


    }
}
