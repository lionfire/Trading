using LionFire.Trading;
using LionFire.Trading.ScottPlot_;
using MudBlazor;
using ScottPlot;
using ScottPlot.Blazor;
using Parameter = Microsoft.AspNetCore.Components.ParameterAttribute;

namespace LionFire.Trading.Charting;

public partial class ScottPlotChart
{

    [@Parameter]
    public int Width { get; set; } = 300;
    [@Parameter]
    public int Height { get; set; } = 180;
    [@Parameter]
    public bool ShowVolume { get; set; } = true;
    [@Parameter]
    public int VolumeHeight { get; set; } = 0;
    public int EffectiveVolumeHeight => VolumeHeight != 0 ? VolumeHeight : (Height / 3);

    [@Parameter]
    public int VolumeVerticalOverlap { get; set; } = 0;
    public int EffectiveVolumeVerticalOverlap => VolumeVerticalOverlap != 0 ? VolumeVerticalOverlap : EffectiveVolumeHeight;

    [@Parameter]
    public IEnumerable<IKline>? Bars { get; set; }

    [@Parameter]
    public string? Name { get; set; }
    [@Parameter]
    public TimeFrame? TimeFrame { get; set; }
    [@Parameter]
    public string? BaseAsset { get; set; }
    [@Parameter]
    public TimeSpan? TimeSpan { get; set; }

#if WebAssembly
    // BlazorPlot BlazorPlot { get; set; } = new();
#endif

    public byte[] ImageBytes { get; set; } = [];
    public byte[] VolumeImageBytes { get; set; } = [];

    protected override Task OnParametersSetAsync()
    {
#if WebAssembly
        // Bars.CreateScottPlot(plot: BlazorPlot.Plot, name: Name, timeSpan: TimeSpan);
#else
        {
            var plot = Bars.CreateScottPlot(name: Name, timeSpan: TimeSpan, frameless: true);
            Image img = plot.GetImage(Width, Height);
            ImageBytes = img.GetImageBytes();
        }

        if (ShowVolume)
        {
            var plot = Bars.CreateVolumeScottPlot(timeSpan: TimeSpan, frameless: true);
            Image? img = plot?.GetImage(Width, EffectiveVolumeHeight);
            VolumeImageBytes = img?.GetImageBytes() ?? [];
        }
#endif
        return base.OnParametersSetAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {

        // BlazorPlot.Plot.Add.Signal(Generate.Sin());
        // BlazorPlot.Plot.Add.Signal(Generate.Cos());
    }
}
