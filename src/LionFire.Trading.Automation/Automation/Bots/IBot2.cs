using LionFire.Trading.IO;

namespace LionFire.Trading.Automation;

public interface IBot2
{
    object Parameters { get; set; }
   

    IBotController Controller { get; set; }
    void OnBar();
}
public interface IBot2<TParameters> : IBot2
{
    new TParameters Parameters { get; set; }
}
