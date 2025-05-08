using LionFire.FlexObjects;
using LionFire.Referencing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation.Portfolios;

[LionFire.Ontology.Alias("Portfolio")]
public class Portfolio : FlexObject
{
    public string? Name { get; set; }
    public string? Comment { get; set; }


    public Dictionary<OptimizationRunReference, Dictionary<BacktestReference, BacktestBatchJournalEntry>>? OptimizationBacktests { get; set; }


    public object TryFind(OptimizationRunInfo batchInfo, BacktestBatchJournalEntry backtest)
    {
        throw new NotImplementedException();
    }
}
