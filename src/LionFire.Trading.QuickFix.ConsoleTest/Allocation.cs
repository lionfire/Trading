using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class Allocation : Message
    {
        public const string MsgType = "J";

        public Allocation():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public Allocation(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocID aAllocID,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocTransType aAllocTransType,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.Side aSide,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.Symbol aSymbol,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.Shares aShares,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPx aAvgPx,
				LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeDate aTradeDate)
               : this()
        {
            this.AllocID = aAllocID;
			this.AllocTransType = aAllocTransType;
			this.Side = aSide;
			this.Symbol = aSymbol;
			this.Shares = aShares;
			this.AvgPx = aAvgPx;
			this.TradeDate = aTradeDate;
        }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocID AllocID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocID val) { this.AllocID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocID val) { return IsSetAllocID(); }

        public bool IsSetAllocID() { return IsSetField(Tags.AllocID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocTransType AllocTransType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocTransType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocTransType val) { this.AllocTransType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocTransType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocTransType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocTransType val) { return IsSetAllocTransType(); }

        public bool IsSetAllocTransType() { return IsSetField(Tags.AllocTransType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RefAllocID RefAllocID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.RefAllocID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.RefAllocID val) { this.RefAllocID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.RefAllocID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.RefAllocID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.RefAllocID val) { return IsSetRefAllocID(); }

        public bool IsSetRefAllocID() { return IsSetField(Tags.RefAllocID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocLinkID AllocLinkID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocLinkID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocLinkID val) { this.AllocLinkID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocLinkID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocLinkID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocLinkID val) { return IsSetAllocLinkID(); }

        public bool IsSetAllocLinkID() { return IsSetField(Tags.AllocLinkID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocLinkType AllocLinkType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocLinkType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocLinkType val) { this.AllocLinkType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocLinkType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocLinkType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocLinkType val) { return IsSetAllocLinkType(); }

        public bool IsSetAllocLinkType() { return IsSetField(Tags.AllocLinkType); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoExecs NoExecs
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NoExecs();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoExecs val) { this.NoExecs = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoExecs Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoExecs val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoExecs val) { return IsSetNoExecs(); }

        public bool IsSetNoExecs() { return IsSetField(Tags.NoExecs); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Shares Shares
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.Shares();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.Shares val) { this.Shares = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.Shares Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.Shares val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.Shares val) { return IsSetShares(); }

        public bool IsSetShares() { return IsSetField(Tags.Shares); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastMkt LastMkt
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LastMkt();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastMkt val) { this.LastMkt = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastMkt Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastMkt val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastMkt val) { return IsSetLastMkt(); }

        public bool IsSetLastMkt() { return IsSetField(Tags.LastMkt); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPrxPrecision AvgPrxPrecision
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPrxPrecision();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPrxPrecision val) { this.AvgPrxPrecision = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPrxPrecision Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPrxPrecision val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AvgPrxPrecision val) { return IsSetAvgPrxPrecision(); }

        public bool IsSetAvgPrxPrecision() { return IsSetField(Tags.AvgPrxPrecision); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.GrossTradeAmt GrossTradeAmt
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.GrossTradeAmt();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.GrossTradeAmt val) { this.GrossTradeAmt = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.GrossTradeAmt Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.GrossTradeAmt val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.GrossTradeAmt val) { return IsSetGrossTradeAmt(); }

        public bool IsSetGrossTradeAmt() { return IsSetField(Tags.GrossTradeAmt); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NetMoney NetMoney
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NetMoney();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NetMoney val) { this.NetMoney = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NetMoney Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NetMoney val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NetMoney val) { return IsSetNetMoney(); }

        public bool IsSetNetMoney() { return IsSetField(Tags.NetMoney); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenClose OpenClose
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenClose();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenClose val) { this.OpenClose = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenClose Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenClose val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenClose val) { return IsSetOpenClose(); }

        public bool IsSetOpenClose() { return IsSetField(Tags.OpenClose); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NumDaysInterest NumDaysInterest
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NumDaysInterest();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NumDaysInterest val) { this.NumDaysInterest = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NumDaysInterest Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NumDaysInterest val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NumDaysInterest val) { return IsSetNumDaysInterest(); }

        public bool IsSetNumDaysInterest() { return IsSetField(Tags.NumDaysInterest); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AccruedInterestRate AccruedInterestRate
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AccruedInterestRate();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AccruedInterestRate val) { this.AccruedInterestRate = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AccruedInterestRate Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AccruedInterestRate val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AccruedInterestRate val) { return IsSetAccruedInterestRate(); }

        public bool IsSetAccruedInterestRate() { return IsSetField(Tags.AccruedInterestRate); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoAllocs NoAllocs
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NoAllocs();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoAllocs val) { this.NoAllocs = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoAllocs Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoAllocs val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoAllocs val) { return IsSetNoAllocs(); }

        public bool IsSetNoAllocs() { return IsSetField(Tags.NoAllocs); }


        public class NoOrdersGroup : Group
        {
            public static int[] fieldOrder = {Tags.ClOrdID, Tags.OrderID, Tags.SecondaryOrderID, Tags.ListID, Tags.WaveNo, 0};

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.WaveNo WaveNo
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.WaveNo();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.WaveNo val) { this.WaveNo = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.WaveNo Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.WaveNo val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.WaveNo val) { return IsSetWaveNo(); }

        public bool IsSetWaveNo() { return IsSetField(Tags.WaveNo); }


        }


        public class NoExecsGroup : Group
        {
            public static int[] fieldOrder = {Tags.LastShares, Tags.ExecID, Tags.LastPx, Tags.LastCapacity, 0};

            public NoExecsGroup() : base(Tags.NoExecs, Tags.LastShares, fieldOrder)
            {
            }

            public override Group Clone()
            {
                var clone = new NoExecsGroup();
                clone.CopyStateFrom(this);
                return clone;
            }
        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastShares LastShares
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LastShares();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastShares val) { this.LastShares = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastShares Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastShares val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastShares val) { return IsSetLastShares(); }

        public bool IsSetLastShares() { return IsSetField(Tags.LastShares); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecID ExecID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecID val) { this.ExecID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecID val) { return IsSetExecID(); }

        public bool IsSetExecID() { return IsSetField(Tags.ExecID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastPx LastPx
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LastPx();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastPx val) { this.LastPx = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastPx Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastPx val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastPx val) { return IsSetLastPx(); }

        public bool IsSetLastPx() { return IsSetField(Tags.LastPx); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastCapacity LastCapacity
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LastCapacity();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastCapacity val) { this.LastCapacity = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LastCapacity Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastCapacity val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LastCapacity val) { return IsSetLastCapacity(); }

        public bool IsSetLastCapacity() { return IsSetField(Tags.LastCapacity); }


        }


        public class NoAllocsGroup : Group
        {
            public static int[] fieldOrder = {Tags.AllocAccount, Tags.AllocPrice, Tags.AllocShares, Tags.ProcessCode, Tags.BrokerOfCredit, Tags.NotifyBrokerOfCredit, Tags.AllocHandlInst, Tags.AllocText, Tags.EncodedAllocTextLen, Tags.EncodedAllocText, Tags.ExecBroker, Tags.ClientID, Tags.Commission, Tags.CommType, Tags.AllocAvgPx, Tags.AllocNetMoney, Tags.SettlCurrAmt, Tags.SettlCurrency, Tags.SettlCurrFxRate, Tags.SettlCurrFxRateCalc, Tags.AccruedInterestAmt, Tags.SettlInstMode, Tags.NoMiscFees, 0};

            public NoAllocsGroup() : base(Tags.NoAllocs, Tags.AllocAccount, fieldOrder)
            {
            }

            public override Group Clone()
            {
                var clone = new NoAllocsGroup();
                clone.CopyStateFrom(this);
                return clone;
            }
        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAccount AllocAccount
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAccount();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAccount val) { this.AllocAccount = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAccount Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAccount val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAccount val) { return IsSetAllocAccount(); }

        public bool IsSetAllocAccount() { return IsSetField(Tags.AllocAccount); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocPrice AllocPrice
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocPrice();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocPrice val) { this.AllocPrice = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocPrice Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocPrice val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocPrice val) { return IsSetAllocPrice(); }

        public bool IsSetAllocPrice() { return IsSetField(Tags.AllocPrice); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocShares AllocShares
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocShares();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocShares val) { this.AllocShares = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocShares Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocShares val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocShares val) { return IsSetAllocShares(); }

        public bool IsSetAllocShares() { return IsSetField(Tags.AllocShares); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ProcessCode ProcessCode
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ProcessCode();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ProcessCode val) { this.ProcessCode = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ProcessCode Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ProcessCode val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ProcessCode val) { return IsSetProcessCode(); }

        public bool IsSetProcessCode() { return IsSetField(Tags.ProcessCode); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BrokerOfCredit BrokerOfCredit
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.BrokerOfCredit();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.BrokerOfCredit val) { this.BrokerOfCredit = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.BrokerOfCredit Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.BrokerOfCredit val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.BrokerOfCredit val) { return IsSetBrokerOfCredit(); }

        public bool IsSetBrokerOfCredit() { return IsSetField(Tags.BrokerOfCredit); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NotifyBrokerOfCredit NotifyBrokerOfCredit
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NotifyBrokerOfCredit();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NotifyBrokerOfCredit val) { this.NotifyBrokerOfCredit = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NotifyBrokerOfCredit Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NotifyBrokerOfCredit val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NotifyBrokerOfCredit val) { return IsSetNotifyBrokerOfCredit(); }

        public bool IsSetNotifyBrokerOfCredit() { return IsSetField(Tags.NotifyBrokerOfCredit); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocHandlInst AllocHandlInst
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocHandlInst();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocHandlInst val) { this.AllocHandlInst = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocHandlInst Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocHandlInst val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocHandlInst val) { return IsSetAllocHandlInst(); }

        public bool IsSetAllocHandlInst() { return IsSetField(Tags.AllocHandlInst); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocText AllocText
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocText();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocText val) { this.AllocText = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocText Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocText val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocText val) { return IsSetAllocText(); }

        public bool IsSetAllocText() { return IsSetField(Tags.AllocText); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedAllocTextLen EncodedAllocTextLen
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedAllocTextLen();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedAllocTextLen val) { this.EncodedAllocTextLen = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedAllocTextLen Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedAllocTextLen val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedAllocTextLen val) { return IsSetEncodedAllocTextLen(); }

        public bool IsSetEncodedAllocTextLen() { return IsSetField(Tags.EncodedAllocTextLen); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedAllocText EncodedAllocText
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedAllocText();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedAllocText val) { this.EncodedAllocText = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedAllocText Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedAllocText val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.EncodedAllocText val) { return IsSetEncodedAllocText(); }

        public bool IsSetEncodedAllocText() { return IsSetField(Tags.EncodedAllocText); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAvgPx AllocAvgPx
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAvgPx();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAvgPx val) { this.AllocAvgPx = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAvgPx Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAvgPx val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocAvgPx val) { return IsSetAllocAvgPx(); }

        public bool IsSetAllocAvgPx() { return IsSetField(Tags.AllocAvgPx); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocNetMoney AllocNetMoney
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocNetMoney();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocNetMoney val) { this.AllocNetMoney = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocNetMoney Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocNetMoney val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AllocNetMoney val) { return IsSetAllocNetMoney(); }

        public bool IsSetAllocNetMoney() { return IsSetField(Tags.AllocNetMoney); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrAmt SettlCurrAmt
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrAmt();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrAmt val) { this.SettlCurrAmt = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrAmt Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrAmt val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrAmt val) { return IsSetSettlCurrAmt(); }

        public bool IsSetSettlCurrAmt() { return IsSetField(Tags.SettlCurrAmt); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrency SettlCurrency
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrency();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrency val) { this.SettlCurrency = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrency Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrency val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrency val) { return IsSetSettlCurrency(); }

        public bool IsSetSettlCurrency() { return IsSetField(Tags.SettlCurrency); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRate SettlCurrFxRate
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRate();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRate val) { this.SettlCurrFxRate = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRate Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRate val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRate val) { return IsSetSettlCurrFxRate(); }

        public bool IsSetSettlCurrFxRate() { return IsSetField(Tags.SettlCurrFxRate); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRateCalc SettlCurrFxRateCalc
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRateCalc();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRateCalc val) { this.SettlCurrFxRateCalc = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRateCalc Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRateCalc val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlCurrFxRateCalc val) { return IsSetSettlCurrFxRateCalc(); }

        public bool IsSetSettlCurrFxRateCalc() { return IsSetField(Tags.SettlCurrFxRateCalc); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AccruedInterestAmt AccruedInterestAmt
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.AccruedInterestAmt();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.AccruedInterestAmt val) { this.AccruedInterestAmt = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.AccruedInterestAmt Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.AccruedInterestAmt val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.AccruedInterestAmt val) { return IsSetAccruedInterestAmt(); }

        public bool IsSetAccruedInterestAmt() { return IsSetField(Tags.AccruedInterestAmt); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstMode SettlInstMode
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstMode();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstMode val) { this.SettlInstMode = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstMode Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstMode val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SettlInstMode val) { return IsSetSettlInstMode(); }

        public bool IsSetSettlInstMode() { return IsSetField(Tags.SettlInstMode); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMiscFees NoMiscFees
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMiscFees();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMiscFees val) { this.NoMiscFees = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMiscFees Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMiscFees val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMiscFees val) { return IsSetNoMiscFees(); }

        public bool IsSetNoMiscFees() { return IsSetField(Tags.NoMiscFees); }


        public class NoMiscFeesGroup : Group
        {
            public static int[] fieldOrder = {Tags.MiscFeeAmt, Tags.MiscFeeCurr, Tags.MiscFeeType, 0};

            public NoMiscFeesGroup() : base(Tags.NoMiscFees, Tags.MiscFeeAmt, fieldOrder)
            {
            }

            public override Group Clone()
            {
                var clone = new NoMiscFeesGroup();
                clone.CopyStateFrom(this);
                return clone;
            }
        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeAmt MiscFeeAmt
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeAmt();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeAmt val) { this.MiscFeeAmt = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeAmt Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeAmt val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeAmt val) { return IsSetMiscFeeAmt(); }

        public bool IsSetMiscFeeAmt() { return IsSetField(Tags.MiscFeeAmt); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeCurr MiscFeeCurr
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeCurr();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeCurr val) { this.MiscFeeCurr = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeCurr Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeCurr val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeCurr val) { return IsSetMiscFeeCurr(); }

        public bool IsSetMiscFeeCurr() { return IsSetField(Tags.MiscFeeCurr); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeType MiscFeeType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeType val) { this.MiscFeeType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MiscFeeType val) { return IsSetMiscFeeType(); }

        public bool IsSetMiscFeeType() { return IsSetField(Tags.MiscFeeType); }


        }
        }


    }
}
