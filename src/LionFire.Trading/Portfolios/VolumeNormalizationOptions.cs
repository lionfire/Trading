namespace LionFire.Trading.Portfolios
{
    public class VolumeNormalizationOptions
    {

        #region Reduction

        public VolumeNormalizationReductionMode? ReductionMode { get; set; }

        /// <summary>
        /// If non-zero, volumes will be capped to a maximum of this value.
        /// </summary>
        public double? MaxSourceValue { get; set; }

        #endregion

        #region Max value for normalization

        public VolumeNormalizationTargetMode? MaxMode { get; set; }

        /// <summary>
        /// If Zero, use the max trade for the backtest
        /// </summary>
        public double? Max { get; set; } = 1.0;

        #endregion

        #region Curve

        public PortfolioNormalizationCurveType? Curve { get; set; }

        /// <summary>
        /// Example: With VolumeNormalizationMax set to one, use 0.5 or 0.8
        /// </summary>
        public double? StepThreshold { get; set; }

        /// <summary>
        /// Suggested starting point: 2
        /// </summary>
        public double? EasingExponent { get; set; }
        public static double DefaultEasingExponent = 2.0;

        #endregion

        // OLD - REVIEW
        ///// <summary>
        ///// Only applies if VolumeNormalizationTargetMax is set to MultipleOfMinTradeVolume
        ///// (Recommended value examples: 1, 3, 5, 10)
        ///// </summary>
        //public double VolumeNormalizationTargetMaxMultiple { get; set; }

        public override string ToString() => this.ToXamlAttribute();
    }
}
