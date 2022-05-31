using Microsoft.Extensions.DependencyInjection;
using LionFire.Trading.HistoricalData.Sources;
using LionFire.Trading.HistoricalData.Retrieval;
using LionFire.ExtensionMethods.Dumping;
using LionFire;
using Oakton;
using Spectre.Console;
using LionFire.Trading;
using LionFire.Trading.Backtesting;

public class BacktestInput : CommonTradingInput
{

    [FlagAlias("balance", true)]
    public decimal StartingBalanceFlag { get; set; } = 1000m;
}

[Area("backtest")]
[Description("Conduct a single backtest", Name = "backtest")]
public class BacktestCommand : OaktonAsyncCommand<BacktestInput>
{
    public BacktestCommand()
    {
        //Usage("List available data").Arguments(x => x.ExchangeFlag, x => x.ExchangeAreaFlag, x => x.Symbol, x => x.IntervalFlag);
    }

    public override async Task<bool> Execute(BacktestInput input)
    {
        var host = input.BuildHost();

        var tba = new TBacktestAccount()
        {
            StartDate = input.FromFlag,
            EndDate = input.ToFlag,
            Exchange = input.ExchangeFlag,
            ExchangeArea = input.ExchangeAreaFlag,
            ExchangeMarketName = input.Symbol,
            StartingBalance = (double)input.StartingBalanceFlag,

            //BacktestSymbolSettings = new Dictionary<string, BacktestSymbolSettings>
            //{
            //    [input.Symbol] = new BacktestSymbolSettings
            //    {
            //        SpreadMode = BacktestSpreadMode.Random
            //    }
            //},
        };

        var t = new LionFire.Applications.Trading.BacktestTask(tba);
        await t.StartAsync();

        var table = new Table();
        table.AddColumn("Date");
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

        AnsiConsole.Write(table);

        return true;
    }
}
