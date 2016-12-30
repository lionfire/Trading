using Caliburn.Micro;
using LionFire.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Dash.Wpf
{
    public class JobViewModel : PropertyChangedBase, IHaveDisplayName
    {
        public IJob Job { get; set; }

        public string DisplayName { get; set; }
        public string Description { get; set; }


        #region Progress

        public double Progress
        {
            get { return progress; }
            set
            {
                if (progress == value) return;
                progress = value;
                NotifyOfPropertyChange(() => Progress);
                NotifyOfPropertyChange(() => IsCompleted);
            }
        }
        private double progress;

        #endregion


        #region IsCompleted

        public bool IsCompleted
        {
            get { return progress >= 1.0; }
        }

        #endregion


        #region IsFaulted

        public bool IsFaulted
        {
            get { return isFaulted; }
            set
            {
                if (isFaulted == value) return;
                isFaulted = value;
                NotifyOfPropertyChange(() => IsFaulted);
            }
        }
        private bool isFaulted;

        #endregion

        #region ErrorMessage

        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                if (errorMessage == value) return;
                errorMessage = value;
                NotifyOfPropertyChange(() => ErrorMessage);
            }
        }
        private string errorMessage;

        #endregion

        public JobViewModel Parent { get; set; }

    }

    public class JobsViewModel
    {
        public BindableCollection<JobViewModel> Jobs { get; set; } = new BindableCollection<JobViewModel>();
        public BindableCollection<JobViewModel> CompletedJobs { get; set; } = new BindableCollection<JobViewModel>();
    }
}
