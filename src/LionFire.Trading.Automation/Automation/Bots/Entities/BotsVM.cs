using DynamicData;
using LionFire.Data.Collections;
using LionFire.Persistence;
using LionFire.Persistence.Filesystem;
using LionFire.Referencing;
using Microsoft.AspNetCore.Http.Features;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LionFire.Trading.Link.Blazor.Components.Pages;


public class BotsVM : ReactiveObject
{
    DocumentStoreVM<BotEntity> documentStoreVM = new();

    public AsyncReadOnlyKeyedFuncCollection<string, BotVM> Items { get; set; } = default!;


}

public interface ICollectionVM<TKey, TValue, TValueVM>
{

}


