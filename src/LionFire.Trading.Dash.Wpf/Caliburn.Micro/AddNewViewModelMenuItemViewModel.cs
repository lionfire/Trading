using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Caliburn.Micro;
using System.Threading.Tasks;

namespace LionFire.Caliburn.Micro
{
    public class AddNewViewModelMenuItemViewModel : IHaveDisplayName
    {
        public IConductor Parent { get; set; }
        public Type Type { get; set; }
        public string DisplayName { get; set; }

        public Action<object> Initializer { get; set; }

        public void NewItemClicked()
        {
            var obj = Activator.CreateInstance(Type) as IScreen;
            if (obj != null)
            {
                if (Initializer != null) Initializer(obj);
                
                Parent.ActivateItem(obj);

            }
        }
    }

    
}
