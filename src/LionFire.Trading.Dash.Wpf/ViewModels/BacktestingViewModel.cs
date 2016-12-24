using Caliburn.Micro;
using LionFire.Avalon.CaliburnMicro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xceed.Wpf.DataGrid;

namespace LionFire.Trading.Dash.Wpf
{

    
    public class BacktestingViewModel : WorkspaceScreen
    {

        #region ResultsFilterBox

        public string ResultsFilterBox
        {
            get { return resultsFilterBox; }
            set
            {
                if (resultsFilterBox == value) return;
                resultsFilterBox = value;
                NotifyOfPropertyChange(() => ResultsFilterBox);
            }
        }
        private string resultsFilterBox;

        #endregion



        #region Results

        public DataGridCollectionView Results
        {
            get { return results; }
            set
            {
                if (results == value) return;
                results = value;
                NotifyOfPropertyChange(() => Results);
            }
        }
        private DataGridCollectionView results;

        #endregion



    }

}
