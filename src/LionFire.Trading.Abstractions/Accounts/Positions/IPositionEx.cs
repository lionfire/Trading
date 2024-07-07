namespace LionFire.Trading
{
    public interface IPositionEx
    {
        double BalanceRisk { get; }
        double BalanceRiskPercent { get; }
        double BalanceRiskValue { get; }
        double MaxRisk { get; set; }
        double MaxRiskBalancePercent { get; set; }
        double? MaxStopLoss { get; set; }
        double Reward { get; }
        double Risk { get; }
        double RiskPercent { get; }
        double RiskValue { get; }
        double RRRatio { get; }
    }
}