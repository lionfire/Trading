using LionFire.Trading.HistoricalData.Binance;
using LionFire.Trading.HistoricalData.Serialization;
using Oakton;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LionFire.ExtensionMethods.Dumping;

namespace LionFire.Trading.HistoricalData.Retrieval;

public class DumpBarsHierarchicalDataInput : HistoricalDataJobInput
{
}


[Description("Dump bars", Name = "dump")]
public class DumpBarsHierarchicalDataCommand : OaktonAsyncCommand<DumpBarsHierarchicalDataInput>
{
    public DumpBarsHierarchicalDataCommand()
    {
        Usage("Dump bars").Arguments(x => x.ExchangeFlag, x => x.ExchangeAreaFlag, x => x.Symbol, x => x.IntervalFlag);
    }

    public override async Task<bool> Execute(DumpBarsHierarchicalDataInput input)
    {
        var host = input.BuildHost();

        var hdp = host.Services.GetService<IOptionsMonitor<HistoricalDataPaths>>()?.CurrentValue ;
        //var logger = host.Services.GetService<ILogger<ListAvailableHierarchicalDataCommand>>();

        var dir = hdp.GetDataDir(input.ExchangeFlag, input.ExchangeAreaFlag, input.Symbol, input.TimeFrame);

        if (Directory.Exists(dir))
        {
            foreach (var path in Directory.GetFiles(dir))
            {
                var filename = Path.GetFileName(path);
                Console.WriteLine($" - {filename}");

                var (info, bars) = KlineFileDeserializer.Deserialize(path);
                Console.WriteLine("info:");
                Console.WriteLine(info.Dump().ToString());

                foreach (var bar in bars)
                {
                    Console.WriteLine(bar.ToString());
                }
            }
        }

        return false;
    }
}

