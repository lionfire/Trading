using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class ListStatus : Message
    {
        public const string MsgType = "N";

        public ListStatus():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public ListStatus(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListID aListID,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.ListStatusType aListStatusType,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRpts aNoRpts,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.ListOrderStatus aListOrderStatus,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.RptSeq aRptSeq,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.TotNoOrders aTotNoOrders)
               : this()
        {
            this.ListID = aListID;
			this.ListStatusType = aListStatusType;
			this.NoRpts = aNoRpts;
			this.ListOrderStatus = aListOrderStatus;
			this.RptSeq = aRptSeq;
			this.TotNoOrders = aTotNoOrders;
        }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ListStatusType ListStatusType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ListStatusType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListStatusType val) { this.ListStatusType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ListStatusType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListStatusType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListStatusType val) { return IsSetListStatusType(); }

        public bool IsSetListStatusType() { return IsSetField(Tags.ListStatusType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRpts NoRpts
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRpts();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRpts val) { this.NoRpts = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRpts Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRpts val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRpts val) { return IsSetNoRpts(); }

        public bool IsSetNoRpts() { return IsSetField(Tags.NoRpts); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ListOrderStatus ListOrderStatus
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ListOrderStatus();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListOrderStatus val) { this.ListOrderStatus = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ListOrderStatus Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListOrderStatus val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListOrderStatus val) { return IsSetListOrderStatus(); }

        public bool IsSetListOrderStatus() { return IsSetField(Tags.ListOrderStatus); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RptSeq RptSeq
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.RptSeq();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.RptSeq val) { this.RptSeq = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RptSeq Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.RptSeq val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.RptSeq val) { return IsSetRptSeq(); }

        public bool IsSetRptSeq() { return IsSetField(Tags.RptSeq); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ListStatusText ListStatusText
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ListStatusText();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListStatusText val) { this.ListStatusText = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ListStatusText Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListStatusText val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListStatusText val) { return IsSetListStatusText(); }

        public bool IsSetListStatusText() { return IsSetField(Tags.ListStatusText); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedListStatusTextLen EncodedListStatusTextLen
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedListStatusTextLen();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedListStatusTextLen val) { this.EncodedListStatusTextLen = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedListStatusTextLen Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedListStatusTextLen val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedListStatusTextLen val) { return IsSetEncodedListStatusTextLen(); }

        public bool IsSetEncodedListStatusTextLen() { return IsSetField(Tags.EncodedListStatusTextLen); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedListStatusText EncodedListStatusText
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedListStatusText();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedListStatusText val) { this.EncodedListStatusText = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedListStatusText Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedListStatusText val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedListStatusText val) { return IsSetEncodedListStatusText(); }

        public bool IsSetEncodedListStatusText() { return IsSetField(Tags.EncodedListStatusText); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TotNoOrders TotNoOrders
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TotNoOrders();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TotNoOrders val) { this.TotNoOrders = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TotNoOrders Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TotNoOrders val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TotNoOrders val) { return IsSetTotNoOrders(); }

        public bool IsSetTotNoOrders() { return IsSetField(Tags.TotNoOrders); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoOrders NoOrders
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NoOrders();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoOrders val) { this.NoOrders = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoOrders Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoOrders val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoOrders val) { return IsSetNoOrders(); }

        public bool IsSetNoOrders() { return IsSetField(Tags.NoOrders); }


        public class NoOrdersGroup : Group
        {
            public static int[] fieldOrder = {Tags.ClOrdID, Tags.CumQty, Tags.OrdStatus, Tags.LeavesQty, Tags.CxlQty, Tags.AvgPx, Tags.OrdRejReason, Tags.Text, Tags.EncodedTextLen, Tags.EncodedText, 0};

            public NoOrdersGroup() : base(Tags.NoOrders, Tags.ClOrdID, fieldOrder)
            {
            }

            public override Group Clone()
            {
                var clone = new NoOrdersGroup();
                clone.CopyStateFrom(this);
                return clone;
            }
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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CumQty CumQty
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CumQty();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CumQty val) { this.CumQty = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CumQty Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CumQty val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CumQty val) { return IsSetCumQty(); }

        public bool IsSetCumQty() { return IsSetField(Tags.CumQty); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LeavesQty LeavesQty
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LeavesQty();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LeavesQty val) { this.LeavesQty = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LeavesQty Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LeavesQty val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LeavesQty val) { return IsSetLeavesQty(); }

        public bool IsSetLeavesQty() { return IsSetField(Tags.LeavesQty); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlQty CxlQty
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlQty();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlQty val) { this.CxlQty = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlQty Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlQty val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CxlQty val) { return IsSetCxlQty(); }

        public bool IsSetCxlQty() { return IsSetField(Tags.CxlQty); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPx AvgPx
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPx();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPx val) { this.AvgPx = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPx Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPx val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPx val) { return IsSetAvgPx(); }

        public bool IsSetAvgPx() { return IsSetField(Tags.AvgPx); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdRejReason OrdRejReason
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdRejReason();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdRejReason val) { this.OrdRejReason = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdRejReason Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdRejReason val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdRejReason val) { return IsSetOrdRejReason(); }

        public bool IsSetOrdRejReason() { return IsSetField(Tags.OrdRejReason); }

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
}
