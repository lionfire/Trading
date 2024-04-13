using LionFire.Trading.Backtesting2;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Optimizing2;

public class IIndicatorDataRequirement
{
    public SymbolIdentifier SymbolIdentifier { get; set; }
    public DateTime? Start { get; set; }

}

public class DataContext
{
    public DateTime Start { get; set; }
    public DateTime EndExclusive { get; set; }

    Dictionary<string, DataSeries> dataSeries = new Dictionary<string, DataSeries>();

    public List<IIndicator> Indicators = new List<IIndicator>();

    public Task Initialize(bool preload = true)
    {
        return Task.CompletedTask;
    }
}

public class OptimizationRunner
{
    public ILogger<BacktestRunner> Logger { get; }
    public OptimizationJobParameters Parameters { get; }

    public OptimizationRunner(OptimizationJobParameters parameters, ILogger<BacktestRunner> logger)
    {
        Parameters = parameters;
        Logger = logger;
    }

    public Task Initialize()
    {
        return Task.CompletedTask;
    }

}
