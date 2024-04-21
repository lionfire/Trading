namespace LionFire.Trading.Automation;

public class StandardBot2<TParameters> : Bot2<TParameters>
      where TParameters : PBot2
{
    public StandardBot2(TParameters parameters) : base(parameters)
    {
    }

    public virtual void Open(long amount = long.MinValue) { }
    public virtual void Close(long amount = long.MinValue) { }
}
