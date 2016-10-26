using Newtonsoft.Json;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Transport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Message = QuickFix.Message;

namespace LionFire.Trading.QuickFixN.ConsoleTest
{
    public class MyQuickFixApp : MessageCracker, IApplication
    {
        Session _session = null;
        public SocketInitiator Initiator { get; set; }

        public void FromApp(Message msg, SessionID sessionID)
        {
            //Crack(msg, sessionID);
            l.Info("FromApp: " + msg.Header.GetString(Tags.MsgType));
        }
        public void OnCreate(SessionID sessionID)
        {
            _session = Session.LookupSession(sessionID);

        }
        public void OnLogon(SessionID sessionID) { Console.WriteLine("Logon - " + sessionID.ToString()); }
        public void OnLogout(SessionID sessionID) { Console.WriteLine("Logout - " + sessionID.ToString()); }


        public void FromAdmin(Message msg, SessionID sessionID)
        {

            Console.WriteLine("FromAdmin: " + msg.Header.GetString(Tags.MsgType));
        }
        public void ToAdmin(Message msg, SessionID sessionID)
        {
            var msgType = msg.Header.GetString(Tags.MsgType);
            Console.WriteLine("ToAdmin: " + msgType);
            if (msgType == "A")
            {
                var settings = this.settings;
                msg.SetField(new Username(settings.Account), true);
                msg.SetField(new Password(settings.Password), true);
            }
        }
        public void ToApp(Message message, SessionID sessionID)
        {
            var msgType = message.Header.GetString(Tags.MsgType);
            if (msgType == "V")
            {
                //message.SetField(new MDUpdateType(MDUpdateType.INCREMENTAL_REFRESH));  // 265

                //message.SetField(new MDEntryType(MDEntryType.BID), false); // 269
                //message.SetField(new MDEntryType(MDEntryType.OFFER), false); // 269
                //message.SetField(new NoMDEntryTypes(1)); // 267 Should be 2 TODO

                //message.SetField(new Symbol("EUR/USD")); // 55
                //message.SetField(new NoRelatedSym(1)); // 146

            }
            Console.WriteLine("ToApp: " + msgType);
            try
            {
                bool possDupFlag = false;
                if (message.Header.IsSetField(QuickFix.Fields.Tags.PossDupFlag))
                {
                    possDupFlag = QuickFix.Fields.Converters.BoolConverter.Convert(
                        message.Header.GetField(QuickFix.Fields.Tags.PossDupFlag)); /// FIXME
                }
                if (possDupFlag)
                    throw new DoNotSend();
            }
            catch (FieldNotFoundException)
            { }

            Console.WriteLine();
            Console.WriteLine("OUT: " + message.ToString());
        }

        //    public void OnMessage(
        //QuickFix.FIX44.NewOrderSingle ord,
        //SessionID sessionID)
        //    {
        //        //ProcessOrder(ord.Price, ord.OrderQty, ord.Account);
        //        //l.Info("");
        //    }

        private void SendMessage(Message m)
        {
            if (_session != null)
                _session.Send(m);
            else
            {
                // This probably won't ever happen.
                Console.WriteLine("Can't send message: session not created.");
            }
        }

        private static ILogger l = Log.Get();

        public class AccountSettings
        {
            public string Account { get; set; }
            public string Password { get; set; }
        }
        public AccountSettings settings {
            get {
                using (var sr = new StreamReader(new FileStream(@"c:\Trading\Accounts\icmarkets-demo.json", FileMode.Open)))
                {
                    return JsonConvert.DeserializeObject<AccountSettings>(sr.ReadToEnd());
                }
            }
        }
        internal void Run()
        {
            Initiator.Start();

            var settings = this.settings;



            bool isExiting = false;
            while (!isExiting)
            {
                Console.Write("> ");
                var key = Console.ReadKey(false);
                Console.WriteLine();
                try
                {
                    bool unhandled = false;
                    switch (key.Key)
                    {
                        case ConsoleKey.Q:
                            Console.WriteLine("Exiting");
                            isExiting = true;
                            break;
                        case ConsoleKey.H:
                            Console.WriteLine("Help: ");
                            break;
                        case ConsoleKey.L:
                            var logon = new Logon();

                            logon.Username = new Username(settings.Account);
                            logon.Password = new Password(settings.Password);

                            Session.SendToTarget(logon);
                            //SendMessage(logon);
                            break;
                        case ConsoleKey.F1:
                            Console.WriteLine("Initiator.IsLoggedOn: " + Initiator.IsLoggedOn);
                            Console.WriteLine("Initiator.IsStopped: " + Initiator.IsStopped);
                            //Console.WriteLine("Session: " + (_session != null));
                            Console.WriteLine("Logged on: " + _session?.IsLoggedOn);
                            break;
                        //case ConsoleKey.H:
                        //    break;
                        default:
                            unhandled = true;
                            break;
                    }
                    if (unhandled)
                    {
                        switch (key.KeyChar)
                        {
                            case '=':
                                {
                                    Console.Write(" Subscribe to: ");
                                    string symbol = Console.ReadLine();
                                    Console.WriteLine("Subscribing to " + symbol);
                                    var mdr = new MarketDataRequest(new MDReqID(symbol), new SubscriptionRequestType(SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES), new MarketDepth(0));


                                    mdr.MDUpdateType = new MDUpdateType(MDUpdateType.INCREMENTAL_REFRESH);

                                    var sym = new MarketDataRequest.NoRelatedSymGroup();
                                    sym.Symbol = new Symbol("1");
                                    mdr.AddGroup(sym);

                                    mdr.NoMDEntryTypes = new NoMDEntryTypes(2);
                                    mdr.AddGroup(new MarketDataRequest.NoMDEntryTypesGroup()
                                    {
                                        MDEntryType = new MDEntryType(MDEntryType.BID)
                                    });
                                    mdr.AddGroup(new MarketDataRequest.NoMDEntryTypesGroup()
                                    {
                                        MDEntryType = new MDEntryType(MDEntryType.OFFER)
                                    });

                                    //message.SetField(new MDEntryType(MDEntryType.BID), false); // 269
                                    //message.SetField(new MDEntryType(MDEntryType.OFFER), false); // 269
                                    //message.SetField(new NoMDEntryTypes(1)); // 267 Should be 2 TODO

                                    //message.SetField(new Symbol("EUR/USD")); // 55
                                    //message.SetField(new NoRelatedSym(1)); // 146

                                    SendMessage(mdr);
                                    break;
                                }
                            case '-':
                                {
                                    Console.Write(" Unubscribe to: ");
                                    string symbol = Console.ReadLine();
                                    Console.WriteLine("Unubscribing to " + symbol);
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.WriteLine(ex.ToString());
                    Console.WriteLine();
                }
            }


            Initiator.Stop();
        }
    }
}
