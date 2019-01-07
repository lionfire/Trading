namespace LionFire.Trading.Notifications
{
    public enum TPriceAlertType
    {
        Bid = 0,
        Ask,
        Last,

        /// <summary>
        /// Average of Bid and ask
        /// </summary>
        Avg,
    }
}
