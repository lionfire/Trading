using LionFire.Mvvm;
using LionFire.Reactive.Persistence;
using LionFire.Trading.Grains.Bots;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MudBlazor;
using Orleans;
using ReactiveUI;

namespace LionFire.Trading.Automation.Blazor.Bots;

public partial class Bot : ComponentBase, IDisposable
{
    [Parameter]
    public string? BotId { get; set; }

    [CascadingParameter(Name = "UserServices")]
    public IServiceProvider? UserServices { get; set; }

    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = null!;

    [Inject]
    private ILogger<Bot> Logger { get; set; } = null!;

    private IClusterClient? ClusterClient { get; set; }
    private ObservableReaderWriterItemVM<string, BotEntity, BotVM>? VM { get; set; }
    private RealtimeBotStatus? HarnessStatus { get; set; }
    private List<BotHarnessSession>? Sessions { get; set; }
    private LogLevel? _selectedLogLevel;
    private bool _operationInProgress;
    private System.Timers.Timer? _statusTimer;

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
                    _selectedLogLevel = v.LogLevel; // Now nullable
                    _breadcrumbs[1] = new BreadcrumbItem(v.Name ?? BotId, href: null, disabled: true);
                }
                InvokeAsync(StateHasChanged);
            });

        // Load harness status and sessions
        await RefreshHarnessInfo();
        StartStatusPolling();

        await base.OnParametersSetAsync();
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

    private Color GetStatusColor(RealtimeBotState state) => state switch
    {
        RealtimeBotState.Running => Color.Success,
        RealtimeBotState.CatchingUp => Color.Warning,
        RealtimeBotState.Starting => Color.Info,
        RealtimeBotState.Faulted => Color.Error,
        RealtimeBotState.Stopped => Color.Default,
        _ => Color.Default
    };

    private string GetStatusIcon(RealtimeBotState state) => state switch
    {
        RealtimeBotState.Running => Icons.Material.Filled.PlayArrow,
        RealtimeBotState.CatchingUp => Icons.Material.Filled.Sync,
        RealtimeBotState.Starting => Icons.Material.Filled.HourglassTop,
        RealtimeBotState.Faulted => Icons.Material.Filled.Error,
        RealtimeBotState.Stopped => Icons.Material.Filled.Stop,
        _ => Icons.Material.Filled.QuestionMark
    };

    private static string FormatDuration(TimeSpan duration)
        => $"{(int)duration.TotalHours:00}:{duration.Minutes:00}:{duration.Seconds:00}";

    public void Dispose()
    {
        _statusTimer?.Stop();
        _statusTimer?.Dispose();
    }
}
