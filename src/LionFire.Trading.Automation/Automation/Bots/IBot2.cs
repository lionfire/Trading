using LionFire.Trading.IO;

namespace LionFire.Trading.Automation;

public interface IBot2
{
    object Parameters { get; }
   

    IBotController Controller { get; set; }
}
public interface IBot2<TParameters> : IBot2
{
    new TParameters Parameters { get; set; }
}
