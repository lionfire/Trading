#define TRACE_PROGRESSIVETASK
using LionFire.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace LionFire.Execution
{
    public class ProgressiveTask : IHasRunTask, IHasDescription, IHasProgress, IHasProgressMessage
    {


        public string Description { get; set; }

        public IObservable<double> Progress { get { return progress; } }
        private BehaviorSubject<double> progress = new BehaviorSubject<double>(double.NaN);

        public CancellationToken CancellationToken { get; set; }

        public Task RunTask { get; set; }

        public IObservable<string> ProgressMessage { get { return progressMessage; } }
        private BehaviorSubject<string> progressMessage = new BehaviorSubject<string>("Not yet started.");

        public void UpdateProgress(double progressFactor, string message = null)
        {
#if TRACE_PROGRESSIVETASK
            Console.WriteLine(this.GetType().Name + $" {progressFactor*100.0}% {message}");
#endif
            progress.OnNext(progressFactor);
            if (message != null) { progressMessage.OnNext(message); }
        }
    }
}
