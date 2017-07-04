using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LionFire.Instantiating;
using System.ComponentModel;
using LionFire.MultiTyping;
using LionFire.ExtensionMethods;
using System.Reflection;

namespace LionFire.Trading.Workspaces
{
    /// <summary>
    /// A workspace has a set of Workspace items, which are effectively top-level "documents" visible in the Dash.  Each may have their own child layouts, or set of child Workspace Items (not implemented).
    /// </summary>
    public class TWorkspaceItem : ITemplate<WorkspaceItem>
    {
        #region Construction

        public TWorkspaceItem() { }
        public TWorkspaceItem(string viewType, string session, object state = null, string parentId = null)
        {
            this.ViewModelType = viewType;
            this.Session = session;
            this.State = state;
            this.ParentId = parentId;
        }

        #endregion

        #region DisplayName

        public string DisplayName
        {
            get { return displayName; }
            set
            {
                if (displayName == value) return;
                displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }
        private string displayName;

        #endregion

        #region ShowInNav

        public bool ShowInNav
        {
            get { return showInNav; }
            set
            {
                if (showInNav == value) return;
                showInNav = value;
                OnPropertyChanged(nameof(ShowInNav));
            }
        }
        private bool showInNav;

        #endregion


        #region ItemState

        public string ItemState
        {
            get {
                return IsOpen ? "O" : ".";
                //return itemState;
            }
            //set
            //{
            //    if (itemState == value) return;
            //    itemState = value;
            //    OnPropertyChanged(nameof(ItemState));
            //}
        }
        //private string itemState;

        #endregion



        #region IsOpen

        public bool IsOpen
        {
            get { return isOpen; }
            set
            {
                if (isOpen == value) return;
                isOpen = value;
                OnPropertyChanged(nameof(IsOpen));
                OnPropertyChanged(nameof(ItemState));
            }
        }
        private bool isOpen;

        #endregion


        public string Session { get; set; }

        // DEPRECATED - use new Instantiator instead, with a StateRestorer element
        public object State { get; set; }
        public string ViewModelType { get; set; }

        public bool IsSelected { get; set; }

        public string ParentId { get; set; }
        public string Id { get; set; } = Guid.NewGuid().ToString();

        #region IsEnabled

        public bool IsEnabled
        {
            get { return isEnabled; }
            set
            {
                if (isEnabled == value) return;
                isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        private bool isEnabled;

        #endregion

        #region Misc

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #endregion
    }

    public class WorkspaceItem : TemplateInstanceBase<TWorkspaceItem>, INotifyPropertyChanged
    {

        #region DisplayName

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(displayName))
                {
                    if (ViewModel == null) return "(null)";

                    var pi = ViewModel.GetType().GetTypeInfo().GetProperty("DisplayName");
                    string result = null;
                    if (pi != null)
                    {
                        result = pi.GetValue(ViewModel) as string;
                    }
                    return result ?? ViewModel.GetType().Name; // TODO: add spaces to name
                }
                return displayName;
            }
            set
            {
                if (displayName == value) return;
                displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }
        private string displayName;

        #endregion



        #region Relationships

        #region Workspace

        public TradingWorkspace Workspace
        {
            get { return workspace; }
            set
            {
                if (workspace == value) return;
                workspace?.Remove(this);
                workspace = value;
                workspace?.Add(this);
            }
        }
        private TradingWorkspace workspace;

        #endregion

        #region Derived

        #region Parent

        public WorkspaceItem Parent
        {
            get
            {
                if (parent == null)
                {
                    if (Workspace != null)
                    {
                        parent = Workspace.ItemsById.TryGetValue(Template.ParentId);
                    }
                }
                return parent;
            }
            set
            {
                if (parent == value) return;
                parent = value;
                OnPropertyChanged(nameof(Parent));
            }
        }
        private WorkspaceItem parent;

        #endregion

        #endregion

        #endregion

        public IEnumerable<WorkspaceItem> Children
        {
            get
            {
                if (Workspace == null) return Enumerable.Empty<WorkspaceItem>();
                return Workspace.ItemsById.Values.Where(wi => wi.Template.ParentId == Template.Id);
            }
        }

        private void Reset()
        {
            parent = null;
        }

        #region ViewModel

        public object ViewModel
        {
            get { return viewModel; }
            set
            {
                if (viewModel == value) return;
                viewModel = value;
                OnPropertyChanged(nameof(ViewModel));
            }
        }
        private object viewModel;

        #endregion

        #region Misc

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #endregion
    }

}
