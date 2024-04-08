namespace LionFire.Trading
{
    public class ProfitTarget
    {
        public double Target { get; set; }


        /// <summary>
        /// If set, close this much volume to take profits once the target is reached.
        /// </summary>
        public double? TakeProfitVolume { get; set; }

        /// FUTURE: Safeguard to prevent TP for a loss
        //public bool AllowTPForLoss { get; set; }

        // FUTURE
        public IProbability? Chance { get; set; }

    }
}
