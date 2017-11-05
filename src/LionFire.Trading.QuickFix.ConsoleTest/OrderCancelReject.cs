using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class OrderCancelReject : Message
    {
        public const string MsgType = "9";

        public OrderCancelReject():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public OrderCancelReject(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderID aOrderID,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.ClOrdID aClOrdID,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigClOrdID aOrigClOrdID,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdStatus aOrdStatus,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlRejResponseTo aCxlRejResponseTo)
               : this()
        {
            this.OrderID = aOrderID;
			this.ClOrdID = aClOrdID;
			this.OrigClOrdID = aOrigClOrdID;
			this.OrdStatus = aOrdStatus;
			this.CxlRejResponseTo = aCxlRejResponseTo;
        }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderID OrderID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderID val) { this.OrderID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderID val) { return IsSetOrderID(); }

        public bool IsSetOrderID() { return IsSetField(Tags.OrderID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecondaryOrderID SecondaryOrderID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SecondaryOrderID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecondaryOrderID val) { this.SecondaryOrderID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecondaryOrderID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecondaryOrderID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecondaryOrderID val) { return IsSetSecondaryOrderID(); }

        public bool IsSetSecondaryOrderID() { return IsSetField(Tags.SecondaryOrderID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ClOrdID ClOrdID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ClOrdID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClOrdID val) { this.ClOrdID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ClOrdID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClOrdID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClOrdID val) { return IsSetClOrdID(); }

        public bool IsSetClOrdID() { return IsSetField(Tags.ClOrdID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigClOrdID OrigClOrdID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigClOrdID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigClOrdID val) { this.OrigClOrdID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigClOrdID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigClOrdID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigClOrdID val) { return IsSetOrigClOrdID(); }

        public bool IsSetOrigClOrdID() { return IsSetField(Tags.OrigClOrdID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdStatus OrdStatus
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdStatus();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdStatus val) { this.OrdStatus = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdStatus Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdStatus val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdStatus val) { return IsSetOrdStatus(); }

        public bool IsSetOrdStatus() { return IsSetField(Tags.OrdStatus); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ListID ListID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ListID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListID val) { this.ListID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ListID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListID val) { return IsSetListID(); }

        public bool IsSetListID() { return IsSetField(Tags.ListID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Account Account
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Account();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Account val) { this.Account = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Account Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Account val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Account val) { return IsSetAccount(); }

        public bool IsSetAccount() { return IsSetField(Tags.Account); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlRejResponseTo CxlRejResponseTo
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlRejResponseTo();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlRejResponseTo val) { this.CxlRejResponseTo = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlRejResponseTo Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlRejResponseTo val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlRejResponseTo val) { return IsSetCxlRejResponseTo(); }

        public bool IsSetCxlRejResponseTo() { return IsSetField(Tags.CxlRejResponseTo); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlRejReason CxlRejReason
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlRejReason();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlRejReason val) { this.CxlRejReason = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlRejReason Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlRejReason val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlRejReason val) { return IsSetCxlRejReason(); }

        public bool IsSetCxlRejReason() { return IsSetField(Tags.CxlRejReason); }

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
