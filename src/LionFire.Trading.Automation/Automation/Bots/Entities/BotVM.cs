using DynamicData;
using LionFire.Mvvm;
using LionFire.Referencing;
using LionFire.Structures;
using LionFire.Trading.Automation.Portfolios;
using LionFire.Trading.Backtesting;
using MediatR;
using YamlDotNet.Core.Tokens;

namespace LionFire.Trading.Automation;

public class BotVM : KeyValueVM<string, BotEntity> //: IKeyed<IReference>
{
    //IReference IKeyed<IReference>.Key=>

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

    // TODO: color of switch depends on whether it is real money or demo account 
    public bool IsLive { get; set; }

    public double? AD { get; set; }

    public IEnumerable<BacktestResult> BR
    {
        get
        {
            if (Value != null)
            {
                foreach (var x in Value.Backtests.Items)
                {

                }
            }
            yield break;

        }
    }

}

public class Portfolio2VM : KeyValueVM<string, Portfolio2>
{
    public Portfolio2VM(string key, Portfolio2 value) : base(key, value)
    {
    }
}
