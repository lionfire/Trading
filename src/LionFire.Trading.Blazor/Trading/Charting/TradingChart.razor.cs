using LightweightCharts.Blazor.Charts;
using LightweightCharts.Blazor.Customization;
using LightweightCharts.Blazor.Customization.Chart;
using LightweightCharts.Blazor.Customization.Enums;
using LightweightCharts.Blazor.Customization.Series;
using LightweightCharts.Blazor.DataItems;
using LightweightCharts.Blazor.Plugins;
using LightweightCharts.Blazor.Series;
using LionFire.Trading.HistoricalData.Retrieval;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System.Drawing;

namespace LionFire.Trading.Charting;

public partial class TradingChart : ComponentBase, IAsyncDisposable
{
    #region Dependencies

    [Inject]
    public ILogger<TradingChart> Logger { get; set; } = default!;

    [Inject]
    public IBars? BarsService { get; set; }

    [Inject]
    public ISymbolIdParser? SymbolIdParser { get; set; }

    #endregion

    #region Parameters

    [Parameter]
    public string? Symbol { get; set; }

    [Parameter]
    public string Exchange { get; set; } = "binance";

    [Parameter]
    public string ExchangeArea { get; set; } = "futures";

    [Parameter]
    public string TimeFrameString { get; set; } = "h1";

    [Parameter]
    public string Height { get; set; } = "400px";

    [Parameter]
    public int BarCount { get; set; } = 500;

    [Parameter]
    public bool AutoFit { get; set; } = true;

    /// <summary>
    /// Number of empty bars to show on the right side of the chart for visual padding.
    /// </summary>
    [Parameter]
    public int RightOffset { get; set; } = 5;

    /// <summary>
    /// If true, automatically scrolls to show the latest bar when new data arrives.
    /// </summary>
    [Parameter]
    public bool AutoScrollToRealTime { get; set; } = true;

    #endregion

    #region State

    private ChartComponent? _chart;
    private ElementReference _containerRef;
    private ISeriesApi<long>? _candlestickSeries;
    private ISeriesMarkersPluginApi<long>? _markersPlugin;
    private List<SeriesMarkerBar<long>> _currentMarkers = new();
    private bool _isInitialized;
    private string? _previousSymbol;
    private string? _previousTimeFrame;
    private string? _previousExchange;
    private string? _previousExchangeArea;

    public bool IsLoading { get; private set; }
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// The open time of the last bar on the chart. Used for gap detection.
    /// </summary>
    public DateTime? LastBarOpenTime { get; private set; }

    private string _selectedExchange = "binance";
    private string SelectedExchange
    {
        get => _selectedExchange;
        set
        {
            if (_selectedExchange != value)
            {
                _selectedExchange = value;
                Exchange = value;
                _ = ReloadChartAsync();
            }
        }
    }

    private string _selectedExchangeArea = "futures";
    private string SelectedExchangeArea
    {
        get => _selectedExchangeArea;
        set
        {
            if (_selectedExchangeArea != value)
            {
                _selectedExchangeArea = value;
                ExchangeArea = value;
                _ = ReloadChartAsync();
            }
        }
    }

    private string _selectedSymbol = "";
    private string SelectedSymbol
    {
        get => _selectedSymbol;
        set
        {
            if (_selectedSymbol != value)
            {
                _selectedSymbol = value;
                Symbol = value;
                _ = ReloadChartAsync();
            }
        }
    }

    #endregion

    #region Computed

    // Using absolute positioning, height is handled by CSS inset properties
    private string ContainerStyle => "";

    private TimeFrame? TimeFrame => LionFire.Trading.TimeFrame.TryParse(TimeFrameString);

    #endregion

    #region Lifecycle

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && _chart != null)
        {
            await InitializeChartAsync();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnParametersSetAsync()
    {
        // Initialize selected values from parameters
        if (_previousSymbol == null)
        {
            _selectedSymbol = Symbol ?? "";
            _selectedExchange = Exchange;
            _selectedExchangeArea = ExchangeArea;
        }

        var parametersChanged = _previousSymbol != Symbol
            || _previousTimeFrame != TimeFrameString
            || _previousExchange != Exchange
            || _previousExchangeArea != ExchangeArea;

        _previousSymbol = Symbol;
        _previousTimeFrame = TimeFrameString;
        _previousExchange = Exchange;
        _previousExchangeArea = ExchangeArea;

        if (_isInitialized && parametersChanged && !string.IsNullOrEmpty(Symbol))
        {
            await LoadDataAsync();
        }

        await base.OnParametersSetAsync();
    }

    public async ValueTask DisposeAsync()
    {
        // Detach markers plugin if it exists
        if (_markersPlugin != null)
        {
            try
            {
                await _markersPlugin.Detach();
            }
            catch
            {
                // Ignore errors during disposal
            }
            _markersPlugin = null;
        }

        _currentMarkers.Clear();

        // ChartComponent handles its own disposal
        _candlestickSeries = null;
        _isInitialized = false;
    }

    #endregion

    #region Chart Initialization

    private async Task InitializeChartAsync()
    {
        if (_chart == null) return;

        try
        {
            await _chart.InitializationCompleted;

            // Configure chart options for dark theme
            var chartOptions = new ChartOptions
            {
                AutoSize = true, // Auto-resize to container
                Layout = new LayoutOptions
                {
                    Background = new SolidColor { Color = ColorTranslator.FromHtml("#1e1e1e") },
                    TextColor = ColorTranslator.FromHtml("#d1d4dc")
                },
                Grid = new GridOptions
                {
                    VerticalLines = new GridLineOptions { Color = ColorTranslator.FromHtml("#2B2B43") },
                    HorizontalLines = new GridLineOptions { Color = ColorTranslator.FromHtml("#2B2B43") }
                },
                RightPriceScale = new PriceScaleOptions
                {
                    BorderColor = ColorTranslator.FromHtml("#2B2B43")
                },
                TimeScale = new TimeScaleOptions
                {
                    BorderColor = ColorTranslator.FromHtml("#2B2B43"),
                    TimeVisible = true,
                    RightOffset = RightOffset
                },
                Crosshair = new CrosshairOptions
                {
                    Mode = CrosshairMode.Normal
                }
            };

            await _chart.ApplyOptions(chartOptions);

            // Add candlestick series
            var candlestickOptions = new CandlestickStyleOptions
            {
                UpColor = ColorTranslator.FromHtml("#26a69a"),
                DownColor = ColorTranslator.FromHtml("#ef5350"),
                BorderUpColor = ColorTranslator.FromHtml("#26a69a"),
                BorderDownColor = ColorTranslator.FromHtml("#ef5350"),
                WickUpColor = ColorTranslator.FromHtml("#26a69a"),
                WickDownColor = ColorTranslator.FromHtml("#ef5350")
            };

            _candlestickSeries = await _chart.AddSeries<CandlestickStyleOptions>(SeriesType.Candlestick, candlestickOptions);

            _isInitialized = true;
            Logger.LogInformation("TradingChart initialized successfully");

            // Load data if symbol is set
            if (!string.IsNullOrEmpty(Symbol))
            {
                await LoadDataAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to initialize TradingChart");
            ErrorMessage = "Failed to initialize chart";
            StateHasChanged();
        }
    }

    #endregion

    #region Data Loading

    private async Task LoadDataAsync()
    {
        if (_candlestickSeries == null || string.IsNullOrEmpty(Symbol) || BarsService == null)
        {
            Logger.LogWarning("Cannot load data: Series={Series}, Symbol={Symbol}, BarsService={BarsService}",
                _candlestickSeries != null, Symbol, BarsService != null);
            return;
        }

        var timeFrame = TimeFrame;
        if (timeFrame == null)
        {
            ErrorMessage = $"Invalid timeframe: {TimeFrameString}";
            StateHasChanged();
            return;
        }

        IsLoading = true;
        ErrorMessage = null;
        StateHasChanged();

        try
        {
            // Calculate date range for historical data
            var endDate = DateTime.UtcNow;
            var barDuration = timeFrame.TimeSpan;
            var startDate = endDate - (barDuration * BarCount);

            var exchangeSymbolTimeFrame = new ExchangeSymbolTimeFrame(Exchange, ExchangeArea, Symbol, timeFrame);
            var range = SymbolBarsRange.FromExchangeSymbolTimeFrame(exchangeSymbolTimeFrame, startDate, endDate);

            Logger.LogInformation("Loading bars for {Symbol} {TimeFrame} from {Start} to {End}",
                Symbol, TimeFrameString, startDate, endDate);

            var result = await BarsService.Get(range);

            if (result?.Values != null && result.Values.Count > 0)
            {
                var chartData = ConvertToChartData(result.Values);
                await _candlestickSeries.SetData(chartData);

                // Track the last bar time for gap detection
                LastBarOpenTime = result.Values[^1].OpenTime;

                if (AutoFit && _chart != null)
                {
                    var timeScale = await _chart.TimeScale();
                    await timeScale.FitContent();
                }

                Logger.LogInformation("Loaded {Count} bars for {Symbol}, last bar at {LastBar}",
                    result.Values.Count, Symbol, LastBarOpenTime);
            }
            else
            {
                Logger.LogWarning("No data returned for {Symbol} {TimeFrame}", Symbol, TimeFrameString);
                ErrorMessage = $"No data available for {Symbol}";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to load chart data for {Symbol}", Symbol);
            ErrorMessage = $"Failed to load data: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private static List<CandlestickData<long>> ConvertToChartData(IReadOnlyList<IKline> bars)
    {
        return bars.Select(bar => new CandlestickData<long>
        {
            Time = new DateTimeOffset(bar.OpenTime, TimeSpan.Zero).ToUnixTimeSeconds(),
            Open = (double)bar.OpenPrice,
            High = (double)bar.HighPrice,
            Low = (double)bar.LowPrice,
            Close = (double)bar.ClosePrice
        }).ToList();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Manually refresh the chart data
    /// </summary>
    public Task RefreshAsync() => LoadDataAsync();

    /// <summary>
    /// Fit all data into view
    /// </summary>
    public async Task FitContentAsync()
    {
        if (_chart != null)
        {
            var timeScale = await _chart.TimeScale();
            await timeScale.FitContent();
        }
    }

    /// <summary>
    /// Update or append a single bar to the chart.
    /// If the bar's time matches the last bar, it updates it. Otherwise, it appends a new bar.
    /// </summary>
    public async Task UpdateBarAsync(IKline bar, bool scrollToRealTime = true)
    {
        if (_candlestickSeries == null)
        {
            Logger.LogWarning("Cannot update bar: series not initialized");
            return;
        }

        try
        {
            var chartData = new CandlestickData<long>
            {
                Time = new DateTimeOffset(bar.OpenTime, TimeSpan.Zero).ToUnixTimeSeconds(),
                Open = (double)bar.OpenPrice,
                High = (double)bar.HighPrice,
                Low = (double)bar.LowPrice,
                Close = (double)bar.ClosePrice
            };

            await _candlestickSeries.Update(chartData);

            // Track the last bar time for gap detection
            if (LastBarOpenTime == null || bar.OpenTime > LastBarOpenTime)
            {
                LastBarOpenTime = bar.OpenTime;
            }

            // Auto-scroll to show latest bar if enabled
            if (scrollToRealTime && AutoScrollToRealTime && _chart != null)
            {
                await ScrollToRealTimeAsync();
            }

            Logger.LogTrace("Updated chart with bar at {Time}", bar.OpenTime);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to update chart with bar at {Time}", bar.OpenTime);
        }
    }

    /// <summary>
    /// Update or append multiple bars to the chart.
    /// </summary>
    public async Task UpdateBarsAsync(IEnumerable<IKline> bars, bool scrollToRealTime = true)
    {
        foreach (var bar in bars)
        {
            // Don't scroll on each individual bar, only after all bars are added
            await UpdateBarAsync(bar, scrollToRealTime: false);
        }

        // Scroll once after all bars are added
        if (scrollToRealTime && AutoScrollToRealTime && _chart != null)
        {
            await ScrollToRealTimeAsync();
        }
    }

    /// <summary>
    /// Scroll the chart to show the latest bar (real-time position).
    /// </summary>
    public async Task ScrollToRealTimeAsync()
    {
        if (_chart != null)
        {
            try
            {
                var timeScale = await _chart.TimeScale();
                await timeScale.ScrollToRealTime();
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Failed to scroll to real time");
            }
        }
    }

    #endregion

    #region Markers (Deal Map)

    /// <summary>
    /// Set trade markers on the chart to visualize buy/sell entries and exits.
    /// </summary>
    /// <param name="trades">Collection of trade markers to display</param>
    public async Task SetTradeMarkersAsync(IEnumerable<ChartTradeMarker> trades)
    {
        if (_candlestickSeries == null)
        {
            Logger.LogWarning("Cannot set markers: series not initialized");
            return;
        }

        try
        {
            _currentMarkers.Clear();

            foreach (var trade in trades.OrderBy(t => t.Time))
            {
                var marker = new SeriesMarkerBar<long>
                {
                    Time = new DateTimeOffset(trade.Time, TimeSpan.Zero).ToUnixTimeSeconds(),
                    Position = trade.IsBuy ? SeriesMarkerBarPosition.BelowBar : SeriesMarkerBarPosition.AboveBar,
                    Shape = trade.IsBuy ? SeriesMarkerShape.ArrowUp : SeriesMarkerShape.ArrowDown,
                    Color = trade.Color ?? (trade.IsBuy ? Color.FromArgb(38, 166, 154) : Color.FromArgb(239, 83, 80)),
                    Text = trade.Text ?? (trade.IsBuy ? "Buy" : "Sell"),
                    Size = trade.Size ?? 1.0
                };

                if (!string.IsNullOrEmpty(trade.Id))
                {
                    marker.Id = trade.Id;
                }

                _currentMarkers.Add(marker);
            }

            if (_markersPlugin == null)
            {
                // Create the markers plugin for the first time
                _markersPlugin = await _candlestickSeries.CreateSeriesMarkers(_currentMarkers);
            }
            else
            {
                // Update existing markers
                await _markersPlugin.SetMarkers(_currentMarkers);
            }

            Logger.LogDebug("Set {Count} trade markers on chart", _currentMarkers.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to set trade markers");
        }
    }

    /// <summary>
    /// Add a single trade marker to the chart.
    /// </summary>
    public async Task AddTradeMarkerAsync(ChartTradeMarker trade)
    {
        if (_candlestickSeries == null)
        {
            Logger.LogWarning("Cannot add marker: series not initialized");
            return;
        }

        try
        {
            var marker = new SeriesMarkerBar<long>
            {
                Time = new DateTimeOffset(trade.Time, TimeSpan.Zero).ToUnixTimeSeconds(),
                Position = trade.IsBuy ? SeriesMarkerBarPosition.BelowBar : SeriesMarkerBarPosition.AboveBar,
                Shape = trade.IsBuy ? SeriesMarkerShape.ArrowUp : SeriesMarkerShape.ArrowDown,
                Color = trade.Color ?? (trade.IsBuy ? Color.FromArgb(38, 166, 154) : Color.FromArgb(239, 83, 80)),
                Text = trade.Text ?? (trade.IsBuy ? "Buy" : "Sell"),
                Size = trade.Size ?? 1.0
            };

            if (!string.IsNullOrEmpty(trade.Id))
            {
                marker.Id = trade.Id;
            }

            _currentMarkers.Add(marker);

            // Re-sort by time
            _currentMarkers = _currentMarkers.OrderBy(m => m.Time).ToList();

            if (_markersPlugin == null)
            {
                _markersPlugin = await _candlestickSeries.CreateSeriesMarkers(_currentMarkers);
            }
            else
            {
                await _markersPlugin.SetMarkers(_currentMarkers);
            }

            Logger.LogDebug("Added trade marker at {Time}", trade.Time);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to add trade marker");
        }
    }

    /// <summary>
    /// Clear all trade markers from the chart.
    /// </summary>
    public async Task ClearMarkersAsync()
    {
        _currentMarkers.Clear();

        if (_markersPlugin != null)
        {
            try
            {
                await _markersPlugin.SetMarkers(_currentMarkers);
                Logger.LogDebug("Cleared all trade markers");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to clear markers");
            }
        }
    }

    #endregion

    #region Private Methods

    private async Task ReloadChartAsync()
    {
        if (_isInitialized && !string.IsNullOrEmpty(Symbol))
        {
            await LoadDataAsync();
        }
    }

    #endregion
}
