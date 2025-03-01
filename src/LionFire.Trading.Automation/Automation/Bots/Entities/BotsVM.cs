using LionFire.Data.Async.Collections.DynamicData_;
using LionFire.Trading.Automation;
using Microsoft.CodeAnalysis.Host;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation;

public abstract class EntitiesVM<TKey, TValue, TValueVM> : ReactiveObject
{
}

public class BotsVM  : EntitiesVM<string, BotEntity, BotVM>
    //: AsyncVMSourceCacheVM<string, BotEntity, BotVM>
    //, IInjectable<Func<TKey, TValue, TValueVM>>
{
    public BotsVM()
    {
    }
}


