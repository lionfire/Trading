using LionFire.Trading.HistoricalData.Binance;
using LionFire.Trading.HistoricalData.Serialization;
using JasperFx.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LionFire.ExtensionMethods.Dumping;
using System.Text;
using Spectre.Console;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.HistoricalData.Sources;

namespace LionFire.Trading.HistoricalData;

[Area("data")]
[Description("List available historical data", Name = "list")]
public class ListAvailableHistoricalDataCommand : JasperFxAsyncCommand<HistoricalDataJobInput>
{
    public ListAvailableHistoricalDataCommand()
    {
        Usage("List available data").Arguments(x => x.ExchangeFlag, x => x.ExchangeAreaFlag, x => x.Symbol, x => x.IntervalFlag);
    }

    public override async Task<bool> Execute(HistoricalDataJobInput input)
    {
        var host = input.BuildHost();
        var barsFileSource = host.Services.GetRequiredService<BarsFileSource>();

        var ShowExtension = false;

        var table = new Table();
        table.AddColumn("Date");
        if (ShowExtension) { table.AddColumn("Ext"); }
        table.AddColumn("%", c => c.Alignment = Justify.Right);
        table.AddColumn("Bars", c => c.Alignment = Justify.Right);
        table.AddColumn("Expected", c => c.Alignment = Justify.Right);

        var listResult = await barsFileSource.List(new(input.ExchangeFlag, input.ExchangeAreaFlag, input.Symbol, input.TimeFrame));
        foreach (var item in listResult.Chunks)
        {
            table.AddRow(item.ChunkName, item.Percent.ToString(), item.Bars.ToString(), item.ExpectedBars.ToString());
            if (input.VerboseFlag) { Console.WriteLine(item.Dump().ToString()); }
        }

        AnsiConsole.Write(table);

        return true;
    }
}
