using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;

namespace LionFire.Trading.Dash.Wpf
{
    public class LayoutItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Template { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is IScreen)
            {
                return Template;
            }
            else
            {
                return base.SelectTemplate(item, container);
            }
        }
    }
}
