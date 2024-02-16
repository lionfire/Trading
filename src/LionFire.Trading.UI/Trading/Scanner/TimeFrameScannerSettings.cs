
namespace LionFire.Trading.Scanner;

public class TimeFrameScannerSettings : IKeyed<string>
{
    public required TimeFrame TimeFrame { get; init; }
    public bool Available { get; set; }
    public bool Favorite { get; set; }
    public bool Alerts { get; set; }
    public bool Visible { get; set; }

    public int ChartBarsToShow { get; set; } = 90;

    public string Key => TimeFrame.ToShortString();

    public static implicit operator KeyValuePair<string, TimeFrameScannerSettings>(TimeFrameScannerSettings s) => new(s.Key, s);
}
