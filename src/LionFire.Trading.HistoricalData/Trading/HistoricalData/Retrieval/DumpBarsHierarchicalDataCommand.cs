using LionFire.Trading.HistoricalData.Binance;
using LionFire.Trading.HistoricalData.Serialization;
using Oakton;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LionFire.ExtensionMethods.Dumping;
using LionFire.Trading.HistoricalData.Sources;

namespace LionFire.Trading.HistoricalData.Retrieval;

public class DumpBarsHierarchicalDataInput : HistoricalDataJobInput
{
}


[Description("Dump bars", Name = "bars")]
public class DumpBarsHierarchicalDataCommand : OaktonAsyncCommand<DumpBarsHierarchicalDataInput>
{
    public DumpBarsHierarchicalDataCommand()
    {
        Usage("Dump bars").Arguments(x => x.ExchangeFlag, x => x.ExchangeAreaFlag, x => x.Symbol, x => x.IntervalFlag);
    }

    public override async Task<bool> Execute(DumpBarsHierarchicalDataInput input)
    {
        var host = input.BuildHost();

        var source = host.Services.GetService<BarsFileSource>();

        var hdp = host.Services.GetService<IOptionsMonitor<HistoricalDataPaths>>()?.CurrentValue;
        //var logger = host.Services.GetService<ILogger<ListAvailableHierarchicalDataCommand>>();

        var dir = hdp.GetDataDir(input.ExchangeFlag, input.ExchangeAreaFlag, input.Symbol, input.TimeFrame);

        if (Directory.Exists(dir))
        {
            DateTime openTime;

            foreach (var path in Directory.GetFiles(dir))
            {
                var filename = Path.GetFileName(path);

                bool isFirstBarForFile = true;
                var (info, bars) = KlineFileDeserializer.Deserialize(path);

                openTime = info.Start;
                foreach (var bar in bars)
                {
                    try
                    {
                        if (input.TimeFrame.TimeSpan.HasValue)
                        {
                            if (openTime < input.FromFlag) continue;
                            Console.Write($"{openTime.ToString("yyyy-MM-dd HH:mm")} ");
                        }

                        if (isFirstBarForFile && input.VerboseFlag)
                        {
                            isFirstBarForFile = false;
                            Console.WriteLine($" - {filename}");
                            Console.WriteLine(info.Dump().ToString());
                        }

                        Console.WriteLine(bar.ToString());
                    }
                    finally
                    {
                        if (input.TimeFrame.TimeSpan.HasValue)
                        {
                            openTime += input.TimeFrame.TimeSpan.Value;
                        }
                    }
                    if (openTime >= input.ToFlag) break;
                }
            }
        }

        return false;
    }
}

