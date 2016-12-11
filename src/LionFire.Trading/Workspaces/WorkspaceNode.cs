using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace LionFire.Trading.Workspaces
{
    public class WorkspaceNode
    {
        public ObservableCollection<WorkspaceNode> Children { get; set; }
        public object Data { get; set; }
    }
}
