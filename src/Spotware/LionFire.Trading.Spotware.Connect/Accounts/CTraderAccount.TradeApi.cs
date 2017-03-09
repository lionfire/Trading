#if NET462
//#define TRACE_DATA_RECEIVED
//#define TRACE_DATA_SENT
//#define TRACE_HEARTBEAT
//#define TRACE_DATA_INCOMING
//#define LOG_SENTITIVE_INFO
//#define TRACE_SUBSCRIPTIONS
//#define GET_SUBS_AFTER_SUB
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Collections.Concurrent;
using OpenApiDeveloperLibrary;
using System.IO;
using Newtonsoft.Json;
using LionFire.Instantiating;
using LionFire.Assets;
using LionFire.Applications;
using System.Threading.Tasks;
using LionFire.Execution;
using LionFire.Extensions.Logging;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;
using LionFire.MultiTyping;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Linq;
using LionFire.Structures;
using OpenApiLib;
using LionFire.Trading;
using LionFire.Trading.Statistics;
using System.Diagnostics;

namespace LionFire.Trading.Spotware.Connect
{


    public partial class CTraderAccount
    {
#region Derived (Convenience)

        string TradeApiHost => ApiInfo.TradeApiHost ?? SpotwareConnectAppInfo.DefaultTradeApiHost;
        int TradeApiPort => ApiInfo.TradeApiPort ?? SpotwareConnectAppInfo.DefaultTradeApiPort;
        string ClientPublicId => ApiInfo.ClientPublicId;
        string ClientSecret => ApiInfo.ClientSecret;

#endregion

#region TradeApi Settings

        int MaxMessageSize = 1000000;
        uint sendMsgTimeout = 20;

#endregion

#region Testing

        bool isDebugIsOn = false;

        //public string SandboxApiHost = "sandbox-tradeapi.spotware.com";
        //public static string TestClientPublicId = "7_5az7pj935owsss8kgokcco84wc8osk0g0gksow0ow4s4ocwwgc";
        //public static string TestClientSecret = "49p1ynqfy7c4sw84gwoogwwsk8cocg8ow8gc8o80c0ws448cs4";

        long orderId = -1;
        long positionId = -1;
        //Dictionary<long, string> testOrdersMap = new Dictionary<long,string>();
        long testVolume = 1000000; // TEMP

#endregion

#region Construction

        partial void CTraderAccount_NetFramework()
        {
            writeQueueSync = Queue.Synchronized(__writeQueue);
            readQueueSync = Queue.Synchronized(__readQueue);
        }

#endregion

#region Internal fields

        SslStream apiSocket;

        string clientMsgId = null;
        DateTime lastSentMsgTimestamp => DateTime.Now.AddSeconds(sendMsgTimeout);

        volatile bool isShutdown;

        Queue __writeQueue = new Queue(); // not thread safe
        Queue __readQueue = new Queue(); // not thread safe
        Queue writeQueueSync; // thread safe
        Queue readQueueSync; // thread safe

        OpenApiMessagesFactory incomingMsgFactory = new OpenApiMessagesFactory();
        OpenApiMessagesFactory outgoingMsgFactory = new OpenApiMessagesFactory();

        Random rndGenerator = new Random();

#endregion

#region Threads

        Thread handlerThread;
        Thread listenerThread;
        Thread senderThread;
        Thread heartbeatThread;

        void HeartbeatTimer(OpenApiMessagesFactory msgFactory, Queue messagesQueue)
        {
            isShutdown = false;
            while (!isShutdown)
            {
                Thread.Sleep(1000);

                if (_nextHeartbeat <= DateTime.UtcNow)
                {
                    SendHeartbeatEvent(outgoingMsgFactory, writeQueueSync);
                }
                //if (DateTime.Now > lastSentMsgTimestamp)
                //{
                //    SendPingRequest(msgFactory, messagesQueue);
                //}
            }
        }

        void Listen(SslStream sslStream, Queue messagesQueue)
        {
            isShutdown = false;
            while (!isShutdown)
            {
                Thread.Sleep(1);

                byte[] _length = new byte[sizeof(int)];
                int readBytes = 0;
                do
                {
                    Thread.Sleep(0);
                    readBytes += sslStream.Read(_length, readBytes, _length.Length - readBytes);
                } while (readBytes < _length.Length);

                int length = BitConverter.ToInt32(_length.Reverse().ToArray(), 0);
                if (length <= 0)
                    continue;

                if (length > MaxMessageSize)
                {
                    string exceptionMsg = "Message length " + length.ToString() + " is out of range (0 - " + MaxMessageSize.ToString() + ")";
                    throw new System.IndexOutOfRangeException();
                }

                byte[] _message = new byte[length];
#if TRACE_DATA_RECEIVED
                //if (isDebugIsOn) Console.WriteLine("Data received: {0}", GetHexadecimal(_length));
                if (isDebugIsOn) { Console.Write($"[recv] {{len:{length}}} "); }
#endif
                readBytes = 0;
                do
                {
                    Thread.Sleep(0);
                    readBytes += sslStream.Read(_message, readBytes, _message.Length - readBytes);
                } while (readBytes < length);
#if TRACE_DATA_RECEIVED
                if (isDebugIsOn) Console.WriteLine($"{GetHexadecimal(_message)}");
#endif

                messagesQueue.Enqueue(_message);
            }
        }

        // sender thread
        void Transmit(SslStream sslStream, Queue messagesQueue, DateTime lastSentMsgTimestamp)
        {
            isShutdown = false;
            while (!isShutdown)
            {
                Thread.Sleep(1);

                if (messagesQueue.Count <= 0)
                    continue;

                byte[] _message = (byte[])messagesQueue.Dequeue();
                byte[] _length = BitConverter.GetBytes(_message.Length).Reverse().ToArray(); ;

                sslStream.Write(_length);
#if TRACE_DATA_SENT
                if (isDebugIsOn) Console.WriteLine("Data sent: {0}", GetHexadecimal(_length));
#endif
                sslStream.Write(_message);
#if TRACE_DATA_SENT
                if (isDebugIsOn) Console.WriteLine("Data sent: {0}", GetHexadecimal(_message));
#endif
                lastSentMsgTimestamp = DateTime.Now.AddSeconds(sendMsgTimeout);
                lastSendTime = DateTime.UtcNow;
            }
        }

        // incoming data processing thread
bool IncomingDataProcessingRunning = false;
        void IncomingDataProcessing(OpenApiMessagesFactory msgFactory, Queue messagesQueue)
        {
            isShutdown = false;
if(IncomingDataProcessingRunning)
{
throw new System.Exception("IncomingDataProcessing Started twice");

}
try{
            while (!isShutdown)
            {
IncomingDataProcessingRunning = true;
                Thread.Sleep(0);

                if (messagesQueue.Count <= 0) continue;

                byte[] _message;
                try
                {
                   _message = (byte[])messagesQueue.Dequeue();
                }
                catch (InvalidOperationException ioe)
                {
                    Debug.WriteLine("InvalidOperationException in IncomingDataProcessing at messagesQueue.Dequeue() " + ioe);
                    continue;
                }
                ProcessIncomingDataStream(msgFactory, _message);
            }
}
finally{
IncomingDataProcessingRunning = false;
}
        }

#endregion

        private bool Connect()
        {
#region open ssl connection

            logger.LogInformation("Establishing trading SSL connection to {0}:{1}...", TradeApiHost, TradeApiPort);
            try
            {
                TcpClient client = new TcpClient(TradeApiHost, TradeApiPort);
                apiSocket = new SslStream(client.GetStream(), false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                apiSocket.AuthenticateAsClient(TradeApiHost);
            }
            catch (Exception e)
            {
                logger.LogError("Establishing SSL connection error: {0}", e);
                return false;
            }
            logger.LogInformation("The connection is established successfully.");

#endregion open ssl connection

#region start incoming data processing thread

            handlerThread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                try
                {
                    IncomingDataProcessing(incomingMsgFactory, readQueueSync);
                }
                catch (Exception e)
                {
                    logger.LogError("Message handler threw exception: {0}", e);
                }
            });
            handlerThread.Start();

#endregion

#region start listener

            listenerThread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                try
                {
                    Listen(apiSocket, readQueueSync);
                }
                catch (Exception e)
                {
                    logger.LogError("Listener throws exception: {0}", e);
                }
            });
            listenerThread.Start();

#endregion

#region start sender

            senderThread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                try
                {
                    Transmit(apiSocket, writeQueueSync, lastSentMsgTimestamp);
                }
                catch (Exception e)
                {
                    logger.LogError("Transmitter throws exception: {0}", e);
                }
            });
            senderThread.Start();

#endregion

#region start timer

            heartbeatThread = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                try
                {
                    HeartbeatTimer(outgoingMsgFactory, writeQueueSync);
                }
                catch (Exception e)
                {
                    logger.LogError("Timer threw exception: {0}", e);
                }
            });
            heartbeatThread.Start();

#endregion

            SendAuthorizationRequest();

            for (int retries = 5; retries > 0 && !IsAuthorized; retries--)
            {
                logger.LogInformation("Waiting for authorization response...");
                Thread.Sleep(1000);
            }
            var not = IsAuthorized ? "" : " NOT ";
            logger.LogInformation($"Connected and {not} authorized");
            return true;
        }

#region Handlers

        bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
            return false;
        }

#endregion Handlers

        public bool IsAuthorized
        {
            get { return isAuthorized; }
            set
            {
                if (isAuthorized == value) return; isAuthorized = value;
                //IsConnectedChangedTo?.Invoke(IsConnected);
                started.OnNext(IsConnected);
            }
        }
        private bool isAuthorized;

        public bool IsConnected { get { return IsAuthorized; } }
        //public event Action<bool> IsConnectedChangedTo;

        partial void Run_TradeApi()
        {
            if (!Connect()) return;

            SendSubscribeForTradingEventsRequest(this.outgoingMsgFactory, writeQueueSync);

            SubscribeToDefaultSymbols();
        }
        partial void Stop_TradeApi()
        {
            CloseConnection();
        }

        partial void CloseConnection()
        {
#region close ssl connection

            isShutdown = true;
            if (apiSocket != null)
            {
                apiSocket.Close();
                apiSocket = null;
            }

#endregion

#region wait for shutting down threads

            Console.Write("Shutting down connection...");
            while (IsTradeConnectionAlive)
            {
                Thread.Sleep(100);
                Console.Write(".");
            }
            Console.WriteLine(" Done.");

#endregion
        }

        public bool IsTradeConnectionAlive
        {
            get
            {
                return (listenerThread != null && listenerThread.IsAlive)
                    || (heartbeatThread != null && heartbeatThread.IsAlive)
                    || (handlerThread != null && handlerThread.IsAlive)
                    || (senderThread != null && senderThread.IsAlive);
            }
        }

        partial void SubscribeToDefaultSymbols()
        {

            //RequestSubscribeForSymbol("EURUSD");
            //RequestSubscribeForSymbol("EURUSD", ProtoOATrendbarPeriod.M1);
            //RequestSubscribeForSymbol("EURUSD", ProtoOATrendbarPeriod.M1, ProtoOATrendbarPeriod.H1);

            {
                var defaultPeriods = new ProtoOATrendbarPeriod[] { ProtoOATrendbarPeriod.M1,
                         //ProtoOATrendbarPeriod.H1
                    //, ProtoOATrendbarPeriod.M2, ProtoOATrendbarPeriod.M3, ProtoOATrendbarPeriod.M5,
                    //ProtoOATrendbarPeriod.M10
                };
                var defaultSymbols = new string[] {
                    //"AUDSGD",
                    //"EURUSD",
                    //"USDCHF",
                };


                foreach (var s in defaultSymbols)
                {
                    RequestSubscribeForSymbol(s, defaultPeriods);
                }
            }

            //{
            //    var defaultPeriods = new ProtoOATrendbarPeriod[] { ProtoOATrendbarPeriod.M1
            //    //, ProtoOATrendbarPeriod.M2, ProtoOATrendbarPeriod.M3, ProtoOATrendbarPeriod.M5
            //};
            //    var defaultSymbols = new string[] {
            //    "CHFSGD",
            //    "AUS200",
            //};

            //    //RequestSubscribeForSymbol("EURUSD");
            //    //RequestSubscribeForSymbol("EURUSD", ProtoOATrendbarPeriod.M1);
            //    //RequestSubscribeForSymbol("EURUSD", ProtoOATrendbarPeriod.M1, ProtoOATrendbarPeriod.H1);
            //    foreach (var s in defaultSymbols)
            //    {
            //        RequestSubscribeForSymbol(s, defaultPeriods);
            //    }
            //}

            //RequestSubscribeForSymbol("USDJPY", ProtoOATrendbarPeriod.M1);

        }

#region Auxilary functions

        public string GetHexadecimal(byte[] byteArray)
        {
            var hex = new StringBuilder(byteArray.Length * 2);
            foreach (var b in byteArray)
                hex.AppendFormat("{0:X2} ", b);
            return hex.ToString();
        }

#endregion

#region Incoming data stream processing

        DateTime lastHeartBeatReceived = default(DateTime);

        void ProcessIncomingDataStream(OpenApiMessagesFactory msgFactory, byte[] rawData)
        {
            var _msg = msgFactory.GetMessage(rawData);
#if TRACE_DATA_INCOMING
            if (isDebugIsOn)
            {
                //if (_msg.PayloadType == (int)OpenApiLib.ProtoOAPayloadType.OA_SPOT_EVENT)
                //{
                //    Console.Write(".");
                //}
                //else
                {
                    Console.WriteLine("ProcessIncomingDataStream() received: " + OpenApiMessagesPresentation.ToString(_msg));
                }
            }
#endif

            if (!_msg.HasPayload)
            {
                return;
            }

            var lastLastHeartBeatReceived = lastHeartBeatReceived;
            lastHeartBeatReceived = DateTime.UtcNow;

            switch (_msg.PayloadType)
            {
                case (int)OpenApiLib.ProtoPayloadType.PING_RES:
                    {
                        var payload = msgFactory.GetPingResponse(rawData);
                        if (payload.HasTimestamp)
                        {
                            ServerTime = new DateTime((long)payload.Timestamp);
                            Console.WriteLine("[time] " + Server.Time.ToShortTimeString());
                            //Server.Time = DateTime.FromFileTimeUtc();
                        }
                        break;
                    }
                case (int)OpenApiLib.ProtoPayloadType.HEARTBEAT_EVENT:
                    //var _payload_msg = msgFactory.GetHeartbeatEvent(rawData);
                    var timeBetween = (lastHeartBeatReceived - lastLastHeartBeatReceived).TotalSeconds;
                    Console.WriteLine($"<3 ({timeBetween.ToString("N1")}s)");
                    break;
                case (int)OpenApiLib.ProtoOAPayloadType.OA_EXECUTION_EVENT:
                    {
                        var msg = "";
                        var _payload_msg = msgFactory.GetExecutionEvent(rawData);

                        if (_payload_msg.HasReasonCode)
                        {
                            var executionType = _payload_msg.ExecutionType.ToString().Replace("OA_ORDER_", "");
                            var reason = _payload_msg.HasReasonCode ? $"({_payload_msg.ReasonCode})" : "";
                            msg += $"*** [EXECUTION: {executionType} {reason}] *** ";
                        }

                        if (_payload_msg.HasOrder)
                        {
                            orderId = _payload_msg.Order.OrderId;
                            var slPrice = _payload_msg.Order.HasStopLossPrice ? " sl:" + _payload_msg.Order.StopLossPrice : "";
                            var tpPrice = _payload_msg.Order.HasTakeProfitPrice ? " tp:" + _payload_msg.Order.TakeProfitPrice : "";
                            var limitPrice = _payload_msg.Order.HasLimitPrice ? " limit:" + _payload_msg.Order.LimitPrice : "";
                            var stopPrice = _payload_msg.Order.HasStopPrice ? " stop:" + _payload_msg.Order.StopPrice : "";
                            msg += $"[ORDER {orderId}] {_payload_msg.Order.TradeSide} {_payload_msg.Order.RequestedVolume} {_payload_msg.Order.SymbolName} {limitPrice}{stopPrice} {slPrice}{tpPrice}";
                        }
                        else if (_payload_msg.HasPosition)
                        {
                            positionId = _payload_msg.Position.PositionId;
                            var p = _payload_msg.Position;
                            msg += $"[POSITION {positionId}] {p.TradeSide} {p.Volume} {p.SymbolName} @ {p.EntryPrice}";
                        }
                        else
                        {
                        }
                        Console.WriteLine(msg);
                    }
                    break;
                case (int)OpenApiLib.ProtoOAPayloadType.OA_AUTH_RES:
                    //var payload = msgFactory.GetAuthorizationResponse(rawData);
                    IsAuthorized = true;
                    Console.WriteLine("[authorized]");
                    break;
                case (int)OpenApiLib.ProtoOAPayloadType.OA_SPOT_EVENT:
                    {
                        if (rawData.Length > 40)
                        {
                            Console.WriteLine("================= GOT LONG SPOT EVENT: " + rawData.Length + " ===================");
                        }
                        var payload = msgFactory.GetSpotEvent(rawData);

                        WriteUnknownFields(_msg.PayloadType, payload);

                        //var timestamp = timestampField.VarintList[0];
                        //var time = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(timestamp);
                        var time = new DateTime(1970, 1, 1) + TimeSpan.FromMilliseconds(payload.Timestamp);

                        if (payload.TrendbarCount > 0 || payload.TrendbarList.Count > 0)
                        {
                            foreach (var bar in payload.TrendbarList)
                            {
                                Console.WriteLine($"*********************** TRENDBAR: {bar.Period} o:{bar.Open} h:{bar.High} l:{bar.Low} c:{bar.Close} [v:{bar.Volume}]");
                                if (bar.Period == ProtoOATrendbarPeriod.H1)
                                {
                                    AccountStats.Increment(StatEventType.H1Bar);
                                }
                                else if (bar.Period == ProtoOATrendbarPeriod.H1)
                                {
                                    AccountStats.Increment(StatEventType.H1Bar);
                                }
                                else
                                {
                                    AccountStats.Increment(StatEventType.Other);
                                }
                                throw new Exception("***** got a trendbar!  Celebrate!");
                            }
                        }
                        var tick = new SymbolTick
                        {
                            Symbol = payload.SymbolName,
                            Ask = payload.HasAskPrice ? payload.AskPrice : double.NaN,
                            Bid = payload.HasBidPrice ? payload.BidPrice : double.NaN,
                            Time = time
                        };
                        if (payload.HasAskPrice || payload.HasBidPrice)
                        {
                            AccountStats.Increment(StatEventType.Tick);
#if DEBUG
                            //if (AccountStats.Totals.Ticks % 100 == 0)
                            //{
                            //    Debug.WriteLine($"[stats] {AccountStats.Totals.Ticks} ticks received");
                            //}
#endif
                        }

                        var symbol = (ISymbolInternal)GetSymbol(payload.SymbolName);

                        symbol.OnTick(tick);
                        break;
                    }
                case (int)OpenApiLib.ProtoOAPayloadType.OA_SUBSCRIBE_FOR_SPOTS_RES:
                    {
                        var payload = msgFactory.GetSubscribeForSpotsResponse(rawData);

                        uint? subId = payload.HasSubscriptionId ? (uint?)payload.SubscriptionId : null;
#if TRACE_SUBSCRIPTIONS
                        Console.WriteLine($"[SUBSCRIBED] {subId}");
#endif

#if GET_SUBS_AFTER_SUB
                        SendGetSpotSubscriptionReq(subId);
                        SendGetAllSpotSubscriptionsReq();
#endif

                        WriteUnknownFields(_msg.PayloadType, payload);
                        break;
                    }
                case (int)OpenApiLib.ProtoOAPayloadType.OA_UNSUBSCRIBE_FROM_SPOTS_RES:
                    {
                        var payload = msgFactory.GetUnsubscribeFromSpotsResponse(rawData);

                        //uint? subId = payload. ? (uint?)payload.SubscriptionId : null;
                        Debug.WriteLine($"[UNSUBSCRIBED]");

#if GET_SUBS_AFTER_SUB
                        SendGetAllSpotSubscriptionsReq();
#endif

                        WriteUnknownFields(_msg.PayloadType, payload);

                        break;
                    }
                case (int)OpenApiLib.ProtoOAPayloadType.OA_GET_ALL_SPOT_SUBSCRIPTIONS_RES:
                    {
#if TRACE_SUBSCRIPTIONS
                        Debug.WriteLine($"--- GET_ALL_SPOT_SUBSCRIPTIONS_RES: ---");
                        var payload = msgFactory.GetGetAllSpotSubscriptionsResponse(rawData);
                        foreach (var x in payload.SpotSubscriptionsList)
                        {
                            foreach (var y in x.SubscribedSymbolsList)
                            {
                                Debug.Write($" - subscription {x.SubscriptionId}: {y.SymbolName} periods: ");
                                foreach (var z in y.PeriodList)
                                {
                                    Debug.Write($" {z.ToString()}");
                                }
                                Debug.WriteLine();
                            }
                        }
                        Debug.WriteLine($"--------------------------------------- ");
#endif
                    }
                    break;
                case (int)OpenApiLib.ProtoOAPayloadType.OA_GET_SPOT_SUBSCRIPTION_RES:
                    {
#if TRACE_SUBSCRIPTIONS
                        var payload = msgFactory.GetGetSpotSubscriptionResponse(rawData);
                        Debug.WriteLine($"--- GET_SPOT_SUBSCRIPTION_RES for subscription {payload.SpotSubscription.SubscriptionId}: --- ");
                        foreach (var y in payload.SpotSubscription.SubscribedSymbolsList)
                        {
                            Debug.Write($" - {y.SymbolName} periods: ");
                            foreach (var z in y.PeriodList)
                            {
                                Debug.Write($"{z.ToString()} ");
                            }
                            Debug.WriteLine();
                        }
                        Debug.WriteLine($"------------------------------------------------------ ");
#endif
                    }
                    break;
                case (int)OpenApiLib.ProtoOAPayloadType.OA_SUBSCRIBE_FOR_TRADING_EVENTS_RES:
                    {
                        var payload = msgFactory.GetSubscribeForTradingEventsResponse(rawData);
                        Console.WriteLine("[TRADE EVENTS] SUBSCRIBED");
                    }
                    break;

                default:
                    Console.WriteLine("UNHANDLED MESSAGE: " + _msg.PayloadType);
                    break;
            };
        }

        private void WriteUnknownFields(uint type, Google.ProtocolBuffers.IMessage payload)
        {
            var u = payload.UnknownFields.FieldDictionary;
            if (u != null)
            {
                foreach (var kvp in u)
                {
                    Console.WriteLine($"[UNKNOWN FIELD] Message type: {type}, Key: {kvp.Key}");
                }
            }
        }

#endregion



#region Outgoing ProtoBuffer objects to Raw data

        void SendPingRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreatePingRequest((ulong)DateTime.Now.Ticks);
#if TRACE_PING
            if (isDebugIsOn) Console.WriteLine("SendPingRequest() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
#endif
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendHeartbeatEvent(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateHeartbeatEvent();
#if TRACE_HEARTBEAT
            if (isDebugIsOn) Console.WriteLine("SendHeartbeatEvent() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
#endif
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendAuthorizationRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateAuthorizationRequest(ClientPublicId, ClientSecret);
            if (isDebugIsOn) Console.WriteLine("SendAuthorizationRequest() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendAuthorizationRequest()
        {
            var _msg = outgoingMsgFactory.CreateAuthorizationRequest(ClientPublicId, ClientSecret);
            if (isDebugIsOn)
            {
                //#if LOG_SENTITIVE_INFO
                Console.WriteLine("SendAuthorizationRequest() Message to be sent:{0}", OpenApiMessagesPresentation.ToString(_msg));
                //#else
                //Console.WriteLine("SendAuthorizationRequest() Message to be sent: *********");
                //#endif
            }
            writeQueueSync.Enqueue(_msg.ToByteArray());
        }
        void SendSubscribeForTradingEventsRequest(long accountId, OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateSubscribeForTradingEventsRequest(accountId, AccessToken);
            if (isDebugIsOn) Console.WriteLine("SendSubscribeForTradingEventsRequest() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendSubscribeForTradingEventsRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            SendSubscribeForTradingEventsRequest(AccountId, msgFactory, writeQueue);
        }
        void SendUnsubscribeForTradingEventsRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateUnsubscribeForTradingEventsRequest(AccountId);
            if (isDebugIsOn) Console.WriteLine("SendUnsubscribeForTradingEventsRequest() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendGetAllSubscriptionsForTradingEventsRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateAllSubscriptionsForTradingEventsRequest();
            if (isDebugIsOn)
            {
                //#if LOG_SENTITIVE_INFO
                //                Console.WriteLine("SendGetAllSubscriptionsForTradingEventsRequest() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
                //#else
                //#endif
                Console.WriteLine("SendGetAllSubscriptionsForTradingEventsRequest() Message to be sent: *********");
            }
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendGetAllSubscriptionsForSpotEventsRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateGetAllSpotSubscriptionsRequest();
            if (isDebugIsOn) Console.WriteLine("SendGetAllSubscriptionsForSpotEventsRequest() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SetClientMessageId(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            if (isDebugIsOn) Console.WriteLine("SetClientMessageId() Current message ID:\"{0}\"", (clientMsgId == null ? "null" : clientMsgId));
            if (clientMsgId != null)
            {
                clientMsgId = null;
            }
            else
            {
                clientMsgId = "customClientMessageID";
            }
            if (isDebugIsOn) Console.WriteLine("SetClientMessageId() New message ID:\"{0}\"", (clientMsgId == null ? "null" : clientMsgId));
        }
        void SendMarketOrderRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateMarketOrderRequest(AccountId, AccessToken, "EURUSD", OpenApiLib.ProtoTradeSide.BUY, testVolume, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendMarketOrderRequest() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendMarketRangeOrderRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateMarketRangeOrderRequest(AccountId, AccessToken, "EURUSD", OpenApiLib.ProtoTradeSide.BUY, testVolume, 1.09, 10, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendMarketRangeOrderRequest() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendLimitOrderRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateLimitOrderRequest(AccountId, AccessToken, "EURUSD", OpenApiLib.ProtoTradeSide.BUY, 1000000, 1.11, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendLimitOrderRequest() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendAmendLimitOrderRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateAmendLimitOrderRequest(AccountId, AccessToken, orderId, 1.10, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendLimitOrderRequest() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendStopOrderRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateStopOrderRequest(AccountId, AccessToken, "EURUSD", OpenApiLib.ProtoTradeSide.BUY, 1000000, 0.2, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendStopOrderRequest() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendClosePositionRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateClosePositionRequest(AccountId, AccessToken, positionId, testVolume, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendClosePositionRequest() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendSubscribeForSpotsRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateSubscribeForSpotsRequest(AccountId, AccessToken, "EURUSD", clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendSubscribeForSpotsRequest() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }

        void NotImplementedCommand(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            Console.WriteLine("Action is NOT IMPLEMENTED!");
        }

#region Request Messages

        Dictionary<string, HashSet<ProtoOATrendbarPeriod>> subscribedSymbols = new Dictionary<string, HashSet<ProtoOATrendbarPeriod>>();

        private HashSet<ProtoOATrendbarPeriod> GetSubscribed(string symbol)
        {
            if (subscribedSymbols.ContainsKey(symbol)) { return subscribedSymbols[symbol]; }
            var set = new HashSet<ProtoOATrendbarPeriod>();
            subscribedSymbols.Add(symbol, set);
            return set;
        }

        private void ValidateConnected()
        {
            if (!IsAuthorized)
            {
                throw new NotConnectedException("Connection is not authorized yet");
            }
        }

        void RequestSubscribeForSymbol(string symbol, params ProtoOATrendbarPeriod[] periods)
        {
            ValidateConnected();

            if (periods.Length == 0)
            {
                periods = new ProtoOATrendbarPeriod[] {
                ProtoOATrendbarPeriod.M1 // TEMP TEST - For Spotware troubleshooting missing trendbars
            };
            }

            var subscribed = GetSubscribed(symbol);

            List<ProtoOATrendbarPeriod> list = new List<ProtoOATrendbarPeriod>(periods.Where(p => !subscribed.Contains(p)));

            // TODO: Verify previously subscribed trendbars stay subscribed.  Otherwise, eliminate the where clause above.

            var _msg = outgoingMsgFactory.CreateSubscribeForSpotsRequest(AccountId, AccessToken, symbol, clientMsgId, list);
            if (isDebugIsOn)
            {
                //#if LOG_SENTITIVE_INFO
#if TRACE_SUBSCRIPTIONS
                Debug.WriteLine("[tradeapi] SendSubscribeForSpotsRequest(): {0}", OpenApiMessagesPresentation.ToString(_msg));
#endif
                //#else
                //                Console.WriteLine("SendSubscribeForSpotsRequest(): {0}", OpenApiMessagesPresentation.ToString(_msg));
                //#endif
            }
            writeQueueSync.Enqueue(_msg.ToByteArray());
        }
        protected void RequestUnsubscribeForSymbol(string symbol, params ProtoOATrendbarPeriod[] periods)
        {
            var _msg = outgoingMsgFactory.CreateUnsubscribeFromSymbolSpotsRequest(symbol, clientMsgId);

#if TRACE_SUBSCRIPTIONS
            if (isDebugIsOn) Console.WriteLine("Send UnsubscribeFromSymbolSpotsRequest(): {0}", OpenApiMessagesPresentation.ToString(_msg));
#endif
            writeQueueSync.Enqueue(_msg.ToByteArray());

            var subscribed = GetSubscribed(symbol);
            foreach (var period in periods)
            {
                subscribed.Remove(period);
            }

            if (subscribed.Count > 0 || periods.Length > 0)
            {
                RequestSubscribeForSymbol(symbol, subscribed.ToArray());
            }
            else
            {
                var s = GetSymbol(symbol);
                s.Bid = double.NaN;
                s.Ask = double.NaN;
            }

            //throw new NotImplementedException();
            //var _msg = outgoingMsgFactory.CreateUnsubscribeAccountFromSpotsRequest(AccountId, AccessToken, symbol, clientMsgId);
            //if (isDebugIsOn) Console.WriteLine("SendSubscribeForSpotsRequest() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            //writeQueueSync.Enqueue(_msg.ToByteArray());
        }

        void SendGetSpotSubscriptionReq(uint? subscriptionId = 0)
        {
            if (!subscriptionId.HasValue) { return; }
            var _msg = outgoingMsgFactory.CreateGetSpotSubscriptionRequest(subscriptionId.Value, clientMsgId);
#if TRACE_SUBSCRIPTIONS
            if (isDebugIsOn) Debug.WriteLine("SendGetSpotSubscriptionReq() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
#endif
            writeQueueSync.Enqueue(_msg.ToByteArray());
        }
        void SendGetAllSpotSubscriptionsReq()
        {
            var _msg = outgoingMsgFactory.CreateGetAllSpotSubscriptionsRequest(clientMsgId);
#if TRACE_SUBSCRIPTIONS
            if (isDebugIsOn) Debug.WriteLine("SendGetAllSpotSubscriptionsReq() Message to be sent:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
#endif

            writeQueueSync.Enqueue(_msg.ToByteArray());
        }

#endregion

#endregion Outgoing ProtoBuffer objects to Raw data...

#region Command line interface


        private void DisplayMenu()
        {
            Console.WriteLine();
            Console.WriteLine("List of actions");
            foreach (var m in MenuItems)
                Console.WriteLine("{0}: {1}", m.cmdKey, m.itemTitle);
            Console.WriteLine("----------------------------");
            Console.WriteLine("R: reconnect");
            Console.WriteLine("Q: quit");

            Thread.Sleep(300);
            Console.WriteLine("Enter the action to perform:");
        }

        private bool ProcessInput()
        {
            char cmd = Console.ReadKey().KeyChar;
            Console.WriteLine();
            if (cmd == 'Q' || cmd == 'q')
            {
                isRestart = false;
                return false;
            }
            else if (cmd == 'R' || cmd == 'r')
            {
                isRestart = true;
                return false;
            }
            else
            {
                foreach (var m in MenuItems)
                {
                    if (string.Join("", cmd).ToUpper() == string.Join("", m.cmdKey).ToUpper())
                    {
                        m.itemHandler(outgoingMsgFactory, writeQueueSync);
                    }
                }
            }
            return true;
        }

#endregion


#region Main Menu

        struct MenuItem
        {
            public delegate void ItemAction(OpenApiMessagesFactory msgFactory, Queue _writeQueue);

            public char cmdKey;
            public string itemTitle;
            public ItemAction itemHandler;

            public MenuItem(char _cmdKey, string _itemTitle, ItemAction _itemHandler)
            {
                cmdKey = _cmdKey;
                itemTitle = _itemTitle;
                itemHandler = _itemHandler;
            }
        };

        List<MenuItem> MenuItems
        {
            get
            {
                if (menuItems == null)
                {
                    menuItems = new List<MenuItem>()
                    {
#if NET462
                        new MenuItem('P', "send ping request", SendPingRequest),
                        new MenuItem('H', "send heartbeat event", SendHeartbeatEvent),
                        new MenuItem('A', "send authorization request", SendAuthorizationRequest),
                        new MenuItem('S', "send subscription request", SendSubscribeForTradingEventsRequest),
                        new MenuItem('U', "send unsubscribe request", SendUnsubscribeForTradingEventsRequest),
                        new MenuItem('G', "send getting all subscriptions request", SendGetAllSubscriptionsForTradingEventsRequest),
                        new MenuItem('N', "send getting all spot subscriptions request", SendGetAllSubscriptionsForSpotEventsRequest),
                        new MenuItem('1', "send market order", SendMarketOrderRequest),
                        new MenuItem('2', "send limit order", SendLimitOrderRequest),
                        new MenuItem('3', "send stop order", SendStopOrderRequest),
                        new MenuItem('4', "send market range order", SendMarketRangeOrderRequest),
                        new MenuItem('5', "send amend limit order", SendAmendLimitOrderRequest),
                        new MenuItem('9', "close last modified position", SendClosePositionRequest),
                        new MenuItem('C', "cancel last pending order", NotImplementedCommand),
                        new MenuItem('L', "set loss level", NotImplementedCommand),
                        new MenuItem('T', "set profit level", NotImplementedCommand),
                        new MenuItem('X', "set expiration time (in secs)", NotImplementedCommand),
                        new MenuItem('M', "set/clear client message ID", SetClientMessageId),
                        new MenuItem('0', "subscribe for EURUSD quites", SendSubscribeForSpotsRequest),
#endif
                    };
                }
                return menuItems;
            }
        }
        List<MenuItem> menuItems;

#endregion Main Menu

    }
}

#endif