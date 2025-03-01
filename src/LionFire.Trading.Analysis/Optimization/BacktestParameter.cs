#nullable enable
using System;
using System.Collections.Generic;


namespace LionFire.Trading.Automation.Optimization;

public class BacktestParameter // Maybe: MOVE to LionFire.Trading or LionFire.Trading.Abstractions
{
    public string Name { get; set; }
    public Type? Type { get; set; }
    public decimal Min { get; set; }
    public decimal Max { get; set; }

    public bool HasFalse { get; set; }
    public bool HasTrue { get; set; }

    public HashSet<string> Values { get; set; }

    public string Field { get => field ?? Name; set => field = value; }
    private string field;
}

