using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class IndicationofInterest : Message
    {
        public const string MsgType = "6";

        public IndicationofInterest():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public IndicationofInterest(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIid aIOIid,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.IOITransType aIOITransType,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.Symbol aSymbol,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.Side aSide,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIShares aIOIShares)
               : this()
        {
            this.IOIid = aIOIid;
			this.IOITransType = aIOITransType;
			this.Symbol = aSymbol;
			this.Side = aSide;
			this.IOIShares = aIOIShares;
        }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIid IOIid
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIid();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIid val) { this.IOIid = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIid Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIid val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIid val) { return IsSetIOIid(); }

        public bool IsSetIOIid() { return IsSetField(Tags.IOIid); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IOITransType IOITransType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.IOITransType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOITransType val) { this.IOITransType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IOITransType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOITransType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOITransType val) { return IsSetIOITransType(); }

        public bool IsSetIOITransType() { return IsSetField(Tags.IOITransType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIRefID IOIRefID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIRefID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIRefID val) { this.IOIRefID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIRefID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIRefID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIRefID val) { return IsSetIOIRefID(); }

        public bool IsSetIOIRefID() { return IsSetField(Tags.IOIRefID); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIShares IOIShares
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIShares();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIShares val) { this.IOIShares = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIShares Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIShares val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIShares val) { return IsSetIOIShares(); }

        public bool IsSetIOIShares() { return IsSetField(Tags.IOIShares); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIQltyInd IOIQltyInd
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIQltyInd();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIQltyInd val) { this.IOIQltyInd = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIQltyInd Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIQltyInd val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIQltyInd val) { return IsSetIOIQltyInd(); }

        public bool IsSetIOIQltyInd() { return IsSetField(Tags.IOIQltyInd); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IOINaturalFlag IOINaturalFlag
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.IOINaturalFlag();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOINaturalFlag val) { this.IOINaturalFlag = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IOINaturalFlag Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOINaturalFlag val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOINaturalFlag val) { return IsSetIOINaturalFlag(); }

        public bool IsSetIOINaturalFlag() { return IsSetField(Tags.IOINaturalFlag); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoIOIQualifiers NoIOIQualifiers
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NoIOIQualifiers();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoIOIQualifiers val) { this.NoIOIQualifiers = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoIOIQualifiers Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoIOIQualifiers val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoIOIQualifiers val) { return IsSetNoIOIQualifiers(); }

        public bool IsSetNoIOIQualifiers() { return IsSetField(Tags.NoIOIQualifiers); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SpreadToBenchmark SpreadToBenchmark
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SpreadToBenchmark();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SpreadToBenchmark val) { this.SpreadToBenchmark = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SpreadToBenchmark Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SpreadToBenchmark val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SpreadToBenchmark val) { return IsSetSpreadToBenchmark(); }

        public bool IsSetSpreadToBenchmark() { return IsSetField(Tags.SpreadToBenchmark); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Benchmark Benchmark
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Benchmark();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Benchmark val) { this.Benchmark = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Benchmark Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Benchmark val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Benchmark val) { return IsSetBenchmark(); }

        public bool IsSetBenchmark() { return IsSetField(Tags.Benchmark); }


        public class NoIOIQualifiersGroup : Group
        {
            public static int[] fieldOrder = {Tags.IOIQualifier, 0};

            public NoIOIQualifiersGroup() : base(Tags.NoIOIQualifiers, Tags.IOIQualifier, fieldOrder)
            {
            }

            public override Group Clone()
            {
                var clone = new NoIOIQualifiersGroup();
                clone.CopyStateFrom(this);
                return clone;
            }
        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIQualifier IOIQualifier
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIQualifier();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIQualifier val) { this.IOIQualifier = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIQualifier Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIQualifier val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.IOIQualifier val) { return IsSetIOIQualifier(); }

        public bool IsSetIOIQualifier() { return IsSetField(Tags.IOIQualifier); }


        }


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


    }
}
