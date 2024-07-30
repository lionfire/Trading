using DynamicData;
using LionFire.Trading.IO;

namespace LionFire.Trading.Automation;

public interface IBot2 : IMarketParticipant2
{
    string BotId { get; set; }
    object Parameters { get; set; }


    IBotController? Controller { get; set; }

    void Init() { }

    IObservableCache<IPosition, int> Positions { get; }
}

public interface IBot2<TParameters> : IBot2
{
    new TParameters Parameters { get; set; }
}
