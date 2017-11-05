using QuickFix;
using LionFire.Trading.QuickFix.ConsoleTest.Fields;
namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class MarketDataSnapshotFullRefresh : Message
    {
        public const string MsgType = "W";

        public MarketDataSnapshotFullRefresh():base()
        {
            this.Header.SetField(new QuickFix.Fields.MsgType(MsgType));
        }
        public MarketDataSnapshotFullRefresh(LionFire.Trading.QuickFix.ConsoleTest.Fields.Symbol aSymbol)
               : this()
        {
            this.Symbol = aSymbol;
        }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqID MDReqID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqID val) { this.MDReqID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDReqID val) { return IsSetMDReqID(); }

        public bool IsSetMDReqID() { return IsSetField(Tags.MDReqID); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.FinancialStatus FinancialStatus
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.FinancialStatus();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.FinancialStatus val) { this.FinancialStatus = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.FinancialStatus Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.FinancialStatus val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.FinancialStatus val) { return IsSetFinancialStatus(); }

        public bool IsSetFinancialStatus() { return IsSetField(Tags.FinancialStatus); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CorporateAction CorporateAction
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.CorporateAction();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.CorporateAction val) { this.CorporateAction = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.CorporateAction Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.CorporateAction val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.CorporateAction val) { return IsSetCorporateAction(); }

        public bool IsSetCorporateAction() { return IsSetField(Tags.CorporateAction); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalVolumeTraded TotalVolumeTraded
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalVolumeTraded();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalVolumeTraded val) { this.TotalVolumeTraded = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalVolumeTraded Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalVolumeTraded val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TotalVolumeTraded val) { return IsSetTotalVolumeTraded(); }

        public bool IsSetTotalVolumeTraded() { return IsSetField(Tags.TotalVolumeTraded); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMDEntries NoMDEntries
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMDEntries();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMDEntries val) { this.NoMDEntries = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMDEntries Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMDEntries val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NoMDEntries val) { return IsSetNoMDEntries(); }

        public bool IsSetNoMDEntries() { return IsSetField(Tags.NoMDEntries); }


        public class NoMDEntriesGroup : Group
        {
            public static int[] fieldOrder = {Tags.MDEntryType, Tags.MDEntryPx, Tags.Currency, Tags.MDEntrySize, Tags.MDEntryDate, Tags.MDEntryTime, Tags.TickDirection, Tags.MDMkt, Tags.TradingSessionID, Tags.QuoteCondition, Tags.TradeCondition, Tags.MDEntryOriginator, Tags.LocationID, Tags.DeskID, Tags.OpenCloseSettleFlag, Tags.TimeInForce, Tags.ExpireDate, Tags.ExpireTime, Tags.MinQty, Tags.ExecInst, Tags.SellerDays, Tags.OrderID, Tags.QuoteEntryID, Tags.MDEntryBuyer, Tags.MDEntrySeller, Tags.NumberOfOrders, Tags.MDEntryPositionNo, Tags.Text, Tags.EncodedTextLen, Tags.EncodedText, 0};

            public NoMDEntriesGroup() : base(Tags.NoMDEntries, Tags.MDEntryType, fieldOrder)
            {
            }

            public override Group Clone()
            {
                var clone = new NoMDEntriesGroup();
                clone.CopyStateFrom(this);
                return clone;
            }
        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryType MDEntryType
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryType();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryType val) { this.MDEntryType = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryType Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryType val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryType val) { return IsSetMDEntryType(); }

        public bool IsSetMDEntryType() { return IsSetField(Tags.MDEntryType); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryPx MDEntryPx
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryPx();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryPx val) { this.MDEntryPx = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryPx Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryPx val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryPx val) { return IsSetMDEntryPx(); }

        public bool IsSetMDEntryPx() { return IsSetField(Tags.MDEntryPx); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntrySize MDEntrySize
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntrySize();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntrySize val) { this.MDEntrySize = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntrySize Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntrySize val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntrySize val) { return IsSetMDEntrySize(); }

        public bool IsSetMDEntrySize() { return IsSetField(Tags.MDEntrySize); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryDate MDEntryDate
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryDate();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryDate val) { this.MDEntryDate = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryDate Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryDate val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryDate val) { return IsSetMDEntryDate(); }

        public bool IsSetMDEntryDate() { return IsSetField(Tags.MDEntryDate); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryTime MDEntryTime
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryTime();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryTime val) { this.MDEntryTime = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryTime Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryTime val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryTime val) { return IsSetMDEntryTime(); }

        public bool IsSetMDEntryTime() { return IsSetField(Tags.MDEntryTime); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TickDirection TickDirection
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TickDirection();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TickDirection val) { this.TickDirection = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TickDirection Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TickDirection val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TickDirection val) { return IsSetTickDirection(); }

        public bool IsSetTickDirection() { return IsSetField(Tags.TickDirection); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDMkt MDMkt
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MDMkt();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDMkt val) { this.MDMkt = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDMkt Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDMkt val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDMkt val) { return IsSetMDMkt(); }

        public bool IsSetMDMkt() { return IsSetField(Tags.MDMkt); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteCondition QuoteCondition
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteCondition();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteCondition val) { this.QuoteCondition = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteCondition Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteCondition val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.QuoteCondition val) { return IsSetQuoteCondition(); }

        public bool IsSetQuoteCondition() { return IsSetField(Tags.QuoteCondition); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeCondition TradeCondition
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeCondition();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeCondition val) { this.TradeCondition = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeCondition Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeCondition val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TradeCondition val) { return IsSetTradeCondition(); }

        public bool IsSetTradeCondition() { return IsSetField(Tags.TradeCondition); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryOriginator MDEntryOriginator
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryOriginator();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryOriginator val) { this.MDEntryOriginator = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryOriginator Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryOriginator val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryOriginator val) { return IsSetMDEntryOriginator(); }

        public bool IsSetMDEntryOriginator() { return IsSetField(Tags.MDEntryOriginator); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LocationID LocationID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.LocationID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.LocationID val) { this.LocationID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.LocationID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.LocationID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.LocationID val) { return IsSetLocationID(); }

        public bool IsSetLocationID() { return IsSetField(Tags.LocationID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.DeskID DeskID
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.DeskID();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.DeskID val) { this.DeskID = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.DeskID Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.DeskID val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.DeskID val) { return IsSetDeskID(); }

        public bool IsSetDeskID() { return IsSetField(Tags.DeskID); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenCloseSettleFlag OpenCloseSettleFlag
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenCloseSettleFlag();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenCloseSettleFlag val) { this.OpenCloseSettleFlag = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenCloseSettleFlag Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenCloseSettleFlag val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.OpenCloseSettleFlag val) { return IsSetOpenCloseSettleFlag(); }

        public bool IsSetOpenCloseSettleFlag() { return IsSetField(Tags.OpenCloseSettleFlag); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TimeInForce TimeInForce
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.TimeInForce();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.TimeInForce val) { this.TimeInForce = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.TimeInForce Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.TimeInForce val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.TimeInForce val) { return IsSetTimeInForce(); }

        public bool IsSetTimeInForce() { return IsSetField(Tags.TimeInForce); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireDate ExpireDate
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireDate();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireDate val) { this.ExpireDate = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireDate Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireDate val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireDate val) { return IsSetExpireDate(); }

        public bool IsSetExpireDate() { return IsSetField(Tags.ExpireDate); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireTime ExpireTime
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireTime();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireTime val) { this.ExpireTime = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireTime Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireTime val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExpireTime val) { return IsSetExpireTime(); }

        public bool IsSetExpireTime() { return IsSetField(Tags.ExpireTime); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MinQty MinQty
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MinQty();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MinQty val) { this.MinQty = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MinQty Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MinQty val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MinQty val) { return IsSetMinQty(); }

        public bool IsSetMinQty() { return IsSetField(Tags.MinQty); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecInst ExecInst
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecInst();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecInst val) { this.ExecInst = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecInst Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecInst val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.ExecInst val) { return IsSetExecInst(); }

        public bool IsSetExecInst() { return IsSetField(Tags.ExecInst); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SellerDays SellerDays
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.SellerDays();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.SellerDays val) { this.SellerDays = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.SellerDays Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.SellerDays val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.SellerDays val) { return IsSetSellerDays(); }

        public bool IsSetSellerDays() { return IsSetField(Tags.SellerDays); }

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

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryBuyer MDEntryBuyer
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryBuyer();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryBuyer val) { this.MDEntryBuyer = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryBuyer Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryBuyer val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryBuyer val) { return IsSetMDEntryBuyer(); }

        public bool IsSetMDEntryBuyer() { return IsSetField(Tags.MDEntryBuyer); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntrySeller MDEntrySeller
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntrySeller();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntrySeller val) { this.MDEntrySeller = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntrySeller Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntrySeller val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntrySeller val) { return IsSetMDEntrySeller(); }

        public bool IsSetMDEntrySeller() { return IsSetField(Tags.MDEntrySeller); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NumberOfOrders NumberOfOrders
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.NumberOfOrders();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.NumberOfOrders val) { this.NumberOfOrders = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.NumberOfOrders Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.NumberOfOrders val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.NumberOfOrders val) { return IsSetNumberOfOrders(); }

        public bool IsSetNumberOfOrders() { return IsSetField(Tags.NumberOfOrders); }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryPositionNo MDEntryPositionNo
        {
            get
            {
                var val = new LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryPositionNo();
                GetField(val);
                return val;
            }
            set { SetField(value); }
        }

        public void Set(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryPositionNo val) { this.MDEntryPositionNo = val; }

        public LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryPositionNo Get(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryPositionNo val)
        {
            GetField(val);
            return val;
        }

        public bool IsSet(LionFire.Trading.QuickFix.ConsoleTest.Fields.MDEntryPositionNo val) { return IsSetMDEntryPositionNo(); }

        public bool IsSetMDEntryPositionNo() { return IsSetField(Tags.MDEntryPositionNo); }

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
