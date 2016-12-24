using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LionFire.ExtensionMethods;

namespace LionFire.Avalon.CaliburnMicro
{
    public class LionFireScreen : Screen
    {
        public LionFireScreen()
        {
            this.DisplayName = this.GetType().Name.Replace("ViewModel", "").InsertSpaceBeforeCaps();
        }
    }

}
