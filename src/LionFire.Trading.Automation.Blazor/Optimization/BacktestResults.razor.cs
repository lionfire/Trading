using LionFire.FlexObjects;
using LionFire.Mvvm;
using LionFire.Trading.Automation.Portfolios;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Plotly.Blazor;
using Plotly.Blazor.Traces;
using Plotly.Blazor.Traces.ScatterLib;
using Radzen;
using System.Diagnostics;

namespace LionFire.Trading.Automation.Blazor.Optimization;

public partial class BacktestResults
{
    [CascadingParameter(Name = "WorkspaceServices")]
    public IServiceProvider? WorkspaceServices { get; set; }

    [CascadingParameter(Name = "WorkspaceId")]
    public string? WorkspaceId { get; set; }

    [CascadingParameter(Name = "Portfolio")]
    public Portfolio? Portfolio { get; set; }


    protected override Task OnInitializedAsync()
    {


        return base.OnInitializedAsync();
    }
    protected override void OnParametersSet()
    {
        if(ViewModel != null)
        {
            ViewModel.Portfolio = Portfolio;
        }

        base.OnParametersSet();
    }


    #region Drawer

    private bool _open;
    private Anchor _anchor = Anchor.Bottom;
    private bool _overlayAutoClose = true;

    private void OpenDrawer(Anchor anchor)
    {
        _open = true;
        _anchor = anchor;
    }

    #endregion

    #region DataGrid

    private SortMode _sortMode = SortMode.Multiple;
    private bool toggled = true;

    #endregion

}