using QuickFix;

namespace LionFire.Trading.QuickFix.ConsoleTest
{
    public class MessageFactory : IMessageFactory
    {
        public QuickFix.Message Create(string beginString, string msgType)
        {
            switch(msgType)
            {
                case Heartbeat.MsgType: return new Heartbeat();
                case Logon.MsgType: return new Logon();
                case TestRequest.MsgType: return new TestRequest();
                case ResendRequest.MsgType: return new ResendRequest();
                case Reject.MsgType: return new Reject();
                case SequenceReset.MsgType: return new SequenceReset();
                case Logout.MsgType: return new Logout();
                case Advertisement.MsgType: return new Advertisement();
                case IndicationofInterest.MsgType: return new IndicationofInterest();
                case News.MsgType: return new News();
                case Email.MsgType: return new Email();
                case QuoteRequest.MsgType: return new QuoteRequest();
                case Quote.MsgType: return new Quote();
                case MassQuote.MsgType: return new MassQuote();
                case QuoteCancel.MsgType: return new QuoteCancel();
                case QuoteStatusRequest.MsgType: return new QuoteStatusRequest();
                case QuoteAcknowledgement.MsgType: return new QuoteAcknowledgement();
                case MarketDataRequest.MsgType: return new MarketDataRequest();
                case MarketDataSnapshotFullRefresh.MsgType: return new MarketDataSnapshotFullRefresh();
                case MarketDataIncrementalRefresh.MsgType: return new MarketDataIncrementalRefresh();
                case MarketDataRequestReject.MsgType: return new MarketDataRequestReject();
                case SecurityDefinitionRequest.MsgType: return new SecurityDefinitionRequest();
                case SecurityDefinition.MsgType: return new SecurityDefinition();
                case SecurityStatusRequest.MsgType: return new SecurityStatusRequest();
                case SecurityStatus.MsgType: return new SecurityStatus();
                case TradingSessionStatusRequest.MsgType: return new TradingSessionStatusRequest();
                case TradingSessionStatus.MsgType: return new TradingSessionStatus();
                case NewOrderSingle.MsgType: return new NewOrderSingle();
                case ExecutionReport.MsgType: return new ExecutionReport();
                case DontKnowTrade.MsgType: return new DontKnowTrade();
                case OrderCancelReplaceRequest.MsgType: return new OrderCancelReplaceRequest();
                case OrderCancelRequest.MsgType: return new OrderCancelRequest();
                case OrderCancelReject.MsgType: return new OrderCancelReject();
                case OrderStatusRequest.MsgType: return new OrderStatusRequest();
                case Allocation.MsgType: return new Allocation();
                case AllocationACK.MsgType: return new AllocationACK();
                case SettlementInstructions.MsgType: return new SettlementInstructions();
                case BidRequest.MsgType: return new BidRequest();
                case BidResponse.MsgType: return new BidResponse();
                case NewOrderList.MsgType: return new NewOrderList();
                case ListStrikePrice.MsgType: return new ListStrikePrice();
                case ListStatus.MsgType: return new ListStatus();
                case ListExecute.MsgType: return new ListExecute();
                case ListCancelRequest.MsgType: return new ListCancelRequest();
                case ListStatusRequest.MsgType: return new ListStatusRequest();
                case BusinessMessageReject.MsgType: return new BusinessMessageReject();
            }

            return new QuickFix.Message();
        }

        public Group Create(string beginString, string msgType, int correspondingFieldID)
        {
          if(Logon.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoMsgTypes: return new LionFire.Trading.QuickFix.ConsoleTest.Logon.NoMsgTypesGroup();
              }
          }
          if(IndicationofInterest.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoIOIQualifiers: return new LionFire.Trading.QuickFix.ConsoleTest.IndicationofInterest.NoIOIQualifiersGroup();
					case QuickFix.Fields.Tags.NoRoutingIDs: return new LionFire.Trading.QuickFix.ConsoleTest.IndicationofInterest.NoRoutingIDsGroup();
              }
          }
          if(News.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoRoutingIDs: return new LionFire.Trading.QuickFix.ConsoleTest.News.NoRoutingIDsGroup();
					case QuickFix.Fields.Tags.NoRelatedSym: return new LionFire.Trading.QuickFix.ConsoleTest.News.NoRelatedSymGroup();
					case QuickFix.Fields.Tags.LinesOfText: return new LionFire.Trading.QuickFix.ConsoleTest.News.LinesOfTextGroup();
              }
          }
          if(Email.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoRoutingIDs: return new LionFire.Trading.QuickFix.ConsoleTest.Email.NoRoutingIDsGroup();
					case QuickFix.Fields.Tags.NoRelatedSym: return new LionFire.Trading.QuickFix.ConsoleTest.Email.NoRelatedSymGroup();
					case QuickFix.Fields.Tags.LinesOfText: return new LionFire.Trading.QuickFix.ConsoleTest.Email.LinesOfTextGroup();
              }
          }
          if(QuoteRequest.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoRelatedSym: return new LionFire.Trading.QuickFix.ConsoleTest.QuoteRequest.NoRelatedSymGroup();
              }
          }
          if(MassQuote.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoQuoteSets: return new LionFire.Trading.QuickFix.ConsoleTest.MassQuote.NoQuoteSetsGroup();
					case QuickFix.Fields.Tags.NoQuoteEntries: return new LionFire.Trading.QuickFix.ConsoleTest.MassQuote.NoQuoteSetsGroup.NoQuoteEntriesGroup();
              }
          }
          if(QuoteCancel.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoQuoteEntries: return new LionFire.Trading.QuickFix.ConsoleTest.QuoteCancel.NoQuoteEntriesGroup();
              }
          }
          if(QuoteAcknowledgement.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoQuoteSets: return new LionFire.Trading.QuickFix.ConsoleTest.QuoteAcknowledgement.NoQuoteSetsGroup();
					case QuickFix.Fields.Tags.NoQuoteEntries: return new LionFire.Trading.QuickFix.ConsoleTest.QuoteAcknowledgement.NoQuoteSetsGroup.NoQuoteEntriesGroup();
              }
          }
          if(MarketDataRequest.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoMDEntryTypes: return new LionFire.Trading.QuickFix.ConsoleTest.MarketDataRequest.NoMDEntryTypesGroup();
					case QuickFix.Fields.Tags.NoRelatedSym: return new LionFire.Trading.QuickFix.ConsoleTest.MarketDataRequest.NoRelatedSymGroup();
              }
          }
          if(MarketDataSnapshotFullRefresh.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoMDEntries: return new LionFire.Trading.QuickFix.ConsoleTest.MarketDataSnapshotFullRefresh.NoMDEntriesGroup();
              }
          }
          if(MarketDataIncrementalRefresh.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoMDEntries: return new LionFire.Trading.QuickFix.ConsoleTest.MarketDataIncrementalRefresh.NoMDEntriesGroup();
              }
          }
          if(SecurityDefinitionRequest.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoRelatedSym: return new LionFire.Trading.QuickFix.ConsoleTest.SecurityDefinitionRequest.NoRelatedSymGroup();
              }
          }
          if(SecurityDefinition.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoRelatedSym: return new LionFire.Trading.QuickFix.ConsoleTest.SecurityDefinition.NoRelatedSymGroup();
              }
          }
          if(NewOrderSingle.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoAllocs: return new LionFire.Trading.QuickFix.ConsoleTest.NewOrderSingle.NoAllocsGroup();
					case QuickFix.Fields.Tags.NoTradingSessions: return new LionFire.Trading.QuickFix.ConsoleTest.NewOrderSingle.NoTradingSessionsGroup();
              }
          }
          if(ExecutionReport.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoContraBrokers: return new LionFire.Trading.QuickFix.ConsoleTest.ExecutionReport.NoContraBrokersGroup();
              }
          }
          if(OrderCancelReplaceRequest.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoAllocs: return new LionFire.Trading.QuickFix.ConsoleTest.OrderCancelReplaceRequest.NoAllocsGroup();
					case QuickFix.Fields.Tags.NoTradingSessions: return new LionFire.Trading.QuickFix.ConsoleTest.OrderCancelReplaceRequest.NoTradingSessionsGroup();
              }
          }
          if(Allocation.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoOrders: return new LionFire.Trading.QuickFix.ConsoleTest.Allocation.NoOrdersGroup();
					case QuickFix.Fields.Tags.NoExecs: return new LionFire.Trading.QuickFix.ConsoleTest.Allocation.NoExecsGroup();
					case QuickFix.Fields.Tags.NoAllocs: return new LionFire.Trading.QuickFix.ConsoleTest.Allocation.NoAllocsGroup();
					case QuickFix.Fields.Tags.NoMiscFees: return new LionFire.Trading.QuickFix.ConsoleTest.Allocation.NoAllocsGroup.NoMiscFeesGroup();
              }
          }
          if(BidRequest.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoBidDescriptors: return new LionFire.Trading.QuickFix.ConsoleTest.BidRequest.NoBidDescriptorsGroup();
					case QuickFix.Fields.Tags.NoBidComponents: return new LionFire.Trading.QuickFix.ConsoleTest.BidRequest.NoBidComponentsGroup();
              }
          }
          if(BidResponse.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoBidComponents: return new LionFire.Trading.QuickFix.ConsoleTest.BidResponse.NoBidComponentsGroup();
              }
          }
          if(NewOrderList.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoOrders: return new LionFire.Trading.QuickFix.ConsoleTest.NewOrderList.NoOrdersGroup();
					case QuickFix.Fields.Tags.NoAllocs: return new LionFire.Trading.QuickFix.ConsoleTest.NewOrderList.NoOrdersGroup.NoAllocsGroup();
					case QuickFix.Fields.Tags.NoTradingSessions: return new LionFire.Trading.QuickFix.ConsoleTest.NewOrderList.NoOrdersGroup.NoTradingSessionsGroup();
              }
          }
          if(ListStrikePrice.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoStrikes: return new LionFire.Trading.QuickFix.ConsoleTest.ListStrikePrice.NoStrikesGroup();
              }
          }
          if(ListStatus.MsgType.Equals(msgType))
          {
              switch(correspondingFieldID)
              {
              case QuickFix.Fields.Tags.NoOrders: return new LionFire.Trading.QuickFix.ConsoleTest.ListStatus.NoOrdersGroup();
              }
          }
            return null;
        }
    }
}
