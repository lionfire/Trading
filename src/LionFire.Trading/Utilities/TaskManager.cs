using LionFire.Structures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Threading.Tasks
{
    // STATUS - beginnings

    public enum TaskFlags
    {
        None,
        Unowned = 1 << 0,
    }

    public class TrackedTask : INotifyPropertyChanged
    {
        public TrackedTask(Task task, TaskFlags flags = TaskFlags.Unowned)
        {
            this.Task = task;
            this.Flags = flags;
        }

        public IProgress<double> P { get; set; }

        public string Name { get; set; }

        public Task Task { get; set; }
        public TaskFlags Flags { get; set; }

        public bool IsFinished { get { return Task.IsCompleted; } }

        #region Misc

        public override string ToString()
        {
            return Name ?? "(unnamed task)";
        }


        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #endregion
    }

    public class TaskManagerSettings
    {
        public int KeepCompletedTasksInCurrentListForMilliseconds { get; set; } = 60000;
    }

    // TODO: Make this a service?
    public class TaskManager
    {
        public static TaskManager Default { get { return ManualSingleton<TaskManager>.GuaranteedInstance; } }

        public static void OnNewTask(Task task, TaskFlags flags = TaskFlags.None)
        {
        }
        public static void OnFireAndForget(Task task, TaskFlags flags = TaskFlags.Unowned, string name = null)
        {
            var tt = new TrackedTask(task, flags)
            {
                Name = name
            };
            if (task != null)
            {
                Task.Run(() =>
                {
                    if (tt.Name != null)
                    {
                        Debug.WriteLine($"Task started: {tt.Name}");
                    }
                    task.Wait();
                    Default.CurrentTasks.Remove(tt);
                    Default.OldTasks.Add(tt);
                    if (tt.Name != null)
                    {
                        Debug.WriteLine($"Task finished: {tt.Name}");
                    }
                });
            }
            Default.CurrentTasks.Add(tt);
        }

        public ObservableCollection<TrackedTask> OldTasks { get; private set; } = new ObservableCollection<TrackedTask>();

        public ObservableCollection<TrackedTask> CurrentTasks { get; private set; } = new ObservableCollection<TrackedTask>();
    }
}
