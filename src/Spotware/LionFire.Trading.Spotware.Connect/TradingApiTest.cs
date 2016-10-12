#if NET462
//#define TRACE_DATA_RECEIVED
//#define TRACE_DATA_SENT
//#define TRACE_HEARTBEAT
//#define TRACE_DATA_INCOMING
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
#if NET462
using OpenApiDeveloperLibrary;
#endif
using System.IO;
using Newtonsoft.Json;

namespace LionFire.Trading.Spotware.Connect
{
    public class TradingApiTest
    {
#region Settings...

        static string apiHost => ConnectionSettings?.ApiHost ?? "sandbox-tradeapi.spotware.com";
        static int apiPort => ConnectionSettings?.ApiPort ?? 5032;

        static string clientPublicId =>
            ConnectionSettings?.ClientPublicId ??
            "7_5az7pj935owsss8kgokcco84wc8osk0g0gksow0ow4s4ocwwgc";
        static string clientSecret =>
            ConnectionSettings?.ClientSecret ??
            "49p1ynqfy7c4sw84gwoogwwsk8cocg8ow8gc8o80c0ws448cs4";

        //static long testAccountId = 62002; // login 3000041 pass:123456 on http://sandbox-ct.spotware.com
        static long testAccountId => ConnectionSettings?.AccountId ?? 62002; // login 3000041 pass:123456 on http://sandbox-ct.spotware.com
        static string testAccessToken = ConnectionSettings?.AccessToken ?? "test002_access_token";

        public static SpotwareConnectSettings ConnectionSettings {
            get {

                if (connectionSettings == null)
                {
#if true
                    //var path = @"E:\Trading\Accounts\spotware-lfdev.json";
                    var path = @"E:\Trading\Accounts\spotware-lionprowl.json";

                    using (var sr = new StreamReader(new FileStream(path, FileMode.Open)))
                    {
                        connectionSettings = JsonConvert.DeserializeObject<SpotwareConnectSettings>(sr.ReadToEnd());
                    }
#endif
                }
                return connectionSettings;
            }
        }
        static SpotwareConnectSettings connectionSettings;



        static long testOrderId = -1;
        static long testPositionId = -1;
        //static Dictionary<long, string> testOrdersMap = new Dictionary<long,string>();
        static long testVolume = 1000000;

        static string clientMsgId = null;

        static uint sendMsgTimeout = 20;
        static DateTime lastSentMsgTimestamp = DateTime.Now.AddSeconds(sendMsgTimeout);

        static int MaxMessageSize = 1000000;
        static bool isDebugIsOn = true;

#endregion Settings...

#region Internal fields
        static SslStream apiSocket;

        volatile static bool isShutdown;
        volatile static bool isRestart;

        static Queue __writeQueue = new Queue(); // not thear safe
        static Queue __readQueue = new Queue(); // not thear safe
        static Queue writeQueueSync = Queue.Synchronized(__writeQueue); // thread safe
        static Queue readQueueSync = Queue.Synchronized(__readQueue); // thread safe

        static OpenApiMessagesFactory incomingMsgFactory = new OpenApiMessagesFactory();
        static OpenApiMessagesFactory outgoingMsgFactory = new OpenApiMessagesFactory();

        static Random rndGenerator = new Random();
#endregion Internal fields

#region Threads
        // timer thread
        static void Timer(OpenApiMessagesFactory msgFactory, Queue messagesQueue)
        {
            isShutdown = false;
            while (!isShutdown)
            {
                Thread.Sleep(1000);

                if (DateTime.Now > lastSentMsgTimestamp)
                {
                    SendPingRequest(msgFactory, messagesQueue);
                }
            }
        }

        // listener thread
        static void Listen(SslStream sslStream, Queue messagesQueue)
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
                if (isDebugIsOn) Console.WriteLine("Data received: {0}", GetHexadecimal(_length));
#endif
                readBytes = 0;
                do
                {
                    Thread.Sleep(0);
                    readBytes += sslStream.Read(_message, readBytes, _message.Length - readBytes);
                } while (readBytes < length);
#if TRACE_DATA_RECEIVED
                if (isDebugIsOn) Console.WriteLine("Data received: {0}", GetHexadecimal(_message));
#endif

                messagesQueue.Enqueue(_message);
            }
        }

        // sender thread
        static void Transmit(SslStream sslStream, Queue messagesQueue, DateTime lastSentMsgTimestamp)
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
            }
        }

        // incoming data processing thread
        static void IncomingDataProcessing(OpenApiMessagesFactory msgFactory, Queue messagesQueue)
        {
            isShutdown = false;
            while (!isShutdown)
            {
                Thread.Sleep(0);

                if (messagesQueue.Count <= 0)
                    continue;

                byte[] _message = (byte[])messagesQueue.Dequeue();
                ProcessIncomingDataStream(msgFactory, _message);
            }
        }
#endregion Threads

#region Handlers
        static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
            return false;
        }
#endregion Handlers

        public static void Main(string[] args)
        {
            do
            {
                isRestart = false;
#region open ssl connection
                Console.WriteLine("Establishing trading SSL connection to {0}:{1}...", apiHost, apiPort);
                try
                {
                    TcpClient client = new TcpClient(apiHost, apiPort);
                    apiSocket = new SslStream(client.GetStream(), false,
                        new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                    apiSocket.AuthenticateAsClient(apiHost);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Establishing SSL connection error: {0}", e);
                    return;
                }
                Console.WriteLine("The connection is established successfully.");
#endregion open ssl connection

#region start incoming data processing thread
                Thread p = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    try
                    {
                        IncomingDataProcessing(incomingMsgFactory, readQueueSync);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("DataProcessor throws exception: {0}", e);
                    }
                });
                p.Start();
#endregion start incoming data processing thread

#region start listener
                Thread tl = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    try
                    {
                        Listen(apiSocket, readQueueSync);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Listener throws exception: {0}", e);
                    }
                });
                tl.Start();
#endregion start listener

#region start sender
                Thread ts = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    try
                    {
                        Transmit(apiSocket, writeQueueSync, lastSentMsgTimestamp);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Transmitter throws exception: {0}", e);
                    }
                });
                ts.Start();
#endregion start sender

#region start timer
                Thread t = new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    try
                    {
                        Timer(outgoingMsgFactory, writeQueueSync);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Listener throws exception: {0}", e);
                    }
                });
                t.Start();
#endregion start timer

#region main loop
                while (tl.IsAlive || t.IsAlive || p.IsAlive || ts.IsAlive)
                {
#region display menu
                    Console.WriteLine();
                    Console.WriteLine("List of actions");
                    foreach (var m in menuItems)
                        Console.WriteLine("{0}: {1}", m.cmdKey, m.itemTitle);
                    Console.WriteLine("----------------------------");
                    Console.WriteLine("R: reconnect");
                    Console.WriteLine("Q: quit");
#endregion display menu

#region process menu actions
                    Thread.Sleep(300);
                    Console.WriteLine("Enter the action to perform:");
                    char cmd = Console.ReadKey().KeyChar;
                    Console.WriteLine();
                    if (cmd == 'Q' || cmd == 'q')
                    {
                        break;
                    }
                    else if (cmd == 'R' || cmd == 'r')
                    {
                        isRestart = true;
                        break;
                    }
                    else
                        foreach (var m in menuItems)
                            if (string.Join("", cmd).ToUpper() == string.Join("", m.cmdKey).ToUpper())
                                m.itemHandler(outgoingMsgFactory, writeQueueSync);
                    Thread.Sleep(700);
#endregion process menu actions
                }
#endregion main loop

#region close ssl connection
                isShutdown = true;
                apiSocket.Close();
#endregion close ssl connection

#region wait for shutting down threads
                Console.WriteLine("Shutting down connection...");
                while (tl.IsAlive || t.IsAlive || p.IsAlive || ts.IsAlive)
                {
                    Thread.Sleep(100);
                }
#endregion wait for shutting down threads
            } while (isRestart);
        }

#region Auxilary functions
        public static string GetHexadecimal(byte[] byteArray)
        {
            var hex = new StringBuilder(byteArray.Length * 2);
            foreach (var b in byteArray)
                hex.AppendFormat("{0:X2} ", b);
            return hex.ToString();
        }
#endregion Auxilary functions

#region Incoming data stream processing...
        static void ProcessIncomingDataStream(OpenApiMessagesFactory msgFactory, byte[] rawData)
        {
            var _msg = msgFactory.GetMessage(rawData);
#if TRACE_DATA_INCOMING
            if (isDebugIsOn) Console.WriteLine("ProcessIncomingDataStream() Message received:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
#endif

            if (!_msg.HasPayload)
            {
                return;
            }

            switch (_msg.PayloadType)
            {
                case (int)OpenApiLib.ProtoPayloadType.HEARTBEAT_EVENT:
                    break;
                case (int)OpenApiLib.ProtoOAPayloadType.OA_EXECUTION_EVENT:
                    var _payload_msg = msgFactory.GetExecutionEvent(rawData);
                    if (_payload_msg.HasOrder)
                    {
                        testOrderId = _payload_msg.Order.OrderId;
                    }
                    if (_payload_msg.HasPosition)
                    {
                        testPositionId = _payload_msg.Position.PositionId;
                    }
                    break;
                default:
                    break;
            };
        }
#endregion Incoming data stream processing...

#region Outgoing ProtoBuffer objects to Raw data...
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
        static List<MenuItem> menuItems = new List<MenuItem>()
        {
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
        };
#endregion Main Menu

        static void SendPingRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreatePingRequest((ulong)DateTime.Now.Ticks);
#if TRACE_PING
            if (isDebugIsOn) Console.WriteLine("SendPingRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
#endif
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        static void SendHeartbeatEvent(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateHeartbeatEvent();
#if TRACE_HEARTBEAT
            if (isDebugIsOn) Console.WriteLine("SendHeartbeatEvent() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
#endif
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        static void SendAuthorizationRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateAuthorizationRequest(clientPublicId, clientSecret);
            if (isDebugIsOn) Console.WriteLine("SendAuthorizationRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        static void SendSubscribeForTradingEventsRequest(long accountId, OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateSubscribeForTradingEventsRequest(accountId, testAccessToken);
            if (isDebugIsOn) Console.WriteLine("SendSubscribeForTradingEventsRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        static void SendSubscribeForTradingEventsRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            SendSubscribeForTradingEventsRequest(testAccountId, msgFactory, writeQueue);
        }
        static void SendUnsubscribeForTradingEventsRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateUnsubscribeForTradingEventsRequest(testAccountId);
            if (isDebugIsOn) Console.WriteLine("SendUnsubscribeForTradingEventsRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        static void SendGetAllSubscriptionsForTradingEventsRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateAllSubscriptionsForTradingEventsRequest();
            if (isDebugIsOn) Console.WriteLine("SendGetAllSubscriptionsForTradingEventsRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        static void SendGetAllSubscriptionsForSpotEventsRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateGetAllSpotSubscriptionsRequest();
            if (isDebugIsOn) Console.WriteLine("SendGetAllSubscriptionsForSpotEventsRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        static void SetClientMessageId(OpenApiMessagesFactory msgFactory, Queue writeQueue)
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
        static void SendMarketOrderRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateMarketOrderRequest(testAccountId, testAccessToken, "EURUSD", OpenApiLib.ProtoTradeSide.BUY, testVolume, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendMarketOrderRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        static void SendMarketRangeOrderRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateMarketRangeOrderRequest(testAccountId, testAccessToken, "EURUSD", OpenApiLib.ProtoTradeSide.BUY, testVolume, 1.09, 10, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendMarketRangeOrderRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        static void SendLimitOrderRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateLimitOrderRequest(testAccountId, testAccessToken, "EURUSD", OpenApiLib.ProtoTradeSide.BUY, 1000000, 1.11, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendLimitOrderRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        static void SendAmendLimitOrderRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateAmendLimitOrderRequest(testAccountId, testAccessToken, testOrderId, 1.10, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendLimitOrderRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        static void SendStopOrderRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateStopOrderRequest(testAccountId, testAccessToken, "EURUSD", OpenApiLib.ProtoTradeSide.BUY, 1000000, 0.2, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendStopOrderRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        static void SendClosePositionRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateClosePositionRequest(testAccountId, testAccessToken, testPositionId, testVolume, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendClosePositionRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        static void SendSubscribeForSpotsRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateSubscribeForSpotsRequest(testAccountId, testAccessToken, "EURUSD", clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendSubscribeForSpotsRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        static void NotImplementedCommand(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            Console.WriteLine("Action is NOT IMPLEMENTED!");
        }
#endregion Outgoing ProtoBuffer objects to Raw data...
    }
}

#endif