using LionFire.Mvvm;
using MudBlazor;
using Plotly.Blazor;
using Plotly.Blazor.Traces;
using Plotly.Blazor.Traces.ScatterLib;
using Radzen;

namespace LionFire.Trading.Automation.Blazor.Optimization;

public partial class BacktestResults
{

    protected override Task OnInitializedAsync()
    {
     

        return base.OnInitializedAsync();
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

    #endregion
}