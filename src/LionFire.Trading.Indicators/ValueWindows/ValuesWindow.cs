namespace LionFire.Trading.ValueWindows;

public class ValuesWindow<T> : ValuesWindowBase<T>
{
    #region Lifecycle

    public ValuesWindow(int period) : base(period)
    {
    }

    #endregion

    #region Methods

    // REVIEW - derived classes may want to disable this method. What's the best way?  Move most of this to a base class and have this PushFront in a concrete class?
    public uint PushFront(T value)
    {
        throw new NotImplementedException();
    }

    #endregion

}
