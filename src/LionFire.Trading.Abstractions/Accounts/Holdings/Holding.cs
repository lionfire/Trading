
namespace LionFire.Trading;

public abstract class Holding<TPrecision> : IHolding<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{

    #region Identity

    public string Symbol => PHolding.Symbol!;
    public string Key => Symbol;

    #endregion

    #region Parameters

    public IPHolding PHolding { get; }
    
    #endregion

    #region Lifecycle

    public Holding(IPHolding pHolding)
    {
        ArgumentNullException.ThrowIfNull(pHolding.Symbol);
        PHolding = pHolding;
    }

    #endregion

    #region State

    public abstract TPrecision Balance { get; set; }
    protected TPrecision balance;

    #endregion
}
