using LionFire.Trading.Optimization.Plans;
using Microsoft.AspNetCore.Components;

namespace LionFire.Trading.Automation.Blazor.Optimization.Matrix;

public partial class MatrixToolbar : ComponentBase
{
    [Parameter]
    public int SelectedCount { get; set; }

    [Parameter]
    public EventCallback<int> OnSetPriority { get; set; }

    [Parameter]
    public EventCallback OnEnableSelected { get; set; }

    [Parameter]
    public EventCallback OnDisableSelected { get; set; }

    [Parameter]
    public EventCallback OnResetToAutopilot { get; set; }

    [Parameter]
    public EventCallback OnClearSelection { get; set; }

    [Parameter]
    public string GradeFilter { get; set; } = "";

    [Parameter]
    public EventCallback<string> GradeFilterChanged { get; set; }

    [Parameter]
    public string StatusFilter { get; set; } = "";

    [Parameter]
    public EventCallback<string> StatusFilterChanged { get; set; }

    /// <summary>
    /// Available date ranges from the plan.
    /// </summary>
    [Parameter]
    public IReadOnlyList<OptimizationDateRange> DateRanges { get; set; } = [];

    /// <summary>
    /// Currently selected date range index.
    /// </summary>
    [Parameter]
    public int SelectedDateRangeIndex { get; set; }

    /// <summary>
    /// Fired when the selected date range changes.
    /// </summary>
    [Parameter]
    public EventCallback<int> SelectedDateRangeIndexChanged { get; set; }

    private bool _showPriorityDialog;
    private int _dialogPriority = 5;

    private void ShowSetPriorityDialog()
    {
        _dialogPriority = 5;
        _showPriorityDialog = true;
    }

    private void ClosePriorityDialog()
    {
        _showPriorityDialog = false;
    }

    private async Task ApplyPriority()
    {
        _showPriorityDialog = false;
        await OnSetPriority.InvokeAsync(_dialogPriority);
    }

    private async Task OnGradeFilterChanged(string value)
    {
        GradeFilter = value;
        await GradeFilterChanged.InvokeAsync(value);
    }

    private async Task OnStatusFilterChanged(string value)
    {
        StatusFilter = value;
        await StatusFilterChanged.InvokeAsync(value);
    }

    private async Task OnDateRangeIndexChanged(int value)
    {
        SelectedDateRangeIndex = value;
        await SelectedDateRangeIndexChanged.InvokeAsync(value);
    }
}
