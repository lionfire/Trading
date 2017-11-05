using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class BidResponse : Message
    {
        public const string MsgType = "l";

        public BidResponse():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidID BidID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.BidID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidID val) { this.BidID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidID val) { return IsSetBidID(); }

        public bool IsSetBidID() { return IsSetField(Tags.BidID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ClientBidID ClientBidID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ClientBidID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClientBidID val) { this.ClientBidID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ClientBidID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClientBidID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClientBidID val) { return IsSetClientBidID(); }

        public bool IsSetClientBidID() { return IsSetField(Tags.ClientBidID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoBidComponents NoBidComponents
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NoBidComponents();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoBidComponents val) { this.NoBidComponents = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoBidComponents Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoBidComponents val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoBidComponents val) { return IsSetNoBidComponents(); }

        public bool IsSetNoBidComponents() { return IsSetField(Tags.NoBidComponents); }


        public class NoBidComponentsGroup : Group
        {
            public static int[] fieldOrder = {Tags.Commission, Tags.CommType, Tags.ListID, Tags.Country, Tags.Side, Tags.Price, Tags.PriceType, Tags.FairValue, Tags.NetGrossInd, Tags.SettlmntTyp, Tags.FutSettDate, Tags.TradingSessionID, Tags.Text, Tags.EncodedTextLen, Tags.EncodedText, 0};

            public NoBidComponentsGroup() : base(Tags.NoBidComponents, Tags.Commission, fieldOrder)
            {
            }

            public override Group Clone()
            {
                var clone = new NoBidComponentsGroup();
                clone.CopyStateFrom(this);
                return clone;
            }
        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Commission Commission
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Commission();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Commission val) { this.Commission = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Commission Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Commission val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Commission val) { return IsSetCommission(); }

        public bool IsSetCommission() { return IsSetField(Tags.Commission); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CommType CommType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CommType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CommType val) { this.CommType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CommType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CommType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CommType val) { return IsSetCommType(); }

        public bool IsSetCommType() { return IsSetField(Tags.CommType); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Country Country
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Country();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Country val) { this.Country = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Country Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Country val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Country val) { return IsSetCountry(); }

        public bool IsSetCountry() { return IsSetField(Tags.Country); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Side Side
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Side();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Side val) { this.Side = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Side Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Side val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Side val) { return IsSetSide(); }

        public bool IsSetSide() { return IsSetField(Tags.Side); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Price Price
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Price();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Price val) { this.Price = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Price Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Price val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Price val) { return IsSetPrice(); }

        public bool IsSetPrice() { return IsSetField(Tags.Price); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.PriceType PriceType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.PriceType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.PriceType val) { this.PriceType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.PriceType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.PriceType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.PriceType val) { return IsSetPriceType(); }

        public bool IsSetPriceType() { return IsSetField(Tags.PriceType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.FairValue FairValue
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.FairValue();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.FairValue val) { this.FairValue = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.FairValue Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.FairValue val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.FairValue val) { return IsSetFairValue(); }

        public bool IsSetFairValue() { return IsSetField(Tags.FairValue); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NetGrossInd NetGrossInd
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NetGrossInd();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NetGrossInd val) { this.NetGrossInd = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NetGrossInd Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NetGrossInd val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NetGrossInd val) { return IsSetNetGrossInd(); }

        public bool IsSetNetGrossInd() { return IsSetField(Tags.NetGrossInd); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlmntTyp SettlmntTyp
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlmntTyp();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlmntTyp val) { this.SettlmntTyp = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlmntTyp Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlmntTyp val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlmntTyp val) { return IsSetSettlmntTyp(); }

        public bool IsSetSettlmntTyp() { return IsSetField(Tags.SettlmntTyp); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate FutSettDate
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate val) { this.FutSettDate = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate val) { return IsSetFutSettDate(); }

        public bool IsSetFutSettDate() { return IsSetField(Tags.FutSettDate); }

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
