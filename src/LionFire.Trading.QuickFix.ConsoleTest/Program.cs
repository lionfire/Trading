using QuickFix;
using QuickFix.FIX44;
using QuickFix.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.QuickFixN.ConsoleTest
{
    class Program
    {

        private static ILogger l = Log.Get();

        //static string BrokerUID = "cServer";

        static void Main(string[] args)
        {
            l.Info($"----- {typeof(Program).FullName}.Main() -----");

            //SessionSettings settings = new SessionSettings(args[0]);

            var configFile = "config.demo.price.ini";

            SessionSettings settings = new SessionSettings(configFile);
            IApplication myApp = new MyQuickFixApp();
            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory logFactory = new FileLogFactory(settings);

            var socketInitiator = new SocketInitiator(myApp, storeFactory, settings);
            //new Session(myApp, storeFactory, 
            //new SocketInitiatorThread(socketInitiator, 

            //ThreadedSocketAcceptor acceptor = new ThreadedSocketAcceptor(
            //    myApp,
            //    storeFactory,
            //    settings,
            //    logFactory);

            //acceptor.Start();

            var logon = new Logon();
            
            logon.Username = new QuickFix.Fields.Username("3235730");
            logon.Password = new QuickFix.Fields.Password("LYMA4870");

            while (true)
            {
                Console.Write("> ");
                var key = Console.ReadKey(false);
                Console.WriteLine();
                switch (key.KeyChar)
                {
                    case 'q':
                        Console.WriteLine("Exiting");
                        break;
                    case 'h':
                        Console.WriteLine("Help: ");
                        break;
                    case 't':

                        break;
                    default:
                        break;
                }
            }

            //acceptor.Stop();

            l.Info($"----- {typeof(Program).FullName}.Main() end -----");
        }
    }
}
