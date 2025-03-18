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
public abstract class ObservableDataViewVM<TKey, TValue> : ReactiveObject
where TValue : notnull
{
    //    //public IServiceProvider? ServiceProvider { get; set; }

    //    public IObservableReader<string, TValue>? Entities { get; set; }
    //    //public IObservableReaderWriter<string, TValue>? WritableEntities { get; set; }

    //    //public IObservableReaderWriter<string, TValueVM>? EntityVMs { get; set; }

    public void Init(IServiceProvider? serviceProvider)
    {
        Data =
            serviceProvider?.GetService<IObservableReaderWriter<string, BotEntity>>()
            ?? serviceProvider?.GetService<IObservableReader<string, BotEntity>>();
    }

    public IObservableReader<string, BotEntity>? Data { get; set; }

}

public class BotsVM : ObservableDataViewVM<string, BotEntity>
//: AsyncVMSourceCacheVM<string, BotEntity, BotVM>
//, IInjectable<Func<TKey, TValue, TValueVM>>
{
    public BotsVM()
    {
    }

    //public IObservableReaderWriter<string, BotEntity>? Entities { get; set; }


}


