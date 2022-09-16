using QuickFix;
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

        //private static ILogger l = LionFire.Logging.Log.Get();

        //static string BrokerUID = "cServer";

        static void Main(string[] args)
        {
            //l.Info($"----- {typeof(Program).FullName}.Main() -----");

            //SessionSettings settings = new SessionSettings(args[0]);

            var configFile = "config.demo.price.ini";

            SessionSettings settings = new SessionSettings(configFile);

            MyQuickFixApp myApp = new MyQuickFixApp();
            IMessageStoreFactory storeFactory = new FileStoreFactory(settings);
            ILogFactory fileLogFactory = new FileLogFactory(settings);
            ILogFactory screenLogFactory = new ScreenLogFactory(settings);
            var socketInitiator = new SocketInitiator(myApp, storeFactory, settings, screenLogFactory);

            myApp.Initiator = socketInitiator;
            
            myApp.Run();

            //new Session(myApp, storeFactory, 
            //new SocketInitiatorThread(socketInitiator, 

            //ThreadedSocketAcceptor acceptor = new ThreadedSocketAcceptor(
            //    myApp,
            //    storeFactory,
            //    settings,
            //    logFactory);

            //acceptor.Start();

            
            //acceptor.Stop();

            //l.Info($"----- {typeof(Program).FullName}.Main() end -----");
        }
    }
}
