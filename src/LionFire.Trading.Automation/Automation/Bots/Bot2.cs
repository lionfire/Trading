namespace LionFire.Trading.Automation;

/// <summary>
/// Long if ATR has increased for N straight bars
/// </summary>
public abstract class Bot2<TParameters> : BotBase2<TParameters>
    where TParameters : PBot2<TParameters>
{

    //public Bot2(TParameters parameters, IBotController? botController = null) : base(parameters, botController)
    //{
    //}
}
