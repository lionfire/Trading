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
using LionFire.Templating;
using LionFire.Assets;
using LionFire.Applications;
using System.Threading.Tasks;
using LionFire.Execution;
using LionFire.Extensions.Logging;
using System.Reactive.Subjects;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Spotware.Connect
{

    [AssetPath(@"Accounts/cTrader")]
    public class TCTraderAccount : ITemplate<CTraderAccount>
    {
        public string ClientPublicId { get; set; }
        public string ClientSecret { get; set; }
        public string ApiHost { get; set; }
        public int? ApiPort { get; set; }

        public string AccessToken { get; set; }
        public long AccountId { get; set; }

        public static int DefaultApiPort = 5032;
    }

    public class CTraderAccount : LiveMarket, IRequiresServices, ITemplateInstance, IAppTask, IStartable, IHasExecutionFlags, IHasRunTask
    //, IHasExecutionState, IChangesExecutionState
    {

        #region Settings

        #region Template

        object ITemplateInstance.Template { get { return Template; } set { Template = (TCTraderAccount)value; } }
        public TCTraderAccount Template { get; set; }

        #endregion

        #region Derived (Convenience)

        //long AccountId = 62002; // login 3000041 pass:123456 on http://sandbox-ct.spotware.com
        long AccountId => Template.AccountId;
        //?? 62002; // login 3000041 pass:123456 on http://sandbox-ct.spotware.com
        string AccessToken => Template.AccessToken;
        //?? "test002_access_token";

        string apiHost => Template.ApiHost;
        //?? SandboxApiHost;
        int apiPort => Template.ApiPort ?? TCTraderAccount.DefaultApiPort;

        string clientPublicId => Template.ClientPublicId;
        //?? TestClientPublicId;
        string clientSecret => Template.ClientSecret;
        //?? TestClientSecret;

        #endregion

        int MaxMessageSize = 1000000;
        uint sendMsgTimeout = 20;

        #endregion

        #region Relationships

        IServiceProvider IRequiresServices.ServiceProvider { get { return ServiceProvider; } set { this.ServiceProvider = value; } }
        protected IServiceProvider ServiceProvider { get; private set; }

        #endregion

        #region Testing

        bool isDebugIsOn = true;

        //public string SandboxApiHost = "sandbox-tradeapi.spotware.com";
        //public static string TestClientPublicId = "7_5az7pj935owsss8kgokcco84wc8osk0g0gksow0ow4s4ocwwgc";
        //public static string TestClientSecret = "49p1ynqfy7c4sw84gwoogwwsk8cocg8ow8gc8o80c0ws448cs4";

        long orderId = -1;
        long positionId = -1;
        //Dictionary<long, string> testOrdersMap = new Dictionary<long,string>();
        long testVolume = 1000000; // TEMP

        #endregion

        #region Construction

        public CTraderAccount()
        {
            writeQueueSync = Queue.Synchronized(__writeQueue);
            readQueueSync = Queue.Synchronized(__readQueue);
            logger = this.GetLogger();
        }

        #endregion

        #region State

        public ExecutionFlags ExecutionFlags { get { return executionFlags; } set { executionFlags = value; } }
        private volatile ExecutionFlags executionFlags;

        //volatile bool isRestart;
        bool isRestart {
            get { return ExecutionFlags.HasFlag(ExecutionFlags.AutoRestart); }
            set {
                if (value) ExecutionFlags |= ExecutionFlags.AutoRestart;
                else ExecutionFlags &= ~ExecutionFlags.AutoRestart;
            }
        }

        //public ExecutionState ExecutionState {
        //    get { return ExecutionStates.Value; }
        //    protected set { ExecutionStates.OnNext(value); }
        //}
        //public BehaviorSubject<ExecutionState> ExecutionStates = new BehaviorSubject<ExecutionState>(ExecutionState.Unspecified);

        #endregion


        #region Initialization

        public bool TryInitialize()
        {
            return Template != null;
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

        // timer thread
        void Timer(OpenApiMessagesFactory msgFactory, Queue messagesQueue)
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
            }
        }

        // incoming data processing thread
        void IncomingDataProcessing(OpenApiMessagesFactory msgFactory, Queue messagesQueue)
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

        #endregion

        #region Handlers

        bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);
            return false;
        }

        #endregion Handlers

        public Task Start()
        {
            Task.Factory.StartNew(Run);
            return Task.CompletedTask;
        }

        Thread p;
        Thread tl;
        Thread ts;
        Thread t;

        private bool Connect()
        {
            #region open ssl connection

            logger.LogInformation("Establishing trading SSL connection to {0}:{1}...", apiHost, apiPort);
            try
            {
                TcpClient client = new TcpClient(apiHost, apiPort);
                apiSocket = new SslStream(client.GetStream(), false,
                    new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
                apiSocket.AuthenticateAsClient(apiHost);
            }
            catch (Exception e)
            {
                logger.LogError("Establishing SSL connection error: {0}", e);
                return false;
            }
            logger.LogInformation("The connection is established successfully.");

            #endregion open ssl connection

            #region start incoming data processing thread

            p = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                try
                {
                    IncomingDataProcessing(incomingMsgFactory, readQueueSync);
                }
                catch (Exception e)
                {
                    logger.LogError("DataProcessor throws exception: {0}", e);
                }
            });
            p.Start();

            #endregion

            #region start listener

            tl = new Thread(() =>
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
            tl.Start();

            #endregion

            #region start sender

            ts = new Thread(() =>
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
            ts.Start();

            #endregion

            #region start timer

            t = new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                try
                {
                    Timer(outgoingMsgFactory, writeQueueSync);
                }
                catch (Exception e)
                {
                    logger.LogError("Listener throws exception: {0}", e);
                }
            });
            t.Start();

            #endregion

            SendAuthorizationRequest();

            while (!isAuthorized)
            {
                logger.LogInformation("Waiting for authorization response...");
                Thread.Sleep(1000);
            }
            logger.LogInformation("Connected and authorized");
            return true;
        }
        private bool isAuthorized;


        private void CloseConnection()
        {
            #region close ssl connection

            isShutdown = true;
            apiSocket.Close();

            #endregion

            #region wait for shutting down threads

            Console.WriteLine("Shutting down connection...");
            while (tl.IsAlive || t.IsAlive || p.IsAlive || ts.IsAlive)
            {
                Thread.Sleep(100);
            }

            #endregion 
        }

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

        public void Run()
        {
            do
            {
                isRestart = false;

                if (!Connect()) return;

                RequestSubscribeForSymbol("XAUUSD");

                while (tl.IsAlive || t.IsAlive || p.IsAlive || ts.IsAlive)
                {
                    //DisplayMenu();

                    if (!ProcessInput()) break;

                    Thread.Sleep(700);
                }

                CloseConnection();

            } while (isRestart);
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

        void ProcessIncomingDataStream(OpenApiMessagesFactory msgFactory, byte[] rawData)
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
                        orderId = _payload_msg.Order.OrderId;
                    }
                    if (_payload_msg.HasPosition)
                    {
                        positionId = _payload_msg.Position.PositionId;
                    }
                    break;
                case (int)OpenApiLib.ProtoOAPayloadType.OA_AUTH_RES:
                    //var payload = msgFactory.GetAuthorizationResponse(rawData);
                    isAuthorized = true;
                    break;
                case (int)OpenApiLib.ProtoOAPayloadType.OA_SPOT_EVENT:
                    {
                        var payload = msgFactory.GetSpotEvent(rawData);
                        var str = payload.SymbolName;
                        if (payload.HasAskPrice) str += " Ask: " + payload.AskPrice;
                        if (payload.HasBidPrice) str += " Bid: " + payload.BidPrice;
                        Console.WriteLine(str);
                        break;
                    }
                default:
                    break;
            };
        }

        #endregion

        #region Outgoing ProtoBuffer objects to Raw data

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

        List<MenuItem> MenuItems {
            get {
                if (menuItems == null)
                {
                    menuItems = new List<MenuItem>()
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
                }
                return menuItems;
            }
        }



        List<MenuItem> menuItems;

        #endregion Main Menu

        void SendPingRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreatePingRequest((ulong)DateTime.Now.Ticks);
#if TRACE_PING
            if (isDebugIsOn) Console.WriteLine("SendPingRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
#endif
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendHeartbeatEvent(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateHeartbeatEvent();
#if TRACE_HEARTBEAT
            if (isDebugIsOn) Console.WriteLine("SendHeartbeatEvent() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
#endif
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendAuthorizationRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateAuthorizationRequest(clientPublicId, clientSecret);
            if (isDebugIsOn) Console.WriteLine("SendAuthorizationRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendAuthorizationRequest()
        {
            var _msg = outgoingMsgFactory.CreateAuthorizationRequest(clientPublicId, clientSecret);
            if (isDebugIsOn) Console.WriteLine("SendAuthorizationRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueueSync.Enqueue(_msg.ToByteArray());
        }
        void SendSubscribeForTradingEventsRequest(long accountId, OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateSubscribeForTradingEventsRequest(accountId, AccessToken);
            if (isDebugIsOn) Console.WriteLine("SendSubscribeForTradingEventsRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendSubscribeForTradingEventsRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            SendSubscribeForTradingEventsRequest(AccountId, msgFactory, writeQueue);
        }
        void SendUnsubscribeForTradingEventsRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateUnsubscribeForTradingEventsRequest(AccountId);
            if (isDebugIsOn) Console.WriteLine("SendUnsubscribeForTradingEventsRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendGetAllSubscriptionsForTradingEventsRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateAllSubscriptionsForTradingEventsRequest();
            if (isDebugIsOn) Console.WriteLine("SendGetAllSubscriptionsForTradingEventsRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendGetAllSubscriptionsForSpotEventsRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateGetAllSpotSubscriptionsRequest();
            if (isDebugIsOn) Console.WriteLine("SendGetAllSubscriptionsForSpotEventsRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
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
            if (isDebugIsOn) Console.WriteLine("SendMarketOrderRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendMarketRangeOrderRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateMarketRangeOrderRequest(AccountId, AccessToken, "EURUSD", OpenApiLib.ProtoTradeSide.BUY, testVolume, 1.09, 10, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendMarketRangeOrderRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendLimitOrderRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateLimitOrderRequest(AccountId, AccessToken, "EURUSD", OpenApiLib.ProtoTradeSide.BUY, 1000000, 1.11, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendLimitOrderRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendAmendLimitOrderRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateAmendLimitOrderRequest(AccountId, AccessToken, orderId, 1.10, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendLimitOrderRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendStopOrderRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateStopOrderRequest(AccountId, AccessToken, "EURUSD", OpenApiLib.ProtoTradeSide.BUY, 1000000, 0.2, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendStopOrderRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendClosePositionRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateClosePositionRequest(AccountId, AccessToken, positionId, testVolume, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendClosePositionRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }
        void SendSubscribeForSpotsRequest(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            var _msg = msgFactory.CreateSubscribeForSpotsRequest(AccountId, AccessToken, "EURUSD", clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendSubscribeForSpotsRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueue.Enqueue(_msg.ToByteArray());
        }

        void NotImplementedCommand(OpenApiMessagesFactory msgFactory, Queue writeQueue)
        {
            Console.WriteLine("Action is NOT IMPLEMENTED!");
        }

        #region Request Messages

        protected void RequestSubscribeForSymbol(string symbol)
        {
            var _msg = outgoingMsgFactory.CreateSubscribeForSpotsRequest(AccountId, AccessToken, symbol, clientMsgId);
            if (isDebugIsOn) Console.WriteLine("SendSubscribeForSpotsRequest() Message to be send:\n{0}", OpenApiMessagesPresentation.ToString(_msg));
            writeQueueSync.Enqueue(_msg.ToByteArray());
        }

        #endregion


        #endregion Outgoing ProtoBuffer objects to Raw data...


        #region IAppTask

        CancellationToken? cancellationToken;

        public void Start(CancellationToken? cancellationToken = default(CancellationToken?))
        {
            this.cancellationToken = cancellationToken;

            if (cancellationToken.HasValue)
            {
                this.RunTask = Task.Factory.StartNew(Run, cancellationToken.Value);
            }
            else
            {
                this.RunTask = Task.Factory.StartNew(Run);
            }
        }

        public bool WaitForCompletion {
            get {
                return false;
            }
        }

        public Task RunTask {
            get; private set;
        }

        #endregion
    }
}

#endif