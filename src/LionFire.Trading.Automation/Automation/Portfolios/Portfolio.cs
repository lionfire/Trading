using LionFire.FlexObjects;
using LionFire.Referencing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData;
using System.Reflection.Metadata.Ecma335;
using ReactiveUI;

namespace LionFire.Trading.Automation.Portfolios;

[LionFire.Ontology.Alias("Portfolio")]
public class Portfolio : ReactiveObject
{
    public string? Name { get; set; }
    public string? Comment { get; set; }


    public SourceCache<
        (OptimizationRunReference key, SourceCache<(BacktestReference key, BacktestBatchJournalEntry value), BacktestReference> value), OptimizationRunReference>? OptimizationBacktests
    { get; set; }
    object OptimizationBacktestsLock = new();

    public void Toggle(OptimizationRunReference orr, BacktestBatchJournalEntry entry)
    {
        this.RaisePropertyChanging(nameof(OptimizationBacktests));

        lock (OptimizationBacktestsLock)
        {

            BacktestReference r = entry;

            if (OptimizationBacktests == null)
            {
                OptimizationBacktests = new SourceCache<
            (OptimizationRunReference key, SourceCache<(BacktestReference, BacktestBatchJournalEntry), BacktestReference>), OptimizationRunReference>(tuple => tuple.Item1);
            }

            SourceCache<(BacktestReference, BacktestBatchJournalEntry), BacktestReference> level1;

            var level1O = OptimizationBacktests.Lookup(orr);
            if (level1O.HasValue) { level1 = level1O.Value.Item2; }
            else
            {
                level1 = new SourceCache<(BacktestReference key, BacktestBatchJournalEntry value), BacktestReference>(tuple => tuple.key);

                OptimizationBacktests.AddOrUpdate((orr, level1));
            }

            var level2O = level1.Lookup(r);
            if (level2O.HasValue) { 
                level1.RemoveKey(r);
            }
            else
            {
                level1.AddOrUpdate((r, entry));
            }
        }
        this.RaisePropertyChanged(nameof(OptimizationBacktests));
    }
    public bool IsInPortfolio(OptimizationRunReference orr, BacktestBatchJournalEntry entry)
    {
        BacktestReference r = entry;

        //if (OptimizationBacktests == null) { return false; }
        //if (!OptimizationBacktests.ContainsKey(orr)) { return false; }
        //if (OptimizationBacktests[orr].ContainsKey(r)) { return true; }

        if (OptimizationBacktests == null) { return false; }

        SourceCache<(BacktestReference, BacktestBatchJournalEntry), BacktestReference> level1;

        var level1O = OptimizationBacktests.Lookup(orr);
        if (!level1O.HasValue) { return false; }
        else { level1 = level1O.Value.Item2; }

        var level2O = level1.Lookup(r);
        if (level2O.HasValue) { return true; }
        else { return false; }
    }
}
