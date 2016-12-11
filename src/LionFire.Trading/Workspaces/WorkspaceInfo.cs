using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;

namespace LionFire.Trading.Workspaces
{
    public class WorkspaceInfo
    {

        #region TestInfo

        public string TestInfo
        {
            get { return testInfo; }
            set
            {
                if (testInfo == value) return;
                testInfo = value;
                OnPropertyChanged(nameof(TestInfo));
            }
        }
        private string testInfo;

        #endregion


        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            var ev = PropertyChanged;
            if (ev != null) ev(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
