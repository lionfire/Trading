using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Dash.Wpf
{
    public class SymbolsViewModel : WorkspaceScreen
    {

        public BindableCollection<SymbolViewModel> Items { get; set; }= new BindableCollection<SymbolViewModel>();

        protected override void OnActivate()
        {
            Debug.WriteLine(this.Parent);
            //Symbols.Add(new SymbolVM(null, null));
            //Symbols.Add(new SymbolVM(null, null));

            base.OnActivate();

            foreach (var symbol in this.SessionViewModel.Session.SymbolsAvailable)
            {
                Items.Add(new SymbolViewModel( this.SessionViewModel.Session.Account.GetSymbol(symbol)));
            }
        }
    }
}
