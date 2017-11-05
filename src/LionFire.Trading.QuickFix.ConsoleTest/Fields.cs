using System;
using QuickFix.Fields;

namespace LionFire.Trading.QuickFix.ConsoleTest.Fields
{
    public sealed class Account : StringField
    {
        public Account(): base(Tags.Account) { }
        public Account(string val): base(Tags.Account, val) { }
    }
    public sealed class AdvId : StringField
    {
        public AdvId(): base(Tags.AdvId) { }
        public AdvId(string val): base(Tags.AdvId, val) { }
    }
    public sealed class AdvRefID : StringField
    {
        public AdvRefID(): base(Tags.AdvRefID) { }
        public AdvRefID(string val): base(Tags.AdvRefID, val) { }
    }
    public sealed class AdvSide : CharField
    {
        public AdvSide(): base(Tags.AdvSide) { }
        public AdvSide(char val): base(Tags.AdvSide, val) { }
        public const char BUY = 'B';
        public const char SELL = 'S';
        public const char CROSS = 'X';
        public const char TRADE = 'T';
    }
    public sealed class AdvTransType : StringField
    {
        public AdvTransType(): base(Tags.AdvTransType) { }
        public AdvTransType(string val): base(Tags.AdvTransType, val) { }
        public const string NEW = "N";
        public const string CANCEL = "C";
        public const string REPLACE = "R";
    }
    public sealed class AvgPx : DecimalField
    {
        public AvgPx(): base(Tags.AvgPx) { }
        public AvgPx(decimal val): base(Tags.AvgPx, val) { }
    }
    public sealed class BeginSeqNo : IntField
    {
        public BeginSeqNo(): base(Tags.BeginSeqNo) { }
        public BeginSeqNo(int val): base(Tags.BeginSeqNo, val) { }
    }
    public sealed class BeginString : StringField
    {
        public BeginString(): base(Tags.BeginString) { }
        public BeginString(string val): base(Tags.BeginString, val) { }
    }
    public sealed class BodyLength : IntField
    {
        public BodyLength(): base(Tags.BodyLength) { }
        public BodyLength(int val): base(Tags.BodyLength, val) { }
    }
    public sealed class CheckSum : StringField
    {
        public CheckSum(): base(Tags.CheckSum) { }
        public CheckSum(string val): base(Tags.CheckSum, val) { }
    }
    public sealed class ClOrdID : StringField
    {
        public ClOrdID(): base(Tags.ClOrdID) { }
        public ClOrdID(string val): base(Tags.ClOrdID, val) { }
    }
    public sealed class Commission : DecimalField
    {
        public Commission(): base(Tags.Commission) { }
        public Commission(decimal val): base(Tags.Commission, val) { }
    }
    public sealed class CommType : CharField
    {
        public CommType(): base(Tags.CommType) { }
        public CommType(char val): base(Tags.CommType, val) { }
        public const char PER_SHARE = '1';
        public const char PERCENTAGE = '2';
        public const char ABSOLUTE = '3';
    }
    public sealed class CumQty : DecimalField
    {
        public CumQty(): base(Tags.CumQty) { }
        public CumQty(decimal val): base(Tags.CumQty, val) { }
    }
    public sealed class Currency : StringField
    {
        public Currency(): base(Tags.Currency) { }
        public Currency(string val): base(Tags.Currency, val) { }
    }
    public sealed class EndSeqNo : IntField
    {
        public EndSeqNo(): base(Tags.EndSeqNo) { }
        public EndSeqNo(int val): base(Tags.EndSeqNo, val) { }
    }
    public sealed class ExecID : StringField
    {
        public ExecID(): base(Tags.ExecID) { }
        public ExecID(string val): base(Tags.ExecID, val) { }
    }
    public sealed class ExecInst : StringField
    {
        public ExecInst(): base(Tags.ExecInst) { }
        public ExecInst(string val): base(Tags.ExecInst, val) { }
        public const string NOT_HELD = "1";
        public const string WORK = "2";
        public const string GO_ALONG = "3";
        public const string OVER_THE_DAY = "4";
        public const string HELD = "5";
        public const string PARTICIPATE_DONT_INITIATE = "6";
        public const string STRICT_SCALE = "7";
        public const string TRY_TO_SCALE = "8";
        public const string STAY_ON_BIDSIDE = "9";
        public const string STAY_ON_OFFERSIDE = "0";
        public const string NO_CROSS = "A";
        public const string OK_TO_CROSS = "B";
        public const string CALL_FIRST = "C";
        public const string PERCENT_OF_VOLUME = "D";
        public const string DO_NOT_INCREASE_DNI = "E";
        public const string DO_NOT_REDUCE_DNR = "F";
        public const string ALL_OR_NONE_AON = "G";
        public const string INSTITUTIONS_ONLY = "I";
        public const string LAST_PEG = "L";
        public const string MIDPRICE_PEG = "M";
        public const string NONNEGOTIABLE = "N";
        public const string OPENING_PEG = "O";
        public const string MARKET_PEG = "P";
        public const string PRIMARY_PEG = "R";
        public const string SUSPEND = "S";
        public const string FIXED_PEG = "T";
        public const string CUSTOMER_DISPLAY_INSTRUCTION = "U";
        public const string NETTING = "V";
        public const string PEG_TO_VWAP = "W";
    }
    public sealed class ExecRefID : StringField
    {
        public ExecRefID(): base(Tags.ExecRefID) { }
        public ExecRefID(string val): base(Tags.ExecRefID, val) { }
    }
    public sealed class ExecTransType : CharField
    {
        public ExecTransType(): base(Tags.ExecTransType) { }
        public ExecTransType(char val): base(Tags.ExecTransType, val) { }
        public const char NEW = '0';
        public const char CANCEL = '1';
        public const char CORRECT = '2';
        public const char STATUS = '3';
    }
    public sealed class HandlInst : CharField
    {
        public HandlInst(): base(Tags.HandlInst) { }
        public HandlInst(char val): base(Tags.HandlInst, val) { }
        public const char AUTOMATED_EXECUTION_ORDER_PRIVATE_NO_BROKER_INTERVENTION = '1';
        public const char AUTOMATED_EXECUTION_ORDER_PUBLIC_BROKER_INTERVENTION_OK = '2';
        public const char MANUAL_ORDER_BEST_EXECUTION = '3';
    }
    public sealed class IDSource : StringField
    {
        public IDSource(): base(Tags.IDSource) { }
        public IDSource(string val): base(Tags.IDSource, val) { }
        public const string CUSIP = "1";
        public const string SEDOL = "2";
        public const string QUIK = "3";
        public const string ISIN_NUMBER = "4";
        public const string RIC_CODE = "5";
        public const string ISO_CURRENCY_CODE = "6";
        public const string ISO_COUNTRY_CODE = "7";
        public const string EXCHANGE_SYMBOL = "8";
        public const string CONSOLIDATED_TAPE_ASSOCIATION = "9";
    }
    public sealed class IOIid : StringField
    {
        public IOIid(): base(Tags.IOIid) { }
        public IOIid(string val): base(Tags.IOIid, val) { }
    }
    public sealed class IOIOthSvc : CharField
    {
        public IOIOthSvc(): base(Tags.IOIOthSvc) { }
        public IOIOthSvc(char val): base(Tags.IOIOthSvc, val) { }
    }
    public sealed class IOIQltyInd : CharField
    {
        public IOIQltyInd(): base(Tags.IOIQltyInd) { }
        public IOIQltyInd(char val): base(Tags.IOIQltyInd, val) { }
        public const char LOW = 'L';
        public const char MEDIUM = 'M';
        public const char HIGH = 'H';
    }
    public sealed class IOIRefID : StringField
    {
        public IOIRefID(): base(Tags.IOIRefID) { }
        public IOIRefID(string val): base(Tags.IOIRefID, val) { }
    }
    public sealed class IOIShares : StringField
    {
        public IOIShares(): base(Tags.IOIShares) { }
        public IOIShares(string val): base(Tags.IOIShares, val) { }
    }
    public sealed class IOITransType : CharField
    {
        public IOITransType(): base(Tags.IOITransType) { }
        public IOITransType(char val): base(Tags.IOITransType, val) { }
        public const char NEW = 'N';
        public const char CANCEL = 'C';
        public const char REPLACE = 'R';
    }
    public sealed class LastCapacity : CharField
    {
        public LastCapacity(): base(Tags.LastCapacity) { }
        public LastCapacity(char val): base(Tags.LastCapacity, val) { }
        public const char AGENT = '1';
        public const char CROSS_AS_AGENT = '2';
        public const char CROSS_AS_PRINCIPAL = '3';
        public const char PRINCIPAL = '4';
    }
    public sealed class LastMkt : StringField
    {
        public LastMkt(): base(Tags.LastMkt) { }
        public LastMkt(string val): base(Tags.LastMkt, val) { }
    }
    public sealed class LastPx : DecimalField
    {
        public LastPx(): base(Tags.LastPx) { }
        public LastPx(decimal val): base(Tags.LastPx, val) { }
    }
    public sealed class LastShares : DecimalField
    {
        public LastShares(): base(Tags.LastShares) { }
        public LastShares(decimal val): base(Tags.LastShares, val) { }
    }
    public sealed class LinesOfText : IntField
    {
        public LinesOfText(): base(Tags.LinesOfText) { }
        public LinesOfText(int val): base(Tags.LinesOfText, val) { }
    }
    public sealed class MsgSeqNum : IntField
    {
        public MsgSeqNum(): base(Tags.MsgSeqNum) { }
        public MsgSeqNum(int val): base(Tags.MsgSeqNum, val) { }
    }
    public sealed class MsgType : StringField
    {
        public MsgType(): base(Tags.MsgType) { }
        public MsgType(string val): base(Tags.MsgType, val) { }
    }
    public sealed class NewSeqNo : IntField
    {
        public NewSeqNo(): base(Tags.NewSeqNo) { }
        public NewSeqNo(int val): base(Tags.NewSeqNo, val) { }
    }
    public sealed class OrderID : StringField
    {
        public OrderID(): base(Tags.OrderID) { }
        public OrderID(string val): base(Tags.OrderID, val) { }
    }
    public sealed class OrderQty : DecimalField
    {
        public OrderQty(): base(Tags.OrderQty) { }
        public OrderQty(decimal val): base(Tags.OrderQty, val) { }
    }
    public sealed class OrdStatus : CharField
    {
        public OrdStatus(): base(Tags.OrdStatus) { }
        public OrdStatus(char val): base(Tags.OrdStatus, val) { }
        public const char NEW = '0';
        public const char PARTIALLY_FILLED = '1';
        public const char FILLED = '2';
        public const char DONE_FOR_DAY = '3';
        public const char CANCELED = '4';
        public const char REPLACED = '5';
        public const char PENDING_CANCEL = '6';
        public const char STOPPED = '7';
        public const char REJECTED = '8';
        public const char SUSPENDED = '9';
        public const char PENDING_NEW = 'A';
        public const char CALCULATED = 'B';
        public const char EXPIRED = 'C';
        public const char ACCEPTED_FOR_BIDDING = 'D';
        public const char PENDING_REPLACE = 'E';
    }
    public sealed class OrdType : CharField
    {
        public OrdType(): base(Tags.OrdType) { }
        public OrdType(char val): base(Tags.OrdType, val) { }
        public const char MARKET = '1';
        public const char LIMIT = '2';
        public const char STOP = '3';
        public const char STOP_LIMIT = '4';
        public const char MARKET_ON_CLOSE = '5';
        public const char WITH_OR_WITHOUT = '6';
        public const char LIMIT_OR_BETTER = '7';
        public const char LIMIT_WITH_OR_WITHOUT = '8';
        public const char ON_BASIS = '9';
        public const char ON_CLOSE = 'A';
        public const char LIMIT_ON_CLOSE = 'B';
        public const char FOREX_MARKET = 'C';
        public const char PREVIOUSLY_QUOTED = 'D';
        public const char PREVIOUSLY_INDICATED = 'E';
        public const char FOREX_LIMIT = 'F';
        public const char FOREX_SWAP = 'G';
        public const char FOREX_PREVIOUSLY_QUOTED = 'H';
        public const char FUNARI = 'I';
        public const char PEGGED = 'P';
    }
    public sealed class OrigClOrdID : StringField
    {
        public OrigClOrdID(): base(Tags.OrigClOrdID) { }
        public OrigClOrdID(string val): base(Tags.OrigClOrdID, val) { }
    }
    public sealed class OrigTime : DateTimeField
    {
        public OrigTime(): base(Tags.OrigTime) { }
        public OrigTime(DateTime val): base(Tags.OrigTime, val) { }
        public OrigTime(DateTime val, bool showMilliseconds): base(Tags.OrigTime, val, showMilliseconds) { }
    }
    public sealed class PossDupFlag : BooleanField
    {
        public PossDupFlag(): base(Tags.PossDupFlag) { }
        public PossDupFlag(bool val): base(Tags.PossDupFlag, val) { }
        public const bool POSSIBLE_DUPLICATE = true;
        public const bool ORIGINAL_TRANSMISSION = false;
    }
    public sealed class Price : DecimalField
    {
        public Price(): base(Tags.Price) { }
        public Price(decimal val): base(Tags.Price, val) { }
    }
    public sealed class RefSeqNum : IntField
    {
        public RefSeqNum(): base(Tags.RefSeqNum) { }
        public RefSeqNum(int val): base(Tags.RefSeqNum, val) { }
    }
    public sealed class RelatdSym : StringField
    {
        public RelatdSym(): base(Tags.RelatdSym) { }
        public RelatdSym(string val): base(Tags.RelatdSym, val) { }
    }
    public sealed class Rule80A : CharField
    {
        public Rule80A(): base(Tags.Rule80A) { }
        public Rule80A(char val): base(Tags.Rule80A, val) { }
        public const char AGENCY_SINGLE_ORDER = 'A';
        public const char SHORT_EXEMPT_TRANSACTION_B = 'B';
        public const char PROGRAM_ORDER_NONINDEX_ARB_FOR_MEMBER_FIRMORG = 'C';
        public const char PROGRAM_ORDER_INDEX_ARB_FOR_MEMBER_FIRMORG = 'D';
        public const char REGISTERED_EQUITY_MARKET_MAKER_TRADES = 'E';
        public const char SHORT_EXEMPT_TRANSACTION_F = 'F';
        public const char SHORT_EXEMPT_TRANSACTION_H = 'H';
        public const char PROGRAM_ORDER_INDEX_ARB_FOR_INDIVIDUAL_CUSTOMER = 'J';
        public const char PROGRAM_ORDER_NONINDEX_ARB_FOR_INDIVIDUAL_CUSTOMER = 'K';
        public const char SHORT_EXEMPT_AFFILIATED = 'L';
        public const char PROGRAM_ORDER_INDEX_ARB_FOR_OTHER_MEMBER = 'M';
        public const char PROGRAM_ORDER_NONINDEX_ARB_FOR_OTHER_MEMBER = 'N';
        public const char COMPETING_DEALER_TRADES_O = 'O';
        public const char PRINCIPAL = 'P';
        public const char COMPETING_DEALER_TRADES_R = 'R';
        public const char SPECIALIST_TRADES = 'S';
        public const char COMPETING_DEALER_TRADES_T = 'T';
        public const char PROGRAM_ORDER_INDEX_ARB_FOR_OTHER_AGENCY = 'U';
        public const char ALL_OTHER_ORDERS_AS_AGENT_FOR_OTHER_MEMBER = 'W';
        public const char SHORT_EXEMPT_NOT_AFFILIATED = 'X';
        public const char PROGRAM_ORDER_NONINDEX_ARB_FOR_OTHER_AGENCY = 'Y';
        public const char SHORT_EXEMPT_NONMEMBER = 'Z';
    }
    public sealed class SecurityID : StringField
    {
        public SecurityID(): base(Tags.SecurityID) { }
        public SecurityID(string val): base(Tags.SecurityID, val) { }
    }
    public sealed class SenderCompID : StringField
    {
        public SenderCompID(): base(Tags.SenderCompID) { }
        public SenderCompID(string val): base(Tags.SenderCompID, val) { }
    }
    public sealed class SenderSubID : StringField
    {
        public SenderSubID(): base(Tags.SenderSubID) { }
        public SenderSubID(string val): base(Tags.SenderSubID, val) { }
    }
    public sealed class SendingTime : DateTimeField
    {
        public SendingTime(): base(Tags.SendingTime) { }
        public SendingTime(DateTime val): base(Tags.SendingTime, val) { }
        public SendingTime(DateTime val, bool showMilliseconds): base(Tags.SendingTime, val, showMilliseconds) { }
    }
    public sealed class Shares : DecimalField
    {
        public Shares(): base(Tags.Shares) { }
        public Shares(decimal val): base(Tags.Shares, val) { }
    }
    public sealed class Side : CharField
    {
        public Side(): base(Tags.Side) { }
        public Side(char val): base(Tags.Side, val) { }
        public const char BUY = '1';
        public const char SELL = '2';
        public const char BUY_MINUS = '3';
        public const char SELL_PLUS = '4';
        public const char SELL_SHORT = '5';
        public const char SELL_SHORT_EXEMPT = '6';
        public const char D = '7';
        public const char CROSS = '8';
        public const char CROSS_SHORT = '9';
    }
    public sealed class Symbol : StringField
    {
        public Symbol(): base(Tags.Symbol) { }
        public Symbol(string val): base(Tags.Symbol, val) { }
    }
    public sealed class TargetCompID : StringField
    {
        public TargetCompID(): base(Tags.TargetCompID) { }
        public TargetCompID(string val): base(Tags.TargetCompID, val) { }
    }
    public sealed class TargetSubID : StringField
    {
        public TargetSubID(): base(Tags.TargetSubID) { }
        public TargetSubID(string val): base(Tags.TargetSubID, val) { }
    }
    public sealed class Text : StringField
    {
        public Text(): base(Tags.Text) { }
        public Text(string val): base(Tags.Text, val) { }
    }
    public sealed class TimeInForce : CharField
    {
        public TimeInForce(): base(Tags.TimeInForce) { }
        public TimeInForce(char val): base(Tags.TimeInForce, val) { }
        public const char DAY = '0';
        public const char GOOD_TILL_CANCEL = '1';
        public const char AT_THE_OPENING = '2';
        public const char IMMEDIATE_OR_CANCEL = '3';
        public const char FILL_OR_KILL = '4';
        public const char GOOD_TILL_CROSSING = '5';
        public const char GOOD_TILL_DATE = '6';
    }
    public sealed class TransactTime : DateTimeField
    {
        public TransactTime(): base(Tags.TransactTime) { }
        public TransactTime(DateTime val): base(Tags.TransactTime, val) { }
        public TransactTime(DateTime val, bool showMilliseconds): base(Tags.TransactTime, val, showMilliseconds) { }
    }
    public sealed class Urgency : CharField
    {
        public Urgency(): base(Tags.Urgency) { }
        public Urgency(char val): base(Tags.Urgency, val) { }
        public const char NORMAL = '0';
        public const char FLASH = '1';
        public const char BACKGROUND = '2';
    }
    public sealed class ValidUntilTime : DateTimeField
    {
        public ValidUntilTime(): base(Tags.ValidUntilTime) { }
        public ValidUntilTime(DateTime val): base(Tags.ValidUntilTime, val) { }
        public ValidUntilTime(DateTime val, bool showMilliseconds): base(Tags.ValidUntilTime, val, showMilliseconds) { }
    }
    public sealed class SettlmntTyp : CharField
    {
        public SettlmntTyp(): base(Tags.SettlmntTyp) { }
        public SettlmntTyp(char val): base(Tags.SettlmntTyp, val) { }
        public const char REGULAR = '0';
        public const char CASH = '1';
        public const char NEXT_DAY = '2';
        public const char TPLUS2 = '3';
        public const char TPLUS3 = '4';
        public const char TPLUS4 = '5';
        public const char FUTURE = '6';
        public const char WHEN_ISSUED = '7';
        public const char SELLERS_OPTION = '8';
        public const char TPLUS5 = '9';
    }
    public sealed class FutSettDate : StringField
    {
        public FutSettDate(): base(Tags.FutSettDate) { }
        public FutSettDate(string val): base(Tags.FutSettDate, val) { }
    }
    public sealed class SymbolSfx : StringField
    {
        public SymbolSfx(): base(Tags.SymbolSfx) { }
        public SymbolSfx(string val): base(Tags.SymbolSfx, val) { }
    }
    public sealed class ListID : StringField
    {
        public ListID(): base(Tags.ListID) { }
        public ListID(string val): base(Tags.ListID, val) { }
    }
    public sealed class ListSeqNo : IntField
    {
        public ListSeqNo(): base(Tags.ListSeqNo) { }
        public ListSeqNo(int val): base(Tags.ListSeqNo, val) { }
    }
    public sealed class TotNoOrders : IntField
    {
        public TotNoOrders(): base(Tags.TotNoOrders) { }
        public TotNoOrders(int val): base(Tags.TotNoOrders, val) { }
    }
    public sealed class ListExecInst : StringField
    {
        public ListExecInst(): base(Tags.ListExecInst) { }
        public ListExecInst(string val): base(Tags.ListExecInst, val) { }
    }
    public sealed class AllocID : StringField
    {
        public AllocID(): base(Tags.AllocID) { }
        public AllocID(string val): base(Tags.AllocID, val) { }
    }
    public sealed class AllocTransType : CharField
    {
        public AllocTransType(): base(Tags.AllocTransType) { }
        public AllocTransType(char val): base(Tags.AllocTransType, val) { }
        public const char NEW = '0';
        public const char REPLACE = '1';
        public const char CANCEL = '2';
        public const char PRELIMINARY = '3';
        public const char CALCULATED = '4';
        public const char CALCULATED_WITHOUT_PRELIMINARY = '5';
    }
    public sealed class RefAllocID : StringField
    {
        public RefAllocID(): base(Tags.RefAllocID) { }
        public RefAllocID(string val): base(Tags.RefAllocID, val) { }
    }
    public sealed class NoOrders : IntField
    {
        public NoOrders(): base(Tags.NoOrders) { }
        public NoOrders(int val): base(Tags.NoOrders, val) { }
    }
    public sealed class AvgPrxPrecision : IntField
    {
        public AvgPrxPrecision(): base(Tags.AvgPrxPrecision) { }
        public AvgPrxPrecision(int val): base(Tags.AvgPrxPrecision, val) { }
    }
    public sealed class TradeDate : StringField
    {
        public TradeDate(): base(Tags.TradeDate) { }
        public TradeDate(string val): base(Tags.TradeDate, val) { }
    }
    public sealed class ExecBroker : StringField
    {
        public ExecBroker(): base(Tags.ExecBroker) { }
        public ExecBroker(string val): base(Tags.ExecBroker, val) { }
    }
    public sealed class OpenClose : CharField
    {
        public OpenClose(): base(Tags.OpenClose) { }
        public OpenClose(char val): base(Tags.OpenClose, val) { }
        public const char OPEN = 'O';
        public const char CLOSE = 'C';
    }
    public sealed class NoAllocs : IntField
    {
        public NoAllocs(): base(Tags.NoAllocs) { }
        public NoAllocs(int val): base(Tags.NoAllocs, val) { }
    }
    public sealed class AllocAccount : StringField
    {
        public AllocAccount(): base(Tags.AllocAccount) { }
        public AllocAccount(string val): base(Tags.AllocAccount, val) { }
    }
    public sealed class AllocShares : DecimalField
    {
        public AllocShares(): base(Tags.AllocShares) { }
        public AllocShares(decimal val): base(Tags.AllocShares, val) { }
    }
    public sealed class ProcessCode : CharField
    {
        public ProcessCode(): base(Tags.ProcessCode) { }
        public ProcessCode(char val): base(Tags.ProcessCode, val) { }
        public const char REGULAR = '0';
        public const char SOFT_DOLLAR = '1';
        public const char STEPIN = '2';
        public const char STEPOUT = '3';
        public const char SOFTDOLLAR_STEPIN = '4';
        public const char SOFTDOLLAR_STEPOUT = '5';
        public const char PLAN_SPONSOR = '6';
    }
    public sealed class NoRpts : IntField
    {
        public NoRpts(): base(Tags.NoRpts) { }
        public NoRpts(int val): base(Tags.NoRpts, val) { }
    }
    public sealed class RptSeq : IntField
    {
        public RptSeq(): base(Tags.RptSeq) { }
        public RptSeq(int val): base(Tags.RptSeq, val) { }
    }
    public sealed class CxlQty : DecimalField
    {
        public CxlQty(): base(Tags.CxlQty) { }
        public CxlQty(decimal val): base(Tags.CxlQty, val) { }
    }
    public sealed class NoDlvyInst : IntField
    {
        public NoDlvyInst(): base(Tags.NoDlvyInst) { }
        public NoDlvyInst(int val): base(Tags.NoDlvyInst, val) { }
    }
    public sealed class DlvyInst : StringField
    {
        public DlvyInst(): base(Tags.DlvyInst) { }
        public DlvyInst(string val): base(Tags.DlvyInst, val) { }
    }
    public sealed class AllocStatus : IntField
    {
        public AllocStatus(): base(Tags.AllocStatus) { }
        public AllocStatus(int val): base(Tags.AllocStatus, val) { }
        public const int ACCEPTED = 0;
        public const int REJECTED = 1;
        public const int PARTIAL_ACCEPT = 2;
        public const int RECEIVED = 3;
    }
    public sealed class AllocRejCode : IntField
    {
        public AllocRejCode(): base(Tags.AllocRejCode) { }
        public AllocRejCode(int val): base(Tags.AllocRejCode, val) { }
        public const int UNKNOWN_ACCOUNT = 0;
        public const int INCORRECT_QUANTITY = 1;
        public const int INCORRECT_AVERAGE_PRICE = 2;
        public const int UNKNOWN_EXECUTING_BROKER_MNEMONIC = 3;
        public const int COMMISSION_DIFFERENCE = 4;
        public const int UNKNOWN_ORDERID = 5;
        public const int UNKNOWN_LISTID = 6;
        public const int OTHER = 7;
    }
    public sealed class Signature : StringField
    {
        public Signature(): base(Tags.Signature) { }
        public Signature(string val): base(Tags.Signature, val) { }
    }
    public sealed class SecureDataLen : IntField
    {
        public SecureDataLen(): base(Tags.SecureDataLen) { }
        public SecureDataLen(int val): base(Tags.SecureDataLen, val) { }
    }
    public sealed class SecureData : StringField
    {
        public SecureData(): base(Tags.SecureData) { }
        public SecureData(string val): base(Tags.SecureData, val) { }
    }
    public sealed class BrokerOfCredit : StringField
    {
        public BrokerOfCredit(): base(Tags.BrokerOfCredit) { }
        public BrokerOfCredit(string val): base(Tags.BrokerOfCredit, val) { }
    }
    public sealed class SignatureLength : IntField
    {
        public SignatureLength(): base(Tags.SignatureLength) { }
        public SignatureLength(int val): base(Tags.SignatureLength, val) { }
    }
    public sealed class EmailType : CharField
    {
        public EmailType(): base(Tags.EmailType) { }
        public EmailType(char val): base(Tags.EmailType, val) { }
        public const char NEW = '0';
        public const char REPLY = '1';
        public const char ADMIN_REPLY = '2';
    }
    public sealed class RawDataLength : IntField
    {
        public RawDataLength(): base(Tags.RawDataLength) { }
        public RawDataLength(int val): base(Tags.RawDataLength, val) { }
    }
    public sealed class RawData : StringField
    {
        public RawData(): base(Tags.RawData) { }
        public RawData(string val): base(Tags.RawData, val) { }
    }
    public sealed class PossResend : BooleanField
    {
        public PossResend(): base(Tags.PossResend) { }
        public PossResend(bool val): base(Tags.PossResend, val) { }
    }
    public sealed class EncryptMethod : IntField
    {
        public EncryptMethod(): base(Tags.EncryptMethod) { }
        public EncryptMethod(int val): base(Tags.EncryptMethod, val) { }
        public const int NONE_OTHER = 0;
        public const int PKCS = 1;
        public const int DES = 2;
        public const int PKCSDES = 3;
        public const int PGPDES = 4;
        public const int PGPDESMD5 = 5;
        public const int PEMDESMD5 = 6;
    }
    public sealed class StopPx : DecimalField
    {
        public StopPx(): base(Tags.StopPx) { }
        public StopPx(decimal val): base(Tags.StopPx, val) { }
    }
    public sealed class ExDestination : StringField
    {
        public ExDestination(): base(Tags.ExDestination) { }
        public ExDestination(string val): base(Tags.ExDestination, val) { }
    }
    public sealed class CxlRejReason : IntField
    {
        public CxlRejReason(): base(Tags.CxlRejReason) { }
        public CxlRejReason(int val): base(Tags.CxlRejReason, val) { }
        public const int TOO_LATE_TO_CANCEL = 0;
        public const int UNKNOWN_ORDER = 1;
        public const int BROKER_OPTION = 2;
        public const int ALREADY_PENDING = 3;
    }
    public sealed class OrdRejReason : IntField
    {
        public OrdRejReason(): base(Tags.OrdRejReason) { }
        public OrdRejReason(int val): base(Tags.OrdRejReason, val) { }
        public const int BROKER_OPTION = 0;
        public const int UNKNOWN_SYMBOL = 1;
        public const int EXCHANGE_CLOSED = 2;
        public const int ORDER_EXCEEDS_LIMIT = 3;
        public const int TOO_LATE_TO_ENTER = 4;
        public const int UNKNOWN_ORDER = 5;
        public const int DUPLICATE_ORDER = 6;
        public const int DUPLICATE_VERBALYES = 7;
        public const int STALE_ORDER = 8;
    }
    public sealed class IOIQualifier : CharField
    {
        public IOIQualifier(): base(Tags.IOIQualifier) { }
        public IOIQualifier(char val): base(Tags.IOIQualifier, val) { }
        public const char ALL_OR_NONE = 'A';
        public const char AT_THE_CLOSE = 'C';
        public const char IN_TOUCH_WITH = 'I';
        public const char LIMIT = 'L';
        public const char MORE_BEHIND = 'M';
        public const char AT_THE_OPEN = 'O';
        public const char TAKING_A_POSITION = 'P';
        public const char AT_THE_MARKET = 'Q';
        public const char READY_TO_TRADE = 'R';
        public const char PORTFOLIO_SHOWN = 'S';
        public const char THROUGH_THE_DAY = 'T';
        public const char VERSUS = 'V';
        public const char INDICATION_WORKING_AWAY = 'W';
        public const char CROSSING_OPPORTUNITY = 'X';
        public const char AT_THE_MIDPOINT = 'Y';
        public const char PREOPEN = 'Z';
    }
    public sealed class WaveNo : StringField
    {
        public WaveNo(): base(Tags.WaveNo) { }
        public WaveNo(string val): base(Tags.WaveNo, val) { }
    }
    public sealed class Issuer : StringField
    {
        public Issuer(): base(Tags.Issuer) { }
        public Issuer(string val): base(Tags.Issuer, val) { }
    }
    public sealed class SecurityDesc : StringField
    {
        public SecurityDesc(): base(Tags.SecurityDesc) { }
        public SecurityDesc(string val): base(Tags.SecurityDesc, val) { }
    }
    public sealed class HeartBtInt : IntField
    {
        public HeartBtInt(): base(Tags.HeartBtInt) { }
        public HeartBtInt(int val): base(Tags.HeartBtInt, val) { }
    }
    public sealed class ClientID : StringField
    {
        public ClientID(): base(Tags.ClientID) { }
        public ClientID(string val): base(Tags.ClientID, val) { }
    }
    public sealed class MinQty : DecimalField
    {
        public MinQty(): base(Tags.MinQty) { }
        public MinQty(decimal val): base(Tags.MinQty, val) { }
    }
    public sealed class MaxFloor : DecimalField
    {
        public MaxFloor(): base(Tags.MaxFloor) { }
        public MaxFloor(decimal val): base(Tags.MaxFloor, val) { }
    }
    public sealed class TestReqID : StringField
    {
        public TestReqID(): base(Tags.TestReqID) { }
        public TestReqID(string val): base(Tags.TestReqID, val) { }
    }
    public sealed class ReportToExch : BooleanField
    {
        public ReportToExch(): base(Tags.ReportToExch) { }
        public ReportToExch(bool val): base(Tags.ReportToExch, val) { }
        public const bool YES = true;
        public const bool NO = false;
    }
    public sealed class LocateReqd : BooleanField
    {
        public LocateReqd(): base(Tags.LocateReqd) { }
        public LocateReqd(bool val): base(Tags.LocateReqd, val) { }
        public const bool YES = true;
        public const bool NO = false;
    }
    public sealed class OnBehalfOfCompID : StringField
    {
        public OnBehalfOfCompID(): base(Tags.OnBehalfOfCompID) { }
        public OnBehalfOfCompID(string val): base(Tags.OnBehalfOfCompID, val) { }
    }
    public sealed class OnBehalfOfSubID : StringField
    {
        public OnBehalfOfSubID(): base(Tags.OnBehalfOfSubID) { }
        public OnBehalfOfSubID(string val): base(Tags.OnBehalfOfSubID, val) { }
    }
    public sealed class QuoteID : StringField
    {
        public QuoteID(): base(Tags.QuoteID) { }
        public QuoteID(string val): base(Tags.QuoteID, val) { }
    }
    public sealed class NetMoney : DecimalField
    {
        public NetMoney(): base(Tags.NetMoney) { }
        public NetMoney(decimal val): base(Tags.NetMoney, val) { }
    }
    public sealed class SettlCurrAmt : DecimalField
    {
        public SettlCurrAmt(): base(Tags.SettlCurrAmt) { }
        public SettlCurrAmt(decimal val): base(Tags.SettlCurrAmt, val) { }
    }
    public sealed class SettlCurrency : StringField
    {
        public SettlCurrency(): base(Tags.SettlCurrency) { }
        public SettlCurrency(string val): base(Tags.SettlCurrency, val) { }
    }
    public sealed class ForexReq : BooleanField
    {
        public ForexReq(): base(Tags.ForexReq) { }
        public ForexReq(bool val): base(Tags.ForexReq, val) { }
        public const bool YES = true;
        public const bool NO = false;
    }
    public sealed class OrigSendingTime : DateTimeField
    {
        public OrigSendingTime(): base(Tags.OrigSendingTime) { }
        public OrigSendingTime(DateTime val): base(Tags.OrigSendingTime, val) { }
        public OrigSendingTime(DateTime val, bool showMilliseconds): base(Tags.OrigSendingTime, val, showMilliseconds) { }
    }
    public sealed class GapFillFlag : BooleanField
    {
        public GapFillFlag(): base(Tags.GapFillFlag) { }
        public GapFillFlag(bool val): base(Tags.GapFillFlag, val) { }
        public const bool GAP_FILL_MESSAGE_MSGSEQNUM_FIELD_VALID = true;
        public const bool SEQUENCE_RESET_IGNORE_MSGSEQNUM = false;
    }
    public sealed class NoExecs : IntField
    {
        public NoExecs(): base(Tags.NoExecs) { }
        public NoExecs(int val): base(Tags.NoExecs, val) { }
    }
    public sealed class CxlType : CharField
    {
        public CxlType(): base(Tags.CxlType) { }
        public CxlType(char val): base(Tags.CxlType, val) { }
    }
    public sealed class ExpireTime : DateTimeField
    {
        public ExpireTime(): base(Tags.ExpireTime) { }
        public ExpireTime(DateTime val): base(Tags.ExpireTime, val) { }
        public ExpireTime(DateTime val, bool showMilliseconds): base(Tags.ExpireTime, val, showMilliseconds) { }
    }
    public sealed class DKReason : CharField
    {
        public DKReason(): base(Tags.DKReason) { }
        public DKReason(char val): base(Tags.DKReason, val) { }
        public const char UNKNOWN_SYMBOL = 'A';
        public const char WRONG_SIDE = 'B';
        public const char QUANTITY_EXCEEDS_ORDER = 'C';
        public const char NO_MATCHING_ORDER = 'D';
        public const char PRICE_EXCEEDS_LIMIT = 'E';
        public const char OTHER = 'Z';
    }
    public sealed class DeliverToCompID : StringField
    {
        public DeliverToCompID(): base(Tags.DeliverToCompID) { }
        public DeliverToCompID(string val): base(Tags.DeliverToCompID, val) { }
    }
    public sealed class DeliverToSubID : StringField
    {
        public DeliverToSubID(): base(Tags.DeliverToSubID) { }
        public DeliverToSubID(string val): base(Tags.DeliverToSubID, val) { }
    }
    public sealed class IOINaturalFlag : BooleanField
    {
        public IOINaturalFlag(): base(Tags.IOINaturalFlag) { }
        public IOINaturalFlag(bool val): base(Tags.IOINaturalFlag, val) { }
        public const bool NATURAL = true;
        public const bool NOT_NATURAL = false;
    }
    public sealed class QuoteReqID : StringField
    {
        public QuoteReqID(): base(Tags.QuoteReqID) { }
        public QuoteReqID(string val): base(Tags.QuoteReqID, val) { }
    }
    public sealed class BidPx : DecimalField
    {
        public BidPx(): base(Tags.BidPx) { }
        public BidPx(decimal val): base(Tags.BidPx, val) { }
    }
    public sealed class OfferPx : DecimalField
    {
        public OfferPx(): base(Tags.OfferPx) { }
        public OfferPx(decimal val): base(Tags.OfferPx, val) { }
    }
    public sealed class BidSize : DecimalField
    {
        public BidSize(): base(Tags.BidSize) { }
        public BidSize(decimal val): base(Tags.BidSize, val) { }
    }
    public sealed class OfferSize : DecimalField
    {
        public OfferSize(): base(Tags.OfferSize) { }
        public OfferSize(decimal val): base(Tags.OfferSize, val) { }
    }
    public sealed class NoMiscFees : IntField
    {
        public NoMiscFees(): base(Tags.NoMiscFees) { }
        public NoMiscFees(int val): base(Tags.NoMiscFees, val) { }
    }
    public sealed class MiscFeeAmt : DecimalField
    {
        public MiscFeeAmt(): base(Tags.MiscFeeAmt) { }
        public MiscFeeAmt(decimal val): base(Tags.MiscFeeAmt, val) { }
    }
    public sealed class MiscFeeCurr : StringField
    {
        public MiscFeeCurr(): base(Tags.MiscFeeCurr) { }
        public MiscFeeCurr(string val): base(Tags.MiscFeeCurr, val) { }
    }
    public sealed class MiscFeeType : CharField
    {
        public MiscFeeType(): base(Tags.MiscFeeType) { }
        public MiscFeeType(char val): base(Tags.MiscFeeType, val) { }
        public const char REGULATORY = '1';
        public const char TAX = '2';
        public const char LOCAL_COMMISSION = '3';
        public const char EXCHANGE_FEES = '4';
        public const char STAMP = '5';
        public const char LEVY = '6';
        public const char OTHER = '7';
        public const char MARKUP = '8';
        public const char CONSUMPTION_TAX = '9';
    }
    public sealed class PrevClosePx : DecimalField
    {
        public PrevClosePx(): base(Tags.PrevClosePx) { }
        public PrevClosePx(decimal val): base(Tags.PrevClosePx, val) { }
    }
    public sealed class ResetSeqNumFlag : BooleanField
    {
        public ResetSeqNumFlag(): base(Tags.ResetSeqNumFlag) { }
        public ResetSeqNumFlag(bool val): base(Tags.ResetSeqNumFlag, val) { }
        public const bool YES_RESET_SEQUENCE_NUMBERS = true;
        public const bool NO = false;
    }
    public sealed class SenderLocationID : StringField
    {
        public SenderLocationID(): base(Tags.SenderLocationID) { }
        public SenderLocationID(string val): base(Tags.SenderLocationID, val) { }
    }
    public sealed class TargetLocationID : StringField
    {
        public TargetLocationID(): base(Tags.TargetLocationID) { }
        public TargetLocationID(string val): base(Tags.TargetLocationID, val) { }
    }
    public sealed class OnBehalfOfLocationID : StringField
    {
        public OnBehalfOfLocationID(): base(Tags.OnBehalfOfLocationID) { }
        public OnBehalfOfLocationID(string val): base(Tags.OnBehalfOfLocationID, val) { }
    }
    public sealed class DeliverToLocationID : StringField
    {
        public DeliverToLocationID(): base(Tags.DeliverToLocationID) { }
        public DeliverToLocationID(string val): base(Tags.DeliverToLocationID, val) { }
    }
    public sealed class NoRelatedSym : IntField
    {
        public NoRelatedSym(): base(Tags.NoRelatedSym) { }
        public NoRelatedSym(int val): base(Tags.NoRelatedSym, val) { }
    }
    public sealed class Subject : StringField
    {
        public Subject(): base(Tags.Subject) { }
        public Subject(string val): base(Tags.Subject, val) { }
    }
    public sealed class Headline : StringField
    {
        public Headline(): base(Tags.Headline) { }
        public Headline(string val): base(Tags.Headline, val) { }
    }
    public sealed class URLLink : StringField
    {
        public URLLink(): base(Tags.URLLink) { }
        public URLLink(string val): base(Tags.URLLink, val) { }
    }
    public sealed class ExecType : CharField
    {
        public ExecType(): base(Tags.ExecType) { }
        public ExecType(char val): base(Tags.ExecType, val) { }
        public const char NEW = '0';
        public const char PARTIAL_FILL = '1';
        public const char FILL = '2';
        public const char DONE_FOR_DAY = '3';
        public const char CANCELED = '4';
        public const char REPLACE = '5';
        public const char PENDING_CANCEL = '6';
        public const char STOPPED = '7';
        public const char REJECTED = '8';
        public const char SUSPENDED = '9';
        public const char PENDING_NEW = 'A';
        public const char CALCULATED = 'B';
        public const char EXPIRED = 'C';
        public const char RESTATED = 'D';
        public const char PENDING_REPLACE = 'E';
    }
    public sealed class LeavesQty : DecimalField
    {
        public LeavesQty(): base(Tags.LeavesQty) { }
        public LeavesQty(decimal val): base(Tags.LeavesQty, val) { }
    }
    public sealed class CashOrderQty : DecimalField
    {
        public CashOrderQty(): base(Tags.CashOrderQty) { }
        public CashOrderQty(decimal val): base(Tags.CashOrderQty, val) { }
    }
    public sealed class AllocAvgPx : DecimalField
    {
        public AllocAvgPx(): base(Tags.AllocAvgPx) { }
        public AllocAvgPx(decimal val): base(Tags.AllocAvgPx, val) { }
    }
    public sealed class AllocNetMoney : DecimalField
    {
        public AllocNetMoney(): base(Tags.AllocNetMoney) { }
        public AllocNetMoney(decimal val): base(Tags.AllocNetMoney, val) { }
    }
    public sealed class SettlCurrFxRate : DecimalField
    {
        public SettlCurrFxRate(): base(Tags.SettlCurrFxRate) { }
        public SettlCurrFxRate(decimal val): base(Tags.SettlCurrFxRate, val) { }
    }
    public sealed class SettlCurrFxRateCalc : CharField
    {
        public SettlCurrFxRateCalc(): base(Tags.SettlCurrFxRateCalc) { }
        public SettlCurrFxRateCalc(char val): base(Tags.SettlCurrFxRateCalc, val) { }
        public const char MULTIPLY = 'M';
        public const char DIVIDE = 'D';
    }
    public sealed class NumDaysInterest : IntField
    {
        public NumDaysInterest(): base(Tags.NumDaysInterest) { }
        public NumDaysInterest(int val): base(Tags.NumDaysInterest, val) { }
    }
    public sealed class AccruedInterestRate : DecimalField
    {
        public AccruedInterestRate(): base(Tags.AccruedInterestRate) { }
        public AccruedInterestRate(decimal val): base(Tags.AccruedInterestRate, val) { }
    }
    public sealed class AccruedInterestAmt : DecimalField
    {
        public AccruedInterestAmt(): base(Tags.AccruedInterestAmt) { }
        public AccruedInterestAmt(decimal val): base(Tags.AccruedInterestAmt, val) { }
    }
    public sealed class SettlInstMode : CharField
    {
        public SettlInstMode(): base(Tags.SettlInstMode) { }
        public SettlInstMode(char val): base(Tags.SettlInstMode, val) { }
        public const char DEFAULT = '0';
        public const char STANDING_INSTRUCTIONS_PROVIDED = '1';
        public const char SPECIFIC_ALLOCATION_ACCOUNT_OVERRIDING = '2';
        public const char SPECIFIC_ALLOCATION_ACCOUNT_STANDING = '3';
    }
    public sealed class AllocText : StringField
    {
        public AllocText(): base(Tags.AllocText) { }
        public AllocText(string val): base(Tags.AllocText, val) { }
    }
    public sealed class SettlInstID : StringField
    {
        public SettlInstID(): base(Tags.SettlInstID) { }
        public SettlInstID(string val): base(Tags.SettlInstID, val) { }
    }
    public sealed class SettlInstTransType : CharField
    {
        public SettlInstTransType(): base(Tags.SettlInstTransType) { }
        public SettlInstTransType(char val): base(Tags.SettlInstTransType, val) { }
        public const char NEW = 'N';
        public const char CANCEL = 'C';
        public const char REPLACE = 'R';
    }
    public sealed class EmailThreadID : StringField
    {
        public EmailThreadID(): base(Tags.EmailThreadID) { }
        public EmailThreadID(string val): base(Tags.EmailThreadID, val) { }
    }
    public sealed class SettlInstSource : CharField
    {
        public SettlInstSource(): base(Tags.SettlInstSource) { }
        public SettlInstSource(char val): base(Tags.SettlInstSource, val) { }
        public const char BROKER = '1';
        public const char INSTITUTION = '2';
    }
    public sealed class SettlLocation : StringField
    {
        public SettlLocation(): base(Tags.SettlLocation) { }
        public SettlLocation(string val): base(Tags.SettlLocation, val) { }
        public const string CEDEL = "CED";
        public const string DEPOSITORY_TRUST_COMPANY = "DTC";
        public const string EUROCLEAR = "EUR";
        public const string FEDERAL_BOOK_ENTRY = "FED";
        public const string PHYSICAL = "PNY";
        public const string PARTICIPANT_TRUST_COMPANY = "PTC";
        public const string LOCAL_MARKET_SETTLE_LOCATION = "ISO";
    }
    public sealed class SecurityType : StringField
    {
        public SecurityType(): base(Tags.SecurityType) { }
        public SecurityType(string val): base(Tags.SecurityType, val) { }
        public const string BANKERS_ACCEPTANCE = "BA";
        public const string CONVERTIBLE_BOND = "CB";
        public const string CERTIFICATE_OF_DEPOSIT = "CD";
        public const string COLLATERALIZE_MORTGAGE_OBLIGATION = "CMO";
        public const string CORPORATE_BOND = "CORP";
        public const string COMMERCIAL_PAPER = "CP";
        public const string CORPORATE_PRIVATE_PLACEMENT = "CPP";
        public const string COMMON_STOCK = "CS";
        public const string FEDERAL_HOUSING_AUTHORITY = "FHA";
        public const string FEDERAL_HOME_LOAN = "FHL";
        public const string FEDERAL_NATIONAL_MORTGAGE_ASSOCIATION = "FN";
        public const string FOREIGN_EXCHANGE_CONTRACT = "FOR";
        public const string FUTURE = "FUT";
        public const string GOVERNMENT_NATIONAL_MORTGAGE_ASSOCIATION = "GN";
        public const string TREASURIES_PLUS_AGENCY_DEBENTURE = "GOVT";
        public const string MUTUAL_FUND = "MF";
        public const string MORTGAGE_INTEREST_ONLY = "MIO";
        public const string MORTGAGE_PRINCIPAL_ONLY = "MPO";
        public const string MORTGAGE_PRIVATE_PLACEMENT = "MPP";
        public const string MISCELLANEOUS_PASSTHRU = "MPT";
        public const string MUNICIPAL_BOND = "MUNI";
        public const string NO_ISITC_SECURITY_TYPE = "NONE";
        public const string OPTION = "OPT";
        public const string PREFERRED_STOCK = "PS";
        public const string REPURCHASE_AGREEMENT = "RP";
        public const string REVERSE_REPURCHASE_AGREEMENT = "RVRP";
        public const string STUDENT_LOAN_MARKETING_ASSOCIATION = "SL";
        public const string TIME_DEPOSIT = "TD";
        public const string US_TREASURY_BILL = "USTB";
        public const string WARRANT = "WAR";
        public const string CATS_TIGERS = "ZOO";
    }
    public sealed class EffectiveTime : DateTimeField
    {
        public EffectiveTime(): base(Tags.EffectiveTime) { }
        public EffectiveTime(DateTime val): base(Tags.EffectiveTime, val) { }
        public EffectiveTime(DateTime val, bool showMilliseconds): base(Tags.EffectiveTime, val, showMilliseconds) { }
    }
    public sealed class StandInstDbType : IntField
    {
        public StandInstDbType(): base(Tags.StandInstDbType) { }
        public StandInstDbType(int val): base(Tags.StandInstDbType, val) { }
        public const int OTHER = 0;
        public const int DTC_SID = 1;
        public const int THOMSON_ALERT = 2;
        public const int A_GLOBAL_CUSTODIAN = 3;
    }
    public sealed class StandInstDbName : StringField
    {
        public StandInstDbName(): base(Tags.StandInstDbName) { }
        public StandInstDbName(string val): base(Tags.StandInstDbName, val) { }
    }
    public sealed class StandInstDbID : StringField
    {
        public StandInstDbID(): base(Tags.StandInstDbID) { }
        public StandInstDbID(string val): base(Tags.StandInstDbID, val) { }
    }
    public sealed class SettlDeliveryType : IntField
    {
        public SettlDeliveryType(): base(Tags.SettlDeliveryType) { }
        public SettlDeliveryType(int val): base(Tags.SettlDeliveryType, val) { }
    }
    public sealed class SettlDepositoryCode : StringField
    {
        public SettlDepositoryCode(): base(Tags.SettlDepositoryCode) { }
        public SettlDepositoryCode(string val): base(Tags.SettlDepositoryCode, val) { }
    }
    public sealed class SettlBrkrCode : StringField
    {
        public SettlBrkrCode(): base(Tags.SettlBrkrCode) { }
        public SettlBrkrCode(string val): base(Tags.SettlBrkrCode, val) { }
    }
    public sealed class SettlInstCode : StringField
    {
        public SettlInstCode(): base(Tags.SettlInstCode) { }
        public SettlInstCode(string val): base(Tags.SettlInstCode, val) { }
    }
    public sealed class SecuritySettlAgentName : StringField
    {
        public SecuritySettlAgentName(): base(Tags.SecuritySettlAgentName) { }
        public SecuritySettlAgentName(string val): base(Tags.SecuritySettlAgentName, val) { }
    }
    public sealed class SecuritySettlAgentCode : StringField
    {
        public SecuritySettlAgentCode(): base(Tags.SecuritySettlAgentCode) { }
        public SecuritySettlAgentCode(string val): base(Tags.SecuritySettlAgentCode, val) { }
    }
    public sealed class SecuritySettlAgentAcctNum : StringField
    {
        public SecuritySettlAgentAcctNum(): base(Tags.SecuritySettlAgentAcctNum) { }
        public SecuritySettlAgentAcctNum(string val): base(Tags.SecuritySettlAgentAcctNum, val) { }
    }
    public sealed class SecuritySettlAgentAcctName : StringField
    {
        public SecuritySettlAgentAcctName(): base(Tags.SecuritySettlAgentAcctName) { }
        public SecuritySettlAgentAcctName(string val): base(Tags.SecuritySettlAgentAcctName, val) { }
    }
    public sealed class SecuritySettlAgentContactName : StringField
    {
        public SecuritySettlAgentContactName(): base(Tags.SecuritySettlAgentContactName) { }
        public SecuritySettlAgentContactName(string val): base(Tags.SecuritySettlAgentContactName, val) { }
    }
    public sealed class SecuritySettlAgentContactPhone : StringField
    {
        public SecuritySettlAgentContactPhone(): base(Tags.SecuritySettlAgentContactPhone) { }
        public SecuritySettlAgentContactPhone(string val): base(Tags.SecuritySettlAgentContactPhone, val) { }
    }
    public sealed class CashSettlAgentName : StringField
    {
        public CashSettlAgentName(): base(Tags.CashSettlAgentName) { }
        public CashSettlAgentName(string val): base(Tags.CashSettlAgentName, val) { }
    }
    public sealed class CashSettlAgentCode : StringField
    {
        public CashSettlAgentCode(): base(Tags.CashSettlAgentCode) { }
        public CashSettlAgentCode(string val): base(Tags.CashSettlAgentCode, val) { }
    }
    public sealed class CashSettlAgentAcctNum : StringField
    {
        public CashSettlAgentAcctNum(): base(Tags.CashSettlAgentAcctNum) { }
        public CashSettlAgentAcctNum(string val): base(Tags.CashSettlAgentAcctNum, val) { }
    }
    public sealed class CashSettlAgentAcctName : StringField
    {
        public CashSettlAgentAcctName(): base(Tags.CashSettlAgentAcctName) { }
        public CashSettlAgentAcctName(string val): base(Tags.CashSettlAgentAcctName, val) { }
    }
    public sealed class CashSettlAgentContactName : StringField
    {
        public CashSettlAgentContactName(): base(Tags.CashSettlAgentContactName) { }
        public CashSettlAgentContactName(string val): base(Tags.CashSettlAgentContactName, val) { }
    }
    public sealed class CashSettlAgentContactPhone : StringField
    {
        public CashSettlAgentContactPhone(): base(Tags.CashSettlAgentContactPhone) { }
        public CashSettlAgentContactPhone(string val): base(Tags.CashSettlAgentContactPhone, val) { }
    }
    public sealed class BidSpotRate : DecimalField
    {
        public BidSpotRate(): base(Tags.BidSpotRate) { }
        public BidSpotRate(decimal val): base(Tags.BidSpotRate, val) { }
    }
    public sealed class BidForwardPoints : DecimalField
    {
        public BidForwardPoints(): base(Tags.BidForwardPoints) { }
        public BidForwardPoints(decimal val): base(Tags.BidForwardPoints, val) { }
    }
    public sealed class OfferSpotRate : DecimalField
    {
        public OfferSpotRate(): base(Tags.OfferSpotRate) { }
        public OfferSpotRate(decimal val): base(Tags.OfferSpotRate, val) { }
    }
    public sealed class OfferForwardPoints : DecimalField
    {
        public OfferForwardPoints(): base(Tags.OfferForwardPoints) { }
        public OfferForwardPoints(decimal val): base(Tags.OfferForwardPoints, val) { }
    }
    public sealed class OrderQty2 : DecimalField
    {
        public OrderQty2(): base(Tags.OrderQty2) { }
        public OrderQty2(decimal val): base(Tags.OrderQty2, val) { }
    }
    public sealed class FutSettDate2 : StringField
    {
        public FutSettDate2(): base(Tags.FutSettDate2) { }
        public FutSettDate2(string val): base(Tags.FutSettDate2, val) { }
    }
    public sealed class LastSpotRate : DecimalField
    {
        public LastSpotRate(): base(Tags.LastSpotRate) { }
        public LastSpotRate(decimal val): base(Tags.LastSpotRate, val) { }
    }
    public sealed class LastForwardPoints : DecimalField
    {
        public LastForwardPoints(): base(Tags.LastForwardPoints) { }
        public LastForwardPoints(decimal val): base(Tags.LastForwardPoints, val) { }
    }
    public sealed class AllocLinkID : StringField
    {
        public AllocLinkID(): base(Tags.AllocLinkID) { }
        public AllocLinkID(string val): base(Tags.AllocLinkID, val) { }
    }
    public sealed class AllocLinkType : IntField
    {
        public AllocLinkType(): base(Tags.AllocLinkType) { }
        public AllocLinkType(int val): base(Tags.AllocLinkType, val) { }
        public const int FX_NETTING = 0;
        public const int FX_SWAP = 1;
    }
    public sealed class SecondaryOrderID : StringField
    {
        public SecondaryOrderID(): base(Tags.SecondaryOrderID) { }
        public SecondaryOrderID(string val): base(Tags.SecondaryOrderID, val) { }
    }
    public sealed class NoIOIQualifiers : IntField
    {
        public NoIOIQualifiers(): base(Tags.NoIOIQualifiers) { }
        public NoIOIQualifiers(int val): base(Tags.NoIOIQualifiers, val) { }
    }
    public sealed class MaturityMonthYear : StringField
    {
        public MaturityMonthYear(): base(Tags.MaturityMonthYear) { }
        public MaturityMonthYear(string val): base(Tags.MaturityMonthYear, val) { }
    }
    public sealed class PutOrCall : IntField
    {
        public PutOrCall(): base(Tags.PutOrCall) { }
        public PutOrCall(int val): base(Tags.PutOrCall, val) { }
        public const int PUT = 0;
        public const int CALL = 1;
    }
    public sealed class StrikePrice : DecimalField
    {
        public StrikePrice(): base(Tags.StrikePrice) { }
        public StrikePrice(decimal val): base(Tags.StrikePrice, val) { }
    }
    public sealed class CoveredOrUncovered : IntField
    {
        public CoveredOrUncovered(): base(Tags.CoveredOrUncovered) { }
        public CoveredOrUncovered(int val): base(Tags.CoveredOrUncovered, val) { }
        public const int COVERED = 0;
        public const int UNCOVERED = 1;
    }
    public sealed class CustomerOrFirm : IntField
    {
        public CustomerOrFirm(): base(Tags.CustomerOrFirm) { }
        public CustomerOrFirm(int val): base(Tags.CustomerOrFirm, val) { }
        public const int CUSTOMER = 0;
        public const int FIRM = 1;
    }
    public sealed class MaturityDay : StringField
    {
        public MaturityDay(): base(Tags.MaturityDay) { }
        public MaturityDay(string val): base(Tags.MaturityDay, val) { }
    }
    public sealed class OptAttribute : CharField
    {
        public OptAttribute(): base(Tags.OptAttribute) { }
        public OptAttribute(char val): base(Tags.OptAttribute, val) { }
    }
    public sealed class SecurityExchange : StringField
    {
        public SecurityExchange(): base(Tags.SecurityExchange) { }
        public SecurityExchange(string val): base(Tags.SecurityExchange, val) { }
    }
    public sealed class NotifyBrokerOfCredit : BooleanField
    {
        public NotifyBrokerOfCredit(): base(Tags.NotifyBrokerOfCredit) { }
        public NotifyBrokerOfCredit(bool val): base(Tags.NotifyBrokerOfCredit, val) { }
        public const bool DETAILS_SHOULD_BE_COMMUNICATED = true;
        public const bool DETAILS_SHOULD_NOT_BE_COMMUNICATED = false;
    }
    public sealed class AllocHandlInst : IntField
    {
        public AllocHandlInst(): base(Tags.AllocHandlInst) { }
        public AllocHandlInst(int val): base(Tags.AllocHandlInst, val) { }
        public const int MATCH = 1;
        public const int FORWARD = 2;
        public const int FORWARD_AND_MATCH = 3;
    }
    public sealed class MaxShow : DecimalField
    {
        public MaxShow(): base(Tags.MaxShow) { }
        public MaxShow(decimal val): base(Tags.MaxShow, val) { }
    }
    public sealed class PegDifference : DecimalField
    {
        public PegDifference(): base(Tags.PegDifference) { }
        public PegDifference(decimal val): base(Tags.PegDifference, val) { }
    }
    public sealed class XmlDataLen : IntField
    {
        public XmlDataLen(): base(Tags.XmlDataLen) { }
        public XmlDataLen(int val): base(Tags.XmlDataLen, val) { }
    }
    public sealed class XmlData : StringField
    {
        public XmlData(): base(Tags.XmlData) { }
        public XmlData(string val): base(Tags.XmlData, val) { }
    }
    public sealed class SettlInstRefID : StringField
    {
        public SettlInstRefID(): base(Tags.SettlInstRefID) { }
        public SettlInstRefID(string val): base(Tags.SettlInstRefID, val) { }
    }
    public sealed class NoRoutingIDs : IntField
    {
        public NoRoutingIDs(): base(Tags.NoRoutingIDs) { }
        public NoRoutingIDs(int val): base(Tags.NoRoutingIDs, val) { }
    }
    public sealed class RoutingType : IntField
    {
        public RoutingType(): base(Tags.RoutingType) { }
        public RoutingType(int val): base(Tags.RoutingType, val) { }
        public const int TARGET_FIRM = 1;
        public const int TARGET_LIST = 2;
        public const int BLOCK_FIRM = 3;
        public const int BLOCK_LIST = 4;
    }
    public sealed class RoutingID : StringField
    {
        public RoutingID(): base(Tags.RoutingID) { }
        public RoutingID(string val): base(Tags.RoutingID, val) { }
    }
    public sealed class SpreadToBenchmark : DecimalField
    {
        public SpreadToBenchmark(): base(Tags.SpreadToBenchmark) { }
        public SpreadToBenchmark(decimal val): base(Tags.SpreadToBenchmark, val) { }
    }
    public sealed class Benchmark : CharField
    {
        public Benchmark(): base(Tags.Benchmark) { }
        public Benchmark(char val): base(Tags.Benchmark, val) { }
        public const char CURVE = '1';
        public const char FIVEYR = '2';
        public const char OLD5 = '3';
        public const char TENYR = '4';
        public const char OLD10 = '5';
        public const char THIRTYYR = '6';
        public const char OLD30 = '7';
        public const char THREEMOLIBOR = '8';
        public const char SIXMOLIBOR = '9';
    }
    public sealed class CouponRate : DecimalField
    {
        public CouponRate(): base(Tags.CouponRate) { }
        public CouponRate(decimal val): base(Tags.CouponRate, val) { }
    }
    public sealed class ContractMultiplier : DecimalField
    {
        public ContractMultiplier(): base(Tags.ContractMultiplier) { }
        public ContractMultiplier(decimal val): base(Tags.ContractMultiplier, val) { }
    }
    public sealed class MDReqID : StringField
    {
        public MDReqID(): base(Tags.MDReqID) { }
        public MDReqID(string val): base(Tags.MDReqID, val) { }
    }
    public sealed class SubscriptionRequestType : CharField
    {
        public SubscriptionRequestType(): base(Tags.SubscriptionRequestType) { }
        public SubscriptionRequestType(char val): base(Tags.SubscriptionRequestType, val) { }
        public const char SNAPSHOT = '0';
        public const char SNAPSHOT_PLUS_UPDATES = '1';
        public const char DISABLE_PREVIOUS = '2';
    }
    public sealed class MarketDepth : IntField
    {
        public MarketDepth(): base(Tags.MarketDepth) { }
        public MarketDepth(int val): base(Tags.MarketDepth, val) { }
    }
    public sealed class MDUpdateType : IntField
    {
        public MDUpdateType(): base(Tags.MDUpdateType) { }
        public MDUpdateType(int val): base(Tags.MDUpdateType, val) { }
        public const int FULL_REFRESH = 0;
        public const int INCREMENTAL_REFRESH = 1;
    }
    public sealed class AggregatedBook : BooleanField
    {
        public AggregatedBook(): base(Tags.AggregatedBook) { }
        public AggregatedBook(bool val): base(Tags.AggregatedBook, val) { }
        public const bool ONE_BOOK_ENTRY_PER_SIDE_PER_PRICE = true;
        public const bool MULTIPLE_ENTRIES_PER_SIDE_PER_PRICE_ALLOWED = false;
    }
    public sealed class NoMDEntryTypes : IntField
    {
        public NoMDEntryTypes(): base(Tags.NoMDEntryTypes) { }
        public NoMDEntryTypes(int val): base(Tags.NoMDEntryTypes, val) { }
    }
    public sealed class NoMDEntries : IntField
    {
        public NoMDEntries(): base(Tags.NoMDEntries) { }
        public NoMDEntries(int val): base(Tags.NoMDEntries, val) { }
    }
    public sealed class MDEntryType : CharField
    {
        public MDEntryType(): base(Tags.MDEntryType) { }
        public MDEntryType(char val): base(Tags.MDEntryType, val) { }
        public const char BID = '0';
        public const char OFFER = '1';
        public const char TRADE = '2';
        public const char INDEX_VALUE = '3';
        public const char OPENING_PRICE = '4';
        public const char CLOSING_PRICE = '5';
        public const char SETTLEMENT_PRICE = '6';
        public const char TRADING_SESSION_HIGH_PRICE = '7';
        public const char TRADING_SESSION_LOW_PRICE = '8';
        public const char TRADING_SESSION_VWAP_PRICE = '9';
    }
    public sealed class MDEntryPx : DecimalField
    {
        public MDEntryPx(): base(Tags.MDEntryPx) { }
        public MDEntryPx(decimal val): base(Tags.MDEntryPx, val) { }
    }
    public sealed class MDEntrySize : DecimalField
    {
        public MDEntrySize(): base(Tags.MDEntrySize) { }
        public MDEntrySize(decimal val): base(Tags.MDEntrySize, val) { }
    }
    public sealed class MDEntryDate : DateOnlyField
    {
        public MDEntryDate(): base(Tags.MDEntryDate) { }
        public MDEntryDate(DateTime val): base(Tags.MDEntryDate, val) { }
    }
    public sealed class MDEntryTime : TimeOnlyField
    {
        public MDEntryTime(): base(Tags.MDEntryTime) { }
        public MDEntryTime(DateTime val): base(Tags.MDEntryTime, val) { }
        public MDEntryTime(DateTime val, bool showMilliseconds): base(Tags.MDEntryTime, val, showMilliseconds) { }
    }
    public sealed class TickDirection : CharField
    {
        public TickDirection(): base(Tags.TickDirection) { }
        public TickDirection(char val): base(Tags.TickDirection, val) { }
        public const char PLUS_TICK = '0';
        public const char ZEROPLUS_TICK = '1';
        public const char MINUS_TICK = '2';
        public const char ZEROMINUS_TICK = '3';
    }
    public sealed class MDMkt : StringField
    {
        public MDMkt(): base(Tags.MDMkt) { }
        public MDMkt(string val): base(Tags.MDMkt, val) { }
    }
    public sealed class QuoteCondition : StringField
    {
        public QuoteCondition(): base(Tags.QuoteCondition) { }
        public QuoteCondition(string val): base(Tags.QuoteCondition, val) { }
        public const string OPEN_ACTIVE = "A";
        public const string CLOSED_INACTIVE = "B";
        public const string EXCHANGE_BEST = "C";
        public const string CONSOLIDATED_BEST = "D";
        public const string LOCKED = "E";
        public const string CROSSED = "F";
        public const string DEPTH = "G";
        public const string FAST_TRADING = "H";
        public const string NONFIRM = "I";
    }
    public sealed class TradeCondition : StringField
    {
        public TradeCondition(): base(Tags.TradeCondition) { }
        public TradeCondition(string val): base(Tags.TradeCondition, val) { }
        public const string CASH = "A";
        public const string AVERAGE_PRICE_TRADE = "B";
        public const string CASH_TRADE = "C";
        public const string NEXT_DAY = "D";
        public const string OPENING_REOPENING_TRADE_DETAIL = "E";
        public const string INTRADAY_TRADE_DETAIL = "F";
        public const string RULE_127_TRADE = "G";
        public const string RULE_155_TRADE = "H";
        public const string SOLD_LAST = "I";
        public const string NEXT_DAY_TRADE = "J";
        public const string OPENED = "K";
        public const string SELLER = "L";
        public const string SOLD = "M";
        public const string STOPPED_STOCK = "N";
    }
    public sealed class MDEntryID : StringField
    {
        public MDEntryID(): base(Tags.MDEntryID) { }
        public MDEntryID(string val): base(Tags.MDEntryID, val) { }
    }
    public sealed class MDUpdateAction : CharField
    {
        public MDUpdateAction(): base(Tags.MDUpdateAction) { }
        public MDUpdateAction(char val): base(Tags.MDUpdateAction, val) { }
        public const char NEW = '0';
        public const char CHANGE = '1';
        public const char DELETE = '2';
    }
    public sealed class MDEntryRefID : StringField
    {
        public MDEntryRefID(): base(Tags.MDEntryRefID) { }
        public MDEntryRefID(string val): base(Tags.MDEntryRefID, val) { }
    }
    public sealed class MDReqRejReason : CharField
    {
        public MDReqRejReason(): base(Tags.MDReqRejReason) { }
        public MDReqRejReason(char val): base(Tags.MDReqRejReason, val) { }
        public const char UNKNOWN_SYMBOL = '0';
        public const char DUPLICATE_MDREQID = '1';
        public const char INSUFFICIENT_BANDWIDTH = '2';
        public const char INSUFFICIENT_PERMISSIONS = '3';
        public const char UNSUPPORTED_SUBSCRIPTIONREQUESTTYPE = '4';
        public const char UNSUPPORTED_MARKETDEPTH = '5';
        public const char UNSUPPORTED_MDUPDATETYPE = '6';
        public const char UNSUPPORTED_AGGREGATEDBOOK = '7';
        public const char UNSUPPORTED_MDENTRYTYPE = '8';
    }
    public sealed class MDEntryOriginator : StringField
    {
        public MDEntryOriginator(): base(Tags.MDEntryOriginator) { }
        public MDEntryOriginator(string val): base(Tags.MDEntryOriginator, val) { }
    }
    public sealed class LocationID : StringField
    {
        public LocationID(): base(Tags.LocationID) { }
        public LocationID(string val): base(Tags.LocationID, val) { }
    }
    public sealed class DeskID : StringField
    {
        public DeskID(): base(Tags.DeskID) { }
        public DeskID(string val): base(Tags.DeskID, val) { }
    }
    public sealed class DeleteReason : CharField
    {
        public DeleteReason(): base(Tags.DeleteReason) { }
        public DeleteReason(char val): base(Tags.DeleteReason, val) { }
        public const char CANCELATION_TRADE_BUST = '0';
        public const char ERROR = '1';
    }
    public sealed class OpenCloseSettleFlag : CharField
    {
        public OpenCloseSettleFlag(): base(Tags.OpenCloseSettleFlag) { }
        public OpenCloseSettleFlag(char val): base(Tags.OpenCloseSettleFlag, val) { }
        public const char DAILY_OPEN_CLOSE__SETTLEMENT_PRICE = '0';
        public const char SESSION_OPEN_CLOSE__SETTLEMENT_PRICE = '1';
        public const char DELIVERY_SETTLEMENT_PRICE = '2';
    }
    public sealed class SellerDays : IntField
    {
        public SellerDays(): base(Tags.SellerDays) { }
        public SellerDays(int val): base(Tags.SellerDays, val) { }
    }
    public sealed class MDEntryBuyer : StringField
    {
        public MDEntryBuyer(): base(Tags.MDEntryBuyer) { }
        public MDEntryBuyer(string val): base(Tags.MDEntryBuyer, val) { }
    }
    public sealed class MDEntrySeller : StringField
    {
        public MDEntrySeller(): base(Tags.MDEntrySeller) { }
        public MDEntrySeller(string val): base(Tags.MDEntrySeller, val) { }
    }
    public sealed class MDEntryPositionNo : IntField
    {
        public MDEntryPositionNo(): base(Tags.MDEntryPositionNo) { }
        public MDEntryPositionNo(int val): base(Tags.MDEntryPositionNo, val) { }
    }
    public sealed class FinancialStatus : CharField
    {
        public FinancialStatus(): base(Tags.FinancialStatus) { }
        public FinancialStatus(char val): base(Tags.FinancialStatus, val) { }
        public const char BANKRUPT = '1';
    }
    public sealed class CorporateAction : CharField
    {
        public CorporateAction(): base(Tags.CorporateAction) { }
        public CorporateAction(char val): base(Tags.CorporateAction, val) { }
        public const char EXDIVIDEND = 'A';
        public const char EXDISTRIBUTION = 'B';
        public const char EXRIGHTS = 'C';
        public const char NEW = 'D';
        public const char EXINTEREST = 'E';
    }
    public sealed class DefBidSize : DecimalField
    {
        public DefBidSize(): base(Tags.DefBidSize) { }
        public DefBidSize(decimal val): base(Tags.DefBidSize, val) { }
    }
    public sealed class DefOfferSize : DecimalField
    {
        public DefOfferSize(): base(Tags.DefOfferSize) { }
        public DefOfferSize(decimal val): base(Tags.DefOfferSize, val) { }
    }
    public sealed class NoQuoteEntries : IntField
    {
        public NoQuoteEntries(): base(Tags.NoQuoteEntries) { }
        public NoQuoteEntries(int val): base(Tags.NoQuoteEntries, val) { }
    }
    public sealed class NoQuoteSets : IntField
    {
        public NoQuoteSets(): base(Tags.NoQuoteSets) { }
        public NoQuoteSets(int val): base(Tags.NoQuoteSets, val) { }
    }
    public sealed class QuoteAckStatus : IntField
    {
        public QuoteAckStatus(): base(Tags.QuoteAckStatus) { }
        public QuoteAckStatus(int val): base(Tags.QuoteAckStatus, val) { }
    }
    public sealed class QuoteCancelType : IntField
    {
        public QuoteCancelType(): base(Tags.QuoteCancelType) { }
        public QuoteCancelType(int val): base(Tags.QuoteCancelType, val) { }
    }
    public sealed class QuoteEntryID : StringField
    {
        public QuoteEntryID(): base(Tags.QuoteEntryID) { }
        public QuoteEntryID(string val): base(Tags.QuoteEntryID, val) { }
    }
    public sealed class QuoteRejectReason : IntField
    {
        public QuoteRejectReason(): base(Tags.QuoteRejectReason) { }
        public QuoteRejectReason(int val): base(Tags.QuoteRejectReason, val) { }
        public const int UNKNOWN_SYMBOL = 1;
        public const int EXCHANGE = 2;
        public const int QUOTE_REQUEST_EXCEEDS_LIMIT = 3;
        public const int TOO_LATE_TO_ENTER = 4;
        public const int UNKNOWN_QUOTE = 5;
        public const int DUPLICATE_QUOTE_7 = 6;
        public const int INVALID_PRICE = 8;
        public const int NOT_AUTHORIZED_TO_QUOTE_SECURITY = 9;
    }
    public sealed class QuoteResponseLevel : IntField
    {
        public QuoteResponseLevel(): base(Tags.QuoteResponseLevel) { }
        public QuoteResponseLevel(int val): base(Tags.QuoteResponseLevel, val) { }
    }
    public sealed class QuoteSetID : StringField
    {
        public QuoteSetID(): base(Tags.QuoteSetID) { }
        public QuoteSetID(string val): base(Tags.QuoteSetID, val) { }
    }
    public sealed class QuoteRequestType : IntField
    {
        public QuoteRequestType(): base(Tags.QuoteRequestType) { }
        public QuoteRequestType(int val): base(Tags.QuoteRequestType, val) { }
    }
    public sealed class TotQuoteEntries : IntField
    {
        public TotQuoteEntries(): base(Tags.TotQuoteEntries) { }
        public TotQuoteEntries(int val): base(Tags.TotQuoteEntries, val) { }
    }
    public sealed class UnderlyingIDSource : StringField
    {
        public UnderlyingIDSource(): base(Tags.UnderlyingIDSource) { }
        public UnderlyingIDSource(string val): base(Tags.UnderlyingIDSource, val) { }
    }
    public sealed class UnderlyingIssuer : StringField
    {
        public UnderlyingIssuer(): base(Tags.UnderlyingIssuer) { }
        public UnderlyingIssuer(string val): base(Tags.UnderlyingIssuer, val) { }
    }
    public sealed class UnderlyingSecurityDesc : StringField
    {
        public UnderlyingSecurityDesc(): base(Tags.UnderlyingSecurityDesc) { }
        public UnderlyingSecurityDesc(string val): base(Tags.UnderlyingSecurityDesc, val) { }
    }
    public sealed class UnderlyingSecurityExchange : StringField
    {
        public UnderlyingSecurityExchange(): base(Tags.UnderlyingSecurityExchange) { }
        public UnderlyingSecurityExchange(string val): base(Tags.UnderlyingSecurityExchange, val) { }
    }
    public sealed class UnderlyingSecurityID : StringField
    {
        public UnderlyingSecurityID(): base(Tags.UnderlyingSecurityID) { }
        public UnderlyingSecurityID(string val): base(Tags.UnderlyingSecurityID, val) { }
    }
    public sealed class UnderlyingSecurityType : StringField
    {
        public UnderlyingSecurityType(): base(Tags.UnderlyingSecurityType) { }
        public UnderlyingSecurityType(string val): base(Tags.UnderlyingSecurityType, val) { }
    }
    public sealed class UnderlyingSymbol : StringField
    {
        public UnderlyingSymbol(): base(Tags.UnderlyingSymbol) { }
        public UnderlyingSymbol(string val): base(Tags.UnderlyingSymbol, val) { }
    }
    public sealed class UnderlyingSymbolSfx : StringField
    {
        public UnderlyingSymbolSfx(): base(Tags.UnderlyingSymbolSfx) { }
        public UnderlyingSymbolSfx(string val): base(Tags.UnderlyingSymbolSfx, val) { }
    }
    public sealed class UnderlyingMaturityMonthYear : StringField
    {
        public UnderlyingMaturityMonthYear(): base(Tags.UnderlyingMaturityMonthYear) { }
        public UnderlyingMaturityMonthYear(string val): base(Tags.UnderlyingMaturityMonthYear, val) { }
    }
    public sealed class UnderlyingMaturityDay : StringField
    {
        public UnderlyingMaturityDay(): base(Tags.UnderlyingMaturityDay) { }
        public UnderlyingMaturityDay(string val): base(Tags.UnderlyingMaturityDay, val) { }
    }
    public sealed class UnderlyingPutOrCall : IntField
    {
        public UnderlyingPutOrCall(): base(Tags.UnderlyingPutOrCall) { }
        public UnderlyingPutOrCall(int val): base(Tags.UnderlyingPutOrCall, val) { }
    }
    public sealed class UnderlyingStrikePrice : DecimalField
    {
        public UnderlyingStrikePrice(): base(Tags.UnderlyingStrikePrice) { }
        public UnderlyingStrikePrice(decimal val): base(Tags.UnderlyingStrikePrice, val) { }
    }
    public sealed class UnderlyingOptAttribute : CharField
    {
        public UnderlyingOptAttribute(): base(Tags.UnderlyingOptAttribute) { }
        public UnderlyingOptAttribute(char val): base(Tags.UnderlyingOptAttribute, val) { }
    }
    public sealed class UnderlyingCurrency : StringField
    {
        public UnderlyingCurrency(): base(Tags.UnderlyingCurrency) { }
        public UnderlyingCurrency(string val): base(Tags.UnderlyingCurrency, val) { }
    }
    public sealed class RatioQty : DecimalField
    {
        public RatioQty(): base(Tags.RatioQty) { }
        public RatioQty(decimal val): base(Tags.RatioQty, val) { }
    }
    public sealed class SecurityReqID : StringField
    {
        public SecurityReqID(): base(Tags.SecurityReqID) { }
        public SecurityReqID(string val): base(Tags.SecurityReqID, val) { }
    }
    public sealed class SecurityRequestType : IntField
    {
        public SecurityRequestType(): base(Tags.SecurityRequestType) { }
        public SecurityRequestType(int val): base(Tags.SecurityRequestType, val) { }
        public const int REQUEST_SECURITY_IDENTITY_AND_SPECIFICATIONS = 0;
        public const int REQUEST_SECURITY_IDENTITY_FOR_THE_SPECIFICATIONS_PROVIDED = 1;
        public const int REQUEST_LIST_SECURITY_TYPES = 2;
        public const int REQUEST_LIST_SECURITIES = 3;
    }
    public sealed class SecurityResponseID : StringField
    {
        public SecurityResponseID(): base(Tags.SecurityResponseID) { }
        public SecurityResponseID(string val): base(Tags.SecurityResponseID, val) { }
    }
    public sealed class SecurityResponseType : IntField
    {
        public SecurityResponseType(): base(Tags.SecurityResponseType) { }
        public SecurityResponseType(int val): base(Tags.SecurityResponseType, val) { }
        public const int ACCEPT_SECURITY_PROPOSAL_AS_IS = 1;
        public const int ACCEPT_SECURITY_PROPOSAL_WITH_REVISIONS_AS_INDICATED_IN_THE_MESSAGE = 2;
        public const int LIST_OF_SECURITY_TYPES_RETURNED_PER_REQUEST = 3;
        public const int LIST_OF_SECURITIES_RETURNED_PER_REQUEST = 4;
        public const int REJECT_SECURITY_PROPOSAL = 5;
        public const int CAN_NOT_MATCH_SELECTION_CRITERIA = 6;
    }
    public sealed class SecurityStatusReqID : StringField
    {
        public SecurityStatusReqID(): base(Tags.SecurityStatusReqID) { }
        public SecurityStatusReqID(string val): base(Tags.SecurityStatusReqID, val) { }
    }
    public sealed class UnsolicitedIndicator : BooleanField
    {
        public UnsolicitedIndicator(): base(Tags.UnsolicitedIndicator) { }
        public UnsolicitedIndicator(bool val): base(Tags.UnsolicitedIndicator, val) { }
        public const bool MESSAGE_IS_BEING_SENT_UNSOLICITED = true;
        public const bool MESSAGE_IS_BEING_SENT_AS_A_RESULT_OF_A_PRIOR_REQUEST = false;
    }
    public sealed class SecurityTradingStatus : IntField
    {
        public SecurityTradingStatus(): base(Tags.SecurityTradingStatus) { }
        public SecurityTradingStatus(int val): base(Tags.SecurityTradingStatus, val) { }
        public const int OPENING_DELAY = 1;
        public const int TRADING_HALT = 2;
        public const int RESUME = 3;
        public const int NO_OPENNO_RESUME = 4;
        public const int PRICE_INDICATION = 5;
        public const int TRADING_RANGE_INDICATION = 6;
        public const int MARKET_IMBALANCE_BUY = 7;
        public const int MARKET_IMBALANCE_SELL = 8;
        public const int MARKET_ON_CLOSE_IMBALANCE_BUY = 9;
        public const int MARKET_ON_CLOSE_IMBALANCE_SELL = 10;
        public const int NOT_ASSIGNED = 11;
        public const int NO_MARKET_IMBALANCE = 12;
        public const int NO_MARKET_ON_CLOSE_IMBALANCE = 13;
        public const int ITS_PREOPENING = 14;
        public const int NEW_PRICE_INDICATION = 15;
        public const int TRADE_DISSEMINATION_TIME = 16;
        public const int READY_TO_TRADE = 17;
        public const int NOT_AVAILABLE_FOR_TRADING = 18;
        public const int NOT_TRADED_ON_THIS_MARKET = 19;
        public const int UNKNOWN_OR_INVALID = 20;
    }
    public sealed class HaltReason : CharField
    {
        public HaltReason(): base(Tags.HaltReason) { }
        public HaltReason(char val): base(Tags.HaltReason, val) { }
        public const char ORDER_IMBALANCE = 'I';
        public const char EQUIPMENT_CHANGEOVER = 'X';
        public const char NEWS_PENDING = 'P';
        public const char NEWS_DISSEMINATION = 'D';
        public const char ORDER_INFLUX = 'E';
        public const char ADDITIONAL_INFORMATION = 'M';
    }
    public sealed class InViewOfCommon : BooleanField
    {
        public InViewOfCommon(): base(Tags.InViewOfCommon) { }
        public InViewOfCommon(bool val): base(Tags.InViewOfCommon, val) { }
        public const bool HALT_WAS_DUE_TO_COMMON_STOCK_BEING_HALTED = true;
        public const bool HALT_WAS_NOT_RELATED_TO_A_HALT_OF_THE_COMMON_STOCK = false;
    }
    public sealed class DueToRelated : BooleanField
    {
        public DueToRelated(): base(Tags.DueToRelated) { }
        public DueToRelated(bool val): base(Tags.DueToRelated, val) { }
        public const bool HALT_WAS_DUE_TO_RELATED_SECURITY_BEING_HALTED = true;
        public const bool HALT_WAS_NOT_RELATED_TO_A_HALT_OF_THE_RELATED_SECURITY = false;
    }
    public sealed class BuyVolume : DecimalField
    {
        public BuyVolume(): base(Tags.BuyVolume) { }
        public BuyVolume(decimal val): base(Tags.BuyVolume, val) { }
    }
    public sealed class SellVolume : DecimalField
    {
        public SellVolume(): base(Tags.SellVolume) { }
        public SellVolume(decimal val): base(Tags.SellVolume, val) { }
    }
    public sealed class HighPx : DecimalField
    {
        public HighPx(): base(Tags.HighPx) { }
        public HighPx(decimal val): base(Tags.HighPx, val) { }
    }
    public sealed class LowPx : DecimalField
    {
        public LowPx(): base(Tags.LowPx) { }
        public LowPx(decimal val): base(Tags.LowPx, val) { }
    }
    public sealed class Adjustment : IntField
    {
        public Adjustment(): base(Tags.Adjustment) { }
        public Adjustment(int val): base(Tags.Adjustment, val) { }
        public const int CANCEL = 1;
        public const int ERROR = 2;
        public const int CORRECTION = 3;
    }
    public sealed class TradSesReqID : StringField
    {
        public TradSesReqID(): base(Tags.TradSesReqID) { }
        public TradSesReqID(string val): base(Tags.TradSesReqID, val) { }
    }
    public sealed class TradingSessionID : StringField
    {
        public TradingSessionID(): base(Tags.TradingSessionID) { }
        public TradingSessionID(string val): base(Tags.TradingSessionID, val) { }
    }
    public sealed class ContraTrader : StringField
    {
        public ContraTrader(): base(Tags.ContraTrader) { }
        public ContraTrader(string val): base(Tags.ContraTrader, val) { }
    }
    public sealed class TradSesMethod : IntField
    {
        public TradSesMethod(): base(Tags.TradSesMethod) { }
        public TradSesMethod(int val): base(Tags.TradSesMethod, val) { }
        public const int ELECTRONIC = 1;
        public const int OPEN_OUTCRY = 2;
        public const int TWO_PARTY = 3;
    }
    public sealed class TradSesMode : IntField
    {
        public TradSesMode(): base(Tags.TradSesMode) { }
        public TradSesMode(int val): base(Tags.TradSesMode, val) { }
        public const int TESTING = 1;
        public const int SIMULATED = 2;
        public const int PRODUCTION = 3;
    }
    public sealed class TradSesStatus : IntField
    {
        public TradSesStatus(): base(Tags.TradSesStatus) { }
        public TradSesStatus(int val): base(Tags.TradSesStatus, val) { }
        public const int HALTED = 1;
        public const int OPEN = 2;
        public const int CLOSED = 3;
        public const int PREOPEN = 4;
        public const int PRECLOSE = 5;
    }
    public sealed class TradSesStartTime : DateTimeField
    {
        public TradSesStartTime(): base(Tags.TradSesStartTime) { }
        public TradSesStartTime(DateTime val): base(Tags.TradSesStartTime, val) { }
        public TradSesStartTime(DateTime val, bool showMilliseconds): base(Tags.TradSesStartTime, val, showMilliseconds) { }
    }
    public sealed class TradSesOpenTime : DateTimeField
    {
        public TradSesOpenTime(): base(Tags.TradSesOpenTime) { }
        public TradSesOpenTime(DateTime val): base(Tags.TradSesOpenTime, val) { }
        public TradSesOpenTime(DateTime val, bool showMilliseconds): base(Tags.TradSesOpenTime, val, showMilliseconds) { }
    }
    public sealed class TradSesPreCloseTime : DateTimeField
    {
        public TradSesPreCloseTime(): base(Tags.TradSesPreCloseTime) { }
        public TradSesPreCloseTime(DateTime val): base(Tags.TradSesPreCloseTime, val) { }
        public TradSesPreCloseTime(DateTime val, bool showMilliseconds): base(Tags.TradSesPreCloseTime, val, showMilliseconds) { }
    }
    public sealed class TradSesCloseTime : DateTimeField
    {
        public TradSesCloseTime(): base(Tags.TradSesCloseTime) { }
        public TradSesCloseTime(DateTime val): base(Tags.TradSesCloseTime, val) { }
        public TradSesCloseTime(DateTime val, bool showMilliseconds): base(Tags.TradSesCloseTime, val, showMilliseconds) { }
    }
    public sealed class TradSesEndTime : DateTimeField
    {
        public TradSesEndTime(): base(Tags.TradSesEndTime) { }
        public TradSesEndTime(DateTime val): base(Tags.TradSesEndTime, val) { }
        public TradSesEndTime(DateTime val, bool showMilliseconds): base(Tags.TradSesEndTime, val, showMilliseconds) { }
    }
    public sealed class NumberOfOrders : IntField
    {
        public NumberOfOrders(): base(Tags.NumberOfOrders) { }
        public NumberOfOrders(int val): base(Tags.NumberOfOrders, val) { }
    }
    public sealed class MessageEncoding : StringField
    {
        public MessageEncoding(): base(Tags.MessageEncoding) { }
        public MessageEncoding(string val): base(Tags.MessageEncoding, val) { }
    }
    public sealed class EncodedIssuerLen : IntField
    {
        public EncodedIssuerLen(): base(Tags.EncodedIssuerLen) { }
        public EncodedIssuerLen(int val): base(Tags.EncodedIssuerLen, val) { }
    }
    public sealed class EncodedIssuer : StringField
    {
        public EncodedIssuer(): base(Tags.EncodedIssuer) { }
        public EncodedIssuer(string val): base(Tags.EncodedIssuer, val) { }
    }
    public sealed class EncodedSecurityDescLen : IntField
    {
        public EncodedSecurityDescLen(): base(Tags.EncodedSecurityDescLen) { }
        public EncodedSecurityDescLen(int val): base(Tags.EncodedSecurityDescLen, val) { }
    }
    public sealed class EncodedSecurityDesc : StringField
    {
        public EncodedSecurityDesc(): base(Tags.EncodedSecurityDesc) { }
        public EncodedSecurityDesc(string val): base(Tags.EncodedSecurityDesc, val) { }
    }
    public sealed class EncodedListExecInstLen : IntField
    {
        public EncodedListExecInstLen(): base(Tags.EncodedListExecInstLen) { }
        public EncodedListExecInstLen(int val): base(Tags.EncodedListExecInstLen, val) { }
    }
    public sealed class EncodedListExecInst : StringField
    {
        public EncodedListExecInst(): base(Tags.EncodedListExecInst) { }
        public EncodedListExecInst(string val): base(Tags.EncodedListExecInst, val) { }
    }
    public sealed class EncodedTextLen : IntField
    {
        public EncodedTextLen(): base(Tags.EncodedTextLen) { }
        public EncodedTextLen(int val): base(Tags.EncodedTextLen, val) { }
    }
    public sealed class EncodedText : StringField
    {
        public EncodedText(): base(Tags.EncodedText) { }
        public EncodedText(string val): base(Tags.EncodedText, val) { }
    }
    public sealed class EncodedSubjectLen : IntField
    {
        public EncodedSubjectLen(): base(Tags.EncodedSubjectLen) { }
        public EncodedSubjectLen(int val): base(Tags.EncodedSubjectLen, val) { }
    }
    public sealed class EncodedSubject : StringField
    {
        public EncodedSubject(): base(Tags.EncodedSubject) { }
        public EncodedSubject(string val): base(Tags.EncodedSubject, val) { }
    }
    public sealed class EncodedHeadlineLen : IntField
    {
        public EncodedHeadlineLen(): base(Tags.EncodedHeadlineLen) { }
        public EncodedHeadlineLen(int val): base(Tags.EncodedHeadlineLen, val) { }
    }
    public sealed class EncodedHeadline : StringField
    {
        public EncodedHeadline(): base(Tags.EncodedHeadline) { }
        public EncodedHeadline(string val): base(Tags.EncodedHeadline, val) { }
    }
    public sealed class EncodedAllocTextLen : IntField
    {
        public EncodedAllocTextLen(): base(Tags.EncodedAllocTextLen) { }
        public EncodedAllocTextLen(int val): base(Tags.EncodedAllocTextLen, val) { }
    }
    public sealed class EncodedAllocText : StringField
    {
        public EncodedAllocText(): base(Tags.EncodedAllocText) { }
        public EncodedAllocText(string val): base(Tags.EncodedAllocText, val) { }
    }
    public sealed class EncodedUnderlyingIssuerLen : IntField
    {
        public EncodedUnderlyingIssuerLen(): base(Tags.EncodedUnderlyingIssuerLen) { }
        public EncodedUnderlyingIssuerLen(int val): base(Tags.EncodedUnderlyingIssuerLen, val) { }
    }
    public sealed class EncodedUnderlyingIssuer : StringField
    {
        public EncodedUnderlyingIssuer(): base(Tags.EncodedUnderlyingIssuer) { }
        public EncodedUnderlyingIssuer(string val): base(Tags.EncodedUnderlyingIssuer, val) { }
    }
    public sealed class EncodedUnderlyingSecurityDescLen : IntField
    {
        public EncodedUnderlyingSecurityDescLen(): base(Tags.EncodedUnderlyingSecurityDescLen) { }
        public EncodedUnderlyingSecurityDescLen(int val): base(Tags.EncodedUnderlyingSecurityDescLen, val) { }
    }
    public sealed class EncodedUnderlyingSecurityDesc : StringField
    {
        public EncodedUnderlyingSecurityDesc(): base(Tags.EncodedUnderlyingSecurityDesc) { }
        public EncodedUnderlyingSecurityDesc(string val): base(Tags.EncodedUnderlyingSecurityDesc, val) { }
    }
    public sealed class AllocPrice : DecimalField
    {
        public AllocPrice(): base(Tags.AllocPrice) { }
        public AllocPrice(decimal val): base(Tags.AllocPrice, val) { }
    }
    public sealed class QuoteSetValidUntilTime : DateTimeField
    {
        public QuoteSetValidUntilTime(): base(Tags.QuoteSetValidUntilTime) { }
        public QuoteSetValidUntilTime(DateTime val): base(Tags.QuoteSetValidUntilTime, val) { }
        public QuoteSetValidUntilTime(DateTime val, bool showMilliseconds): base(Tags.QuoteSetValidUntilTime, val, showMilliseconds) { }
    }
    public sealed class QuoteEntryRejectReason : IntField
    {
        public QuoteEntryRejectReason(): base(Tags.QuoteEntryRejectReason) { }
        public QuoteEntryRejectReason(int val): base(Tags.QuoteEntryRejectReason, val) { }
        public const int UNKNOWN_SYMBOL = 1;
        public const int EXCHANGE = 2;
        public const int QUOTE_EXCEEDS_LIMIT = 3;
        public const int TOO_LATE_TO_ENTER = 4;
        public const int UNKNOWN_QUOTE = 5;
        public const int DUPLICATE_QUOTE = 6;
        public const int INVALID_BIDASK_SPREAD = 7;
        public const int INVALID_PRICE = 8;
        public const int NOT_AUTHORIZED_TO_QUOTE_SECURITY = 9;
    }
    public sealed class LastMsgSeqNumProcessed : IntField
    {
        public LastMsgSeqNumProcessed(): base(Tags.LastMsgSeqNumProcessed) { }
        public LastMsgSeqNumProcessed(int val): base(Tags.LastMsgSeqNumProcessed, val) { }
    }
    public sealed class OnBehalfOfSendingTime : DateTimeField
    {
        public OnBehalfOfSendingTime(): base(Tags.OnBehalfOfSendingTime) { }
        public OnBehalfOfSendingTime(DateTime val): base(Tags.OnBehalfOfSendingTime, val) { }
        public OnBehalfOfSendingTime(DateTime val, bool showMilliseconds): base(Tags.OnBehalfOfSendingTime, val, showMilliseconds) { }
    }
    public sealed class RefTagID : IntField
    {
        public RefTagID(): base(Tags.RefTagID) { }
        public RefTagID(int val): base(Tags.RefTagID, val) { }
    }
    public sealed class RefMsgType : StringField
    {
        public RefMsgType(): base(Tags.RefMsgType) { }
        public RefMsgType(string val): base(Tags.RefMsgType, val) { }
    }
    public sealed class SessionRejectReason : IntField
    {
        public SessionRejectReason(): base(Tags.SessionRejectReason) { }
        public SessionRejectReason(int val): base(Tags.SessionRejectReason, val) { }
        public const int INVALID_TAG_NUMBER = 0;
        public const int REQUIRED_TAG_MISSING = 1;
        public const int TAG_NOT_DEFINED_FOR_THIS_MESSAGE_TYPE = 2;
        public const int UNDEFINED_TAG = 3;
        public const int TAG_SPECIFIED_WITHOUT_A_VALUE = 4;
        public const int VALUE_IS_INCORRECT = 5;
        public const int INCORRECT_DATA_FORMAT_FOR_VALUE = 6;
        public const int DECRYPTION_PROBLEM = 7;
        public const int SIGNATURE_PROBLEM = 8;
        public const int COMPID_PROBLEM = 9;
        public const int SENDINGTIME_ACCURACY_PROBLEM = 10;
        public const int E = 11;
    }
    public sealed class BidRequestTransType : CharField
    {
        public BidRequestTransType(): base(Tags.BidRequestTransType) { }
        public BidRequestTransType(char val): base(Tags.BidRequestTransType, val) { }
        public const char NEW = 'N';
        public const char CANCEL = 'C';
    }
    public sealed class ContraBroker : StringField
    {
        public ContraBroker(): base(Tags.ContraBroker) { }
        public ContraBroker(string val): base(Tags.ContraBroker, val) { }
    }
    public sealed class ComplianceID : StringField
    {
        public ComplianceID(): base(Tags.ComplianceID) { }
        public ComplianceID(string val): base(Tags.ComplianceID, val) { }
    }
    public sealed class SolicitedFlag : BooleanField
    {
        public SolicitedFlag(): base(Tags.SolicitedFlag) { }
        public SolicitedFlag(bool val): base(Tags.SolicitedFlag, val) { }
        public const bool WAS_SOLCITIED = true;
        public const bool WAS_NOT_SOLICITED = false;
    }
    public sealed class ExecRestatementReason : IntField
    {
        public ExecRestatementReason(): base(Tags.ExecRestatementReason) { }
        public ExecRestatementReason(int val): base(Tags.ExecRestatementReason, val) { }
        public const int GT_CORPORATE_ACTION = 0;
        public const int GT_RENEWAL_RESTATEMENT = 1;
        public const int VERBAL_CHANGE = 2;
        public const int REPRICING_OF_ORDER = 3;
        public const int BROKER_OPTION = 4;
        public const int PARTIAL_DECLINE_OF_ORDERQTY = 5;
    }
    public sealed class BusinessRejectRefID : StringField
    {
        public BusinessRejectRefID(): base(Tags.BusinessRejectRefID) { }
        public BusinessRejectRefID(string val): base(Tags.BusinessRejectRefID, val) { }
    }
    public sealed class BusinessRejectReason : IntField
    {
        public BusinessRejectReason(): base(Tags.BusinessRejectReason) { }
        public BusinessRejectReason(int val): base(Tags.BusinessRejectReason, val) { }
        public const int OTHER = 0;
        public const int UNKOWN_ID = 1;
        public const int UNKNOWN_SECURITY = 2;
        public const int UNSUPPORTED_MESSAGE_TYPE = 3;
        public const int APPLICATION_NOT_AVAILABLE = 4;
        public const int CONDITIONALLY_REQUIRED_FIELD_MISSING = 5;
    }
    public sealed class GrossTradeAmt : DecimalField
    {
        public GrossTradeAmt(): base(Tags.GrossTradeAmt) { }
        public GrossTradeAmt(decimal val): base(Tags.GrossTradeAmt, val) { }
    }
    public sealed class NoContraBrokers : IntField
    {
        public NoContraBrokers(): base(Tags.NoContraBrokers) { }
        public NoContraBrokers(int val): base(Tags.NoContraBrokers, val) { }
    }
    public sealed class MaxMessageSize : IntField
    {
        public MaxMessageSize(): base(Tags.MaxMessageSize) { }
        public MaxMessageSize(int val): base(Tags.MaxMessageSize, val) { }
    }
    public sealed class NoMsgTypes : IntField
    {
        public NoMsgTypes(): base(Tags.NoMsgTypes) { }
        public NoMsgTypes(int val): base(Tags.NoMsgTypes, val) { }
    }
    public sealed class MsgDirection : CharField
    {
        public MsgDirection(): base(Tags.MsgDirection) { }
        public MsgDirection(char val): base(Tags.MsgDirection, val) { }
        public const char SEND = 'S';
        public const char RECEIVE = 'R';
    }
    public sealed class NoTradingSessions : IntField
    {
        public NoTradingSessions(): base(Tags.NoTradingSessions) { }
        public NoTradingSessions(int val): base(Tags.NoTradingSessions, val) { }
    }
    public sealed class TotalVolumeTraded : DecimalField
    {
        public TotalVolumeTraded(): base(Tags.TotalVolumeTraded) { }
        public TotalVolumeTraded(decimal val): base(Tags.TotalVolumeTraded, val) { }
    }
    public sealed class DiscretionInst : CharField
    {
        public DiscretionInst(): base(Tags.DiscretionInst) { }
        public DiscretionInst(char val): base(Tags.DiscretionInst, val) { }
        public const char RELATED_TO_DISPLAYED_PRICE = '0';
        public const char RELATED_TO_MARKET_PRICE = '1';
        public const char RELATED_TO_PRIMARY_PRICE = '2';
        public const char RELATED_TO_LOCAL_PRIMARY_PRICE = '3';
        public const char RELATED_TO_MIDPOINT_PRICE = '4';
        public const char RELATED_TO_LAST_TRADE_PRICE = '5';
    }
    public sealed class DiscretionOffset : DecimalField
    {
        public DiscretionOffset(): base(Tags.DiscretionOffset) { }
        public DiscretionOffset(decimal val): base(Tags.DiscretionOffset, val) { }
    }
    public sealed class BidID : StringField
    {
        public BidID(): base(Tags.BidID) { }
        public BidID(string val): base(Tags.BidID, val) { }
    }
    public sealed class ClientBidID : StringField
    {
        public ClientBidID(): base(Tags.ClientBidID) { }
        public ClientBidID(string val): base(Tags.ClientBidID, val) { }
    }
    public sealed class ListName : StringField
    {
        public ListName(): base(Tags.ListName) { }
        public ListName(string val): base(Tags.ListName, val) { }
    }
    public sealed class TotalNumSecurities : IntField
    {
        public TotalNumSecurities(): base(Tags.TotalNumSecurities) { }
        public TotalNumSecurities(int val): base(Tags.TotalNumSecurities, val) { }
    }
    public sealed class BidType : IntField
    {
        public BidType(): base(Tags.BidType) { }
        public BidType(int val): base(Tags.BidType, val) { }
    }
    public sealed class NumTickets : IntField
    {
        public NumTickets(): base(Tags.NumTickets) { }
        public NumTickets(int val): base(Tags.NumTickets, val) { }
    }
    public sealed class SideValue1 : DecimalField
    {
        public SideValue1(): base(Tags.SideValue1) { }
        public SideValue1(decimal val): base(Tags.SideValue1, val) { }
    }
    public sealed class SideValue2 : DecimalField
    {
        public SideValue2(): base(Tags.SideValue2) { }
        public SideValue2(decimal val): base(Tags.SideValue2, val) { }
    }
    public sealed class NoBidDescriptors : IntField
    {
        public NoBidDescriptors(): base(Tags.NoBidDescriptors) { }
        public NoBidDescriptors(int val): base(Tags.NoBidDescriptors, val) { }
    }
    public sealed class BidDescriptorType : IntField
    {
        public BidDescriptorType(): base(Tags.BidDescriptorType) { }
        public BidDescriptorType(int val): base(Tags.BidDescriptorType, val) { }
    }
    public sealed class BidDescriptor : StringField
    {
        public BidDescriptor(): base(Tags.BidDescriptor) { }
        public BidDescriptor(string val): base(Tags.BidDescriptor, val) { }
    }
    public sealed class SideValueInd : IntField
    {
        public SideValueInd(): base(Tags.SideValueInd) { }
        public SideValueInd(int val): base(Tags.SideValueInd, val) { }
    }
    public sealed class LiquidityPctLow : DecimalField
    {
        public LiquidityPctLow(): base(Tags.LiquidityPctLow) { }
        public LiquidityPctLow(decimal val): base(Tags.LiquidityPctLow, val) { }
    }
    public sealed class LiquidityPctHigh : DecimalField
    {
        public LiquidityPctHigh(): base(Tags.LiquidityPctHigh) { }
        public LiquidityPctHigh(decimal val): base(Tags.LiquidityPctHigh, val) { }
    }
    public sealed class LiquidityValue : DecimalField
    {
        public LiquidityValue(): base(Tags.LiquidityValue) { }
        public LiquidityValue(decimal val): base(Tags.LiquidityValue, val) { }
    }
    public sealed class EFPTrackingError : DecimalField
    {
        public EFPTrackingError(): base(Tags.EFPTrackingError) { }
        public EFPTrackingError(decimal val): base(Tags.EFPTrackingError, val) { }
    }
    public sealed class FairValue : DecimalField
    {
        public FairValue(): base(Tags.FairValue) { }
        public FairValue(decimal val): base(Tags.FairValue, val) { }
    }
    public sealed class OutsideIndexPct : DecimalField
    {
        public OutsideIndexPct(): base(Tags.OutsideIndexPct) { }
        public OutsideIndexPct(decimal val): base(Tags.OutsideIndexPct, val) { }
    }
    public sealed class ValueOfFutures : DecimalField
    {
        public ValueOfFutures(): base(Tags.ValueOfFutures) { }
        public ValueOfFutures(decimal val): base(Tags.ValueOfFutures, val) { }
    }
    public sealed class LiquidityIndType : IntField
    {
        public LiquidityIndType(): base(Tags.LiquidityIndType) { }
        public LiquidityIndType(int val): base(Tags.LiquidityIndType, val) { }
    }
    public sealed class WtAverageLiquidity : DecimalField
    {
        public WtAverageLiquidity(): base(Tags.WtAverageLiquidity) { }
        public WtAverageLiquidity(decimal val): base(Tags.WtAverageLiquidity, val) { }
    }
    public sealed class ExchangeForPhysical : BooleanField
    {
        public ExchangeForPhysical(): base(Tags.ExchangeForPhysical) { }
        public ExchangeForPhysical(bool val): base(Tags.ExchangeForPhysical, val) { }
        public const bool TRUE = true;
        public const bool FALSE = false;
    }
    public sealed class OutMainCntryUIndex : DecimalField
    {
        public OutMainCntryUIndex(): base(Tags.OutMainCntryUIndex) { }
        public OutMainCntryUIndex(decimal val): base(Tags.OutMainCntryUIndex, val) { }
    }
    public sealed class CrossPercent : DecimalField
    {
        public CrossPercent(): base(Tags.CrossPercent) { }
        public CrossPercent(decimal val): base(Tags.CrossPercent, val) { }
    }
    public sealed class ProgRptReqs : IntField
    {
        public ProgRptReqs(): base(Tags.ProgRptReqs) { }
        public ProgRptReqs(int val): base(Tags.ProgRptReqs, val) { }
    }
    public sealed class ProgPeriodInterval : IntField
    {
        public ProgPeriodInterval(): base(Tags.ProgPeriodInterval) { }
        public ProgPeriodInterval(int val): base(Tags.ProgPeriodInterval, val) { }
    }
    public sealed class IncTaxInd : IntField
    {
        public IncTaxInd(): base(Tags.IncTaxInd) { }
        public IncTaxInd(int val): base(Tags.IncTaxInd, val) { }
    }
    public sealed class NumBidders : IntField
    {
        public NumBidders(): base(Tags.NumBidders) { }
        public NumBidders(int val): base(Tags.NumBidders, val) { }
    }
    public sealed class TradeType : CharField
    {
        public TradeType(): base(Tags.TradeType) { }
        public TradeType(char val): base(Tags.TradeType, val) { }
    }
    public sealed class BasisPxType : CharField
    {
        public BasisPxType(): base(Tags.BasisPxType) { }
        public BasisPxType(char val): base(Tags.BasisPxType, val) { }
    }
    public sealed class NoBidComponents : IntField
    {
        public NoBidComponents(): base(Tags.NoBidComponents) { }
        public NoBidComponents(int val): base(Tags.NoBidComponents, val) { }
    }
    public sealed class Country : StringField
    {
        public Country(): base(Tags.Country) { }
        public Country(string val): base(Tags.Country, val) { }
    }
    public sealed class TotNoStrikes : IntField
    {
        public TotNoStrikes(): base(Tags.TotNoStrikes) { }
        public TotNoStrikes(int val): base(Tags.TotNoStrikes, val) { }
    }
    public sealed class PriceType : IntField
    {
        public PriceType(): base(Tags.PriceType) { }
        public PriceType(int val): base(Tags.PriceType, val) { }
    }
    public sealed class DayOrderQty : DecimalField
    {
        public DayOrderQty(): base(Tags.DayOrderQty) { }
        public DayOrderQty(decimal val): base(Tags.DayOrderQty, val) { }
    }
    public sealed class DayCumQty : DecimalField
    {
        public DayCumQty(): base(Tags.DayCumQty) { }
        public DayCumQty(decimal val): base(Tags.DayCumQty, val) { }
    }
    public sealed class DayAvgPx : DecimalField
    {
        public DayAvgPx(): base(Tags.DayAvgPx) { }
        public DayAvgPx(decimal val): base(Tags.DayAvgPx, val) { }
    }
    public sealed class GTBookingInst : IntField
    {
        public GTBookingInst(): base(Tags.GTBookingInst) { }
        public GTBookingInst(int val): base(Tags.GTBookingInst, val) { }
        public const int BOOK_OUT_ALL_TRADES_ON_DAY_OF_EXECUTION = 0;
        public const int ACCUMULATE_EXECUTIONS_UNTIL_ORDER_IS_FILLED_OR_EXPIRES = 1;
        public const int ACCUMULATE_UNTIL_VERBALLY_NOTIFIED_OTHERWISE = 2;
    }
    public sealed class NoStrikes : IntField
    {
        public NoStrikes(): base(Tags.NoStrikes) { }
        public NoStrikes(int val): base(Tags.NoStrikes, val) { }
    }
    public sealed class ListStatusType : IntField
    {
        public ListStatusType(): base(Tags.ListStatusType) { }
        public ListStatusType(int val): base(Tags.ListStatusType, val) { }
    }
    public sealed class NetGrossInd : IntField
    {
        public NetGrossInd(): base(Tags.NetGrossInd) { }
        public NetGrossInd(int val): base(Tags.NetGrossInd, val) { }
    }
    public sealed class ListOrderStatus : IntField
    {
        public ListOrderStatus(): base(Tags.ListOrderStatus) { }
        public ListOrderStatus(int val): base(Tags.ListOrderStatus, val) { }
    }
    public sealed class ExpireDate : StringField
    {
        public ExpireDate(): base(Tags.ExpireDate) { }
        public ExpireDate(string val): base(Tags.ExpireDate, val) { }
    }
    public sealed class ListExecInstType : CharField
    {
        public ListExecInstType(): base(Tags.ListExecInstType) { }
        public ListExecInstType(char val): base(Tags.ListExecInstType, val) { }
    }
    public sealed class CxlRejResponseTo : CharField
    {
        public CxlRejResponseTo(): base(Tags.CxlRejResponseTo) { }
        public CxlRejResponseTo(char val): base(Tags.CxlRejResponseTo, val) { }
    }
    public sealed class UnderlyingCouponRate : DecimalField
    {
        public UnderlyingCouponRate(): base(Tags.UnderlyingCouponRate) { }
        public UnderlyingCouponRate(decimal val): base(Tags.UnderlyingCouponRate, val) { }
    }
    public sealed class UnderlyingContractMultiplier : DecimalField
    {
        public UnderlyingContractMultiplier(): base(Tags.UnderlyingContractMultiplier) { }
        public UnderlyingContractMultiplier(decimal val): base(Tags.UnderlyingContractMultiplier, val) { }
    }
    public sealed class ContraTradeQty : DecimalField
    {
        public ContraTradeQty(): base(Tags.ContraTradeQty) { }
        public ContraTradeQty(decimal val): base(Tags.ContraTradeQty, val) { }
    }
    public sealed class ContraTradeTime : DateTimeField
    {
        public ContraTradeTime(): base(Tags.ContraTradeTime) { }
        public ContraTradeTime(DateTime val): base(Tags.ContraTradeTime, val) { }
        public ContraTradeTime(DateTime val, bool showMilliseconds): base(Tags.ContraTradeTime, val, showMilliseconds) { }
    }
    public sealed class ClearingFirm : StringField
    {
        public ClearingFirm(): base(Tags.ClearingFirm) { }
        public ClearingFirm(string val): base(Tags.ClearingFirm, val) { }
    }
    public sealed class ClearingAccount : StringField
    {
        public ClearingAccount(): base(Tags.ClearingAccount) { }
        public ClearingAccount(string val): base(Tags.ClearingAccount, val) { }
    }
    public sealed class LiquidityNumSecurities : IntField
    {
        public LiquidityNumSecurities(): base(Tags.LiquidityNumSecurities) { }
        public LiquidityNumSecurities(int val): base(Tags.LiquidityNumSecurities, val) { }
    }
    public sealed class MultiLegReportingType : CharField
    {
        public MultiLegReportingType(): base(Tags.MultiLegReportingType) { }
        public MultiLegReportingType(char val): base(Tags.MultiLegReportingType, val) { }
    }
    public sealed class StrikeTime : DateTimeField
    {
        public StrikeTime(): base(Tags.StrikeTime) { }
        public StrikeTime(DateTime val): base(Tags.StrikeTime, val) { }
        public StrikeTime(DateTime val, bool showMilliseconds): base(Tags.StrikeTime, val, showMilliseconds) { }
    }
    public sealed class ListStatusText : StringField
    {
        public ListStatusText(): base(Tags.ListStatusText) { }
        public ListStatusText(string val): base(Tags.ListStatusText, val) { }
    }
    public sealed class EncodedListStatusTextLen : IntField
    {
        public EncodedListStatusTextLen(): base(Tags.EncodedListStatusTextLen) { }
        public EncodedListStatusTextLen(int val): base(Tags.EncodedListStatusTextLen, val) { }
    }
    public sealed class EncodedListStatusText : StringField
    {
        public EncodedListStatusText(): base(Tags.EncodedListStatusText) { }
        public EncodedListStatusText(string val): base(Tags.EncodedListStatusText, val) { }
    }
}

