using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace LionFire.Trading.Dash.Wpf
{
    public interface IWorkspaceViewModel : IScreen
    {
         SessionViewModel Session { get; set; }
        WorkspaceViewModel WorkspaceViewModel { get; set; }
    }
}
