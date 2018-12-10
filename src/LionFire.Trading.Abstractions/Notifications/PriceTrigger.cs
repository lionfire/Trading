using System;

namespace LionFire.Trading.Notifications
{
    public class PriceTrigger
    {
        public int UserId { get; set; }
        public Guid AlertSinkId { get; set; }


        public TPriceAlert TPriceAlert { get; set; }


        //public void OnTrigger() => Console.WriteLine($"OnTrigger - {Id} - {TPriceAlert}");
    }
}
