using DynamicData;
using LionFire.Trading.IO;

namespace LionFire.Trading.Automation;

public interface IBot2 : IBarListener
{
    static Type ParametersType { get; } = null!;

    string BotId { get; set; }
    new IPBot2 Parameters { get; set; }

    void Init() { }

    ValueTask Stop();
    ValueTask OnBacktestFinished();

    IBotContext Context { get; set; }

}

public interface IBot2<TPrecision> : IBot2
    where TPrecision : struct, INumber<TPrecision>
{
    IObservableCache<IPosition<TPrecision>, int> Positions { get; }
    new IBotContext2<TPrecision>? Context { get; set; }
}
public interface IBot2<TParameters, TPrecision> : IBot2<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    new TParameters Parameters { get; set; }
}
