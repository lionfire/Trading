using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Execution;
using System.ComponentModel;

namespace LionFire.Trading.Sessions
{
    public class SessionBase : IExecutable, IControllableExecutable, INotifyPropertyChanged
    {
        public IAccount Account { get; set; }

        public Reactive.IBehaviorObservable<ExecutionState> State { get { return state; } }
        Reactive.Subjects.BehaviorObservable<ExecutionState> state = new Reactive.Subjects.BehaviorObservable<ExecutionState>(ExecutionState.Uninitialized);

        #region DesiredState

        public ExecutionState DesiredState
        {
            get { return desiredState; }
            set
            {
                if (desiredState == value) return;
                desiredState = value;
                OnPropertyChanged(nameof(DesiredState));
            }
        }
        private ExecutionState desiredState;

        protected virtual void OnDesiredStateChanged()
        {

        }

        #endregion

        #region Misc


        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion


        #endregion

    }
}
