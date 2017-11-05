using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class BidRequest : Message
    {
        public const string MsgType = "k";

        public BidRequest():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public BidRequest(LionFire.Trading.QuickFix.ConsoleTest.Fields.ClientBidID aClientBidID,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.BidRequestTransType aBidRequestTransType,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalNumSecurities aTotalNumSecurities,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.BidType aBidType,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeType aTradeType,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.BasisPxType aBasisPxType)
               : this()
        {
            this.ClientBidID = aClientBidID;
			this.BidRequestTransType = aBidRequestTransType;
			this.TotalNumSecurities = aTotalNumSecurities;
			this.BidType = aBidType;
			this.TradeType = aTradeType;
			this.BasisPxType = aBasisPxType;
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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidRequestTransType BidRequestTransType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.BidRequestTransType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidRequestTransType val) { this.BidRequestTransType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidRequestTransType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidRequestTransType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidRequestTransType val) { return IsSetBidRequestTransType(); }

        public bool IsSetBidRequestTransType() { return IsSetField(Tags.BidRequestTransType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ListName ListName
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ListName();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListName val) { this.ListName = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ListName Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListName val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ListName val) { return IsSetListName(); }

        public bool IsSetListName() { return IsSetField(Tags.ListName); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalNumSecurities TotalNumSecurities
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalNumSecurities();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalNumSecurities val) { this.TotalNumSecurities = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalNumSecurities Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalNumSecurities val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalNumSecurities val) { return IsSetTotalNumSecurities(); }

        public bool IsSetTotalNumSecurities() { return IsSetField(Tags.TotalNumSecurities); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidType BidType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.BidType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidType val) { this.BidType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidType val) { return IsSetBidType(); }

        public bool IsSetBidType() { return IsSetField(Tags.BidType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NumTickets NumTickets
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NumTickets();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NumTickets val) { this.NumTickets = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NumTickets Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NumTickets val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NumTickets val) { return IsSetNumTickets(); }

        public bool IsSetNumTickets() { return IsSetField(Tags.NumTickets); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Currency Currency
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Currency();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Currency val) { this.Currency = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Currency Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Currency val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Currency val) { return IsSetCurrency(); }

        public bool IsSetCurrency() { return IsSetField(Tags.Currency); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValue1 SideValue1
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValue1();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValue1 val) { this.SideValue1 = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValue1 Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValue1 val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValue1 val) { return IsSetSideValue1(); }

        public bool IsSetSideValue1() { return IsSetField(Tags.SideValue1); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValue2 SideValue2
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValue2();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValue2 val) { this.SideValue2 = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValue2 Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValue2 val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValue2 val) { return IsSetSideValue2(); }

        public bool IsSetSideValue2() { return IsSetField(Tags.SideValue2); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoBidDescriptors NoBidDescriptors
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NoBidDescriptors();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoBidDescriptors val) { this.NoBidDescriptors = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoBidDescriptors Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoBidDescriptors val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoBidDescriptors val) { return IsSetNoBidDescriptors(); }

        public bool IsSetNoBidDescriptors() { return IsSetField(Tags.NoBidDescriptors); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityIndType LiquidityIndType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityIndType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityIndType val) { this.LiquidityIndType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityIndType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityIndType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityIndType val) { return IsSetLiquidityIndType(); }

        public bool IsSetLiquidityIndType() { return IsSetField(Tags.LiquidityIndType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.WtAverageLiquidity WtAverageLiquidity
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.WtAverageLiquidity();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.WtAverageLiquidity val) { this.WtAverageLiquidity = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.WtAverageLiquidity Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.WtAverageLiquidity val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.WtAverageLiquidity val) { return IsSetWtAverageLiquidity(); }

        public bool IsSetWtAverageLiquidity() { return IsSetField(Tags.WtAverageLiquidity); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExchangeForPhysical ExchangeForPhysical
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ExchangeForPhysical();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExchangeForPhysical val) { this.ExchangeForPhysical = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExchangeForPhysical Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExchangeForPhysical val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExchangeForPhysical val) { return IsSetExchangeForPhysical(); }

        public bool IsSetExchangeForPhysical() { return IsSetField(Tags.ExchangeForPhysical); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OutMainCntryUIndex OutMainCntryUIndex
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OutMainCntryUIndex();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OutMainCntryUIndex val) { this.OutMainCntryUIndex = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OutMainCntryUIndex Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OutMainCntryUIndex val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OutMainCntryUIndex val) { return IsSetOutMainCntryUIndex(); }

        public bool IsSetOutMainCntryUIndex() { return IsSetField(Tags.OutMainCntryUIndex); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CrossPercent CrossPercent
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CrossPercent();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CrossPercent val) { this.CrossPercent = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CrossPercent Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CrossPercent val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CrossPercent val) { return IsSetCrossPercent(); }

        public bool IsSetCrossPercent() { return IsSetField(Tags.CrossPercent); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ProgRptReqs ProgRptReqs
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ProgRptReqs();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ProgRptReqs val) { this.ProgRptReqs = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ProgRptReqs Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ProgRptReqs val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ProgRptReqs val) { return IsSetProgRptReqs(); }

        public bool IsSetProgRptReqs() { return IsSetField(Tags.ProgRptReqs); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ProgPeriodInterval ProgPeriodInterval
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ProgPeriodInterval();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ProgPeriodInterval val) { this.ProgPeriodInterval = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ProgPeriodInterval Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ProgPeriodInterval val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ProgPeriodInterval val) { return IsSetProgPeriodInterval(); }

        public bool IsSetProgPeriodInterval() { return IsSetField(Tags.ProgPeriodInterval); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IncTaxInd IncTaxInd
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.IncTaxInd();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.IncTaxInd val) { this.IncTaxInd = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IncTaxInd Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.IncTaxInd val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.IncTaxInd val) { return IsSetIncTaxInd(); }

        public bool IsSetIncTaxInd() { return IsSetField(Tags.IncTaxInd); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ForexReq ForexReq
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ForexReq();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ForexReq val) { this.ForexReq = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ForexReq Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ForexReq val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ForexReq val) { return IsSetForexReq(); }

        public bool IsSetForexReq() { return IsSetField(Tags.ForexReq); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NumBidders NumBidders
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NumBidders();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NumBidders val) { this.NumBidders = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NumBidders Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NumBidders val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NumBidders val) { return IsSetNumBidders(); }

        public bool IsSetNumBidders() { return IsSetField(Tags.NumBidders); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeDate TradeDate
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeDate();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeDate val) { this.TradeDate = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeDate Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeDate val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeDate val) { return IsSetTradeDate(); }

        public bool IsSetTradeDate() { return IsSetField(Tags.TradeDate); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeType TradeType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeType val) { this.TradeType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeType val) { return IsSetTradeType(); }

        public bool IsSetTradeType() { return IsSetField(Tags.TradeType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BasisPxType BasisPxType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.BasisPxType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.BasisPxType val) { this.BasisPxType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BasisPxType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.BasisPxType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.BasisPxType val) { return IsSetBasisPxType(); }

        public bool IsSetBasisPxType() { return IsSetField(Tags.BasisPxType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikeTime StrikeTime
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikeTime();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikeTime val) { this.StrikeTime = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikeTime Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikeTime val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikeTime val) { return IsSetStrikeTime(); }

        public bool IsSetStrikeTime() { return IsSetField(Tags.StrikeTime); }

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


        public class NoBidDescriptorsGroup : Group
        {
            public static int[] fieldOrder = {Tags.BidDescriptorType, Tags.BidDescriptor, Tags.SideValueInd, Tags.LiquidityValue, Tags.LiquidityNumSecurities, Tags.LiquidityPctLow, Tags.LiquidityPctHigh, Tags.EFPTrackingError, Tags.FairValue, Tags.OutsideIndexPct, Tags.ValueOfFutures, 0};

            public NoBidDescriptorsGroup() : base(Tags.NoBidDescriptors, Tags.BidDescriptorType, fieldOrder)
            {
            }

            public override Group Clone()
            {
                var clone = new NoBidDescriptorsGroup();
                clone.CopyStateFrom(this);
                return clone;
            }
        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidDescriptorType BidDescriptorType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.BidDescriptorType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidDescriptorType val) { this.BidDescriptorType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidDescriptorType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidDescriptorType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidDescriptorType val) { return IsSetBidDescriptorType(); }

        public bool IsSetBidDescriptorType() { return IsSetField(Tags.BidDescriptorType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidDescriptor BidDescriptor
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.BidDescriptor();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidDescriptor val) { this.BidDescriptor = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidDescriptor Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidDescriptor val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidDescriptor val) { return IsSetBidDescriptor(); }

        public bool IsSetBidDescriptor() { return IsSetField(Tags.BidDescriptor); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValueInd SideValueInd
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValueInd();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValueInd val) { this.SideValueInd = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValueInd Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValueInd val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SideValueInd val) { return IsSetSideValueInd(); }

        public bool IsSetSideValueInd() { return IsSetField(Tags.SideValueInd); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityValue LiquidityValue
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityValue();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityValue val) { this.LiquidityValue = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityValue Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityValue val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityValue val) { return IsSetLiquidityValue(); }

        public bool IsSetLiquidityValue() { return IsSetField(Tags.LiquidityValue); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityNumSecurities LiquidityNumSecurities
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityNumSecurities();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityNumSecurities val) { this.LiquidityNumSecurities = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityNumSecurities Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityNumSecurities val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityNumSecurities val) { return IsSetLiquidityNumSecurities(); }

        public bool IsSetLiquidityNumSecurities() { return IsSetField(Tags.LiquidityNumSecurities); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityPctLow LiquidityPctLow
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityPctLow();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityPctLow val) { this.LiquidityPctLow = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityPctLow Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityPctLow val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityPctLow val) { return IsSetLiquidityPctLow(); }

        public bool IsSetLiquidityPctLow() { return IsSetField(Tags.LiquidityPctLow); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityPctHigh LiquidityPctHigh
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityPctHigh();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityPctHigh val) { this.LiquidityPctHigh = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityPctHigh Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityPctHigh val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LiquidityPctHigh val) { return IsSetLiquidityPctHigh(); }

        public bool IsSetLiquidityPctHigh() { return IsSetField(Tags.LiquidityPctHigh); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EFPTrackingError EFPTrackingError
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EFPTrackingError();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EFPTrackingError val) { this.EFPTrackingError = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EFPTrackingError Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EFPTrackingError val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EFPTrackingError val) { return IsSetEFPTrackingError(); }

        public bool IsSetEFPTrackingError() { return IsSetField(Tags.EFPTrackingError); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OutsideIndexPct OutsideIndexPct
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OutsideIndexPct();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OutsideIndexPct val) { this.OutsideIndexPct = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OutsideIndexPct Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OutsideIndexPct val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OutsideIndexPct val) { return IsSetOutsideIndexPct(); }

        public bool IsSetOutsideIndexPct() { return IsSetField(Tags.OutsideIndexPct); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ValueOfFutures ValueOfFutures
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ValueOfFutures();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ValueOfFutures val) { this.ValueOfFutures = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ValueOfFutures Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ValueOfFutures val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ValueOfFutures val) { return IsSetValueOfFutures(); }

        public bool IsSetValueOfFutures() { return IsSetField(Tags.ValueOfFutures); }


        }


        public class NoBidComponentsGroup : Group
        {
            public static int[] fieldOrder = {Tags.ListID, Tags.Side, Tags.TradingSessionID, Tags.NetGrossInd, Tags.SettlmntTyp, Tags.FutSettDate, Tags.Account, 0};

            public NoBidComponentsGroup() : base(Tags.NoBidComponents, Tags.ListID, fieldOrder)
            {
            }

            public override Group Clone()
            {
                var clone = new NoBidComponentsGroup();
                clone.CopyStateFrom(this);
                return clone;
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


        }


    }
}
