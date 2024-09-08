using DynamicData;
using LionFire.Trading.IO;

namespace LionFire.Trading.Automation;

public interface IBot2 : IMarketParticipant2
{
    string BotId { get; set; }
    IPMarketProcessor Parameters { get; set; }

    void Init() { }

    ValueTask Stop();
    ValueTask OnBacktestFinished();

    IObservableCache<IPosition, int> Positions { get; }
}

public interface IBot2<TPrecision> : IBot2
    where TPrecision : struct, INumber<TPrecision>
{
    IBotController<TPrecision>? Controller { get; set; }
}
public interface IBot2<TParameters, TPrecision> : IBot2<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    new TParameters Parameters { get; set; }
}
