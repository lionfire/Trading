using LionFire.Trading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LionFire.Trading
{
    //public class BacktestBroker
    //{
    //    #region Construction
    //    public BacktestBroker() { }
    //    public BacktestBroker(string name) { this.Name = name; }

    //    #endregion

    //    public string Name { get; set; }

    //    public BacktestSymbolSettings DefaultBacktestSymbolSettings { get; set; }
    //}

    public class BrokerInfoUtils
    {
        public static string BrokersDir { get { return @"e:\temp\Brokers"; } }
        public static string SymbolsDirName { get { return "Symbols"; } }
        public static string AccountInfoFileName { get { return "AccountInfo"; } }
        public static string FileSuffix { get { return ".json"; } }

        public static string GetBrokerInfoDir(string brokerName)
        {
            return Path.Combine(BrokersDir, brokerName);
        }

        public static AccountInfo GetAccountInfo(string brokerName)
        {
            var dir = GetBrokerInfoDir(brokerName);
            var path = Path.Combine(dir, AccountInfoFileName + FileSuffix);

            using (var sr = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                var json = sr.ReadToEnd();
                var info = (AccountInfo)JsonConvert.DeserializeObject(json, typeof(AccountInfo));
                return info;
            }
        }

        public static string GetSymbolInfoPath(string brokerName, string symbolCode)
        {
            var dir = Path.Combine(GetBrokerInfoDir(brokerName), SymbolsDirName);
            return Path.Combine(dir, symbolCode + FileSuffix); 
        }
        
        public static SymbolInfo GetSymbolInfo(string brokerName, string symbolCode)
        {
            var path = GetSymbolInfoPath(brokerName, symbolCode);

            using (var sr = new StreamReader(new FileStream(path, FileMode.Open)))
            {
                var json = sr.ReadToEnd();
                var info = (SymbolInfo)JsonConvert.DeserializeObject(json, typeof(SymbolInfo));
                return info;
            }
        }
    }
    
}
