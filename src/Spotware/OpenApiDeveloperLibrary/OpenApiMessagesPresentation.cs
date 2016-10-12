using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenApiLib;
using Google.ProtocolBuffers;

namespace OpenApiDeveloperLibrary
{
    public class OpenApiMessagesPresentation
    {
        static string ProtoMessageToString(ProtoMessage msg)
        {
            if (!msg.HasPayload)
                return "ERROR in ProtoMessage: Corrupted execution event, no payload found";
            var _str = "ProtoMessage{";
            switch ((ProtoPayloadType)msg.PayloadType)
            {
                case ProtoPayloadType.PROTO_MESSAGE:
                    var _msg = ProtoMessage.CreateBuilder().MergeFrom(msg.Payload).Build();
                    _str += ProtoMessageToString(_msg);
                    break;
                case ProtoPayloadType.PING_REQ:
                    var _ping_req = ProtoPingReq.CreateBuilder().MergeFrom(msg.Payload).Build();
                    _str += "PingRequest{timestamp:" + _ping_req.Timestamp.ToString() + "}";
                    break;
                case ProtoPayloadType.PING_RES:
                    var _ping_res = ProtoPingRes.CreateBuilder().MergeFrom(msg.Payload).Build();
                    _str += "PingResponse{timestamp:" + _ping_res.Timestamp + "}";
                    break;
                case ProtoPayloadType.HEARTBEAT_EVENT:
                    var _hb = ProtoHeartbeatEvent.CreateBuilder().MergeFrom(msg.Payload).Build();
                    _str += "Heartbeat";
                    break;
                case ProtoPayloadType.ERROR_RES:
                    var _err = ProtoErrorRes.CreateBuilder().MergeFrom(msg.Payload).Build();
                    _str += "ErrorResponse{errorCode:" + _err.ErrorCode + (_err.HasDescription ? ", description:" + _err.Description : "") + "}";
                    break;
                default:
                    _str += OpenApiMessageToString(msg);
                    break;
            }
            _str += (msg.HasClientMsgId ? ", clientMsgId:" + msg.ClientMsgId : "") + (msg.HasPayloadString ? ", payloadString:" + msg.PayloadString : "") + "}";

            return _str;
        }
        static string OpenApiMessageToString(ProtoMessage msg)
        {
            switch ((ProtoOAPayloadType)msg.PayloadType)
            {
                case ProtoOAPayloadType.OA_AUTH_REQ:
                    var _auth_req = ProtoOAAuthReq.CreateBuilder().MergeFrom(msg.Payload).Build();
                    return "AuthRequest{clientId:" + _auth_req.ClientId + ", clientSecret:" + _auth_req.ClientSecret + "}";
                case ProtoOAPayloadType.OA_AUTH_RES:
                    return "AuthResponse";
                case ProtoOAPayloadType.OA_GET_SUBSCRIBED_ACCOUNTS_REQ:
                    return "GetSubscribedAccountsRequest";
                case ProtoOAPayloadType.OA_GET_SUBSCRIBED_ACCOUNTS_RES:
                    var _subscr_res = ProtoOAGetSubscribedAccountsRes.CreateBuilder().MergeFrom(msg.Payload).Build();
                    var _subscr_res_str = "GetSubscribedAccountsResponse{";
                    var _subscr_count = _subscr_res.AccountIdCount;
                    foreach (var accountId in _subscr_res.AccountIdList)
                        _subscr_res_str += "accountId:" + accountId.ToString() + (--_subscr_count == 0 ? "" : ", ");
                    return _subscr_res_str + "}";
                case ProtoOAPayloadType.OA_SUBSCRIBE_FOR_TRADING_EVENTS_REQ:
                    var _subscr_req = ProtoOASubscribeForTradingEventsReq.CreateBuilder().MergeFrom(msg.Payload).Build();
                    return "SubscrbeTradingEventsRequest{accountId:" + _subscr_req.AccountId.ToString() + ", accessToken:" + _subscr_req.AccessToken + "}";
                case ProtoOAPayloadType.OA_SUBSCRIBE_FOR_TRADING_EVENTS_RES:
                    return "SubscrbeTradingEventsResponse";
                case ProtoOAPayloadType.OA_UNSUBSCRIBE_FROM_TRADING_EVENTS_REQ:
                    var _unsubscr_req = ProtoOAUnsubscribeFromTradingEventsReq.CreateBuilder().MergeFrom(msg.Payload).Build();
                    return "UnsubscrbeTradingEventsRequest{accountId:" + _unsubscr_req.AccountId + "}";
                case ProtoOAPayloadType.OA_UNSUBSCRIBE_FROM_TRADING_EVENTS_RES:
                    return "UnsubscrbeTradingEventsResponse";
                case ProtoOAPayloadType.OA_EXECUTION_EVENT:
                    return OpenApiExecEventsToString(msg);
                case ProtoOAPayloadType.OA_CANCEL_ORDER_REQ:
                    return "CancelOrderRequest{}";
                case ProtoOAPayloadType.OA_CREATE_ORDER_REQ:
                    return "CreateOrderRequest{}";
                case ProtoOAPayloadType.OA_CLOSE_POSITION_REQ:
                    return "ClosePositionRequest{}";
                case ProtoOAPayloadType.OA_AMEND_ORDER_REQ:
                    return "AmendOrderRequest{}";
                case ProtoOAPayloadType.OA_AMEND_POSITION_SL_TP_REQ:
                    return "AmendPositionRequest{}";
                case ProtoOAPayloadType.OA_SUBSCRIBE_FOR_SPOTS_REQ:
                    return "SubscribeForSpotsRequest{}";
                case ProtoOAPayloadType.OA_SUBSCRIBE_FOR_SPOTS_RES:
                    return "SubscribeForSpotsResponse{}";
                case ProtoOAPayloadType.OA_UNSUBSCRIBE_FROM_SPOTS_REQ:
                    return "UnsubscribeFromSpotsRequest{}";
                case ProtoOAPayloadType.OA_UNSUBSCRIBE_FROM_SPOTS_RES:
                    return "UnsubscribeFromSpotsResponse{}";
                case ProtoOAPayloadType.OA_GET_SPOT_SUBSCRIPTION_REQ:
                    return "GetSpotSubscriptionRequest{}";
                case ProtoOAPayloadType.OA_GET_SPOT_SUBSCRIPTION_RES:
                    return "GetSpotSubscriptionResponse{}";
                case ProtoOAPayloadType.OA_GET_ALL_SPOT_SUBSCRIPTIONS_REQ:
                    return "GetAllSpotSubscriptionsRequest{}";
                case ProtoOAPayloadType.OA_GET_ALL_SPOT_SUBSCRIPTIONS_RES:
                    String _all_str = "GetAllSpotSubscriptionsResponse{";
                     ProtoOAGetAllSpotSubscriptionsRes _all_res = ProtoOAGetAllSpotSubscriptionsRes.CreateBuilder().MergeFrom(msg.Payload).Build();
                     _all_str += "subscriptions=[";
                     foreach (ProtoOASpotSubscription subscription in _all_res.SpotSubscriptionsList) {
                        _all_str += "{AccountId=" + subscription.AccountId + ", SubscriptionId=" + subscription.SubscriptionId + ", SymbolNamesList=[";
                        foreach (String symbolName in subscription.SymbolNamesList) {
                            _all_str += symbolName + ", ";
                        }
                        _all_str += "]}, ";
                        }
                    _all_str += "]}";
                    return _all_str;
                case ProtoOAPayloadType.OA_SPOT_EVENT:
                    var _spot_event = ProtoOASpotEvent.CreateBuilder().MergeFrom(msg.Payload).Build();
                    return "SpotEvent{subscriptionId:" + _spot_event.SubscriptionId + ", symbolName:" + _spot_event.SymbolName + ", bidPrice:" + (_spot_event.HasBidPrice ? _spot_event.BidPrice.ToString() : "       ") + ", askPrice:" + (_spot_event.HasAskPrice ? _spot_event.AskPrice.ToString() : "       ") + "}";
                default:
                    return "unknown";
            }
        }
        static string OpenApiExecutionTypeToString(ProtoOAExecutionType executionType)
        {
            switch (executionType)
            {
                case ProtoOAExecutionType.OA_ORDER_ACCEPTED:
                    return "OrderAccepted";
                case ProtoOAExecutionType.OA_ORDER_AMENDED:
                    return "OrderAmended";
                case ProtoOAExecutionType.OA_ORDER_CANCEL_REJECTED:
                    return "OrderCancelRejected";
                case ProtoOAExecutionType.OA_ORDER_CANCELLED:
                    return "OrderCancelled";
                case ProtoOAExecutionType.OA_ORDER_EXPIRED:
                    return "OrderExpired";
                case ProtoOAExecutionType.OA_ORDER_FILLED:
                    return "OrderFilled";
                case ProtoOAExecutionType.OA_ORDER_REJECTED:
                    return "OrderRejected";
                default:
                    return "unknown";
            }
        }
        static string OpenApiExecEventsToString(ProtoMessage msg)
        {
            if ((ProtoOAPayloadType)msg.PayloadType != ProtoOAPayloadType.OA_EXECUTION_EVENT)
                return "ERROR in OpenApiExecutionEvents: Wrong message type";

            if (!msg.HasPayload)
                return "ERROR in OpenApiExecutionEvents: Corrupted execution event, no payload found";

            var _msg = ProtoOAExecutionEvent.CreateBuilder().MergeFrom(msg.Payload).Build();
            var _str = OpenApiExecutionTypeToString(_msg.ExecutionType) + "{" +
                OpenApiOrderToString(_msg.Order) +
                (_msg.HasPosition ? ", " + OpenApiPositionToString(_msg.Position) : "") +
                (_msg.HasReasonCode ? ", reasonCode:" + _msg.ReasonCode : "");

            return _str + "}";
        }
        static public string OpenApiOrderTypeToString(ProtoOAOrderType orderType)
        {
            switch (orderType)
            {
                case ProtoOAOrderType.OA_LIMIT:
                    return "LIMIT";
                case ProtoOAOrderType.OA_MARKET:
                    return "MARKET";
                case ProtoOAOrderType.OA_MARKET_RANGE:
                    return "MARKET RANGE";
                case ProtoOAOrderType.OA_PROTECTION:
                    return "PROTECTION";
                case ProtoOAOrderType.OA_STOP:
                    return "STOP";
                default:
                    return "unknown";
            }
        }
        static public string TradeSideToString(ProtoTradeSide tradeSide)
        {
            switch (tradeSide)
            {
                case ProtoTradeSide.BUY:
                    return "BUY";
                case ProtoTradeSide.SELL:
                    return "SELL";
                default:
                    return "unknown";
            }
        }
        static public string OpenApiOrderToString(ProtoOAOrder order)
        {
            var _str = "Order{orderId:" + order.OrderId.ToString() + ", accountId:" + order.AccountId + ", orderType:" + OpenApiOrderTypeToString(order.OrderType);
            _str += ", tradeSide:" + TradeSideToString(order.TradeSide);
            _str += ", symbolName:" + order.SymbolName + ", requestedVolume:" + order.RequestedVolume.ToString() + ", executedVolume:" + order.ExecutedVolume.ToString() + ", closingOrder:" +
                (order.ClosingOrder ? "TRUE" : "FALSE") +
                (order.HasExecutionPrice ? ", executionPrice:" + order.ExecutionPrice.ToString() : "") +
                (order.HasLimitPrice ? ", limitPrice:" + order.LimitPrice.ToString() : "") +
                (order.HasStopPrice ? ", stopPrice:" + order.StopPrice.ToString() : "") +
                (order.HasStopLossPrice ? ", stopLossPrice:" + order.StopLossPrice.ToString() : "") +
                (order.HasTakeProfitPrice ? ", takeProfitPrice:" + order.TakeProfitPrice.ToString() : "") +
                (order.HasBaseSlippagePrice ? ", baseSlippagePrice:" + order.BaseSlippagePrice.ToString() : "") +
                (order.HasSlippageInPips ? ", slippageInPips:" + order.SlippageInPips.ToString() : "") +
                (order.HasRelativeStopLossInPips ? ", relativeStopLossInPips:" + order.RelativeStopLossInPips.ToString() : "") +
                (order.HasRelativeTakeProfitInPips ? ", relativeTakeProfitInPips:" + order.RelativeTakeProfitInPips.ToString() : "") +
                (order.HasCommission ? ", commission:" + order.Commission.ToString() : "") +
                (order.HasOpenTimestamp ? ", openTimestamp:" + order.OpenTimestamp.ToString() : "") +
                (order.HasCloseTimestamp ? ", closeTimestamp:" + order.CloseTimestamp.ToString() : "") +
                (order.HasExpirationTimestamp ? ", expirationTimestamp:" + order.ExpirationTimestamp.ToString() : "") +
                (order.HasChannel ? ", channel:" + order.Channel : "") +
                (order.HasComment ? ", comment:" + order.Comment : "") +
                (order.HasClosePositionDetails ? ", " + OpenApiClosePositionDetails(order.ClosePositionDetails) : "");

            return _str + "}";
        }
        static public string OpenApiPositionStatusToString(ProtoOAPositionStatus positionStatus)
        {
            switch (positionStatus)
            {
                case ProtoOAPositionStatus.OA_POSITION_STATUS_CLOSED:
                    return "CLOSED";
                case ProtoOAPositionStatus.OA_POSITION_STATUS_OPEN:
                    return "OPENED";
                default:
                    return "unknown";
            }
        }
        static public string OpenApiPositionToString(ProtoOAPosition position)
        {
            var _str = "Position{positionId:" + position.PositionId.ToString() + ", positionStatus:" + OpenApiPositionStatusToString(position.PositionStatus) + 
                ", accountId:" + position.AccountId.ToString();
            _str += ", tradeSide:" + TradeSideToString(position.TradeSide);
            _str += ", symbolName:" + position.SymbolName + ", volume:" + position.Volume.ToString() + ", entryPrice:" + position.EntryPrice.ToString() + ", swap:" + position.Swap.ToString() +
                ", commission:" + position.Commission.ToString() + ", openTimestamp:" + position.OpenTimestamp.ToString() +
                (position.HasCloseTimestamp ? ", closeTimestamp:" + position.CloseTimestamp.ToString() : "") +
                (position.HasStopLossPrice ? ", stopLossPrice:" + position.StopLossPrice.ToString() : "") +
                (position.HasTakeProfitPrice ? ", takeProfitPrice:" + position.TakeProfitPrice.ToString() : "") +
                (position.HasChannel ? ", channel:" + position.Channel : "") +
                (position.HasComment ? ", comment:" + position.Comment : "");

            return _str + "}";
        }
        static public string OpenApiClosePositionDetails(ProtoOAClosePositionDetails closePositionDetails)
        {
            return "ClosePositionDetails{entryPrice:" + closePositionDetails.EntryPrice.ToString() +
                ", profit:" + closePositionDetails.Profit.ToString() +
                ", swap:" + closePositionDetails.Swap.ToString() +
                ", commission:" + closePositionDetails.Commission.ToString() +
                ", balance:" + closePositionDetails.Balance.ToString() +
                (closePositionDetails.HasComment ? ", comment:" + closePositionDetails.Comment : "") +
                (closePositionDetails.HasStopLossPrice ? ", stopLossPrice:" + closePositionDetails.StopLossPrice.ToString() : "") +
                (closePositionDetails.HasTakeProfitPrice ? ", takeProfitPrice:" + closePositionDetails.TakeProfitPrice.ToString() : "") +
                (closePositionDetails.HasQuoteToDepositConversionRate ? ", quoteToDepositConversionRate:" + closePositionDetails.QuoteToDepositConversionRate.ToString() : "") +
                ", closedVolume:" + closePositionDetails.ClosedVolume.ToString() +
                ", closedByStopOut:" + (closePositionDetails.ClosedByStopOut ? "TRUE" : "FALSE") +
                "}";
        }
        static public string ToString(ProtoMessage msg)
        {
            return ProtoMessageToString(msg);
        }
    }
}
