namespace LionFire.Trading.Automation;

public class PAssetProtection<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    #region (static)

    public static PAssetProtection<TPrecision> Default { get; }

    static PAssetProtection()
    {
        Default = new PAssetProtection<TPrecision>
        {
            AbortOnBalanceDrawdownPerunum = TPrecision.CreateChecked(0.5),
        };
    }

    #endregion
    public TPrecision AbortOnBalanceDrawdownPerunum { get; set; }
}
