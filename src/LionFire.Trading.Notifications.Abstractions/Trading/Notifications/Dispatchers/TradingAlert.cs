using Orleans;

namespace LionFire.Trading.Alerts;

[GenerateSerializer]
public class TradingAlert
{
    [Id(0)]
    public TradingAlertDomains Domains { get; set; }
    [Id(1)]
    public string? Key { get; set; }
    [Id(2)]
    public string? Message { get; set; }
    [Id(3)]
    public ExchangeSymbolTimeFrame? ExchangeSymbolTimeFrame { get; set; }
    [Id(4)]
    public bool IsTriggered { get; set; }
    [Id(5)]
    public List<IKline>? LastBars { get; set; }
    [Id(6)]
    public double? OverThresholdRatio { get; set; }

    [Id(7)]
    public string? Status { get; set; }

    [Id(8)]
    public string? AlertTypeCode { get; set; }

    [Id(9)]
    public int? Severity { get; set; }

    #region Derived

    public string? Symbol => ExchangeSymbolTimeFrame?.Symbol;
    public TimeFrame? TimeFrame => ExchangeSymbolTimeFrame?.TimeFrame;

    #endregion


}

public class SymbolWithAlerts
{
    public string? Symbol { get; set; }
    public List<TradingAlert>? Alerts { get; set; }
}
