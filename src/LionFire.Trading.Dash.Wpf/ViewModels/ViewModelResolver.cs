using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Dash.Wpf
{
    public class ViewModelResolver
    {
        public static Type GetViewModelType(string typeName)
        {
            switch (typeName)
            {
                //case "Bots":
                //    return typeof(BotsViewModel);
                //case "HistoricalData":
                //    return typeof(HistoricalDataViewModel);
                //case "Symbols":
                //    return typeof(SymbolsViewModel);
                default:
                    return Type.GetType("LionFire.Trading.Dash.Wpf." + typeName + "ViewModel");
            }
        }

    }

}
