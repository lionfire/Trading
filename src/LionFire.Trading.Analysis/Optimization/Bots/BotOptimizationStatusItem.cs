using CommunityToolkit.Mvvm.ComponentModel;
using LionFire.Trading.Structures;

namespace LionFire.Trading.Automation.Optimization;

public class BotOptimizationStatusItem : ObservableObject
{
    public string Bot { get; set; }

    
    public int Backtests { get; set; }
    public int OptimizationRunCount { get; set; }


    #region StatsList

    public List<OptimizationRunStats> StatsList
    {
        //get => statsList;
        set
        {
            //if (statsList == value) return;)
            //statsList = value;
            ScoreHistogram = new Histogram(0.1, 0.0, 1.0, value.Select(s => s.Score));
        }
    }
    //private List<OptimizationRunStats> statsList;

    #endregion

    public double Progress { get; set; }

    public BotOptimizationStatusData Data { get; set; }

    #region ScoreHistogram

    public Histogram ScoreHistogram
    {
        get => scoreHistogram;
        set => SetProperty(ref scoreHistogram, value);
    }
    private Histogram scoreHistogram;

    #endregion

}

public class BotOptimizationStatusData
{
    public string Notes { get; set; }
}