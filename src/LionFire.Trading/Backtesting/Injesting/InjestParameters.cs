namespace LionFire.Trading.Backtesting;

public class InjestParameters
{
    public int MaxCount { get; set; }
    public bool WithTradesOnly { get; set; }

    public bool ContinueOnFail { get; set; }
    public string TimeFrame { get; set; }
    public string Algo { get; set; }
    public string Symbol { get; set; }
    public int Verbosity { get; set; }
    public bool Pretend { get; set; }
}
