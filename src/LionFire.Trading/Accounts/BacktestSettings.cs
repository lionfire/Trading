using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Accounts
{

      /*  public interface ISpreadProvider
        {
            double GetSpread(string code, DateTime time);
        }
*/

public enum SpreadMode
{
Fixed,
Random,
Lookup,
}

public class BacktestAccountSettings
{
    public BacktestSymbolSettings DefaultSymbolSettings{get;set;}

    public Dictionary<string, BacktestSymbolSettings> SymbolSettings{get;set;} = new Dictionary<string, BacktestSymbolSettings>();

    public double StopOutLevel{ get;set; } = 0.8;
}

public class BacktestSymbolSettings
{

        public SpreadMode SpreadMode{get;set;} = SpreadMode.Fixed;

        public double FixedSpread {get;set;} = 0.0;
        public double MinRandomSpread {get;set;} = 0.5;
        public double MaxRandomSpread {get;set;} = 2.0;

        public double MarginRequirement {get;set;} = 1.0 / 100.0 ;
        public double CommissionPerMillion { get; internal set; } = 30.0;
    }

    public class BacktestSettings
    {
        public double StartingBalance { get;set; } = 1000.0;
        

        public BacktestAccountSettings AccountSettings{get;set;} = new BacktestAccountSettings();

    }
}
