using System.Collections;
using System.Collections.Generic;

namespace LionFire.Trading.Bots
{

    public abstract partial class SignalBotBase<TIndicator, TConfig, TIndicatorConfig> : BotBase<TConfig>, ISignalBot
        where TIndicator : class, ISignalIndicator, new()
        where TConfig : TSignalBot<TIndicatorConfig>, new()
            where TIndicatorConfig : class, ITIndicator, new()
    {

        

    }
}