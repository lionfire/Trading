using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class News : Message
    {
        public const string MsgType = "B";

        public News():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public News(LionFire.Trading.QuickFix.ConsoleTest.Fields.Headline aHeadline)
               : this()
        {
            this.Headline = aHeadline;
        }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigTime OrigTime
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigTime();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigTime val) { this.OrigTime = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigTime Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigTime val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OrigTime val) { return IsSetOrigTime(); }

        public bool IsSetOrigTime() { return IsSetField(Tags.OrigTime); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Urgency Urgency
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Urgency();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Urgency val) { this.Urgency = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Urgency Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Urgency val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Urgency val) { return IsSetUrgency(); }

        public bool IsSetUrgency() { return IsSetField(Tags.Urgency); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Headline Headline
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Headline();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Headline val) { this.Headline = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Headline Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Headline val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Headline val) { return IsSetHeadline(); }

        public bool IsSetHeadline() { return IsSetField(Tags.Headline); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedHeadlineLen EncodedHeadlineLen
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedHeadlineLen();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedHeadlineLen val) { this.EncodedHeadlineLen = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedHeadlineLen Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedHeadlineLen val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedHeadlineLen val) { return IsSetEncodedHeadlineLen(); }

        public bool IsSetEncodedHeadlineLen() { return IsSetField(Tags.EncodedHeadlineLen); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedHeadline EncodedHeadline
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedHeadline();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedHeadline val) { this.EncodedHeadline = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedHeadline Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedHeadline val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedHeadline val) { return IsSetEncodedHeadline(); }

        public bool IsSetEncodedHeadline() { return IsSetField(Tags.EncodedHeadline); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRoutingIDs NoRoutingIDs
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRoutingIDs();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRoutingIDs val) { this.NoRoutingIDs = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRoutingIDs Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRoutingIDs val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRoutingIDs val) { return IsSetNoRoutingIDs(); }

        public bool IsSetNoRoutingIDs() { return IsSetField(Tags.NoRoutingIDs); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRelatedSym NoRelatedSym
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRelatedSym();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRelatedSym val) { this.NoRelatedSym = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRelatedSym Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRelatedSym val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoRelatedSym val) { return IsSetNoRelatedSym(); }

        public bool IsSetNoRelatedSym() { return IsSetField(Tags.NoRelatedSym); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LinesOfText LinesOfText
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LinesOfText();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LinesOfText val) { this.LinesOfText = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LinesOfText Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LinesOfText val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LinesOfText val) { return IsSetLinesOfText(); }

        public bool IsSetLinesOfText() { return IsSetField(Tags.LinesOfText); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.URLLink URLLink
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.URLLink();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.URLLink val) { this.URLLink = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.URLLink Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.URLLink val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.URLLink val) { return IsSetURLLink(); }

        public bool IsSetURLLink() { return IsSetField(Tags.URLLink); }

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


        public class NoRoutingIDsGroup : Group
        {
            public static int[] fieldOrder = {Tags.RoutingType, Tags.RoutingID, 0};

            public NoRoutingIDsGroup() : base(Tags.NoRoutingIDs, Tags.RoutingType, fieldOrder)
            {
            }

            public override Group Clone()
            {
                var clone = new NoRoutingIDsGroup();
                clone.CopyStateFrom(this);
                return clone;
            }
        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RoutingType RoutingType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.RoutingType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.RoutingType val) { this.RoutingType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RoutingType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.RoutingType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.RoutingType val) { return IsSetRoutingType(); }

        public bool IsSetRoutingType() { return IsSetField(Tags.RoutingType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RoutingID RoutingID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.RoutingID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.RoutingID val) { this.RoutingID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RoutingID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.RoutingID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.RoutingID val) { return IsSetRoutingID(); }

        public bool IsSetRoutingID() { return IsSetField(Tags.RoutingID); }


        }


        public class NoRelatedSymGroup : Group
        {
            public static int[] fieldOrder = {Tags.RelatdSym, Tags.SymbolSfx, Tags.SecurityID, Tags.IDSource, Tags.SecurityType, Tags.MaturityMonthYear, Tags.MaturityDay, Tags.PutOrCall, Tags.StrikePrice, Tags.OptAttribute, Tags.ContractMultiplier, Tags.CouponRate, Tags.SecurityExchange, Tags.Issuer, Tags.EncodedIssuerLen, Tags.EncodedIssuer, Tags.SecurityDesc, Tags.EncodedSecurityDescLen, Tags.EncodedSecurityDesc, 0};

            public NoRelatedSymGroup() : base(Tags.NoRelatedSym, Tags.RelatdSym, fieldOrder)
            {
            }

            public override Group Clone()
            {
                var clone = new NoRelatedSymGroup();
                clone.CopyStateFrom(this);
                return clone;
            }
        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RelatdSym RelatdSym
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.RelatdSym();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.RelatdSym val) { this.RelatdSym = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RelatdSym Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.RelatdSym val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.RelatdSym val) { return IsSetRelatdSym(); }

        public bool IsSetRelatdSym() { return IsSetField(Tags.RelatdSym); }

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


        }


        public class LinesOfTextGroup : Group
        {
            public static int[] fieldOrder = {Tags.Text, Tags.EncodedTextLen, Tags.EncodedText, 0};

            public LinesOfTextGroup() : base(Tags.LinesOfText, Tags.Text, fieldOrder)
            {
            }

            public override Group Clone()
            {
                var clone = new LinesOfTextGroup();
                clone.CopyStateFrom(this);
                return clone;
            }
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
