//using BlazorComponents.ChartJS;
using LionFire.Trading;
using LionFire.Trading.Backtesting;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using LionFire.Parsing.String;

namespace LionFire.Trading.Backtesting
{
    // REFACTOR - Compare with BacktestResultHandle
    public class BacktestResultFileHandle
    {
        public bool IsSelected { get; set; }
        public string FileName => System.IO.Path.GetFileNameWithoutExtension(Path);

        public BacktestResult Meta { get; set; }
        public BacktestResult BacktestResult { get; set; }

        public string Id => Meta.Id;
                

        #region Path

        public string Path
        {
            get => path;
            set
            {
                path = value;
                if (path != null)
                {

                    HasTrades = path != null && File.Exists(TradesPath);
                    Meta = new BacktestResult();
                    Meta.AssignFromString(FileName);
                }
            }
        }
        private string path;

        #endregion

        public void Load()
        {
            //    if (!File.Exists(path))
            //    {
            //        Console.WriteLine("File does not exist: " + path);
            //        return;
            //    }
            BacktestResult = JsonConvert.DeserializeObject<BacktestResult>(File.ReadAllText(Path));
        }

        public bool IsChecked { get; set; }
        public bool HasTrades { get; private set; }

        public string TradesPath => Path.Replace(".json", ".trades.json");
        public _HistoricalTrade[] Trades { get; set; }

        public async Task<bool> TryLoadTrades()
        {
            if (HasTrades)
            {
                await Task.Run(() =>
                {
                    Trades = JsonConvert.DeserializeObject<_HistoricalTrade[]>(File.ReadAllText(TradesPath));
                });
                return true;
            }
            return false;
        }

    }
}
