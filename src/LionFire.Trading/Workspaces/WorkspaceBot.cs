using LionFire.Instantiating;
using System;
using System.Collections.Generic;
using System.Linq;
using LionFire.Trading.Bots;
using System.Threading.Tasks;

namespace LionFire.Trading.Workspaces
{
    
    public class TWorkspaceBot : ITemplate<WorkspaceBot>
    {
        #region Construction

        public TWorkspaceBot() { }
        public TWorkspaceBot(string type, string id) { this.Type = type; this.Id = id; }
                
        #endregion

        public string Type { get; set; }

        public string Id { get; set; }
    }

    public class WorkspaceBot : ITemplateInstance<TWorkspaceBot>
    {
        public TWorkspaceBot Template { get; set; }
        ITemplate ITemplateInstance.Template { get { return Template; } set { Template = (TWorkspaceBot)value; } }

        public IBot Bot{get;set;}

    }

 }
