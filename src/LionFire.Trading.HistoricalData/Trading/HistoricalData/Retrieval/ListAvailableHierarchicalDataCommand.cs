using LionFire.Trading.HistoricalData.Binance;
using LionFire.Trading.HistoricalData.Serialization;
using Oakton;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LionFire.ExtensionMethods.Dumping;
using System.Text;
using Spectre.Console;
using LionFire.Trading.HistoricalData.Persistence;

namespace LionFire.Trading.HistoricalData.Retrieval;

public class ListAvailableHierarchicalDataInput : HistoricalDataJobInput
{
}

[Description("List available hierarchical data", Name = "list")]
public class ListAvailableHierarchicalDataCommand : OaktonAsyncCommand<ListAvailableHierarchicalDataInput>
{
    public ListAvailableHierarchicalDataCommand()
    {
        Usage("List available data").Arguments(x => x.ExchangeFlag, x => x.ExchangeAreaFlag, x => x.Symbol, x => x.IntervalFlag);
    }

    public override async Task<bool> Execute(ListAvailableHierarchicalDataInput input)
    {
        var host = input.BuildHost();

        var hdp = host.Services.GetService<IOptionsMonitor<HistoricalDataPaths>>()?.CurrentValue;
        //var logger = host.Services.GetService<ILogger<ListAvailableHierarchicalDataCommand>>();

        var dir = hdp.GetDataDir(input.ExchangeFlag, input.ExchangeAreaFlag, input.Symbol, input.TimeFrame);

        if (Directory.Exists(dir))
        {
            var sb = new StringBuilder();

            var ShowExtension = false;
            var table = new Table();
            table.AddColumn("Date");
            if (ShowExtension) { table.AddColumn("Ext"); }
            table.AddColumn("%", c => c.Alignment = Justify.Right);
            table.AddColumn("Bars", c => c.Alignment = Justify.Right);
            table.AddColumn("Expected", c => c.Alignment = Justify.Right);

            foreach (var path in Directory.GetFiles(dir))
            {
                var extension = Path.GetExtension(path);

                var filename = Path.GetFileName(path);

                KlineArrayInfo info;
                try
                {
                    info = KlineFileDeserializer.DeserializeInfo(path);
                }
                catch (Exception)
                {
                    if (input.VerboseFlag)
                    {
                        Console.WriteLine($"Unrecognized file: {Path.GetFileName(path)}");
                    }
                    continue;
                }

                var expectedBarCount = input.TimeFrame.GetExpectedBarCount(info.Start, info.EndExclusive);
                var percent = (info.Bars / (double)expectedBarCount.Value).ToString("P1");
                var dateName = Path.GetFileNameWithoutExtension(path).Replace(KlineArrayFileConstants.PartialFileExtension, "");
                table.AddRow(dateName, percent, info.Bars.ToString(), expectedBarCount.ToString());

                if (input.VerboseFlag) { Console.WriteLine(info.Dump().ToString()); }
            }
            AnsiConsole.Write(table);
        }

        return false;
    }
}

