using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class MassQuote : Message
    {
        public const string MsgType = "i";

        public MassQuote():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public MassQuote(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteID aQuoteID)
               : this()
        {
            this.QuoteID = aQuoteID;
        }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteReqID QuoteReqID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteReqID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteReqID val) { this.QuoteReqID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteReqID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteReqID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteReqID val) { return IsSetQuoteReqID(); }

        public bool IsSetQuoteReqID() { return IsSetField(Tags.QuoteReqID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteID QuoteID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteID val) { this.QuoteID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteID val) { return IsSetQuoteID(); }

        public bool IsSetQuoteID() { return IsSetField(Tags.QuoteID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteResponseLevel QuoteResponseLevel
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteResponseLevel();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteResponseLevel val) { this.QuoteResponseLevel = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteResponseLevel Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteResponseLevel val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteResponseLevel val) { return IsSetQuoteResponseLevel(); }

        public bool IsSetQuoteResponseLevel() { return IsSetField(Tags.QuoteResponseLevel); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.DefBidSize DefBidSize
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.DefBidSize();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.DefBidSize val) { this.DefBidSize = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.DefBidSize Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.DefBidSize val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.DefBidSize val) { return IsSetDefBidSize(); }

        public bool IsSetDefBidSize() { return IsSetField(Tags.DefBidSize); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.DefOfferSize DefOfferSize
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.DefOfferSize();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.DefOfferSize val) { this.DefOfferSize = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.DefOfferSize Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.DefOfferSize val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.DefOfferSize val) { return IsSetDefOfferSize(); }

        public bool IsSetDefOfferSize() { return IsSetField(Tags.DefOfferSize); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoQuoteSets NoQuoteSets
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NoQuoteSets();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoQuoteSets val) { this.NoQuoteSets = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoQuoteSets Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoQuoteSets val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoQuoteSets val) { return IsSetNoQuoteSets(); }

        public bool IsSetNoQuoteSets() { return IsSetField(Tags.NoQuoteSets); }


        public class NoQuoteSetsGroup : Group
        {
            public static int[] fieldOrder = {Tags.QuoteSetID, Tags.UnderlyingSymbol, Tags.UnderlyingSymbolSfx, Tags.UnderlyingSecurityID, Tags.UnderlyingIDSource, Tags.UnderlyingSecurityType, Tags.UnderlyingMaturityMonthYear, Tags.UnderlyingMaturityDay, Tags.UnderlyingPutOrCall, Tags.UnderlyingStrikePrice, Tags.UnderlyingOptAttribute, Tags.UnderlyingContractMultiplier, Tags.UnderlyingCouponRate, Tags.UnderlyingSecurityExchange, Tags.UnderlyingIssuer, Tags.EncodedUnderlyingIssuerLen, Tags.EncodedUnderlyingIssuer, Tags.UnderlyingSecurityDesc, Tags.EncodedUnderlyingSecurityDescLen, Tags.EncodedUnderlyingSecurityDesc, Tags.QuoteSetValidUntilTime, Tags.TotQuoteEntries, Tags.NoQuoteEntries, 0};

            public NoQuoteSetsGroup() : base(Tags.NoQuoteSets, Tags.QuoteSetID, fieldOrder)
            {
            }

            public override Group Clone()
            {
                var clone = new NoQuoteSetsGroup();
                clone.CopyStateFrom(this);
                return clone;
            }
        public LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteSetID QuoteSetID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteSetID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteSetID val) { this.QuoteSetID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteSetID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteSetID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteSetID val) { return IsSetQuoteSetID(); }

        public bool IsSetQuoteSetID() { return IsSetField(Tags.QuoteSetID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSymbol UnderlyingSymbol
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSymbol();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSymbol val) { this.UnderlyingSymbol = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSymbol Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSymbol val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSymbol val) { return IsSetUnderlyingSymbol(); }

        public bool IsSetUnderlyingSymbol() { return IsSetField(Tags.UnderlyingSymbol); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSymbolSfx UnderlyingSymbolSfx
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSymbolSfx();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSymbolSfx val) { this.UnderlyingSymbolSfx = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSymbolSfx Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSymbolSfx val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSymbolSfx val) { return IsSetUnderlyingSymbolSfx(); }

        public bool IsSetUnderlyingSymbolSfx() { return IsSetField(Tags.UnderlyingSymbolSfx); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityID UnderlyingSecurityID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityID val) { this.UnderlyingSecurityID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityID val) { return IsSetUnderlyingSecurityID(); }

        public bool IsSetUnderlyingSecurityID() { return IsSetField(Tags.UnderlyingSecurityID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingIDSource UnderlyingIDSource
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingIDSource();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingIDSource val) { this.UnderlyingIDSource = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingIDSource Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingIDSource val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingIDSource val) { return IsSetUnderlyingIDSource(); }

        public bool IsSetUnderlyingIDSource() { return IsSetField(Tags.UnderlyingIDSource); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityType UnderlyingSecurityType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityType val) { this.UnderlyingSecurityType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityType val) { return IsSetUnderlyingSecurityType(); }

        public bool IsSetUnderlyingSecurityType() { return IsSetField(Tags.UnderlyingSecurityType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingMaturityMonthYear UnderlyingMaturityMonthYear
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingMaturityMonthYear();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingMaturityMonthYear val) { this.UnderlyingMaturityMonthYear = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingMaturityMonthYear Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingMaturityMonthYear val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingMaturityMonthYear val) { return IsSetUnderlyingMaturityMonthYear(); }

        public bool IsSetUnderlyingMaturityMonthYear() { return IsSetField(Tags.UnderlyingMaturityMonthYear); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingMaturityDay UnderlyingMaturityDay
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingMaturityDay();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingMaturityDay val) { this.UnderlyingMaturityDay = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingMaturityDay Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingMaturityDay val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingMaturityDay val) { return IsSetUnderlyingMaturityDay(); }

        public bool IsSetUnderlyingMaturityDay() { return IsSetField(Tags.UnderlyingMaturityDay); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingPutOrCall UnderlyingPutOrCall
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingPutOrCall();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingPutOrCall val) { this.UnderlyingPutOrCall = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingPutOrCall Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingPutOrCall val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingPutOrCall val) { return IsSetUnderlyingPutOrCall(); }

        public bool IsSetUnderlyingPutOrCall() { return IsSetField(Tags.UnderlyingPutOrCall); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingStrikePrice UnderlyingStrikePrice
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingStrikePrice();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingStrikePrice val) { this.UnderlyingStrikePrice = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingStrikePrice Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingStrikePrice val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingStrikePrice val) { return IsSetUnderlyingStrikePrice(); }

        public bool IsSetUnderlyingStrikePrice() { return IsSetField(Tags.UnderlyingStrikePrice); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingOptAttribute UnderlyingOptAttribute
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingOptAttribute();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingOptAttribute val) { this.UnderlyingOptAttribute = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingOptAttribute Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingOptAttribute val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingOptAttribute val) { return IsSetUnderlyingOptAttribute(); }

        public bool IsSetUnderlyingOptAttribute() { return IsSetField(Tags.UnderlyingOptAttribute); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingContractMultiplier UnderlyingContractMultiplier
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingContractMultiplier();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingContractMultiplier val) { this.UnderlyingContractMultiplier = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingContractMultiplier Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingContractMultiplier val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingContractMultiplier val) { return IsSetUnderlyingContractMultiplier(); }

        public bool IsSetUnderlyingContractMultiplier() { return IsSetField(Tags.UnderlyingContractMultiplier); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingCouponRate UnderlyingCouponRate
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingCouponRate();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingCouponRate val) { this.UnderlyingCouponRate = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingCouponRate Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingCouponRate val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingCouponRate val) { return IsSetUnderlyingCouponRate(); }

        public bool IsSetUnderlyingCouponRate() { return IsSetField(Tags.UnderlyingCouponRate); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityExchange UnderlyingSecurityExchange
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityExchange();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityExchange val) { this.UnderlyingSecurityExchange = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityExchange Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityExchange val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityExchange val) { return IsSetUnderlyingSecurityExchange(); }

        public bool IsSetUnderlyingSecurityExchange() { return IsSetField(Tags.UnderlyingSecurityExchange); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingIssuer UnderlyingIssuer
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingIssuer();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingIssuer val) { this.UnderlyingIssuer = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingIssuer Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingIssuer val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingIssuer val) { return IsSetUnderlyingIssuer(); }

        public bool IsSetUnderlyingIssuer() { return IsSetField(Tags.UnderlyingIssuer); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingIssuerLen EncodedUnderlyingIssuerLen
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingIssuerLen();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingIssuerLen val) { this.EncodedUnderlyingIssuerLen = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingIssuerLen Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingIssuerLen val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingIssuerLen val) { return IsSetEncodedUnderlyingIssuerLen(); }

        public bool IsSetEncodedUnderlyingIssuerLen() { return IsSetField(Tags.EncodedUnderlyingIssuerLen); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingIssuer EncodedUnderlyingIssuer
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingIssuer();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingIssuer val) { this.EncodedUnderlyingIssuer = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingIssuer Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingIssuer val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingIssuer val) { return IsSetEncodedUnderlyingIssuer(); }

        public bool IsSetEncodedUnderlyingIssuer() { return IsSetField(Tags.EncodedUnderlyingIssuer); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityDesc UnderlyingSecurityDesc
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityDesc();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityDesc val) { this.UnderlyingSecurityDesc = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityDesc Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityDesc val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.UnderlyingSecurityDesc val) { return IsSetUnderlyingSecurityDesc(); }

        public bool IsSetUnderlyingSecurityDesc() { return IsSetField(Tags.UnderlyingSecurityDesc); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingSecurityDescLen EncodedUnderlyingSecurityDescLen
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingSecurityDescLen();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingSecurityDescLen val) { this.EncodedUnderlyingSecurityDescLen = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingSecurityDescLen Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingSecurityDescLen val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingSecurityDescLen val) { return IsSetEncodedUnderlyingSecurityDescLen(); }

        public bool IsSetEncodedUnderlyingSecurityDescLen() { return IsSetField(Tags.EncodedUnderlyingSecurityDescLen); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingSecurityDesc EncodedUnderlyingSecurityDesc
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingSecurityDesc();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingSecurityDesc val) { this.EncodedUnderlyingSecurityDesc = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingSecurityDesc Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingSecurityDesc val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedUnderlyingSecurityDesc val) { return IsSetEncodedUnderlyingSecurityDesc(); }

        public bool IsSetEncodedUnderlyingSecurityDesc() { return IsSetField(Tags.EncodedUnderlyingSecurityDesc); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteSetValidUntilTime QuoteSetValidUntilTime
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteSetValidUntilTime();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteSetValidUntilTime val) { this.QuoteSetValidUntilTime = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteSetValidUntilTime Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteSetValidUntilTime val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteSetValidUntilTime val) { return IsSetQuoteSetValidUntilTime(); }

        public bool IsSetQuoteSetValidUntilTime() { return IsSetField(Tags.QuoteSetValidUntilTime); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TotQuoteEntries TotQuoteEntries
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TotQuoteEntries();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TotQuoteEntries val) { this.TotQuoteEntries = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TotQuoteEntries Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TotQuoteEntries val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TotQuoteEntries val) { return IsSetTotQuoteEntries(); }

        public bool IsSetTotQuoteEntries() { return IsSetField(Tags.TotQuoteEntries); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoQuoteEntries NoQuoteEntries
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NoQuoteEntries();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoQuoteEntries val) { this.NoQuoteEntries = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoQuoteEntries Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoQuoteEntries val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoQuoteEntries val) { return IsSetNoQuoteEntries(); }

        public bool IsSetNoQuoteEntries() { return IsSetField(Tags.NoQuoteEntries); }


        public class NoQuoteEntriesGroup : Group
        {
            public static int[] fieldOrder = {Tags.QuoteEntryID, Tags.Symbol, Tags.SymbolSfx, Tags.SecurityID, Tags.IDSource, Tags.SecurityType, Tags.MaturityMonthYear, Tags.MaturityDay, Tags.PutOrCall, Tags.StrikePrice, Tags.OptAttribute, Tags.ContractMultiplier, Tags.CouponRate, Tags.SecurityExchange, Tags.Issuer, Tags.EncodedIssuerLen, Tags.EncodedIssuer, Tags.SecurityDesc, Tags.EncodedSecurityDescLen, Tags.EncodedSecurityDesc, Tags.BidPx, Tags.OfferPx, Tags.BidSize, Tags.OfferSize, Tags.ValidUntilTime, Tags.BidSpotRate, Tags.OfferSpotRate, Tags.BidForwardPoints, Tags.OfferForwardPoints, Tags.TransactTime, Tags.TradingSessionID, Tags.FutSettDate, Tags.OrdType, Tags.FutSettDate2, Tags.OrderQty2, Tags.Currency, 0};

            public NoQuoteEntriesGroup() : base(Tags.NoQuoteEntries, Tags.QuoteEntryID, fieldOrder)
            {
            }

            public override Group Clone()
            {
                var clone = new NoQuoteEntriesGroup();
                clone.CopyStateFrom(this);
                return clone;
            }
        public LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteEntryID QuoteEntryID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteEntryID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteEntryID val) { this.QuoteEntryID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteEntryID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteEntryID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteEntryID val) { return IsSetQuoteEntryID(); }

        public bool IsSetQuoteEntryID() { return IsSetField(Tags.QuoteEntryID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Symbol Symbol
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Symbol();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Symbol val) { this.Symbol = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Symbol Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Symbol val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Symbol val) { return IsSetSymbol(); }

        public bool IsSetSymbol() { return IsSetField(Tags.Symbol); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SymbolSfx SymbolSfx
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SymbolSfx();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SymbolSfx val) { this.SymbolSfx = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SymbolSfx Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SymbolSfx val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SymbolSfx val) { return IsSetSymbolSfx(); }

        public bool IsSetSymbolSfx() { return IsSetField(Tags.SymbolSfx); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityID SecurityID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityID val) { this.SecurityID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityID val) { return IsSetSecurityID(); }

        public bool IsSetSecurityID() { return IsSetField(Tags.SecurityID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IDSource IDSource
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.IDSource();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.IDSource val) { this.IDSource = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IDSource Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.IDSource val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.IDSource val) { return IsSetIDSource(); }

        public bool IsSetIDSource() { return IsSetField(Tags.IDSource); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityType SecurityType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityType val) { this.SecurityType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityType val) { return IsSetSecurityType(); }

        public bool IsSetSecurityType() { return IsSetField(Tags.SecurityType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityMonthYear MaturityMonthYear
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityMonthYear();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityMonthYear val) { this.MaturityMonthYear = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityMonthYear Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityMonthYear val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityMonthYear val) { return IsSetMaturityMonthYear(); }

        public bool IsSetMaturityMonthYear() { return IsSetField(Tags.MaturityMonthYear); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityDay MaturityDay
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityDay();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityDay val) { this.MaturityDay = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityDay Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityDay val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MaturityDay val) { return IsSetMaturityDay(); }

        public bool IsSetMaturityDay() { return IsSetField(Tags.MaturityDay); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.PutOrCall PutOrCall
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.PutOrCall();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.PutOrCall val) { this.PutOrCall = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.PutOrCall Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.PutOrCall val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.PutOrCall val) { return IsSetPutOrCall(); }

        public bool IsSetPutOrCall() { return IsSetField(Tags.PutOrCall); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikePrice StrikePrice
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikePrice();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikePrice val) { this.StrikePrice = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikePrice Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikePrice val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.StrikePrice val) { return IsSetStrikePrice(); }

        public bool IsSetStrikePrice() { return IsSetField(Tags.StrikePrice); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OptAttribute OptAttribute
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OptAttribute();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OptAttribute val) { this.OptAttribute = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OptAttribute Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OptAttribute val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OptAttribute val) { return IsSetOptAttribute(); }

        public bool IsSetOptAttribute() { return IsSetField(Tags.OptAttribute); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ContractMultiplier ContractMultiplier
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ContractMultiplier();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContractMultiplier val) { this.ContractMultiplier = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ContractMultiplier Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContractMultiplier val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ContractMultiplier val) { return IsSetContractMultiplier(); }

        public bool IsSetContractMultiplier() { return IsSetField(Tags.ContractMultiplier); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CouponRate CouponRate
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CouponRate();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CouponRate val) { this.CouponRate = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CouponRate Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CouponRate val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CouponRate val) { return IsSetCouponRate(); }

        public bool IsSetCouponRate() { return IsSetField(Tags.CouponRate); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityExchange SecurityExchange
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityExchange();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityExchange val) { this.SecurityExchange = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityExchange Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityExchange val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityExchange val) { return IsSetSecurityExchange(); }

        public bool IsSetSecurityExchange() { return IsSetField(Tags.SecurityExchange); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Issuer Issuer
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Issuer();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Issuer val) { this.Issuer = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Issuer Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Issuer val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Issuer val) { return IsSetIssuer(); }

        public bool IsSetIssuer() { return IsSetField(Tags.Issuer); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuerLen EncodedIssuerLen
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuerLen();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuerLen val) { this.EncodedIssuerLen = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuerLen Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuerLen val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuerLen val) { return IsSetEncodedIssuerLen(); }

        public bool IsSetEncodedIssuerLen() { return IsSetField(Tags.EncodedIssuerLen); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuer EncodedIssuer
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuer();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuer val) { this.EncodedIssuer = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuer Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuer val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedIssuer val) { return IsSetEncodedIssuer(); }

        public bool IsSetEncodedIssuer() { return IsSetField(Tags.EncodedIssuer); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityDesc SecurityDesc
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityDesc();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityDesc val) { this.SecurityDesc = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityDesc Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityDesc val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SecurityDesc val) { return IsSetSecurityDesc(); }

        public bool IsSetSecurityDesc() { return IsSetField(Tags.SecurityDesc); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDescLen EncodedSecurityDescLen
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDescLen();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDescLen val) { this.EncodedSecurityDescLen = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDescLen Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDescLen val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDescLen val) { return IsSetEncodedSecurityDescLen(); }

        public bool IsSetEncodedSecurityDescLen() { return IsSetField(Tags.EncodedSecurityDescLen); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDesc EncodedSecurityDesc
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDesc();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDesc val) { this.EncodedSecurityDesc = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDesc Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDesc val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedSecurityDesc val) { return IsSetEncodedSecurityDesc(); }

        public bool IsSetEncodedSecurityDesc() { return IsSetField(Tags.EncodedSecurityDesc); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidPx BidPx
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.BidPx();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidPx val) { this.BidPx = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidPx Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidPx val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidPx val) { return IsSetBidPx(); }

        public bool IsSetBidPx() { return IsSetField(Tags.BidPx); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferPx OfferPx
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferPx();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferPx val) { this.OfferPx = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferPx Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferPx val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferPx val) { return IsSetOfferPx(); }

        public bool IsSetOfferPx() { return IsSetField(Tags.OfferPx); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidSize BidSize
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.BidSize();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidSize val) { this.BidSize = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidSize Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidSize val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidSize val) { return IsSetBidSize(); }

        public bool IsSetBidSize() { return IsSetField(Tags.BidSize); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferSize OfferSize
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferSize();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferSize val) { this.OfferSize = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferSize Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferSize val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferSize val) { return IsSetOfferSize(); }

        public bool IsSetOfferSize() { return IsSetField(Tags.OfferSize); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ValidUntilTime ValidUntilTime
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ValidUntilTime();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ValidUntilTime val) { this.ValidUntilTime = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ValidUntilTime Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ValidUntilTime val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ValidUntilTime val) { return IsSetValidUntilTime(); }

        public bool IsSetValidUntilTime() { return IsSetField(Tags.ValidUntilTime); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidSpotRate BidSpotRate
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.BidSpotRate();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidSpotRate val) { this.BidSpotRate = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidSpotRate Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidSpotRate val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidSpotRate val) { return IsSetBidSpotRate(); }

        public bool IsSetBidSpotRate() { return IsSetField(Tags.BidSpotRate); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferSpotRate OfferSpotRate
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferSpotRate();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferSpotRate val) { this.OfferSpotRate = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferSpotRate Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferSpotRate val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferSpotRate val) { return IsSetOfferSpotRate(); }

        public bool IsSetOfferSpotRate() { return IsSetField(Tags.OfferSpotRate); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidForwardPoints BidForwardPoints
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.BidForwardPoints();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidForwardPoints val) { this.BidForwardPoints = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BidForwardPoints Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidForwardPoints val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.BidForwardPoints val) { return IsSetBidForwardPoints(); }

        public bool IsSetBidForwardPoints() { return IsSetField(Tags.BidForwardPoints); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferForwardPoints OfferForwardPoints
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferForwardPoints();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferForwardPoints val) { this.OfferForwardPoints = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferForwardPoints Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferForwardPoints val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OfferForwardPoints val) { return IsSetOfferForwardPoints(); }

        public bool IsSetOfferForwardPoints() { return IsSetField(Tags.OfferForwardPoints); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdType OrdType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdType val) { this.OrdType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrdType val) { return IsSetOrdType(); }

        public bool IsSetOrdType() { return IsSetField(Tags.OrdType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate2 FutSettDate2
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate2();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate2 val) { this.FutSettDate2 = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate2 Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate2 val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.FutSettDate2 val) { return IsSetFutSettDate2(); }

        public bool IsSetFutSettDate2() { return IsSetField(Tags.FutSettDate2); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty2 OrderQty2
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty2();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty2 val) { this.OrderQty2 = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty2 Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty2 val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrderQty2 val) { return IsSetOrderQty2(); }

        public bool IsSetOrderQty2() { return IsSetField(Tags.OrderQty2); }

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


        }
        }


    }
}
