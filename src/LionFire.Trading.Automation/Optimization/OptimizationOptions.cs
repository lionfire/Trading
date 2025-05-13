using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation;

public class OptimizationOptions
{
    public const string ConfigurationLocation = "Trading:Optimization";

    public bool ZipOutput { get; set; } = true;
}
