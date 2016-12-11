using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Workspaces
{
    public class ExposureLimits
    {
        /// <summary>
        /// Syntax: special keyword, or a currency: "Equities, RiskOn, RiskOff, USD, EUR, JPY, Equities-US, Equities-EUR, etc."
        /// </summary>
        public string ExposureType { get; set; }

        public double MaxRiskPercent { get; set; }
        public double MaxRiskAmount { get; set; }
    }

}
