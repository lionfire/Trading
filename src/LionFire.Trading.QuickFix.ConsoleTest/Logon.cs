using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class Logon : Message
    {
        public const string MsgType = "A";

        public Logon():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public Logon(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncryptMethod aEncryptMethod,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.HeartBtInt aHeartBtInt)
               : this()
        {
            this.EncryptMethod = aEncryptMethod;
			this.HeartBtInt = aHeartBtInt;
        }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncryptMethod EncryptMethod
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncryptMethod();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncryptMethod val) { this.EncryptMethod = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncryptMethod Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncryptMethod val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncryptMethod val) { return IsSetEncryptMethod(); }

        public bool IsSetEncryptMethod() { return IsSetField(Tags.EncryptMethod); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.HeartBtInt HeartBtInt
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.HeartBtInt();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.HeartBtInt val) { this.HeartBtInt = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.HeartBtInt Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.HeartBtInt val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.HeartBtInt val) { return IsSetHeartBtInt(); }

        public bool IsSetHeartBtInt() { return IsSetField(Tags.HeartBtInt); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RawDataLength RawDataLength
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.RawDataLength();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.RawDataLength val) { this.RawDataLength = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RawDataLength Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.RawDataLength val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.RawDataLength val) { return IsSetRawDataLength(); }

        public bool IsSetRawDataLength() { return IsSetField(Tags.RawDataLength); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RawData RawData
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.RawData();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.RawData val) { this.RawData = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RawData Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.RawData val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.RawData val) { return IsSetRawData(); }

        public bool IsSetRawData() { return IsSetField(Tags.RawData); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ResetSeqNumFlag ResetSeqNumFlag
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ResetSeqNumFlag();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ResetSeqNumFlag val) { this.ResetSeqNumFlag = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ResetSeqNumFlag Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ResetSeqNumFlag val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ResetSeqNumFlag val) { return IsSetResetSeqNumFlag(); }

        public bool IsSetResetSeqNumFlag() { return IsSetField(Tags.ResetSeqNumFlag); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxMessageSize MaxMessageSize
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxMessageSize();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxMessageSize val) { this.MaxMessageSize = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxMessageSize Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxMessageSize val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaxMessageSize val) { return IsSetMaxMessageSize(); }

        public bool IsSetMaxMessageSize() { return IsSetField(Tags.MaxMessageSize); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMsgTypes NoMsgTypes
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMsgTypes();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMsgTypes val) { this.NoMsgTypes = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMsgTypes Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMsgTypes val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMsgTypes val) { return IsSetNoMsgTypes(); }

        public bool IsSetNoMsgTypes() { return IsSetField(Tags.NoMsgTypes); }


        public class NoMsgTypesGroup : Group
        {
            public static int[] fieldOrder = {Tags.RefMsgType, Tags.MsgDirection, 0};

            public NoMsgTypesGroup() : base(Tags.NoMsgTypes, Tags.RefMsgType, fieldOrder)
            {
            }

            public override Group Clone()
            {
                var clone = new NoMsgTypesGroup();
                clone.CopyStateFrom(this);
                return clone;
            }
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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MsgDirection MsgDirection
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MsgDirection();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MsgDirection val) { this.MsgDirection = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MsgDirection Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MsgDirection val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MsgDirection val) { return IsSetMsgDirection(); }

        public bool IsSetMsgDirection() { return IsSetField(Tags.MsgDirection); }


        }


    }
}
