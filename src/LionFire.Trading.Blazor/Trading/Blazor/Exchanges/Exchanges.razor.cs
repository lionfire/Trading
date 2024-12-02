using LionFire.Trading.Exchanges;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace LionFire.Trading.Blazor.Exchanges;

public partial class Exchanges
{
    #region Dependencies


    #endregion

    #region Lifecycle

    protected override Task OnInitializedAsync()
    {

        return base.OnInitializedAsync();
    }

    #endregion

    #region State

    public IEnumerable<string> Items => ExchangeInfos.Items;

    #endregion

    #region Event Handlers

    void OnRowClick(DataGridRowClickEventArgs<string> args)
    {
        NavigationManager.NavigateTo($"/exchanges/{args.Item}");
    }

    #endregion

}