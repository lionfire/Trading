using System.CommandLine;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Spectre.Console;
using LionFire.Hosting;
using LionFire.Hosting.CommandLine;
using LionFire.Trading.Symbols;
using LionFire.Trading.Symbols.Providers;

namespace LionFire.Trading.Cli.Commands;

public static class CryptoHandlers
{
    #region Market Cap Command

    public static void ConfigureMcapCommand(IHostingBuilderBuilder builderBuilder)
    {
        var cmd = builderBuilder.Command!;

        var limitArg = new Argument<int>("limit", () => 50, "Maximum number of results");
        cmd.AddArgument(limitArg);

        var quoteOption = new Option<string>("--quote", () => "USDT", "Quote currency filter");
        quoteOption.AddAlias("-q");
        cmd.AddOption(quoteOption);

        cmd.AddOption(new Option<bool>("--json", () => false, "Output as JSON"));
    }

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> Mcap =>
        (context, builder) =>
        {
            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
                var options = ParseMcapArgs(args);

                using var httpClient = new HttpClient();
                var provider = new CoinLoreSymbolProvider(
                    httpClient,
                    Options.Create(new CoinLoreProviderOptions()),
                    NullLogger<CoinLoreSymbolProvider>.Instance);

                var query = new SymbolCollectionQuery
                {
                    Exchange = "Binance",
                    Area = "spot",
                    QuoteCurrency = options.QuoteCurrency,
                    SortBy = "marketCap",
                    Direction = SortDirection.Descending,
                    Limit = options.Limit
                };

                try
                {
                    var results = await provider.GetTopSymbolsAsync(query);

                    if (options.Json)
                    {
                        OutputJson(results);
                    }
                    else
                    {
                        OutputMcapTable(results);
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
                    Environment.ExitCode = 1;
                }
            });
        };

    private static CryptoMcapOptions ParseMcapArgs(string[] args)
    {
        var options = new CryptoMcapOptions();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var nextArg = i + 1 < args.Length ? args[i + 1] : null;

            // Check for limit argument (positional, after "crypto mcap")
            if (int.TryParse(arg, out var limit) && limit > 0)
            {
                options.Limit = limit;
                continue;
            }

            switch (arg.ToLowerInvariant())
            {
                case "--quote":
                case "-q":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        options.QuoteCurrency = nextArg;
                        i++;
                    }
                    break;

                case "--json":
                    options.Json = true;
                    break;
            }
        }

        return options;
    }

    private static void OutputMcapTable(IReadOnlyList<SymbolMarketData> results)
    {
        var table = new Table();
        table.AddColumn(new TableColumn("Rank").RightAligned());
        table.AddColumn("Symbol");
        table.AddColumn(new TableColumn("Market Cap").RightAligned());
        table.AddColumn(new TableColumn("24h Volume").RightAligned());

        foreach (var item in results)
        {
            table.AddRow(
                $"#{item.MarketCapRank}",
                item.BaseCurrency,
                FormatCurrency(item.MarketCapUsd),
                FormatCurrency(item.Volume24hUsd));
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[dim]Source: CoinLore | {results.Count} results[/]");
    }

    #endregion

    #region Volume Command

    public static void ConfigureVolCommand(IHostingBuilderBuilder builderBuilder)
    {
        var cmd = builderBuilder.Command!;

        var limitArg = new Argument<int>("limit", () => 50, "Maximum number of results");
        cmd.AddArgument(limitArg);

        var exchangeOption = new Option<string>("--exchange", () => "Binance", "Exchange to query");
        exchangeOption.AddAlias("-e");
        cmd.AddOption(exchangeOption);

        var areaOption = new Option<string>("--area", () => "futures", "Exchange area (futures, spot)");
        areaOption.AddAlias("-a");
        cmd.AddOption(areaOption);

        var quoteOption = new Option<string>("--quote", () => "USDT", "Quote currency filter");
        quoteOption.AddAlias("-q");
        cmd.AddOption(quoteOption);

        cmd.AddOption(new Option<bool>("--json", () => false, "Output as JSON"));
    }

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> Vol =>
        (context, builder) =>
        {
            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
                var options = ParseVolArgs(args);

                using var httpClient = new HttpClient();
                var provider = new BinanceSymbolProvider(
                    httpClient,
                    Options.Create(new BinanceProviderOptions()),
                    NullLogger<BinanceSymbolProvider>.Instance);

                var query = new SymbolCollectionQuery
                {
                    Exchange = options.Exchange,
                    Area = options.Area,
                    QuoteCurrency = options.QuoteCurrency,
                    SortBy = "volume24h",
                    Direction = SortDirection.Descending,
                    Limit = options.Limit
                };

                try
                {
                    var results = await provider.GetTopSymbolsAsync(query);

                    if (options.Json)
                    {
                        OutputJson(results);
                    }
                    else
                    {
                        OutputVolTable(results, options);
                    }
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
                    Environment.ExitCode = 1;
                }
            });
        };

    private static CryptoVolOptions ParseVolArgs(string[] args)
    {
        var options = new CryptoVolOptions();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];
            var nextArg = i + 1 < args.Length ? args[i + 1] : null;

            // Check for limit argument (positional, after "crypto vol")
            if (int.TryParse(arg, out var limit) && limit > 0)
            {
                options.Limit = limit;
                continue;
            }

            switch (arg.ToLowerInvariant())
            {
                case "--exchange":
                case "-e":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        options.Exchange = nextArg;
                        i++;
                    }
                    break;

                case "--area":
                case "-a":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        options.Area = nextArg;
                        i++;
                    }
                    break;

                case "--quote":
                case "-q":
                    if (nextArg != null && !nextArg.StartsWith("-"))
                    {
                        options.QuoteCurrency = nextArg;
                        i++;
                    }
                    break;

                case "--json":
                    options.Json = true;
                    break;
            }
        }

        return options;
    }

    private static void OutputVolTable(IReadOnlyList<SymbolMarketData> results, CryptoVolOptions options)
    {
        var table = new Table();
        table.AddColumn(new TableColumn("#").RightAligned());
        table.AddColumn("Symbol");
        table.AddColumn(new TableColumn("24h Volume").RightAligned());

        var rank = 1;
        foreach (var item in results)
        {
            table.AddRow(
                rank++.ToString(),
                item.Symbol,
                FormatCurrency(item.Volume24hUsd));
        }

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine($"\n[dim]Source: {options.Exchange} {options.Area} | {results.Count} results[/]");
    }

    #endregion

    #region Helpers

    private static void OutputJson(IReadOnlyList<SymbolMarketData> results)
    {
        var output = results.Select(r => new
        {
            symbol = r.Symbol,
            baseCurrency = r.BaseCurrency,
            quoteCurrency = r.QuoteCurrency,
            marketCapUsd = r.MarketCapUsd,
            volume24hUsd = r.Volume24hUsd,
            marketCapRank = r.MarketCapRank,
            source = r.Source,
            retrievedAt = r.RetrievedAt
        });

        Console.WriteLine(JsonSerializer.Serialize(output, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));
    }

    private static string FormatCurrency(decimal value)
    {
        if (value >= 1_000_000_000_000)
            return $"${value / 1_000_000_000_000:N2}T";
        if (value >= 1_000_000_000)
            return $"${value / 1_000_000_000:N2}B";
        if (value >= 1_000_000)
            return $"${value / 1_000_000:N2}M";
        if (value >= 1_000)
            return $"${value / 1_000:N2}K";
        return $"${value:N2}";
    }

    #endregion
}
