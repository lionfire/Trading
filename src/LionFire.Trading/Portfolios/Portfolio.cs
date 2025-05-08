using DeferredEvents;
using LionFire.Structures;
using LionFire.Trading.Analysis;
using LionFire.Trading.Bots;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Trading.Portfolios;

public class Portfolio : INotifyPropertyChanged
{
    #region Identity

    [Key]
    public string PortfolioId { get; set; }

    #endregion
    
    #region Construction

    public Portfolio() { EnsureHasId(); }
    public Portfolio(IEnumerable<string> ids) : this()
    {
        foreach (var id in ids)
        {
            Components.Add(new PortfolioComponent(id));
        }
    }

    public Portfolio(IEnumerable<PortfolioComponent> components)
    {
        foreach (var component in components)
        {
            Components.Add(component);
        }
    }

    public void EnsureHasId()
    {
        if (PortfolioId == null) PortfolioId = IdUtils.GenerateId(5);
    }
    #endregion

    #region Properties

    #region Name

    public string Name {
        get => name;
        set {
            if (name == value) return;
            name = value;
            OnPropertyChanged(nameof(Name));
        }
    }
    private string name;

    #endregion
    
    #region Comments

    public string Comments {
        get => comments;
        set {
            if (comments == value) return;
            comments = value;
            OnPropertyChanged(nameof(Comments));
        }
    }
    private string comments;

    #endregion

    public DateTime? Deleted { get; set; }

    #endregion
    
    #region Owned Objects

    public List<PortfolioComponent> Components { get; set; } = new List<PortfolioComponent>();

    #endregion

    #region Properties

    // OPTIMIZE Make this a dictionary?
    internal PortfolioComponent FindComponent(string id) => Components?.Where(c => c.ComponentId == id).FirstOrDefault();

    #endregion

    #region Derived Properties (OPTIMIZE - calculate once)

    [NotMapped]
    public IEnumerable<string> ComponentIds { // RENAME BacktestIds
        get => Components.Select(c=>c.BacktestResultId);
        //set {
        //    componentIds = value;
        //    OnComponentIdsChanged().Wait();
        //}
    }
    //private IEnumerable<string> componentIds;

    //protected async Task OnComponentIdsChanged() => await SetComponents(ComponentIds);

    public int TotalTrades => (int)Components.Where(b => b.BacktestResult != null).Select(b => b.BacktestResult.TotalTrades).Sum();
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
            return date == DateTime.MinValue ? (DateTime?)null : date;
        }
    }

    public IEnumerable<string> AllCorrelations => 
        allCorrelations ??= CorrelationUtils.AllCorrellationIds(Components);
    private IEnumerable<string> allCorrelations;

    #endregion

    #region Add

    public void AddRange(IEnumerable<PortfolioComponent> backtestResults)
    {
        foreach (var item in backtestResults) { Add(item, false); }
        //await ComponentsChanged.InvokeAsync(this, DeferredEventArgs.Empty);
    }

    public void Add(PortfolioComponent component, bool raiseEvents = true)
    {
        Components.Add(component);
        //if(raiseEvents) await ComponentsChanged.InvokeAsync(this, DeferredEventArgs.Empty);
    }

    #endregion

    #region Misc

    #region INotifyPropertyChanged Implementation

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    public override string ToString() => $"{{Portfolio {Name}, PortfolioId={PortfolioId}}}";

    #endregion
}
