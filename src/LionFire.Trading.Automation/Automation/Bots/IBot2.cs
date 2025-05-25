using DynamicData;
using LionFire.Trading.IO;

namespace LionFire.Trading.Automation;

public interface IBot2 : IMarketParticipant2
{
    static Type ParametersType { get; } = null!;

    string BotId { get; set; }
    IPMarketProcessor Parameters { get; set; }

    void Init() { }

    ValueTask Stop();
    ValueTask OnBacktestFinished();

}

public interface IBot2<TPrecision> : IBot2
    where TPrecision : struct, INumber<TPrecision>
{
    IObservableCache<IPosition<TPrecision>, int> Positions { get; }
    IBotController<TPrecision>? Controller { get; set; }
}
public interface IBot2<TParameters, TPrecision> : IBot2<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    new TParameters Parameters { get; set; }
}
