#if cAlgo
using cAlgo.API;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading
{
    public interface IOnBar
    {
        void OnBar(object sender, TimeFrame timeFrame);
    }
}
