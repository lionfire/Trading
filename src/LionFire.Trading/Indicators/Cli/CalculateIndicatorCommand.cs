using Oakton;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LionFire.ExtensionMethods.Dumping;
using System.Text;
using Spectre.Console;
using System.Threading.Tasks;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.Trading.Indicators;

namespace LionFire.Trading.HistoricalData;

public class CalculateIndicatorInput : CommonTradingInput
{
    [FlagAlias("indicator", 'i')]
    public string IndicatorAbbreviation { get; set; }
}

[Area("indicator")]
[Description("Calculate indicator values for hierarchical data", Name = "calculate")]
public class CalculateIndicatorCommand : OaktonAsyncCommand<CalculateIndicatorInput>
{
    public CalculateIndicatorCommand()
    {
        //Usage("Calculate indicator values for ").Arguments(x => x.ExchangeFlag, x => x.ExchangeAreaFlag, x => x.Symbol, x => x.IntervalFlag);
    }

    public override async Task<bool> Execute(CalculateIndicatorInput input)
    {
        throw new NotImplementedException();
        var host = input.BuildHost();
        var indicators = host.Services.GetRequiredService<IndicatorProvider>();
        //var indictator = indicators.GetIndicator
        //var barsFileSource = host.Services.GetRequiredService<BarsFileSource>();

        //var ShowExtension = false;

        //var table = new Table();
        //table.AddColumn("Date");
        //if (ShowExtension) { table.AddColumn("Ext"); }
        //table.AddColumn("%", c => c.Alignment = Justify.Right);
        //table.AddColumn("Bars", c => c.Alignment = Justify.Right);
        //table.AddColumn("Expected", c => c.Alignment = Justify.Right);

        //var listResult = await barsFileSource.List(input.ExchangeFlag, input.ExchangeAreaFlag, input.Symbol, input.TimeFrame);
        //foreach (var item in listResult.Chunks)
        //{
        //    table.AddRow(item.ChunkName, item.Percent.ToString(), item.Bars.ToString(), item.ExpectedBars.ToString());
        //    if (input.VerboseFlag) { Console.WriteLine(item.Dump().ToString()); }
        //}

        //AnsiConsole.Write(table);

        //return true;
    }
}
