using LionFire.Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace LionFire.Trading.Dash.Wpf
{
    public class NewMenuItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate SeparatorTemplate { get; set; }
        public DataTemplate SessionSelect { get; set; }
        public DataTemplate NewItem { get; set; }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item == null)
            {
                return SeparatorTemplate;
            }
            else if (item is SessionViewModel)
            {
                return SessionSelect;
            }
            else if (item is AddNewViewModelMenuItemViewModel)
            {
                return NewItem;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
