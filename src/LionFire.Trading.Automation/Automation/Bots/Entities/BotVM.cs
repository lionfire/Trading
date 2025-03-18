using DynamicData;
using LionFire.Mvvm;
using MediatR;
using Oakton.Descriptions;
using YamlDotNet.Core.Tokens;

namespace LionFire.Trading.Automation;

public class BotVM : KeyValueVM<string, BotEntity>
{
    public BotVM(string key, BotEntity value) : base(key, value)
    {
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

