//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.ComponentModel;
//using System.Threading.Tasks;

//namespace LionFire.Trading.Workspaces
//{
//    public class WorkspaceSettings : INotifyPropertyChanged
//    {

//        #region TestSetting

//        public string TestSetting
//        {
//            get { return testSetting; }
//            set
//            {
//                if (testSetting == value) return;
//                testSetting = value;
//                OnPropertyChanged(nameof(TestSetting));
//            }
//        }
//        private string testSetting;

//        #endregion


//        #region INotifyPropertyChanged Implementation

//        public event PropertyChangedEventHandler PropertyChanged;

//        private void OnPropertyChanged(string propertyName)
//        {
//            var ev = PropertyChanged;
//            if (ev != null) ev(this, new PropertyChangedEventArgs(propertyName));
//        }

//        #endregion

//    }
//}
