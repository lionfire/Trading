namespace LionFire.Trading.Alerts;

public interface ITradingAlertPublisher 
{
    Task Publish(TradingAlert tradingAlert);

}
