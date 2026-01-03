using BlazorGridStack;
using BlazorGridStack.Models;
using LionFire.Mvvm;
using LionFire.Reactive.Persistence;
using LionFire.Trading.Grains.Bots;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using Orleans;
using ReactiveUI;

namespace LionFire.Trading.Automation.Blazor.Bots;

public partial class Bot : ComponentBase, IDisposable
{
    private const string LayoutStorageKey = "bot-dashboard-layout";

    [Parameter]
    public string? BotId { get; set; }

    [CascadingParameter(Name = "UserServices")]
    public IServiceProvider? UserServices { get; set; }

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = null!;

    [Inject]
    private ILogger<Bot> Logger { get; set; } = null!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = null!;

    private IClusterClient? ClusterClient { get; set; }
    private ObservableReaderWriterItemVM<string, BotEntity, BotVM>? VM { get; set; }
    private RealtimeBotStatus? HarnessStatus { get; set; }
    private BotAccountStatus? AccountStatus { get; set; }
    private List<BotHarnessSession>? Sessions { get; set; }
    private List<BotPositionStatus>? OpenPositions { get; set; }
    private List<BotClosedPositionStatus>? ClosedPositions { get; set; }
    private List<BotOrderStatus>? OpenOrders { get; set; }
    private LogLevel? _selectedLogLevel;
    private bool _operationInProgress;
    private System.Timers.Timer? _statusTimer;

    // BlazorGridStack
    private BlazorGridStackBody? _grid;
    private BlazorGridStackBodyOptions _gridOptions = new()
    {
        Column = 12,
        CellHeight = "80",
        Margin = "4",
        Float = true,
        Animate = true,
        DisableOneColumnMode = true
    };

    private List<BreadcrumbItem> _breadcrumbs = new()
    {
        new BreadcrumbItem("Bots", href: "/bots"),
        new BreadcrumbItem("Loading...", href: null, disabled: true)
    };

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrEmpty(BotId))
        {
            Logger.LogError("BotId parameter is required");
            return;
        }

        _breadcrumbs[1] = new BreadcrumbItem(BotId, href: null, disabled: true);

        // Get cluster client
        ClusterClient = ServiceProvider.GetService<IClusterClient>();

        // Try user services first, fall back to root services
        var effectiveServices = UserServices ?? ServiceProvider;

        if (UserServices == null)
        {
            Logger.LogWarning("UserServices cascading parameter not found. Falling back to root ServiceProvider.");
        }

        // Get reader/writer from services
        var readerWriter = effectiveServices.GetService<IObservableReaderWriter<string, BotEntity>>();
        IObservableReader<string, BotEntity>? reader = readerWriter ?? effectiveServices.GetService<IObservableReader<string, BotEntity>>();
        IObservableWriter<string, BotEntity>? writer = readerWriter ?? effectiveServices.GetService<IObservableWriter<string, BotEntity>>();

        if (reader == null || writer == null)
        {
            Logger.LogError("Bot persistence services not registered.");
            return;
        }

        Logger.LogInformation("Loaded Bot persistence services for bot {BotId}", BotId);

        // Create VM with services
        VM = new ObservableReaderWriterItemVM<string, BotEntity, BotVM>(reader, writer);
        VM.Id = BotId;

        VM.WhenAnyValue(x => x.Value)
            .Subscribe(v =>
            {
                if (v != null)
                {
                    _selectedLogLevel = v.LogLevel;
                    _breadcrumbs[1] = new BreadcrumbItem(v.Name ?? BotId, href: null, disabled: true);
                }
                InvokeAsync(StateHasChanged);
            });

        // Load harness status and sessions
        await RefreshHarnessInfo();
        StartStatusPolling();

        await base.OnParametersSetAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Restore saved layout from localStorage
            await RestoreLayout();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    private void StartStatusPolling()
    {
        _statusTimer = new System.Timers.Timer(3000);
        _statusTimer.Elapsed += async (s, e) =>
        {
            await RefreshHarnessInfo();
            await InvokeAsync(StateHasChanged);
        };
        _statusTimer.Start();
    }

    private async Task RefreshHarnessInfo()
    {
        if (ClusterClient == null || string.IsNullOrEmpty(BotId)) return;

        try
        {
            var grain = ClusterClient.GetGrain<IRealtimeBotHarnessG>(BotId);
            HarnessStatus = await grain.GetStatus();
            Sessions = await grain.GetSessions();
            AccountStatus = await grain.GetAccountStatus();
            OpenPositions = await grain.GetOpenPositions();
            ClosedPositions = await grain.GetClosedPositions();
            OpenOrders = await grain.GetOpenOrders();
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Failed to refresh harness info for {BotId}", BotId);
        }
    }

    private async Task RefreshSessions()
    {
        if (ClusterClient == null || string.IsNullOrEmpty(BotId)) return;

        try
        {
            var grain = ClusterClient.GetGrain<IRealtimeBotHarnessG>(BotId);
            Sessions = await grain.GetSessions();
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to refresh sessions for {BotId}", BotId);
        }
    }

    private async Task StartBot()
    {
        if (ClusterClient == null || VM?.Value == null || string.IsNullOrEmpty(BotId)) return;

        try
        {
            _operationInProgress = true;
            await InvokeAsync(StateHasChanged);

            var config = new RealtimeBotConfiguration
            {
                BotTypeName = VM.Value.BotTypeName ?? string.Empty,
                AccountId = $"{VM.Value.Exchange}.{VM.Value.ExchangeArea}:default",
                Parameters = new Dictionary<string, object>()
            };

            if (!string.IsNullOrEmpty(VM.Value.Symbol) && VM.Value.TimeFrame != null)
            {
                config.Markets.Add(new MarketSubscription
                {
                    Exchange = VM.Value.Exchange ?? "binance",
                    ExchangeArea = VM.Value.ExchangeArea ?? "futures",
                    Symbol = VM.Value.Symbol,
                    TimeFrame = VM.Value.TimeFrame.ToShortString()
                });
            }

            var grain = ClusterClient.GetGrain<IRealtimeBotHarnessG>(BotId);

            // Set log level before starting
            await grain.SetLogLevel(_selectedLogLevel);

            var result = await grain.Start(config);
            if (result)
            {
                Logger.LogInformation("Started bot {BotId}", BotId);
            }
            else
            {
                Logger.LogWarning("Failed to start bot {BotId}", BotId);
            }

            await RefreshHarnessInfo();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error starting bot {BotId}", BotId);
        }
        finally
        {
            _operationInProgress = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task StopBot()
    {
        if (ClusterClient == null || string.IsNullOrEmpty(BotId)) return;

        try
        {
            _operationInProgress = true;
            await InvokeAsync(StateHasChanged);

            var grain = ClusterClient.GetGrain<IRealtimeBotHarnessG>(BotId);
            await grain.Stop();
            Logger.LogInformation("Stopped bot {BotId}", BotId);
            await RefreshHarnessInfo();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error stopping bot {BotId}", BotId);
        }
        finally
        {
            _operationInProgress = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task Save()
    {
        if (VM?.Value == null) return;

        try
        {
            // Update log level in entity
            VM.Value.LogLevel = _selectedLogLevel;
            await VM.Write();
            Logger.LogInformation("Saved bot {BotId}", BotId);

            // Update log level on running harness if available
            if (ClusterClient != null && HarnessStatus?.State != RealtimeBotState.Stopped)
            {
                try
                {
                    var grain = ClusterClient.GetGrain<IRealtimeBotHarnessG>(BotId!);
                    await grain.SetLogLevel(_selectedLogLevel);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning(ex, "Failed to update log level on running harness");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving bot {BotId}", BotId);
        }
    }

    #region Layout Persistence

    private Task OnLayoutChanged(BlazorGridStackWidgetListEventArgs args)
    {
        // TODO: Implement layout persistence with BlazorGridStack API
        // The Save/Load API requires serializing BlazorGridStackWidgetOptions
        Logger.LogDebug("Layout changed - persistence not yet implemented");
        return Task.CompletedTask;
    }

    private Task RestoreLayout()
    {
        // TODO: Implement layout restoration from localStorage
        // Requires deserializing saved widget positions and calling _grid.Load()
        return Task.CompletedTask;
    }

    #endregion

    public void Dispose()
    {
        _statusTimer?.Stop();
        _statusTimer?.Dispose();
    }
}
