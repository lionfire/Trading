using LionFire.Structures;
using LionFire.Trading.Analysis;
using LionFire.Trading.Bots;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Portfolios
{
    public class Portfolio
    {
        #region Identity

        [Key]
        public string PortfolioId { get; set; }

        public bool IsHistoricalBackup { get; set; }

        public string Name { get; set; }

        #endregion

        #region Construction

        public Portfolio() { PortfolioId = IdUtils.GenerateId(5); }
        public Portfolio(IEnumerable<string> ids) : this() { Components = ids.Select(id => new PortfolioComponent(id)).ToList(); }

        #endregion

        #region Owned Objects

        public List<PortfolioComponent> Components { get; set; }

        #endregion

        #region Properties

        // OPTIMIZE Make this a dictionary?
        internal PortfolioComponent FindComponent(string id) => Components?.Where(c => c.ComponentId == id).FirstOrDefault();

        #endregion

        #region Derived Properties (OPTIMIZE - calculate once)

        public int TotalTrades => (int)Components.Where(b=>b.BacktestResult != null).Select(b => b.BacktestResult.TotalTrades).Sum();
        public int WinningTrades => (int)Components.Where(b => b.BacktestResult != null).Select(b => b.BacktestResult.WinningTrades).Sum();
        public int LosingTrades => (int)Components.Where(b => b.BacktestResult != null).Select(b => b.BacktestResult.LosingTrades).Sum();

        public DateTime? Start {
            get {
                if (start == null)
                {
                    DateTime date = DateTime.MaxValue;

                    foreach (var b in Components.Where(b => b.BacktestResult != null))
                    {
                        if (b.BacktestResult.Start.HasValue && b.BacktestResult.Start.Value < date)
                        {
                            date = b.BacktestResult.Start.Value;
                        }
                    }
                    start = date == DateTime.MaxValue ? (DateTime?)null : date;
                }
                return start;
            }
        }
        private DateTime? start;

        public DateTime? End {
            get {
                DateTime date = DateTime.MinValue;

                foreach (var b in Components.Where(b => b.BacktestResult != null))
                {
                    if (b.BacktestResult.End.HasValue && b.BacktestResult.End.Value > date)
                    {
                        date = b.BacktestResult.End.Value;
                    }
                }
                return date == DateTime.MaxValue ? (DateTime?)null : date;
            }
        }

        public IEnumerable<string> AllCorrelations {
            get {
                if (allCorrelations == null)
                {
                    allCorrelations = CorrelationUtils.AllCorrellationIds(Components);
                }
                return allCorrelations;
            }
        }
        private IEnumerable<string> allCorrelations;
               
        #endregion

        public void SetComponents(List<string> backtestResultIds, bool addOnly = false)
        {
            if (addOnly) {
                Components.AddMissingFrom(backtestResultIds,
                        id => new PortfolioComponent(id),
                        (component, id) => component.BacktestResultId == id);
            }
            else {
                Components.SetTo(backtestResultIds, id => new PortfolioComponent(id), (component, id) => component.BacktestResultId == id);
            }
        }

        #region Add

        public void AddRange(IEnumerable<PortfolioComponent> backtestResults)
        {
            foreach (var item in backtestResults) { Add(item); }
        }

        public void Add(PortfolioComponent component)
        {
            if (Components == null) Components = new List<PortfolioComponent>();
            Components.Add(component);
        }

        #endregion

    }
}
