using LionFire.Execution;
using LionFire.Reactive;
using LionFire.Reactive.Subjects;
using LionFire.Templating;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using LionFire.Validation;
using System.Threading.Tasks;

namespace LionFire.Trading.Workspaces
{
    /// <summary>
    /// Trading session:
    ///  - One live account
    ///  - One demo account
    ///  - Mode switch toggles:
    ///     - live
    ///     - demo
    ///     - scan
    ///     - paper
    /// </summary>
    public class Session : TemplateInstanceBase<TSession>, IInitializable, IStartable
    {

        #region Properties

        public string Name { get; set; }
        public string Description { get; set; }

        public Workspace Workspace { get; set; }
        public IAccount LiveAccount { get; set; }
        public IAccount DemoAccount { get; set; }
        public IAccount PaperAccount { get; set; }
        public IAccount ScanAccount { get; set; }

        #endregion

        #region Lifecycle

        public Task<bool> Initialize()
        {
            this.Validate().PropertyNonDefault(nameof(Workspace), Workspace).EnsureValid();

            LiveAccount = Workspace.GetAccount(Template.LiveAccount);
            DemoAccount = Workspace.GetAccount(Template.DemoAccount);
            ScanAccount = Workspace.GetAccount(Template.ScanAccount);
            PaperAccount = Workspace.GetAccount(Template.PaperAccount);

            return Task.FromResult(true);
        }

        public Task Start()
        {
            LiveAccount?.TryAdd(this);
            DemoAccount?.TryAdd(this);
            ScanAccount?.TryAdd(this);
            PaperAccount?.TryAdd(this);
            return Task.CompletedTask;
        }

        #endregion

    }
    /*
    public class LiveSession : SessionBase
    {
    }
    public class DemoSession : SessionBase
    {
    }
    public class PaperSession : SessionBase
    {
    }
    public class ScannerSession : SessionBase
    {
    }*/
}
