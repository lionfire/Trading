
namespace LionFire.Trading.Alerts;

public interface ITradingAlertPublisherStrategy : ITradingAlertPublisher
{
    string Key => GetType().FullName ?? "";
}
