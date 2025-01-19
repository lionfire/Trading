using LionFire.Blazor.Components;
using LionFire.Data.Async.Gets;
using LionFire.Data.Collections;
using LionFire.Mvvm;
using LionFire.Orleans_;
using LionFire.Structures;
using LionFire.UI.ViewModels;

namespace LionFire.Trading.Link.Blazor;

public partial class Workers
{
    KeyedCollectionView<IAddressable, IWorkerInfoG, WorkerInfoGVM>? ItemsEditor { get; set; }

    #region State

    public AsyncReadOnlyKeyedFuncCollection<IAddressable, IWorkerInfoG> Items { get; set; } = default!;

    #endregion
}

public interface IWorkerInfoG : Orleans.IGrainWithStringKey, IKeyedG
{
    ValueTask<string> Id();
    Task<WorkerStatus> Status();
}

[GenerateSerializer]
public class WorkerStatus
{
}

public interface IWorkerO : IGrainObserver, IWorkerInfoG
{
    //Task<SimServerStartResult> StartBot(Guid id);


    //Task Restart();
    //Task Shutdown();
}

public class WorkerO : IWorkerO
{
    public ValueTask<string> Id()
    {
        throw new NotImplementedException();
    }

    public Task<WorkerStatus> Status()
    {
        throw new NotImplementedException();
    }
}

public class WorkerInfoGVM : AsyncItemVM<IWorkerInfoG>
, IKeyed<string>
{
    public WorkerInfoGVM(IWorkerInfoG value) : base(value)
    {
        WorkerStatus = new DynamicGetter<IWorkerInfoG, WorkerStatus>(value, GetterOptions)
        {
            Getter = async (g, ct) => GetResult<WorkerStatus>.Success(await g.Status())
        };

        _ = WorkerStatus.Get();
    }

    public string Key => Value.GetPrimaryKeyString();


    GetterOptions GetterOptions = new GetterOptions()
    {
        AutoGet = true,
        AutoGetDelay = TimeSpan.FromSeconds(2)
    };

    public DynamicGetter<IWorkerInfoG, WorkerStatus> WorkerStatus { get; }

}

