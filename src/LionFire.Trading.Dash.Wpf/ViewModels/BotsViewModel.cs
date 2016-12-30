using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LionFire.Structures;
using LionFire.Avalon;
using LionFire.Trading.Bots;
using LionFire.Execution;
using LionFire.Trading.Proprietary.Bots;

namespace LionFire.Trading.Dash.Wpf
{
    public class ExecutionStateChanged<T> 
    {
        public T Source { get; set; }
    }
    public class BotsViewModel : WorkspaceScreen, IHasStateType, IHandle<ExecutionStateChanged<IBot>>
    {
        public Type StateType { get { return typeof(BotsScreenState); } }

        #region Mode

        public BotMode Mode
        {
            get { return mode; }
            set
            {
                if (mode == value) return;
                mode = value;
                switch (mode)
                {
                    case BotMode.Live:
                        Items = SessionViewModel.LiveBots;
                        break;
                    case BotMode.Demo:
                        Items = SessionViewModel.DemoBots;
                        break;
                    case BotMode.Scanner:
                        Items = SessionViewModel.Scanners;
                        break;
                    case BotMode.Paper:
                        Items = SessionViewModel.PaperBots;
                        break;
                    default:
                        throw new Exception($"Invalid mode: {mode}");
                }
            }
        }
        private BotMode mode;

        #endregion

        #region Bots

        public VmCollection<BotViewModel, IBot> Items
        {
            get { return items; }
            set
            {
                if (items == value) return;
                items = value;
                NotifyOfPropertyChange(() => Items);
            }
        }
        private VmCollection<BotViewModel, IBot> items;

        #endregion


        #region SelectedItem

        public BotViewModel SelectedItem
        {
            get { return selectedItem; }
            set
            {
                if (selectedItem == value) return;
                selectedItem = value;
                NotifyOfPropertyChange(() => SelectedItem);
            }
        }
        private BotViewModel selectedItem;

        #endregion


        public class BotVMProvider : IViewModelProvider
        {
            public T GetFor<T>(object model, object context)
            {
                throw new NotImplementedException();
            }
        }


        protected override void OnActivate()
        {
            if (Mode == BotMode.None)
            {
                Mode = this.SessionViewModel.Session.Mode;
            }
            NotifyOfPropertyChange(() => IsAllEnabled);
            NotifyOfPropertyChange(() => AllScannersEnabled);
            base.OnActivate();

            //Items.Clear();
            //IEnumerable<IBot> botC = Session.Session.AllBots.Where(b => b.Mode.HasFlag(Mode));

            //foreach (var b in botC)
            //{
            //    Items.Add(new BotViewModel(b));
            //}
        }

        public void RefreshSelected()
        {
            BotViewModel botVM = SelectedItem as BotViewModel;
            if (botVM == null) return;

            botVM.UpdateSignalText();
            botVM.Update();
            LionTrender t = SelectedItem?.Bot as LionTrender;
            if (t != null)
            {
                t.Evaluate();
            }
        }

        public void Handle(ExecutionStateChanged<IBot> message)
        {
            if (items.Where(vm => vm.Bot == message.Source).Any())
            {
                NotifyOfPropertyChange(() => IsAllEnabled);
            }
        }


        #region IsAllEnabled

        // TODO: FIXME - make sure the change notification happens
        public bool IsAllEnabled
        {
            get
            {
                return !this.Items.Select(vm => vm.Bot).OfType<IExecutable>().Where(s => !s.IsStarted()).Any();
            }
            set
            {
                if (value)
                {
                    foreach (var bot in this.Items.Select(vm => vm.Bot).OfType<IStartable>())
                    {
                        bot.Start();
                    }
                }
                else
                {
                    foreach (var bot in this.Items.Select(vm=>vm.Bot).OfType<IStoppable>())
                    {
                        bot.Stop();
                    }
                }
                NotifyOfPropertyChange(() => IsAllEnabled);
            }
        }


        #endregion

        #region AllScannersEnabled

        // TODO: FIXME - make sure the change notification happens
        public bool AllScannersEnabled
        {
            get
            {
                return !this.Items.Where(s => !s.IsScanEnabled).Any();
            }
            set
            {
                foreach (var bot in Items)
                {
                    bot.IsScanEnabled = value;
                }
                NotifyOfPropertyChange(() => AllScannersEnabled);
            }
        }


        #endregion



    }
    public class BotsScreenState
    {
    }
}
