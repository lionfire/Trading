using LionFire.Mvvm;

namespace LionFire.Trading.Link.Blazor.Components.Pages;

public class BotVM : IViewModel<BotEntity>
{
    public BotEntity? Value { get; }

    public BotVM(BotEntity botDocument)
    {
        Value = botDocument;
    }

    #region Event Handlers

    public ValueTask OnStart()
    {
        return ValueTask.CompletedTask;
    }
    public ValueTask OnStop()
    {
        return ValueTask.CompletedTask;
    }

    #endregion
}
