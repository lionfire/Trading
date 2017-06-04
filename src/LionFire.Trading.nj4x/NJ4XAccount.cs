using LionFire.Trading.Accounts;
using System;
using System;
using System.Configuration;
using nj4x;
using nj4x.Metatrader;
using static nj4x.Strategy;

namespace LionFire.Trading.nj4x
{
    public class TNJ4XAccount : TAccount
    {
    }

    public class NJ4XAccount : LiveAccountBase<TNJ4XAccount>
    {
        protected override Symbol CreateSymbol(string symbolCode)
        {
            throw new NotImplementedException();
        }
        public override MarketSeries CreateMarketSeries(string symbol, TimeFrame timeFrame)
        {
            throw new NotImplementedException();
        }

        public override TradeResult ClosePosition(Position position)
        {
            throw new NotImplementedException();
        }


        public override TradeResult ExecuteMarketOrder(TradeType tradeType, Symbol symbol, long volume, string label = null, double? stopLossPips = default(double?), double? takeProfitPips = default(double?), double? marketRangePips = default(double?), string comment = null)
        {
            throw new NotImplementedException();
        }

        public override TradeResult ModifyPosition(Position position, double? stopLoss, double? takeProfit)
        {
            throw new NotImplementedException();
        }
    }

    /* Get ticks:
     * 
     for (int i = 0; i < 10; i++)
 {
 double bid = mt4.Marketinfo(symbol: "EURUSD", type: MarketInfo.MODE_BID);
 double ask = mt4.Marketinfo("GBPUSD", MarketInfo.MODE_ASK);
 Console.WriteLine($"EURUSD bid={bid}");
 Console.WriteLine($"GBPUSD ask={ask}");
 Task.Delay(100).Wait();
 } */

    /* Make order

        try
 {
 var ticket = mt4.OrderSend(
 symbol: "EURUSD",
cmd: TradeOperation.OP_SELL,
volume: 0.1, price: bid,
 slippage: 2,
 stoploss: 0, takeprofit: 0,
comment: "my order",
magic: 0, expiration: MT4.NoExpiration
 );
    Console.WriteLine($"New order: {ticket}");
 }
 catch (MT4Exception e)
{
 Console.WriteLine($"Order placing error #{e.ErrorCode}: {e.Message}");
 }

    */

    //class Program
    //{
    //    static void Main(string[] args)
    //    {
    //        // Create strategy
    //        var mt4 = new Strategy();
    //        // Connect to the Terminal Server
    //        mt4.Connect(
    //        ConfigurationManager.AppSettings["terminal_host"],
    //        int.Parse(ConfigurationManager.AppSettings["terminal_port"]),
    //        new Broker(ConfigurationManager.AppSettings["broker"]),
    //        ConfigurationManager.AppSettings["account"],
    //        ConfigurationManager.AppSettings["password"]
    //        );

    //        // Use API methods …
    //        Console.WriteLine($"Account {mt4.AccountNumber()}");
    //        Console.WriteLine($"Equity {mt4.AccountEquity()}");

    //        Console.ReadLine();
    //    }
    //}
}
