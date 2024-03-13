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
    public DumpBarsHierarchicalDataInput Input { get; set; }

    public DumpBarsHierarchicalDataCommand()
    {
        Usage("Dump bars").Arguments(x => x.ExchangeFlag, x => x.ExchangeAreaFlag, x => x.Symbol, x => x.IntervalFlag);
    }

    public BarsFileSource? source { get; private set; }
    public BarFilesPaths? HistoricalDataPaths { get; private set; }

    public override async Task<bool> Execute(DumpBarsHierarchicalDataInput input)
    {
        this.Input = input;
        var host = input.BuildHost();
        source = host.Services.GetService<BarsFileSource>();
        HistoricalDataPaths = host.Services.GetService<IOptionsMonitor<BarFilesPaths>>()?.CurrentValue;
        //var logger = host.Services.GetService<ILogger<ListAvailableHierarchicalDataCommand>>();
        return await Execute();
    }

    public async Task<bool> Execute()
    {
        var dir = HistoricalDataPaths.GetDataDir(new(Input.ExchangeFlag, Input.ExchangeAreaFlag, Input.Symbol, Input.TimeFrame));

        if (Directory.Exists(dir))
        {
            DateTimeOffset openTime;

            foreach (var path in Directory.GetFiles(dir))
            {
                var filename = Path.GetFileName(path);
                if (filename == BarsInfo.InfoFileName) continue;

                bool isFirstBarForFile = true;
                KlineArrayInfo? info;
                IReadOnlyList<IKline>? bars;
                try
                {
                    (info, bars) = KlineFileDeserializer.Deserialize(path);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Failed to deserialize file: {path}", ex);
                }

                if (info == null)
                {
                    Console.WriteLine($"Failed to deserialize info from file: {path}");
                    continue;
                }
                if (bars == null)
                {
                    Console.WriteLine($"Failed to deserialize bars from file: {path}");
                    continue;
                }
                openTime = info.Start;
                foreach (var bar in bars)
                {
                    if (openTime >= Input.ToFlag) break;
                    try
                    {
                        if (Input.TimeFrame.TimeSpan > TimeSpan.Zero)
                        {
                            if (openTime < Input.FromFlag) continue;
                            Console.Write($"{openTime.ToString("yyyy-MM-dd HH:mm")} ");
                        }

                        if (isFirstBarForFile && Input.VerboseFlag)
                        {
                            isFirstBarForFile = false;
                            Console.WriteLine($" - {filename}");
                            Console.WriteLine(info.Dump().ToString());
                        }

                        Console.WriteLine(bar.ToString());
                    }
                    finally
                    {
                        if (Input.TimeFrame.TimeSpan > TimeSpan.Zero)
                        {
                            openTime += Input.TimeFrame.TimeSpan;
                        }
                    }
                }
            }
        }

        return false;
    }
}

