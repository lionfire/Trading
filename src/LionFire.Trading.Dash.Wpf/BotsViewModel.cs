using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LionFire.Structures;
using LionFire.Avalon;
using LionFire.Trading.Bots;

namespace LionFire.Trading.Dash.Wpf
{
    public class BotsViewModel : WorkspaceScreen, IHasStateType
    {

        public Type StateType { get { return typeof(BotsScreenState); } }

        public BotMode Mode { get; set; }

        //public VmCollection<BotVM, IBot> Bots { get; set; }

        #region Bots

        public BindableCollection<BotViewModel> Items
        {
            get { return items; }
            set
            {
                if (items == value) return;
                items = value;
                NotifyOfPropertyChange(() => Items);
            }
        }
        private BindableCollection<BotViewModel> items = new BindableCollection<BotViewModel>();

        #endregion



        protected override void OnActivate()
        {
            base.OnActivate();

            Items.Clear();
            IEnumerable<IBot> botC = Session.Session.AllBots.Where(b => b.Mode.HasFlag(Mode));
            foreach (var b in botC)
            {
                Items.Add(new BotViewModel(b));
            }
        }

    }
    public class BotsScreenState
    {
    }
}
