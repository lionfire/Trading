using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Link.Blazor.Components.Pages;

public class BotVM : ReactiveObject
{
    public string? BotId { get; set; }
}