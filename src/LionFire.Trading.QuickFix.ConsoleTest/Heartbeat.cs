using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class Heartbeat : Message
    {
        public const string MsgType = "0";

        public Heartbeat():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TestReqID TestReqID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TestReqID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TestReqID val) { this.TestReqID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TestReqID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TestReqID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TestReqID val) { return IsSetTestReqID(); }

        public bool IsSetTestReqID() { return IsSetField(Tags.TestReqID); }


    }
}
