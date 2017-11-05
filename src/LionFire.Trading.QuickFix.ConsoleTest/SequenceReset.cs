using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class SequenceReset : Message
    {
        public const string MsgType = "4";

        public SequenceReset():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public SequenceReset(LionFire.Trading.QuickFix.ConsoleTest.Fields.NewSeqNo aNewSeqNo)
               : this()
        {
            this.NewSeqNo = aNewSeqNo;
        }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.GapFillFlag GapFillFlag
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.GapFillFlag();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.GapFillFlag val) { this.GapFillFlag = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.GapFillFlag Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.GapFillFlag val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.GapFillFlag val) { return IsSetGapFillFlag(); }

        public bool IsSetGapFillFlag() { return IsSetField(Tags.GapFillFlag); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NewSeqNo NewSeqNo
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NewSeqNo();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NewSeqNo val) { this.NewSeqNo = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NewSeqNo Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NewSeqNo val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NewSeqNo val) { return IsSetNewSeqNo(); }

        public bool IsSetNewSeqNo() { return IsSetField(Tags.NewSeqNo); }


    }
}
