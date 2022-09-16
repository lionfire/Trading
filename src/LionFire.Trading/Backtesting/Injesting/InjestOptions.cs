public class InjestOptions
{
    /// <summary>
    /// Set this to true on the injestion host.
    /// Affects: 
    ///  - BacktestFileMover
    /// </summary>
    public bool Enabled { get; set; }


    // OLD: symbol/bot/tf
    public string BacktestsRoot_Old { get; set; } = @"F:\st\Investing-Output\.local\Results"; // HARDCODE HARDPATH

    public List<string> MultiMachineResultDirs { get; set; } = new List<string>()
    {
        @"F:\st\Investing-Output\Results", // HARDCODE HARDPATH
        @"F:\st\Investing-Output\.local\Machines", // HARDCODE HARDPATH
    };
    //public List<string> MarketsResultDirs { get; set; } = new List<string>()
    //{
    //};
}
