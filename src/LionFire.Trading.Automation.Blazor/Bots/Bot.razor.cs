using LionFire.Mvvm;
using LionFire.Reactive.Persistence;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ReactiveUI;


namespace LionFire.Trading.Automation.Blazor.Bots;

public partial class Bot : ComponentBase
{

    [Parameter]
    public string? BotId { get; set; }

    /// <summary>
    /// User-level services where bots are stored.
    /// </summary>
    [CascadingParameter(Name = "UserServices")]
    public IServiceProvider? UserServices { get; set; }

    /// <summary>
    /// Root service provider fallback (for debugging/testing).
    /// </summary>
    [Inject]
    private IServiceProvider ServiceProvider { get; set; } = null!;

    [Inject]
    private ILogger<Bot> Logger { get; set; } = null!;

    private ObservableReaderWriterItemVM<string, BotEntity, BotVM>? VM { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (string.IsNullOrEmpty(BotId))
        {
            Logger.LogError("BotId parameter is required");
            return;
        }

        // Try user services first, fall back to root services (for debugging/testing)
        var effectiveServices = UserServices ?? ServiceProvider;

        if (UserServices == null)
        {
            Logger.LogWarning("UserServices cascading parameter not found. Falling back to root ServiceProvider. " +
                "For production use, this page should be rendered within a layout that provides UserServices.");
        }

        // Get reader/writer from services 
        var readerWriter = effectiveServices.GetService<IObservableReaderWriter<string, BotEntity>>();
        IObservableReader<string, BotEntity>? reader = readerWriter ?? effectiveServices.GetService<IObservableReader<string, BotEntity>>();
        IObservableWriter<string, BotEntity>? writer = readerWriter ?? effectiveServices.GetService<IObservableWriter<string, BotEntity>>();

        if (reader == null || writer == null)
        {
            Logger.LogError("Bot persistence services not registered. Reader: {ReaderAvailable}, Writer: {WriterAvailable}. " +
                "Source: {ServiceSource}. ",
                reader != null, writer != null, UserServices != null ? "User" : "Root");

            Logger.LogError("This means either: 1) Layout is not providing UserServices, or 2) UserBotServicesConfigurator hasn't run yet, or 3) BotEntity user-level configurator not registered");
            return;
        }

        Logger.LogInformation("Loaded Bot persistence services from {ServiceSource} for bot {BotId}",
            UserServices != null ? "User" : "Root", BotId);

        // Create VM with services
        VM = new ObservableReaderWriterItemVM<string, BotEntity, BotVM>(reader, writer);
        VM.Id = BotId;

        VM.WhenAnyValue(x => x.Value)
            .Subscribe(async _ => await InvokeAsync(StateHasChanged));

        await base.OnParametersSetAsync();
    }

    private async Task Save()
    {
        if (VM?.Value != null)
        {
            await VM.Write();
            Logger.LogInformation("Saved bot {BotId}", BotId);
            // TODO: Show success message via Snackbar
        }
    }
}