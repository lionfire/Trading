using Oakton;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LionFire.ExtensionMethods.Dumping;
using System.Text;
using Spectre.Console;
using System.Threading.Tasks;
using System.Linq;
using LionFire.Trading.Indicators;

namespace LionFire.Trading.HistoricalData;

public class ListIndicatorsInput : NetCoreInput { }

[Area("indicator")]
[Description("List available hierarchical data", Name = "list")]
public class ListIndicatorsCommand : OaktonAsyncCommand<ListIndicatorsInput>
{
    public ListIndicatorsCommand()
    {
        //Usage("List available data").Arguments(x => x.ExchangeFlag, x => x.ExchangeAreaFlag, x => x.Symbol, x => x.IntervalFlag);
    }

    public override async Task<bool> Execute(ListIndicatorsInput input)
    {
        var host = input.BuildHost();
        var provider = host.Services.GetRequiredService<IndicatorProvider>();

        var table = new Table();
        table.AddColumn("Abbreviation");
        table.AddColumn("Name");
        table.AddColumn("Long Name");
        table.AddColumn("Description");
        table.AddColumn("Tags");

        foreach (var item in (await provider.GetIndicators()).Values)
        {
            table.AddRow(item.Abbreviation, item.Name, item.LongName, item.Description ?? "", item.Tags?.Aggregate((x, y) => $"{x}, {y}") ?? "");
        }

        AnsiConsole.Write(table);

        return true;
    }
}


