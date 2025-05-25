using LionFire.Mvvm;
using LionFire.Reactive.Persistence;
using LionFire.Trading.Automation;
using Microsoft.CodeAnalysis.Host;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Automation;

//public abstract class EntitiesVM<TKey, TValue, TValueVM> : ReactiveObject
//    where TValue : notnull
//    where TValueVM : notnull
//{


public class BotsVM : ReactiveObject //, ObservableDataViewVM<string, BotEntity>
//: AsyncVMSourceCacheVM<string, BotEntity, BotVM>
//, IInjectable<Func<TKey, TValue, TValueVM>>
{
    public BotsVM()
    {
    }

    //public IObservableReaderWriter<string, BotEntity>? Entities { get; set; }


}

