
using DynamicData;
using LionFire.Data;
using LionFire.Data.Collections;
using LionFire.DependencyMachines;
using LionFire.Referencing;
using LionFire.Trading.Automation;
using LionFire.Trading.Automation.Bots;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MorseCode.ITask;
using Orleans.Serialization.Buffers;
using System.Collections;

namespace LionFire.Trading.Link.Blazor.Components.Layout;

public interface IEntityMount
{

}

public class EntityMount<TKey, TValue> : IEntityMount
{
    public IReference? Root { get; set; }

}

public class BotsNavMenuVM
{

}
public partial class BotsNavMenu
{
    #region Cascading Parameters

    [CascadingParameter]
    string? Workspace { get; set; }

    #endregion

    #region Parameters

    public IReference? Root => $"workspace:{Workspace}".ToReference();

    #region Derived

    public IEntityMount EntityMount
    {
        get => entityMount ?? new EntityMount<IReference, BotEntity> { Root = Root };
        set => entityMount = value;
    }
    IEntityMount? entityMount;

    #endregion

    #endregion

    BotsNavMenuVM NavVM { get; } = new();

    protected override Task OnInitializedAsync()
    {
        return base.OnInitializedAsync();
    }

    protected override Task OnParametersSetAsync()
    {
        EntityMount = new EntityMount<IReference, BotEntity> { Root = Root };

        return base.OnParametersSetAsync();
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            List<Type> BotTypes = [
                typeof(PAtrBot<double>),
                typeof(PDualAtrBot<double>),
                ];

            Creatables = BotTypes.Select(t => new CreatableBot(t)).ToList();

        }

        return base.OnAfterRenderAsync(firstRender);
    }

    void SetButtonText(int x)
    {
    }

    #region Quick

    public IEnumerable<CreatableBot> Creatables { get; set; } = [];


    #endregion

    void Create(CreatableBot creatableBot)
    {
        var pBot = ActivatorUtilities.CreateInstance(ServiceProvider, creatableBot.ParameterType);

        //ViewModel.Create();

    }

    void NavigateToBot(string botId)
    {
        NavigationManager.NavigateTo($"/bots/{botId}");
    }
}
