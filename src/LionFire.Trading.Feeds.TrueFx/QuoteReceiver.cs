using LionFire.Instantiating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Feeds.TrueFx
{

    public class TQuoteReceiver : ITemplate<QuoteReceiver>
    {

        public List<string> PairsOfInterest { get; set; }
        //    = new List<string>
        //{
        //    "EUR/USD",
        //    "USD/CAD",
        //    "USD/JPY"
        //};

    }

    public class TickQuote
    {
        public decimal Bid { get; set; }
        public decimal Ask { get; set; }
    }


    /* Supported pairs:
     * 
     * Unauthenticated:
     * 
     * EUR/USD 
    * USD/JPY
    * GBP/USD
    * EUR/GBP
    * USD/CHF
    * EUR/JPY
    * EUR/CHF
    * USD/CAD
    * AUD/USD
    * GBP/JPY
    * 
    * Authenticated also includes:
    * 
    * CAD/CHF
    * CAD/JPY
    * CHF/JPY
    * EUR/AUD
    * EUR/CHF
    * AUD/CAD
    * AUD/CHF
    * AUD/JPY
    * AUD/NZD
    * EUR/CAD
    * EUR/NOK
    * EUR/NZD
    * GBP/CAD
    * GBP/CHF
    * NZD/JPY
    * NZD/USD
    * USD/NOK
    * USD/SEK
    */

    public class QuoteReceiver : IDisposable
    {
        public QuoteReceiver()
        {
            Quote
            Run().Wait();
        }

        #region IDisposable

        bool isDisposed;
        public void Dispose() { isDisposed = true; }

        #endregion





        public async Task Run()
        {
            HttpClient c = new HttpClient();
            var baseUrl = "http://webrates.truefx.com/rates/connect.html";
            var url = baseUrl + "?f=csv";

            while (!isDisposed)
            {
                try
                {
                    var msg = await c.GetAsync(url);
                    var result = await msg.Content.ReadAsStringAsync();
                    Console.WriteLine("QUOTES: " + Environment.NewLine + result + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("QuoteReceiver Exception: " + ex.ToString());
                }
                Thread.Sleep(4000);
            }
        }
    }
}
