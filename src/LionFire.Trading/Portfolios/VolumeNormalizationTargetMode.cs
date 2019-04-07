namespace LionFire.Trading.Portfolios
{
    public enum VolumeNormalizationTargetMode
    {
        Unspecified = 0,
        
        None = 1,

        /// <summary>
        /// Potential problem: impossibly small position sizes
        /// Examples for VolumeNormalizationTargetMaxConstant:
        ///  - 2: algos that open a max of 2 
        /// </summary>
        ToConstant = 2,


        #region FUTURE, maybe

        ///// <summary>
        ///// Avoids impossibly small position sizes
        ///// </summary>
        //ScaleToMinTradeVolume = 3,

        //ToBacktestMaxTradeSize = 4,


        //ToMarketATR, 
        //ToDD,
        //ToAD,

        #endregion
    }
    //public enum PortfolioNormalizationSourceMaxMode
    //{
    //    Unspecified = 0,

    //    /// <summary>
    //    /// Example: normalize to 3x of minimum trade size. Uses VolumeNormalizationTargetMaxMultiple to set the multiple.
    //    /// </summary>
    //    MultipleOfMinTradeVolume = 4,
    //}
}
