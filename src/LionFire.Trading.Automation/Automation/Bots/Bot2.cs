namespace LionFire.Trading.Automation;

public abstract class Bot2<TParameters, TPrecision> : BotBase2<TParameters, TPrecision>
    where TParameters : PBot2<TParameters>
    where TPrecision : struct, INumber<TPrecision>
{

    //public Bot2(TParameters parameters, IBotContext? botController = null) : base(parameters, botController)
    //{
    //}
}
