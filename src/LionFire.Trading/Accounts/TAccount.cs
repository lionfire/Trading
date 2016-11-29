using LionFire.Assets;
using LionFire.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Accounts
{
    [AssetPath(@"Accounts")]
    public abstract class TAccount : IHierarchicalTemplate, ITemplate
    {
        public string AccountId { get; set; }
        public string AccessToken { get; set; }
        //public string AccountName { get; set; }

        public string BrokerName { get; set; }
        public double CommissionPerMillion { get; set; }
        public string Currency { get; set; }
        public bool IsLive { get; set; }
        public double Leverage { get; set; }

        public int? BackFillMinutes { get; set; }

        public double StopOutLevel { get; set; } = double.NaN;

        public List<ITemplate> Children { get; set; }

        #region Derived

        public bool IsDemo { get { return !IsLive; } }

        #endregion
    }
}
