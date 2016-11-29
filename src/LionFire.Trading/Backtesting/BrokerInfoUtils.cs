using LionFire.Trading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using LionFire.Assets;

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
        public static string BrokersDir { get { return @"c:\Trading\Brokers"; } }
        public static string SymbolsDirName { get { return "Symbols"; } }
        public static string AccountInfoFileName { get { return "AccountInfo"; } }
        public static string FileSuffix { get { return ".json"; } }

        public static string GetBrokerInfoDir(string brokerName)
        {
            return Path.Combine(BrokersDir, brokerName);
        }

        //public static AccountInfo GetAccountInfo(string brokerName)
        //{
            
        //    return brokerName.Load<AccountInfo>();

        //    //if (brokerName == null) return null;

        //    //var dir = GetBrokerInfoDir(brokerName);
        //    //var path = Path.Combine(dir, AccountInfoFileName + FileSuffix);

        //    //using (var sr = new StreamReader(new FileStream(path, FileMode.Open)))
        //    //{
        //    //    var json = sr.ReadToEnd();
        //    //    var info = (AccountInfo)JsonConvert.DeserializeObject(json, typeof(AccountInfo));
        //    //    return info;
        //    //}
        //}

        public static string GetSymbolInfoPath(string brokerName, string symbolCode = null)
        {
            var path = Path.Combine(GetBrokerInfoDir(brokerName), SymbolsDirName);
            if (symbolCode != null)
            {
                path = Path.Combine(path, symbolCode + FileSuffix);
            }
            return path;
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
        public static IEnumerable<string> GetSymbolsAvailable(string brokerName)
        {
            if (brokerName == null) { return null; }
            return Directory.GetFiles(GetSymbolInfoPath(brokerName), "*" + FileSuffix).Select(file => file.Substring(0, file.IndexOf(FileSuffix)));
        }
    }
    
}
