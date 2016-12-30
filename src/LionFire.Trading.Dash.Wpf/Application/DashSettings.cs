using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Dash
{
    

    public class DashSettings
    {

        public bool LoadLastActiveWorkspaces { get; set; } = true;
        public bool AllowMultipleWorkspaces { get; set; } = true;
        public bool CreateDefaultWorkspaceIfNoneExists { get; set; } = true;
        public bool IsAutoSaveEnabled { get; set; } = true;
        public string LastWorkspace { get; internal set; } = "Default";
    }
}
