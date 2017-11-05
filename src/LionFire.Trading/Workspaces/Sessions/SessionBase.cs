using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Execution;
using System.ComponentModel;
using LionFire.Execution.Executables;

namespace LionFire.Trading.Sessions
{
    public class SessionBase : ExecutableExBase, IExecutableEx, IControllableExecutable, INotifyPropertyChanged
    {
        public IAccount Account { get; set; }

        #region DesiredState

        public ExecutionStateEx DesiredExecutionState
        {
            get { return desiredState; }
            set
            {
                if (desiredState == value) return;
                desiredState = value;
                OnPropertyChanged(nameof(DesiredExecutionState));
            }
        }
        private ExecutionStateEx desiredState;

        protected virtual void OnDesiredStateChanged()
        {
        }

        #endregion

    }
}
