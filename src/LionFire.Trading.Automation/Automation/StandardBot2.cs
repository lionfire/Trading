namespace LionFire.Trading.Automation;

public abstract class StandardBot2<TParameters> : Bot2<TParameters>
      where TParameters : PBot2<TParameters>
{
    public StandardBot2(TParameters parameters, IBotController botController) : base(parameters, botController)
    {
    }

    public virtual void Open(long amount = long.MinValue) { }
    public virtual void Close(long amount = long.MinValue) { }
}
