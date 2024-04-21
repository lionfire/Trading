namespace LionFire.Trading.Automation;

/// <summary>
/// Long if ATR has increased for N straight bars
/// </summary>
public class Bot2<TParameters> : BotBase2<TParameters>
    where TParameters : PBot2
{
    public Bot2(TParameters parameters) : base(parameters)
    {
    }


}
