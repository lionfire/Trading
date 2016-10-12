using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenApiLib;
using Google.ProtocolBuffers;


namespace OpenApiDeveloperLibrary
{
    public class OpenApiModelObjectsFactory
    {
        #region Building Proto Model objects from Byte array methods
        public ProtoOAOrder GetOrder(byte[] obj = null)
        {
            return ProtoOAOrder.CreateBuilder().MergeFrom(obj).Build();
        }
        public ProtoOAPosition GetPosition(byte[] obj = null)
        {
            return ProtoOAPosition.CreateBuilder().MergeFrom(obj).Build();
        }
        public ProtoOAClosePositionDetails GetClosePositionDetails(byte[] obj = null)
        {
            return ProtoOAClosePositionDetails.CreateBuilder().MergeFrom(obj).Build();
        }
        public ProtoOASpotSubscription GetSpotSubscription(byte[] obj = null)
        {
            return ProtoOASpotSubscription.CreateBuilder().MergeFrom(obj).Build();
        }
        #endregion

        #region Creating new Proto Model objects with parameters specified
        public ProtoOAOrder.Builder CreateOrderBuilder(long orderId, long accountId, ProtoOAOrderType orderType, ProtoTradeSide tradeSide, string symbolName, long requestedVolume, long executedVolume, bool closingOrder,
            string channel = null, string comment=null)
        {
            var _obj = ProtoOAOrder.CreateBuilder();
            _obj.SetOrderId(orderId);
            _obj.SetAccountId(accountId);
            _obj.SetOrderType(orderType);
            _obj.SetTradeSide(tradeSide);
            _obj.SetSymbolName(symbolName);
            _obj.SetRequestedVolume(requestedVolume);
            _obj.SetExecutedVolume(executedVolume);
            _obj.SetClosingOrder(closingOrder);
            if (channel != null)
                _obj.SetChannel(channel);
            if (comment != null)
                _obj.SetComment(comment);
            return _obj;
        }
        public ProtoOAPosition.Builder CreatePositionBuilder(long positionId, ProtoOAPositionStatus positionStatus, long accountId, ProtoTradeSide tradeSide, string symbolName, long volume, double entryPrice, long swap,
            long commission, long openTimestamp, string channel = null, string comment = null)
        {
            var _obj = ProtoOAPosition.CreateBuilder();
            _obj.SetPositionId(positionId);
            _obj.SetPositionStatus(positionStatus);
            _obj.SetAccountId(accountId);
            _obj.SetTradeSide(tradeSide);
            _obj.SetSymbolName(symbolName);
            _obj.SetVolume(volume);
            _obj.SetEntryPrice(entryPrice);
            _obj.SetSwap(swap);
            _obj.SetCommission(commission);
            _obj.SetOpenTimestamp(openTimestamp);
            if (channel != null)
                _obj.SetChannel(channel);
            if (comment != null)
                _obj.SetComment(comment);
            return _obj;
        }
        public ProtoOAClosePositionDetails.Builder CreateClosePositionDetailsBuilder(double entryPrice, long profit, long swap, long commission, long balance, long closedVolume, bool closedByStopOut, string comment = null)
        {
            var _obj = ProtoOAClosePositionDetails.CreateBuilder();
            _obj.SetEntryPrice(entryPrice);
            _obj.SetProfit(profit);
            _obj.SetSwap(swap);
            _obj.SetCommission(commission);
            _obj.SetBalance(balance);
            _obj.SetClosedVolume(closedVolume);
            _obj.SetClosedByStopOut(closedByStopOut);
            if (comment != null)
                _obj.SetComment(comment);
            return _obj;
        }
        public ProtoOASpotSubscription.Builder CreateSpotSubscriptionBuilder(long accountId, uint subscriptionId)
        {
            var _obj = ProtoOASpotSubscription.CreateBuilder();
            _obj.SetAccountId(accountId);
            _obj.SetSubscriptionId(subscriptionId);
            return _obj;
        }
        #endregion
    }
}
