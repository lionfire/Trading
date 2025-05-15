﻿using DynamicData;
using LionFire.DynamicData_;
using LionFire.FlexObjects;
using LionFire.Referencing;
using Microsoft.CodeAnalysis;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation.Portfolios;

[LionFire.Ontology.Alias("Portfolio")]
public class Portfolio : ReactiveObject
{
    static ObjectIDGenerator generator = new ObjectIDGenerator();

    public long InstanceId { get; set; }
    public Portfolio()
    {
        InstanceId = generator.GetId(this, out bool firstTime);
        if (firstTime)
        {
            Debug.WriteLine($"Portfolio {InstanceId} created");
        }
        else
        {
            Debug.WriteLine($"Portfolio {InstanceId} re-created");
        }
    }

    public string? Name { get; set; }
    public string? Comment { get; set; }

    [Set]
    public SourceCache<
        (OptimizationRunReference key, SourceCache<BacktestReference, BacktestReference>), OptimizationRunReference>? OptimizationBacktestReferences
    { get; set; }
    object OptimizationBacktestReferencesLock = new();

    public SourceCache<
        (OptimizationRunReference key, SourceCache<(BacktestReference key, BacktestBatchJournalEntry value), BacktestReference> value), OptimizationRunReference>? OptimizationBacktests
    { get; set; }

    object OptimizationBacktestsLock = new();

    public void Toggle(OptimizationRunReference orr, BacktestBatchJournalEntry entry)
    {
        bool embed = false;

        if (!embed)
        {
            this.RaisePropertyChanging(nameof(OptimizationBacktestReferences));

            lock (OptimizationBacktestReferencesLock)
            {
                OptimizationBacktestReferences ??= new(tuple => tuple.key);

                SourceCache<BacktestReference, BacktestReference> level1;
                var level1O = OptimizationBacktestReferences.Lookup(orr);
                if (level1O.HasValue) { level1 = level1O.Value.Item2; }
                else
                {
                    level1 = new(x => x);
                    OptimizationBacktestReferences.AddOrUpdate((orr, level1));
                }

                BacktestReference r = entry;
                var level2O = level1.Lookup(r);
                if (level2O.HasValue) { level1.RemoveKey(r); }
                else { level1.AddOrUpdate(r); }
            }
            Debug.WriteLine($"Portfolio RaisePropertyChanged - {InstanceId}");
            this.RaisePropertyChanged(nameof(OptimizationBacktestReferences));
        }
        else
        {
            this.RaisePropertyChanging(nameof(OptimizationBacktests));

            lock (OptimizationBacktestsLock)
            {
                OptimizationBacktests ??= new(tuple => tuple.key);

                SourceCache<(BacktestReference, BacktestBatchJournalEntry), BacktestReference> level1;
                var level1O = OptimizationBacktests.Lookup(orr);
                if (level1O.HasValue) { level1 = level1O.Value.value; }
                else
                {
                    level1 = new SourceCache<(BacktestReference key, BacktestBatchJournalEntry value), BacktestReference>(tuple => tuple.key);

                    OptimizationBacktests.AddOrUpdate((orr, level1));
                }

                BacktestReference r = entry;
                var level2O = level1.Lookup(r);
                if (level2O.HasValue)
                {
                    level1.RemoveKey(r);
                }
                else
                {
                    level1.AddOrUpdate((r, entry));
                }
            }
            this.RaisePropertyChanged(nameof(OptimizationBacktests));
        }
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

    public override string ToString()
    {
        return $"Portfolio {Name} #{InstanceId} ({OptimizationBacktestReferences?.KeyValues.SelectMany(kv=>kv.Value.Item2.Keys).Count()} backtest refs)";
    }
}
