using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Feeds.TrueFx
{
    /*
        public static class Program
    {
    public static void Main(string[] args)
    {

    }
    }*/

    public class QuoteReceiver : IDisposable
    {
        public QuoteReceiver()
        {
             Run().Wait();
        }

        #region IDisposable

        bool isDisposed;
        public void Dispose() { isDisposed = true; }

        #endregion

        List<string> pairsOfInterest = new List<string>
        {
            "EUR/USD",
            "USD/CAD",
            "USD/JPY"
        };

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
