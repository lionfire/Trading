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

    [CascadingParameter(Name = "WorkspaceServices")]
    public IServiceProvider? WorkspaceServices { get; set; }

    private ObservableReaderWriterItemVM<string, BotEntity, BotVM>? VM { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        // Try workspace services first, fall back to root services (for debugging/testing)
        var effectiveServices = WorkspaceServices ?? ServiceProvider;

        if (WorkspaceServices == null)
        {
            Logger.LogWarning("WorkspaceServices cascading parameter not found. Falling back to root ServiceProvider. " +
                "For production use, this page should be rendered within a workspace layout that provides WorkspaceServices.");
        }

        // Get reader/writer from services
        var readerWriter = effectiveServices.GetService<IObservableReaderWriter<string, BotEntity>>();
        IObservableReader<string, BotEntity>? reader = readerWriter ?? effectiveServices.GetService<IObservableReader<string, BotEntity>>();
        IObservableWriter<string, BotEntity>? writer = readerWriter ?? effectiveServices.GetService<IObservableWriter<string, BotEntity>>();

        if (reader == null || writer == null)
        {
            Logger.LogError("Bot persistence services not registered. Reader: {ReaderAvailable}, Writer: {WriterAvailable}. " +
                "Source: {ServiceSource}",
                reader != null, writer != null, WorkspaceServices != null ? "Workspace" : "Root");

            Logger.LogError("This means either: 1) Workspace layout is not being used, or 2) Workspace configurators haven't run yet, or 3) BotEntity workspace configurator not registered");
            return;
        }

        Logger.LogInformation("Loaded Bot persistence services from {ServiceSource}", WorkspaceServices != null ? "Workspace" : "Root");

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
            // TODO: Show success message via Snackbar
        }
    }
}