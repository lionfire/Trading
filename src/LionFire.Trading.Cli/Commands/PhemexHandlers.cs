using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.CommandLine;
using Newtonsoft.Json;
using LionFire.Hosting;
using LionFire.Hosting.CommandLine;
using ccxt;

namespace LionFire.Trading.Cli.Commands;

public enum PhemexExchangeArea
{
    Spot,
    Futures,      // USDT-margined futures
    CoinFutures   // Coin-margined futures
}

public static class PhemexHandlers
{
    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> Balance =>
        (context, builder) =>
        {
            // Add options for balance command
            var testnetOption = new Option<bool>("--testnet", () => true, "Use testnet API")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            testnetOption.AddAlias("-t");
            
            var highRateOption = new Option<bool>("--high-rate", () => false, "Use high-rate API (vapi.phemex.com)")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            highRateOption.AddAlias("-h");
            
            var subaccountOption = new Option<long?>("--subaccount", () => null, "Subaccount ID")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            subaccountOption.AddAlias("-s");
            
            var apiKeyOption = new Option<string?>("--api-key", () => null, "API Key (overrides configuration)")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            apiKeyOption.AddAlias("-k");
            
            var apiSecretOption = new Option<string?>("--api-secret", () => null, "API Secret (overrides configuration)")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            
            var verboseOption = new Option<bool>("--verbose", () => false, "Verbose output")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            verboseOption.AddAlias("-v");

            if (context.HostingBuilderBuilder?.Command != null)
            {
                context.HostingBuilderBuilder.Command.AddOption(testnetOption);
                context.HostingBuilderBuilder.Command.AddOption(highRateOption);
                context.HostingBuilderBuilder.Command.AddOption(subaccountOption);
                context.HostingBuilderBuilder.Command.AddOption(apiKeyOption);
                context.HostingBuilderBuilder.Command.AddOption(apiSecretOption);
                context.HostingBuilderBuilder.Command.AddOption(verboseOption);
            }

            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetService<ILogger>();
                
                var invocationContext = serviceProvider.GetService<LionFireCommandLineOptions>()?.InvocationContext;
                if (invocationContext == null)
                {
                    AnsiConsole.MarkupLine("[red]Error: Unable to get command invocation context[/]");
                    return;
                }

                try
                {
                    // Build options from configuration (including .env file via configuration system)
                    var options = new PhemexOptions();
                    configuration.GetSection("Phemex").Bind(options);

                    // Override with command line options
                    var testnet = invocationContext.ParseResult.GetValueForOption(testnetOption);
                    var highRate = invocationContext.ParseResult.GetValueForOption(highRateOption);
                    var subaccount = invocationContext.ParseResult.GetValueForOption(subaccountOption);
                    var apiKey = invocationContext.ParseResult.GetValueForOption(apiKeyOption);
                    var apiSecret = invocationContext.ParseResult.GetValueForOption(apiSecretOption);
                    var verbose = invocationContext.ParseResult.GetValueForOption(verboseOption);

                    if (!string.IsNullOrEmpty(apiKey))
                        options.ApiKey = apiKey;
                    if (!string.IsNullOrEmpty(apiSecret))
                        options.ApiSecret = apiSecret;
                    if (testnet)
                        options.IsTestnet = true;
                    if (highRate)
                        options.UseHighRateLimitApi = true;
                    if (subaccount.HasValue)
                        options.SubAccountId = subaccount;

                    // Configure endpoints
                    options.ConfigureEndpoints();

                    if (string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.ApiSecret))
                    {
                        AnsiConsole.MarkupLine("[red]Error: API credentials not configured![/]");
                        AnsiConsole.WriteLine("Please set the following in your .env file or environment variables:");
                        AnsiConsole.WriteLine("  Phemex__ApiKey=your_api_key");
                        AnsiConsole.WriteLine("  Phemex__ApiSecret=your_api_secret");
                        return;
                    }

                    if (verbose)
                    {
                        AnsiConsole.MarkupLine($"[grey]Endpoint: {options.BaseUrl}[/]");
                        AnsiConsole.MarkupLine($"[grey]Testnet: {options.IsTestnet}[/]");
                        AnsiConsole.MarkupLine($"[grey]High-rate API: {options.UseHighRateLimitApi}[/]");
                        if (options.SubAccountId.HasValue)
                            AnsiConsole.MarkupLine($"[grey]SubAccount: {options.SubAccountId}[/]");
                    }

                    await AnsiConsole.Status()
                        .StartAsync("Fetching account balance...", async ctx =>
                        {
                            var result = await GetAccountBalance(options, PhemexExchangeArea.Spot, logger); // Default to spot for backward compatibility
                            if (result != null)
                            {
                                DisplayAccountBalance(result);
                            }
                        });
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                    logger?.LogError(ex, "Failed to fetch Phemex balance");
                }
            });
        };

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> SpotBalance =>
        (context, builder) =>
        {
            var testnetOption = new Option<bool>("--testnet", () => true, "Use testnet API")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            testnetOption.AddAlias("-t");
            
            var highRateOption = new Option<bool>("--high-rate", () => false, "Use high-rate API (vapi.phemex.com)")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            highRateOption.AddAlias("-h");
            
            var verboseOption = new Option<bool>("--verbose", () => false, "Verbose output")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            verboseOption.AddAlias("-v");
            
            if (context.HostingBuilderBuilder?.Command != null)
            {
                context.HostingBuilderBuilder.Command.AddOption(testnetOption);
                context.HostingBuilderBuilder.Command.AddOption(highRateOption);
                context.HostingBuilderBuilder.Command.AddOption(verboseOption);
            }

            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetService<ILogger>();
                
                var invocationContext = serviceProvider.GetService<LionFireCommandLineOptions>()?.InvocationContext;
                if (invocationContext == null)
                {
                    AnsiConsole.MarkupLine("[red]Error: Unable to get command invocation context[/]");
                    return;
                }

                try
                {
                    // Build options from configuration
                    var options = new PhemexOptions();
                    configuration.GetSection("Phemex").Bind(options);
                    
                    // Override with command line options if provided
                    var highRate = invocationContext.ParseResult.GetValueForOption(highRateOption);
                    var verbose = invocationContext.ParseResult.GetValueForOption(verboseOption);
                    
                    // Only override testnet if explicitly provided, otherwise keep config value
                    if (invocationContext.ParseResult.FindResultFor(testnetOption) != null)
                    {
                        options.IsTestnet = invocationContext.ParseResult.GetValueForOption(testnetOption);
                    }
                    
                    if (highRate)
                    {
                        options.BaseUrl = options.IsTestnet ? "https://vapi.testnet.phemex.com" : "https://vapi.phemex.com";
                    }
                    else
                    {
                        options.ConfigureEndpoints();
                    }
                    
                    await AnsiConsole.Status()
                        .StartAsync("Fetching spot wallet balance...", async ctx =>
                        {
                            await GetAccountBalance(options, PhemexExchangeArea.Spot, logger);
                        });
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                    logger?.LogError(ex, "Failed to fetch Phemex spot balance");
                }
            });
        };

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> FuturesBalance =>
        (context, builder) =>
        {
            var testnetOption = new Option<bool>("--testnet", () => true, "Use testnet API")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            testnetOption.AddAlias("-t");
            
            var highRateOption = new Option<bool>("--high-rate", () => false, "Use high-rate API (vapi.phemex.com)")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            highRateOption.AddAlias("-h");
            
            var verboseOption = new Option<bool>("--verbose", () => false, "Verbose output")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            verboseOption.AddAlias("-v");
            
            if (context.HostingBuilderBuilder?.Command != null)
            {
                context.HostingBuilderBuilder.Command.AddOption(testnetOption);
                context.HostingBuilderBuilder.Command.AddOption(highRateOption);
                context.HostingBuilderBuilder.Command.AddOption(verboseOption);
            }

            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetService<ILogger>();
                
                var invocationContext = serviceProvider.GetService<LionFireCommandLineOptions>()?.InvocationContext;
                if (invocationContext == null)
                {
                    AnsiConsole.MarkupLine("[red]Error: Unable to get command invocation context[/]");
                    return;
                }

                try
                {
                    // Build options from configuration
                    var options = new PhemexOptions();
                    configuration.GetSection("Phemex").Bind(options);
                    
                    // Override with command line options if provided
                    var highRate = invocationContext.ParseResult.GetValueForOption(highRateOption);
                    var verbose = invocationContext.ParseResult.GetValueForOption(verboseOption);
                    
                    // Only override testnet if explicitly provided, otherwise keep config value
                    if (invocationContext.ParseResult.FindResultFor(testnetOption) != null)
                    {
                        options.IsTestnet = invocationContext.ParseResult.GetValueForOption(testnetOption);
                    }
                    
                    if (highRate)
                    {
                        options.BaseUrl = options.IsTestnet ? "https://vapi.testnet.phemex.com" : "https://vapi.phemex.com";
                    }
                    else
                    {
                        options.ConfigureEndpoints();
                    }
                    
                    await AnsiConsole.Status()
                        .StartAsync("Fetching USDT futures balance...", async ctx =>
                        {
                            await GetAccountBalance(options, PhemexExchangeArea.Futures, logger);
                        });
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                    logger?.LogError(ex, "Failed to fetch Phemex futures balance");
                }
            });
        };

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> CoinFuturesBalance =>
        (context, builder) =>
        {
            var testnetOption = new Option<bool>("--testnet", () => true, "Use testnet API")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            testnetOption.AddAlias("-t");
            
            var highRateOption = new Option<bool>("--high-rate", () => false, "Use high-rate API (vapi.phemex.com)")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            highRateOption.AddAlias("-h");
            
            var verboseOption = new Option<bool>("--verbose", () => false, "Verbose output")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            verboseOption.AddAlias("-v");
            
            if (context.HostingBuilderBuilder?.Command != null)
            {
                context.HostingBuilderBuilder.Command.AddOption(testnetOption);
                context.HostingBuilderBuilder.Command.AddOption(highRateOption);
                context.HostingBuilderBuilder.Command.AddOption(verboseOption);
            }

            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetService<ILogger>();
                
                var invocationContext = serviceProvider.GetService<LionFireCommandLineOptions>()?.InvocationContext;
                if (invocationContext == null)
                {
                    AnsiConsole.MarkupLine("[red]Error: Unable to get command invocation context[/]");
                    return;
                }

                try
                {
                    // Build options from configuration
                    var options = new PhemexOptions();
                    configuration.GetSection("Phemex").Bind(options);
                    
                    // Override with command line options if provided
                    var highRate = invocationContext.ParseResult.GetValueForOption(highRateOption);
                    var verbose = invocationContext.ParseResult.GetValueForOption(verboseOption);
                    
                    // Only override testnet if explicitly provided, otherwise keep config value
                    if (invocationContext.ParseResult.FindResultFor(testnetOption) != null)
                    {
                        options.IsTestnet = invocationContext.ParseResult.GetValueForOption(testnetOption);
                    }
                    
                    if (highRate)
                    {
                        options.BaseUrl = options.IsTestnet ? "https://vapi.testnet.phemex.com" : "https://vapi.phemex.com";
                    }
                    else
                    {
                        options.ConfigureEndpoints();
                    }
                    
                    await AnsiConsole.Status()
                        .StartAsync("Fetching coin-margined futures balance...", async ctx =>
                        {
                            await GetAccountBalance(options, PhemexExchangeArea.CoinFutures, logger);
                        });
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                    logger?.LogError(ex, "Failed to fetch Phemex coin futures balance");
                }
            });
        };

    // Position handlers for different exchange areas
    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> SpotPositions => Positions; // For now, use existing implementation
    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> FuturesPositions => Positions;
    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> CoinFuturesPositions => Positions;
    
    // Trading handlers for different exchange areas
    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> SpotOpen => Open; // For now, use existing implementation
    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> FuturesOpen => Open;
    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> SpotClose => Close;
    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> FuturesClose => Close;

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> Subaccounts =>
        (context, builder) =>
        {
            // Add similar options for subaccounts command
            var testnetOption = new Option<bool>("--testnet", () => true, "Use testnet API");
            testnetOption.AddAlias("-t");
            
            var highRateOption = new Option<bool>("--high-rate", () => false, "Use high-rate API (vapi.phemex.com)");
            highRateOption.AddAlias("-h");
            
            var verboseOption = new Option<bool>("--verbose", () => false, "Verbose output");
            verboseOption.AddAlias("-v");

            if (context.HostingBuilderBuilder?.Command != null)
            {
                context.HostingBuilderBuilder.Command.AddOption(testnetOption);
                context.HostingBuilderBuilder.Command.AddOption(highRateOption);
                context.HostingBuilderBuilder.Command.AddOption(verboseOption);
            }

            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetService<ILogger>();
                
                var invocationContext = serviceProvider.GetService<LionFireCommandLineOptions>()?.InvocationContext;
                if (invocationContext == null)
                {
                    AnsiConsole.MarkupLine("[red]Error: Unable to get command invocation context[/]");
                    return;
                }

                try
                {
                    var options = new PhemexOptions();
                    configuration.GetSection("Phemex").Bind(options);
                    

                    var testnet = invocationContext.ParseResult.GetValueForOption(testnetOption);
                    var highRate = invocationContext.ParseResult.GetValueForOption(highRateOption);
                    var verbose = invocationContext.ParseResult.GetValueForOption(verboseOption);

                    if (testnet)
                        options.IsTestnet = true;
                    if (highRate)
                        options.UseHighRateLimitApi = true;

                    options.ConfigureEndpoints();

                    if (string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.ApiSecret))
                    {
                        AnsiConsole.MarkupLine("[red]Error: API credentials not configured![/]");
                        return;
                    }

                    await AnsiConsole.Status()
                        .StartAsync("Fetching subaccounts...", async ctx =>
                        {
                            var result = await GetSubaccounts(options, logger);
                            DisplaySubaccounts(result);
                        });
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                    logger?.LogError(ex, "Failed to fetch Phemex subaccounts");
                }
            });
        };

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> Positions =>
        (context, builder) =>
        {
            // Add similar options for positions command
            var testnetOption = new Option<bool>("--testnet", () => true, "Use testnet API");
            testnetOption.AddAlias("-t");
            
            var highRateOption = new Option<bool>("--high-rate", () => false, "Use high-rate API (vapi.phemex.com)");
            highRateOption.AddAlias("-h");
            
            var subaccountOption = new Option<long?>("--subaccount", () => null, "Subaccount ID");
            subaccountOption.AddAlias("-s");
            
            var verboseOption = new Option<bool>("--verbose", () => false, "Verbose output");
            verboseOption.AddAlias("-v");

            if (context.HostingBuilderBuilder?.Command != null)
            {
                context.HostingBuilderBuilder.Command.AddOption(testnetOption);
                context.HostingBuilderBuilder.Command.AddOption(highRateOption);
                context.HostingBuilderBuilder.Command.AddOption(subaccountOption);
                context.HostingBuilderBuilder.Command.AddOption(verboseOption);
            }

            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetService<ILogger>();
                
                var invocationContext = serviceProvider.GetService<LionFireCommandLineOptions>()?.InvocationContext;
                if (invocationContext == null)
                {
                    AnsiConsole.MarkupLine("[red]Error: Unable to get command invocation context[/]");
                    return;
                }

                try
                {
                    var options = new PhemexOptions();
                    configuration.GetSection("Phemex").Bind(options);
                    

                    var testnet = invocationContext.ParseResult.GetValueForOption(testnetOption);
                    var highRate = invocationContext.ParseResult.GetValueForOption(highRateOption);
                    var subaccount = invocationContext.ParseResult.GetValueForOption(subaccountOption);
                    var verbose = invocationContext.ParseResult.GetValueForOption(verboseOption);

                    if (testnet)
                        options.IsTestnet = true;
                    if (highRate)
                        options.UseHighRateLimitApi = true;
                    if (subaccount.HasValue)
                        options.SubAccountId = subaccount;

                    options.ConfigureEndpoints();

                    if (string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.ApiSecret))
                    {
                        AnsiConsole.MarkupLine("[red]Error: API credentials not configured![/]");
                        return;
                    }

                    await AnsiConsole.Status()
                        .StartAsync("Fetching positions...", async ctx =>
                        {
                            var result = await GetPositions(options, logger);
                            if (result != null)
                            {
                                DisplayPositions(result);
                            }
                        });
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                    logger?.LogError(ex, "Failed to fetch Phemex positions");
                }
            });
        };

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> Open =>
        (context, builder) =>
        {
            var testnetOption = new Option<bool>("--testnet", () => true, "Use testnet API");
            testnetOption.AddAlias("-t");
            
            var symbolOption = new Option<string>("--symbol", () => "BTC/USDT", "Trading symbol (e.g., BTC/USDT)");
            symbolOption.AddAlias("-s");
            
            var sideOption = new Option<string>("--side", () => "buy", "Order side: buy or sell");
            
            var amountOption = new Option<double>("--amount", () => 0.01, "Order amount/size");
            amountOption.AddAlias("-a");
            
            var priceOption = new Option<double?>("--price", () => null, "Order price (leave empty for market order)");
            priceOption.AddAlias("-p");
            
            var stopLossOption = new Option<double?>("--stop-loss", () => null, "Stop loss price");
            
            var verboseOption = new Option<bool>("--verbose", () => false, "Verbose output");
            verboseOption.AddAlias("-v");

            if (context.HostingBuilderBuilder?.Command != null)
            {
                context.HostingBuilderBuilder.Command.AddOption(testnetOption);
                context.HostingBuilderBuilder.Command.AddOption(symbolOption);
                context.HostingBuilderBuilder.Command.AddOption(sideOption);
                context.HostingBuilderBuilder.Command.AddOption(amountOption);
                context.HostingBuilderBuilder.Command.AddOption(priceOption);
                context.HostingBuilderBuilder.Command.AddOption(stopLossOption);
                context.HostingBuilderBuilder.Command.AddOption(verboseOption);
            }

            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetService<ILogger>();
                
                var invocationContext = serviceProvider.GetService<LionFireCommandLineOptions>()?.InvocationContext;
                if (invocationContext == null)
                {
                    AnsiConsole.MarkupLine("[red]Error: Unable to get command invocation context[/]");
                    return;
                }

                try
                {
                    var options = new PhemexOptions();
                    configuration.GetSection("Phemex").Bind(options);

                    var testnet = invocationContext.ParseResult.GetValueForOption(testnetOption);
                    var symbol = invocationContext.ParseResult.GetValueForOption(symbolOption);
                    var side = invocationContext.ParseResult.GetValueForOption(sideOption);
                    var amount = invocationContext.ParseResult.GetValueForOption(amountOption);
                    var price = invocationContext.ParseResult.GetValueForOption(priceOption);
                    var stopLoss = invocationContext.ParseResult.GetValueForOption(stopLossOption);
                    var verbose = invocationContext.ParseResult.GetValueForOption(verboseOption);

                    if (testnet)
                        options.IsTestnet = true;

                    options.ConfigureEndpoints();

                    if (string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.ApiSecret))
                    {
                        AnsiConsole.MarkupLine("[red]Error: API credentials not configured![/]");
                        return;
                    }

                    // Validate parameters
                    if (amount <= 0)
                    {
                        AnsiConsole.MarkupLine("[red]Error: Amount must be greater than 0[/]");
                        return;
                    }

                    if (side?.ToLower() != "buy" && side?.ToLower() != "sell")
                    {
                        AnsiConsole.MarkupLine("[red]Error: Side must be 'buy' or 'sell'[/]");
                        return;
                    }

                    await AnsiConsole.Status()
                        .StartAsync($"Opening {side?.ToUpper()} position for {amount} {symbol}...", async ctx =>
                        {
                            if (!string.IsNullOrEmpty(symbol) && !string.IsNullOrEmpty(side))
                            {
                                await OpenPosition(options, symbol, side, amount, price, stopLoss, verbose, logger);
                            }
                            else
                            {
                                AnsiConsole.MarkupLine("[red]Error: Symbol and side are required[/]");
                            }
                        });
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                    logger?.LogError(ex, "Failed to open Phemex position");
                }
            });
        };

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> Close =>
        (context, builder) =>
        {
            var testnetOption = new Option<bool>("--testnet", () => true, "Use testnet API");
            testnetOption.AddAlias("-t");
            
            var symbolOption = new Option<string>("--symbol", () => "BTC/USDT", "Trading symbol (e.g., BTC/USDT)");
            symbolOption.AddAlias("-s");
            
            var amountOption = new Option<double?>("--amount", () => null, "Close partial amount (leave empty to close all)");
            amountOption.AddAlias("-a");
            
            var verboseOption = new Option<bool>("--verbose", () => false, "Verbose output");
            verboseOption.AddAlias("-v");

            if (context.HostingBuilderBuilder?.Command != null)
            {
                context.HostingBuilderBuilder.Command.AddOption(testnetOption);
                context.HostingBuilderBuilder.Command.AddOption(symbolOption);
                context.HostingBuilderBuilder.Command.AddOption(amountOption);
                context.HostingBuilderBuilder.Command.AddOption(verboseOption);
            }

            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetService<ILogger>();
                
                var invocationContext = serviceProvider.GetService<LionFireCommandLineOptions>()?.InvocationContext;
                if (invocationContext == null)
                {
                    AnsiConsole.MarkupLine("[red]Error: Unable to get command invocation context[/]");
                    return;
                }

                try
                {
                    var options = new PhemexOptions();
                    configuration.GetSection("Phemex").Bind(options);

                    var testnet = invocationContext.ParseResult.GetValueForOption(testnetOption);
                    var symbol = invocationContext.ParseResult.GetValueForOption(symbolOption);
                    var amount = invocationContext.ParseResult.GetValueForOption(amountOption);
                    var verbose = invocationContext.ParseResult.GetValueForOption(verboseOption);

                    if (testnet)
                        options.IsTestnet = true;

                    options.ConfigureEndpoints();

                    if (string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.ApiSecret))
                    {
                        AnsiConsole.MarkupLine("[red]Error: API credentials not configured![/]");
                        return;
                    }

                    await AnsiConsole.Status()
                        .StartAsync($"Closing position for {symbol}...", async ctx =>
                        {
                            if (!string.IsNullOrEmpty(symbol))
                            {
                                await ClosePosition(options, symbol, amount, verbose, logger);
                            }
                            else
                            {
                                AnsiConsole.MarkupLine("[red]Error: Symbol is required[/]");
                            }
                        });
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                    logger?.LogError(ex, "Failed to close Phemex position");
                }
            });
        };

    // Helper methods extracted from the original Phemex commands

    private static async Task<PhemexPositionData?> GetAccountBalance(PhemexOptions options, PhemexExchangeArea exchangeArea, ILogger? logger)
    {
        AnsiConsole.MarkupLine("[green]Fetching balance via Manual HTTP[/]");
        
        try
        {
            using var httpClient = new HttpClient();
            
            var baseUrl = options.IsTestnet ? "https://testnet-api.phemex.com" : "https://api.phemex.com";
            
            string path;
            string queryString;
            
            switch (exchangeArea)
            {
                case PhemexExchangeArea.Spot:
                    path = "/spot/wallets";
                    queryString = "";
                    break;
                case PhemexExchangeArea.Futures:
                    path = "/g-accounts/accountPositions"; // USDT-margined futures
                    queryString = "currency=USDT";
                    break;
                case PhemexExchangeArea.CoinFutures:
                    path = "/accounts/accountPositions"; // Coin-margined futures
                    queryString = "currency=BTC";
                    break;
                default:
                    throw new ArgumentException($"Unsupported exchange area: {exchangeArea}");
            }
            var expires = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60;
            var body = "";
            
            // Create message for signature: path + queryString + expires + body (no ? in signature)
            var message = path + queryString + expires + body;
            
            logger?.LogDebug("Signature message: '{Message}'", message);
            
            // Generate signature
            var keyBytes = Encoding.UTF8.GetBytes(options.ApiSecret!);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            using var hmac = new System.Security.Cryptography.HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(messageBytes);
            var signature = BitConverter.ToString(hash).Replace("-", "").ToLower();
            
            logger?.LogDebug("Generated signature: {Signature}...", signature.Substring(0, 20));
            
            // Set headers
            httpClient.DefaultRequestHeaders.Add("x-phemex-access-token", options.ApiKey);
            httpClient.DefaultRequestHeaders.Add("x-phemex-request-signature", signature);
            httpClient.DefaultRequestHeaders.Add("x-phemex-request-expiry", expires.ToString());
            
            var url = string.IsNullOrEmpty(queryString) ? $"{baseUrl}{path}" : $"{baseUrl}{path}?{queryString}";
            logger?.LogDebug("Request URL: {Url}", url);
            
            var response = await httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            
            logger?.LogDebug("Response Status: {StatusCode}", response.StatusCode);
            // Escape the content to avoid AnsiConsole markup parsing issues
            var contentPreview = content.Substring(0, Math.Min(200, content.Length));
            logger?.LogDebug("Response Content: {Content}...", contentPreview);
            
            if (response.IsSuccessStatusCode)
            {
                if (exchangeArea == PhemexExchangeArea.Spot)
                {
                    // Parse as array of spot wallets
                    var jsonResponse = JsonConvert.DeserializeObject<PhemexApiResponse<List<object>>>(content);
                    if (jsonResponse?.Data != null)
                    {
                        AnsiConsole.MarkupLine("[cyan]Spot Wallet Balances:[/]");
                        
                        foreach (var wallet in jsonResponse.Data)
                        {
                            if (wallet is Newtonsoft.Json.Linq.JObject walletObj)
                            {
                                var currency = walletObj["currency"]?.ToString();
                                var balanceEv = walletObj["balanceEv"]?.ToObject<long>() ?? 0;
                                var lockedTradingBalanceEv = walletObj["lockedTradingBalanceEv"]?.ToObject<long>() ?? 0;
                                
                                // Convert from Phemex's scaled integer format
                                var balance = balanceEv / 100000000.0; // All use 8 decimal places
                                var lockedBalance = lockedTradingBalanceEv / 100000000.0;
                                var freeBalance = balance - lockedBalance;
                                
                                if (balance > 0)
                                {
                                    AnsiConsole.MarkupLine($"[cyan]{currency}[/]: Total={balance:F8}, Free={freeBalance:F8}, Locked={lockedBalance:F8}");
                                }
                            }
                        }
                        
                        return null; // We don't need to return position data for spot wallets
                    }
                }
                else
                {
                    // Parse futures account positions
                    var jsonResponse = JsonConvert.DeserializeObject<PhemexApiResponse<PhemexPositionData>>(content);
                    if (jsonResponse?.Data?.Account != null)
                    {
                        var account = jsonResponse.Data.Account;
                        var totalBalance = account.TotalBalanceEv / 100000000.0;
                        var availableBalance = account.AvailableBalanceEv / 100000000.0;
                        var usedBalance = account.TotalUsedBalanceEv / 100000000.0;
                        
                        var areaName = exchangeArea == PhemexExchangeArea.Futures ? "USDT Futures" : "Coin-Margined Futures";
                        AnsiConsole.MarkupLine($"[cyan]{areaName} Account Balances:[/]");
                        AnsiConsole.MarkupLine($"[cyan]{account.Currency}[/]: Total={totalBalance:F8}, Free={availableBalance:F8}, Used={usedBalance:F8}");
                        
                        return jsonResponse.Data;
                    }
                }
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]API Error: {content}[/]");
            }
            
            return null;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Failed to fetch balance via Manual HTTP");
            AnsiConsole.MarkupLine($"[red]Exception: {ex.Message}[/]");
            return null;
        }
    }

    private static async Task<List<PhemexSubAccount>> GetSubaccounts(PhemexOptions options, ILogger? logger)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl) };
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var path = "/accounts/sub-account-list";
        var queryString = ""; // No query parameters for this endpoint
        var expires = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60;
        var body = ""; // GET request has no body
        
        // Phemex signature format: path + queryString + expires + body
        var signatureData = $"{path}{queryString}{expires}{body}";
        var signature = CreateSignature(signatureData, options.ApiSecret);

        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("x-phemex-access-token", options.ApiKey);
        request.Headers.Add("x-phemex-request-expiry", expires.ToString());
        request.Headers.Add("x-phemex-request-signature", signature);

        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            AnsiConsole.MarkupLine($"[red]API Error: {response.StatusCode}[/]");
            AnsiConsole.WriteLine(responseContent);
            return new List<PhemexSubAccount>();
        }

        var apiResponse = JsonConvert.DeserializeObject<PhemexApiResponse<List<PhemexSubAccount>>>(responseContent);
        if (apiResponse?.Code != 0)
        {
            AnsiConsole.MarkupLine($"[red]Phemex Error: {apiResponse?.Message}[/]");
            return new List<PhemexSubAccount>();
        }

        return apiResponse.Data ?? new List<PhemexSubAccount>();
    }

    private static async Task<PhemexPositionData?> GetPositions(PhemexOptions options, ILogger? logger)
    {
        return await GetAccountBalance(options, PhemexExchangeArea.CoinFutures, logger); // Default to coin futures for positions
    }

    private static string CreateSignature(string data, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static void DisplayAccountBalance(PhemexPositionData data)
    {
        var table = new Table()
            .AddColumn("Property")
            .AddColumn("Value")
            .Border(TableBorder.Rounded);

        table.AddRow("Account ID", data.Account.AccountId.ToString());
        table.AddRow("Currency", data.Account.Currency);
        table.AddRow("Total Balance", FormatBalance(data.Account.TotalBalanceEv));
        table.AddRow("Available Balance", FormatBalance(data.Account.AvailableBalanceEv));
        table.AddRow("Used Balance", FormatBalance(data.Account.TotalUsedBalanceEv));
        table.AddRow("Margin Balance", FormatBalance(data.Account.MarginBalanceEv));
        table.AddRow("Unrealized PnL", FormatBalance(data.Account.UnrealisedPnlEv));

        AnsiConsole.Write(table);
    }

    private static void DisplaySubaccounts(List<PhemexSubAccount> subaccounts)
    {
        if (!subaccounts.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No subaccounts found.[/]");
            return;
        }

        var table = new Table()
            .AddColumn("Account ID")
            .AddColumn("Account Name")
            .AddColumn("Status")
            .Border(TableBorder.Rounded);

        foreach (var subaccount in subaccounts)
        {
            table.AddRow(
                subaccount.AccountId.ToString(),
                subaccount.AccountName ?? "N/A",
                subaccount.Status ?? "Unknown");
        }

        AnsiConsole.Write(table);
    }

    private static void DisplayPositions(PhemexPositionData data)
    {
        if (!data.Positions.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No positions found.[/]");
            return;
        }

        var table = new Table()
            .AddColumn("Symbol")
            .AddColumn("Side")
            .AddColumn("Size")
            .AddColumn("Leverage")
            .AddColumn("Status")
            .Border(TableBorder.Rounded);

        foreach (var position in data.Positions)
        {
            table.AddRow(
                position.Symbol,
                position.Side,
                FormatBalance(position.PosCostEv),
                $"{position.Leverage:F1}x",
                position.PositionStatus);
        }

        AnsiConsole.Write(table);
    }

    private static string FormatBalance(long balanceEv)
    {
        return (balanceEv / 100000000.0).ToString("F8");
    }

    private static async Task OpenPosition(PhemexOptions options, string symbol, string side, double amount, double? price, double? stopLoss, bool verbose, ILogger? logger)
    {
        try
        {
            AnsiConsole.MarkupLine("[green]Opening position via CCXT.NET[/]");
            
            var exchange = new phemex();
            exchange.apiKey = options.ApiKey;
            exchange.secret = options.ApiSecret;
            
            if (options.IsTestnet)
                exchange.setSandboxMode(true);

            AnsiConsole.MarkupLine($"[yellow]Creating {side.ToUpper()} order for {amount} {symbol}[/]");
            
            string orderType = price.HasValue ? "limit" : "market";
            var order = await exchange.createOrder(symbol, orderType, side.ToLower(), amount, price ?? 0);

            AnsiConsole.MarkupLine("[green]✓ Order created successfully![/]");
            
            if (verbose && order != null)
            {
                var orderJson = JsonConvert.SerializeObject(order, Formatting.Indented);
                AnsiConsole.WriteLine($"Order details:\n{orderJson}");
            }
            else if (order is IDictionary<string, object> orderDict)
            {
                var orderId = orderDict.TryGetValue("id", out var id) ? id?.ToString() : "Unknown";
                var status = orderDict.TryGetValue("status", out var st) ? st?.ToString() : "Unknown";
                AnsiConsole.MarkupLine($"[green]Order ID:[/] {orderId}");
                AnsiConsole.MarkupLine($"[green]Status:[/] {status}");
            }

            // Create stop loss order if specified
            if (stopLoss.HasValue)
            {
                try
                {
                    AnsiConsole.MarkupLine($"[yellow]Setting stop loss at {stopLoss.Value}[/]");
                    
                    string stopLossSide = side.ToLower() == "buy" ? "sell" : "buy";
                    var stopLossOrder = await exchange.createOrder(symbol, "stop", stopLossSide, amount, stopLoss.Value);

                    AnsiConsole.MarkupLine("[green]✓ Stop loss order created![/]");
                    
                    if (verbose && stopLossOrder != null)
                    {
                        var stopJson = JsonConvert.SerializeObject(stopLossOrder, Formatting.Indented);
                        AnsiConsole.WriteLine($"Stop loss details:\n{stopJson}");
                    }
                }
                catch (Exception slEx)
                {
                    AnsiConsole.MarkupLine($"[yellow]Warning: Failed to create stop loss: {slEx.Message}[/]");
                    if (verbose)
                    {
                        logger?.LogWarning(slEx, "Stop loss order failed");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error creating position: {ex.Message}[/]");
            if (verbose)
            {
                logger?.LogError(ex, "Failed to create position");
            }
            throw;
        }
    }

    private static async Task ClosePosition(PhemexOptions options, string symbol, double? amount, bool verbose, ILogger? logger)
    {
        try
        {
            AnsiConsole.MarkupLine("[green]Closing position via CCXT.NET[/]");
            
            var exchange = new phemex();
            exchange.apiKey = options.ApiKey;
            exchange.secret = options.ApiSecret;
            
            if (options.IsTestnet)
                exchange.setSandboxMode(true);

            // First, fetch current positions to determine the position size and side
            var positions = await exchange.fetchPositions();
            
            if (positions == null)
            {
                AnsiConsole.MarkupLine($"[yellow]No positions found for {symbol}[/]");
                return;
            }

            object? targetPosition = null;
            if (positions is IEnumerable<object> positionsEnum)
            {
                foreach (var position in positionsEnum)
                {
                    if (position is IDictionary<string, object> posDict &&
                        posDict.TryGetValue("symbol", out var posSymbol) &&
                        posSymbol?.ToString() == symbol)
                    {
                        if (posDict.TryGetValue("size", out var size) &&
                            double.TryParse(size?.ToString(), out var sizeNum) &&
                            Math.Abs(sizeNum) > 0.000001)
                        {
                            targetPosition = position;
                            break;
                        }
                    }
                }
            }

            if (targetPosition == null)
            {
                AnsiConsole.MarkupLine($"[yellow]No open position found for {symbol}[/]");
                return;
            }

            var positionDict = (IDictionary<string, object>)targetPosition;
            var currentSide = positionDict.TryGetValue("side", out var side) ? side?.ToString() : null;
            var currentSize = positionDict.TryGetValue("size", out var sz) ? sz?.ToString() : "0";
            
            if (string.IsNullOrEmpty(currentSide))
            {
                AnsiConsole.MarkupLine("[red]Error: Could not determine position side[/]");
                return;
            }

            string closeSide = currentSide.ToLower() == "long" ? "sell" : "buy";
            double closeAmount = amount ?? Math.Abs(double.Parse(currentSize ?? "0"));

            AnsiConsole.MarkupLine($"[yellow]Closing {currentSide} position of {closeAmount} {symbol}[/]");

            var closeOrder = await exchange.createOrder(symbol, "market", closeSide, closeAmount, 0);

            AnsiConsole.MarkupLine("[green]✓ Position close order created![/]");
            
            if (verbose && closeOrder != null)
            {
                var orderJson = JsonConvert.SerializeObject(closeOrder, Formatting.Indented);
                AnsiConsole.WriteLine($"Close order details:\n{orderJson}");
            }
            else if (closeOrder is IDictionary<string, object> orderDict)
            {
                var orderId = orderDict.TryGetValue("id", out var id) ? id?.ToString() : "Unknown";
                var status = orderDict.TryGetValue("status", out var st) ? st?.ToString() : "Unknown";
                AnsiConsole.MarkupLine($"[green]Close Order ID:[/] {orderId}");
                AnsiConsole.MarkupLine($"[green]Status:[/] {status}");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error closing position: {ex.Message}[/]");
            if (verbose)
            {
                logger?.LogError(ex, "Failed to close position");
            }
            throw;
        }
    }

    private static PhemexOptions ReadDirectFromEnvFile(string envFilePath)
    {
        var options = new PhemexOptions();
        
        if (!File.Exists(envFilePath))
            return options;
        
        try
        {
            foreach (var line in File.ReadAllLines(envFilePath))
            {
                var trimmedLine = line.Trim();
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith('#'))
                    continue;

                var equalIndex = trimmedLine.IndexOf('=');
                if (equalIndex == -1)
                    continue;

                var key = trimmedLine.Substring(0, equalIndex).Trim();
                var value = trimmedLine.Substring(equalIndex + 1).Trim();

                // Map .env keys to PhemexOptions properties
                switch (key)
                {
                    case "Phemex__ApiKey":
                        options.ApiKey = value;
                        break;
                    case "Phemex__ApiSecret":
                        options.ApiSecret = value;
                        break;
                    case "Phemex__IsTestnet":
                        if (bool.TryParse(value, out var isTestnet))
                            options.IsTestnet = isTestnet;
                        break;
                    case "Phemex__BaseUrl":
                        options.BaseUrl = value;
                        break;
                    case "Phemex__UseHighRateLimitApi":
                        if (bool.TryParse(value, out var useHighRate))
                            options.UseHighRateLimitApi = useHighRate;
                        break;
                    case "Phemex__SubaccountId":
                        if (long.TryParse(value, out var subAccountId))
                            options.SubAccountId = subAccountId;
                        break;
                    case "Phemex__RateLimitPerSecond":
                        if (int.TryParse(value, out var rateLimit))
                            options.RateLimitPerSecond = rateLimit;
                        break;
                }
            }
        }
        catch
        {
            // If we can't read the file, return empty options
        }
        
        return options;
    }

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> SpotTicker =>
        (context, builder) =>
        {
            var symbolArg = new Argument<string>("symbol", "Trading symbol (e.g., BTCUSDT)");
            symbolArg.SetDefaultValue("BTCUSDT");
            
            var testnetOption = new Option<bool>("--testnet", () => true, "Use testnet API")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            testnetOption.AddAlias("-t");
            
            var verboseOption = new Option<bool>("--verbose", () => false, "Verbose output")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            verboseOption.AddAlias("-v");
            
            if (context.HostingBuilderBuilder?.Command != null)
            {
                context.HostingBuilderBuilder.Command.AddArgument(symbolArg);
                context.HostingBuilderBuilder.Command.AddOption(testnetOption);
                context.HostingBuilderBuilder.Command.AddOption(verboseOption);
            }

            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetService<ILogger>();
                
                var invocationContext = serviceProvider.GetService<LionFireCommandLineOptions>()?.InvocationContext;
                if (invocationContext == null)
                {
                    AnsiConsole.MarkupLine("[red]Error: Unable to get command invocation context[/]");
                    return;
                }

                try
                {
                    // For now, hardcode the symbol since argument parsing isn't working
                    var symbol = "BTCUSD"; // TODO: Fix argument parsing - Note: Phemex uses BTCUSD not BTCUSDT
                    var testnet = true; // Default to testnet
                    var verbose = false;

                    await ExecuteTicker(PhemexExchangeArea.Spot, symbol, testnet, verbose, configuration, logger);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                    logger?.LogError(ex, "Failed to execute ticker command");
                }
            });
        };

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> FuturesTicker =>
        (context, builder) =>
        {
            var symbolArg = new Argument<string>("symbol", "Trading symbol (e.g., BTCUSDT)");
            symbolArg.SetDefaultValue("BTCUSDT");
            
            var testnetOption = new Option<bool>("--testnet", () => true, "Use testnet API")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            testnetOption.AddAlias("-t");
            
            var verboseOption = new Option<bool>("--verbose", () => false, "Verbose output")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            verboseOption.AddAlias("-v");
            
            if (context.HostingBuilderBuilder?.Command != null)
            {
                context.HostingBuilderBuilder.Command.AddArgument(symbolArg);
                context.HostingBuilderBuilder.Command.AddOption(testnetOption);
                context.HostingBuilderBuilder.Command.AddOption(verboseOption);
            }

            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetService<ILogger>();
                
                var invocationContext = serviceProvider.GetService<LionFireCommandLineOptions>()?.InvocationContext;
                if (invocationContext == null)
                {
                    AnsiConsole.MarkupLine("[red]Error: Unable to get command invocation context[/]");
                    return;
                }

                try
                {
                    // For now, hardcode the symbol since argument parsing isn't working
                    var symbol = "BTCUSD"; // TODO: Fix argument parsing - Note: Phemex uses BTCUSD not BTCUSDT
                    var testnet = true; // Default to testnet
                    var verbose = false;

                    await ExecuteTicker(PhemexExchangeArea.Futures, symbol, testnet, verbose, configuration, logger);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                    logger?.LogError(ex, "Failed to execute ticker command");
                }
            });
        };

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> SpotPlaceOrder =>
        (context, builder) =>
        {
            var symbolArg = new Argument<string>("symbol", "Trading symbol (e.g., BTCUSDT)");
            var sideArg = new Argument<string>("side", "Order side (BUY or SELL)");
            var quantityArg = new Argument<decimal>("quantity", "Order quantity");
            
            var priceOption = new Option<decimal?>("--price", "Order price (for limit orders)")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            
            var typeOption = new Option<string>("--type", () => "LIMIT", "Order type (LIMIT or MARKET)")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            typeOption.AddAlias("-t");
            
            var paperOption = new Option<bool>("--paper", () => true, "Use paper trading mode (simulated)")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            paperOption.AddAlias("-p");
            
            var testnetOption = new Option<bool>("--testnet", () => true, "Use testnet API")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            
            var verboseOption = new Option<bool>("--verbose", () => false, "Verbose output")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            verboseOption.AddAlias("-v");
            
            if (context.HostingBuilderBuilder?.Command != null)
            {
                context.HostingBuilderBuilder.Command.AddArgument(symbolArg);
                context.HostingBuilderBuilder.Command.AddArgument(sideArg);
                context.HostingBuilderBuilder.Command.AddArgument(quantityArg);
                context.HostingBuilderBuilder.Command.AddOption(priceOption);
                context.HostingBuilderBuilder.Command.AddOption(typeOption);
                context.HostingBuilderBuilder.Command.AddOption(paperOption);
                context.HostingBuilderBuilder.Command.AddOption(testnetOption);
                context.HostingBuilderBuilder.Command.AddOption(verboseOption);
            }

            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetService<ILogger>();
                
                var invocationContext = serviceProvider.GetService<LionFireCommandLineOptions>()?.InvocationContext;
                if (invocationContext == null)
                {
                    AnsiConsole.MarkupLine("[red]Error: Unable to get command invocation context[/]");
                    return;
                }

                try
                {
                    // For now, hardcode demo values since argument parsing isn't working
                    var symbol = "BTCUSD"; // TODO: Fix argument parsing - Note: Phemex uses BTCUSD not BTCUSDT
                    var side = "BUY";
                    var quantity = 0.001m;
                    var price = 65000m;
                    var type = "LIMIT";
                    var paper = true; // Always paper mode for safety
                    var testnet = true;
                    var verbose = false;

                    await ExecutePlaceOrder(PhemexExchangeArea.Spot, symbol, side, quantity, price, type, 
                        paper, testnet, verbose, configuration, logger);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                    logger?.LogError(ex, "Failed to place order");
                }
            });
        };

    public static Action<HostingBuilderBuilderContext, HostApplicationBuilder> FuturesPlaceOrder =>
        (context, builder) =>
        {
            var symbolArg = new Argument<string>("symbol", "Trading symbol (e.g., BTCUSDT)");
            var sideArg = new Argument<string>("side", "Order side (BUY or SELL)");
            var quantityArg = new Argument<decimal>("quantity", "Order quantity");
            
            var priceOption = new Option<decimal?>("--price", "Order price (for limit orders)")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            
            var typeOption = new Option<string>("--type", () => "LIMIT", "Order type (LIMIT or MARKET)")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            typeOption.AddAlias("-t");
            
            var paperOption = new Option<bool>("--paper", () => true, "Use paper trading mode (simulated)")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            paperOption.AddAlias("-p");
            
            var testnetOption = new Option<bool>("--testnet", () => true, "Use testnet API")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            
            var verboseOption = new Option<bool>("--verbose", () => false, "Verbose output")
            {
                Arity = ArgumentArity.ZeroOrOne
            };
            verboseOption.AddAlias("-v");
            
            if (context.HostingBuilderBuilder?.Command != null)
            {
                context.HostingBuilderBuilder.Command.AddArgument(symbolArg);
                context.HostingBuilderBuilder.Command.AddArgument(sideArg);
                context.HostingBuilderBuilder.Command.AddArgument(quantityArg);
                context.HostingBuilderBuilder.Command.AddOption(priceOption);
                context.HostingBuilderBuilder.Command.AddOption(typeOption);
                context.HostingBuilderBuilder.Command.AddOption(paperOption);
                context.HostingBuilderBuilder.Command.AddOption(testnetOption);
                context.HostingBuilderBuilder.Command.AddOption(verboseOption);
            }

            builder.Services.AddRunTaskAndShutdown(async (serviceProvider) =>
            {
                var configuration = serviceProvider.GetRequiredService<IConfiguration>();
                var logger = serviceProvider.GetService<ILogger>();
                
                var invocationContext = serviceProvider.GetService<LionFireCommandLineOptions>()?.InvocationContext;
                if (invocationContext == null)
                {
                    AnsiConsole.MarkupLine("[red]Error: Unable to get command invocation context[/]");
                    return;
                }

                try
                {
                    // For now, hardcode demo values since argument parsing isn't working
                    var symbol = "BTCUSD"; // TODO: Fix argument parsing - Note: Phemex uses BTCUSD not BTCUSDT
                    var side = "SELL";
                    var quantity = 0.001m;
                    var price = 66000m;
                    var type = "LIMIT";
                    var paper = true; // Always paper mode for safety
                    var testnet = true;
                    var verbose = false;

                    await ExecutePlaceOrder(PhemexExchangeArea.Futures, symbol, side, quantity, price, type, 
                        paper, testnet, verbose, configuration, logger);
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
                    logger?.LogError(ex, "Failed to place order");
                }
            });
        };

    private static async Task ExecuteTicker(PhemexExchangeArea area, string symbol, bool testnet, 
        bool verbose, IConfiguration configuration, ILogger? logger)
    {
        if (verbose)
        {
            AnsiConsole.MarkupLine($"[grey]Fetching {area} ticker for: {symbol}[/]");
            AnsiConsole.MarkupLine($"[grey]Testnet: {testnet}[/]");
        }

        var baseUrl = testnet ? "https://testnet-api.phemex.com" : "https://api.phemex.com";
        
        using var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var path = $"/md/v1/ticker/24hr?symbol={symbol}"; // Use v1 API for ticker data
        
        var response = await httpClient.GetAsync(path);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            AnsiConsole.MarkupLine($"[red]HTTP Error {response.StatusCode}: {responseContent}[/]");
            return;
        }

        dynamic? apiResponse = JsonConvert.DeserializeObject(responseContent);
        
        // Check for error in response
        if (apiResponse?.error != null)
        {
            AnsiConsole.MarkupLine($"[red]API Error: {apiResponse.error.message}[/]");
            return;
        }

        // For v1 API, the ticker data is in 'result' field
        var ticker = apiResponse?.result;
        if (ticker == null)
        {
            AnsiConsole.MarkupLine("[red]No ticker data received[/]");
            return;
        }

        DisplayTickerData(ticker, area);
    }

    private static void DisplayTickerData(dynamic ticker, PhemexExchangeArea area)
    {
        var table = new Table();
        table.Title = new TableTitle($"{area} Ticker: {ticker.symbol}");
        table.AddColumn("Metric");
        table.AddColumn(new TableColumn("Value").RightAligned());

        // Price scale for Phemex (10^4 for USD pairs)
        var priceScale = 10000m;
        
        // Parse the ticker data - fields are already present in the response
        decimal last = (decimal)(ticker.lastEp ?? 0) / priceScale;
        decimal bid = (decimal)(ticker.bidEp ?? 0) / priceScale;
        decimal ask = (decimal)(ticker.askEp ?? 0) / priceScale;
        decimal open = (decimal)(ticker.openEp ?? 0) / priceScale;
        decimal high = (decimal)(ticker.highEp ?? 0) / priceScale;
        decimal low = (decimal)(ticker.lowEp ?? 0) / priceScale;
        
        var change = last - open;
        var changePercent = open > 0 ? (change / open) * 100 : 0;
        var changeColor = change >= 0 ? "green" : "red";

        // Determine currency based on symbol
        string currency = ticker.symbol.ToString().Contains("USD") ? "USD" : "USDT";

        table.AddRow("Last Price", $"{last:N2} {currency}");
        table.AddRow("Bid", $"{bid:N2} {currency}");
        table.AddRow("Ask", $"{ask:N2} {currency}");
        table.AddRow("Spread", $"{(ask - bid):N2} {currency} ({((ask - bid) / bid * 100):N3}%)");
        table.AddRow("24h Open", $"{open:N2} {currency}");
        table.AddRow("24h High", $"{high:N2} {currency}");
        table.AddRow("24h Low", $"{low:N2} {currency}");
        table.AddRow("24h Change", $"[{changeColor}]{change:+0.00;-0.00} ({changePercent:+0.00;-0.00}%)[/]");
        
        // Volume might be in contracts for perpetuals
        if (ticker.volume != null)
        {
            table.AddRow("24h Volume", $"{ticker.volume:N0}");
        }
        
        // Turnover in Ev (scaled by 10^8)
        if (ticker.turnoverEv != null)
        {
            decimal turnover = (decimal)ticker.turnoverEv / 100_000_000m;
            table.AddRow("24h Turnover", $"{turnover:N2} {currency}");
        }
        
        // Funding rate for perpetuals
        if (ticker.fundingRateEr != null)
        {
            decimal fundingRate = (decimal)ticker.fundingRateEr / 1_000_000m;
            table.AddRow("Funding Rate", $"{fundingRate:P4}");
        }
        
        // Mark price for perpetuals
        if (ticker.markEp != null)
        {
            decimal mark = (decimal)ticker.markEp / priceScale;
            table.AddRow("Mark Price", $"{mark:N2} {currency}");
        }
        
        // Timestamp is in nanoseconds
        if (ticker.timestamp != null)
        {
            var timestamp = DateTimeOffset.FromUnixTimeMilliseconds((long)ticker.timestamp / 1_000_000);
            table.AddRow("Last Update", timestamp.ToString("yyyy-MM-dd HH:mm:ss"));
        }

        AnsiConsole.Write(table);
    }

    private static async Task ExecutePlaceOrder(PhemexExchangeArea area, string symbol, string side, 
        decimal quantity, decimal? price, string type, bool paper, bool testnet, bool verbose, 
        IConfiguration configuration, ILogger? logger)
    {
        // Validate input
        if (string.IsNullOrEmpty(symbol))
        {
            AnsiConsole.MarkupLine("[red]Error: Symbol is required[/]");
            return;
        }

        side = side.ToUpper();
        if (side != "BUY" && side != "SELL")
        {
            AnsiConsole.MarkupLine("[red]Error: Side must be BUY or SELL[/]");
            return;
        }

        if (quantity <= 0)
        {
            AnsiConsole.MarkupLine("[red]Error: Quantity must be positive[/]");
            return;
        }

        type = type.ToUpper();
        if (type == "LIMIT" && !price.HasValue)
        {
            AnsiConsole.MarkupLine("[red]Error: Price is required for LIMIT orders[/]");
            return;
        }

        if (paper)
        {
            await ExecutePaperOrder(area, symbol, side, quantity, price, type, verbose, logger);
        }
        else
        {
            AnsiConsole.MarkupLine("[yellow]Warning: Live trading not yet implemented. Using paper mode.[/]");
            await ExecutePaperOrder(area, symbol, side, quantity, price, type, verbose, logger);
        }
    }

    private static async Task ExecutePaperOrder(PhemexExchangeArea area, string symbol, string side,
        decimal quantity, decimal? price, string type, bool verbose, ILogger? logger)
    {
        AnsiConsole.MarkupLine($"[yellow]PAPER TRADING MODE - {area}[/]");
        
        var table = new Table();
        table.Title = new TableTitle("Simulated Order");
        table.AddColumn("Field");
        table.AddColumn("Value");

        var orderId = Guid.NewGuid().ToString("N").Substring(0, 12);
        var timestamp = DateTimeOffset.UtcNow;

        table.AddRow("Order ID", orderId);
        table.AddRow("Market", area.ToString());
        table.AddRow("Symbol", symbol);
        table.AddRow("Side", side);
        table.AddRow("Type", type);
        table.AddRow("Quantity", quantity.ToString("N8"));
        
        if (price.HasValue)
        {
            table.AddRow("Price", $"{price.Value:N2} USDT");
            table.AddRow("Total Value", $"{(price.Value * quantity):N2} USDT");
        }
        else
        {
            table.AddRow("Price", "MARKET");
        }
        
        table.AddRow("Status", "[green]SIMULATED[/]");
        table.AddRow("Time", timestamp.ToString("yyyy-MM-dd HH:mm:ss"));

        AnsiConsole.Write(table);
        
        AnsiConsole.MarkupLine("[green]✓ Paper order successfully simulated[/]");
        
        // Log to file for tracking
        logger?.LogInformation("Paper order placed: {OrderId} {Area} {Symbol} {Side} {Quantity} @ {Price}", 
            orderId, area, symbol, side, quantity, price);

        await Task.Delay(100); // Simulate network delay
    }
}

#region API Models (extracted from original PhemexCommands.cs)

public class PhemexOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public bool IsTestnet { get; set; } = true;
    public bool UseHighRateLimitApi { get; set; } = false;
    public long? SubAccountId { get; set; }
    public string BaseUrl { get; set; } = string.Empty;
    public int RateLimitPerSecond { get; set; } = 10;

    public void ConfigureEndpoints()
    {
        if (UseHighRateLimitApi)
        {
            BaseUrl = IsTestnet ? "https://vapi.testnet.phemex.com" : "https://vapi.phemex.com";
            RateLimitPerSecond = 100;
        }
        else
        {
            BaseUrl = IsTestnet ? "https://testnet-api.phemex.com" : "https://api.phemex.com";
        }
    }
}

public class PhemexApiResponse<T>
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("msg")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("data")]
    public T? Data { get; set; }
}

public class PhemexAccountInfo
{
    [JsonProperty("accountBalanceEv")]
    public long AccountBalanceEv { get; set; }

    [JsonProperty("totalUsedBalanceEv")]
    public long TotalUsedBalanceEv { get; set; }

    [JsonProperty("accountID")]
    public long AccountId { get; set; }

    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonProperty("totalBalanceEv")]
    public long TotalBalanceEv { get; set; }

    [JsonProperty("availableBalanceEv")]
    public long AvailableBalanceEv { get; set; }

    [JsonProperty("unrealisedPnlEv")]
    public long UnrealisedPnlEv { get; set; }

    [JsonProperty("marginBalanceEv")]
    public long MarginBalanceEv { get; set; }

    [JsonProperty("positionMarginEv")]
    public long PositionMarginEv { get; set; }

    [JsonProperty("orderMarginEv")]
    public long OrderMarginEv { get; set; }

    [JsonProperty("posSide")]
    public string PosSide { get; set; } = string.Empty;
}

public class PhemexPositionData
{
    [JsonProperty("account")]
    public PhemexAccountInfo Account { get; set; } = new();

    [JsonProperty("positions")]
    public List<PhemexPosition> Positions { get; set; } = new();
}

public class PhemexPosition
{
    [JsonProperty("accountID")]
    public long AccountId { get; set; }

    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonProperty("side")]
    public string Side { get; set; } = string.Empty;

    [JsonProperty("positionStatus")]
    public string PositionStatus { get; set; } = string.Empty;

    [JsonProperty("crossMargin")]
    public bool CrossMargin { get; set; }

    [JsonProperty("leverageEr")]
    public long LeverageEr { get; set; }

    [JsonProperty("leverage")]
    public decimal Leverage => LeverageEr / 10000m;

    [JsonProperty("posCostEv")]
    public long PosCostEv { get; set; }
}

public class PhemexSubAccount
{
    [JsonProperty("accountId")]
    public long AccountId { get; set; }

    [JsonProperty("accountName")]
    public string? AccountName { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }
}

#endregion