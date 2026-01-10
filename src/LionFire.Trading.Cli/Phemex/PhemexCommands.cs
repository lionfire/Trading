using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using JasperFx.CommandLine;
using Spectre.Console;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using LionFire.Trading.HistoricalData.Retrieval;

namespace LionFire.Trading.Cli.Phemex;

#region Input Classes

public class PhemexBalanceInput : NetCoreInput
{
    [Description("Use testnet API")]
    [FlagAlias("testnet", 't')]
    public bool TestnetFlag { get; set; } = true;

    [Description("Use high-rate API (vapi.phemex.com)")]
    [FlagAlias("high-rate", 'h')]
    public bool HighRateFlag { get; set; } = false;

    [Description("Subaccount ID")]
    [FlagAlias("subaccount", 's')]
    public long? SubaccountFlag { get; set; }

    [Description("API Key (overrides configuration)")]
    [FlagAlias("api-key", 'k')]
    public string? ApiKeyFlag { get; set; }

    [Description("API Secret (overrides configuration)")]
    [FlagAlias("api-secret")]
    public string? ApiSecretFlag { get; set; }

    [Description("Verbose output")]
    [FlagAlias("verbose", 'v')]
    public new bool VerboseFlag { get; set; }
}

public class PhemexSubaccountsInput : NetCoreInput
{
    [Description("Use testnet API")]
    [FlagAlias("testnet", 't')]
    public bool TestnetFlag { get; set; } = true;

    [Description("Verbose output")]
    [FlagAlias("verbose", 'v')]
    public new bool VerboseFlag { get; set; }
}

public class PhemexPositionsInput : NetCoreInput
{
    [Description("Use testnet API")]
    [FlagAlias("testnet", 't')]
    public bool TestnetFlag { get; set; } = true;

    [Description("Subaccount ID")]
    [FlagAlias("subaccount", 's')]
    public long? SubaccountFlag { get; set; }

    [Description("Currency (USD or BTC)")]
    [FlagAlias("currency", 'c')]
    public string CurrencyFlag { get; set; } = "USD";

    [Description("Verbose output")]
    [FlagAlias("verbose", 'v')]
    public new bool VerboseFlag { get; set; }
}

public class PhemexTickerInput : NetCoreInput
{
    [Description("Trading symbol (e.g., BTCUSDT, ETHUSDT)")]
    public string Symbol { get; set; } = "BTCUSDT";

    [Description("Use testnet API")]
    [FlagAlias("testnet", 't')]
    public bool TestnetFlag { get; set; } = true;

    [Description("Verbose output")]
    [FlagAlias("verbose", 'v')]
    public new bool VerboseFlag { get; set; }
}

public class PhemexOrderInput : NetCoreInput
{
    [Description("Trading symbol (e.g., BTCUSDT)")]
    public string Symbol { get; set; } = "";

    [Description("Order side (BUY or SELL)")]
    public string Side { get; set; } = "";

    [Description("Order quantity")]
    public decimal Quantity { get; set; }

    [Description("Order price (for limit orders)")]
    public decimal? Price { get; set; }

    [Description("Order type (LIMIT or MARKET)")]
    [FlagAlias("type", 't')]
    public string TypeFlag { get; set; } = "LIMIT";

    [Description("Use paper trading mode (simulated)")]
    [FlagAlias("paper", 'p')]
    public bool PaperFlag { get; set; } = true;

    [Description("Use testnet API")]
    [FlagAlias("testnet")]
    public bool TestnetFlag { get; set; } = true;

    [Description("Verbose output")]
    [FlagAlias("verbose", 'v')]
    public new bool VerboseFlag { get; set; }
}

#endregion

#region Configuration

public class PhemexOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string ApiSecret { get; set; } = string.Empty;
    public bool IsTestnet { get; set; } = true;
    public bool UseHighRateLimitApi { get; set; } = false;
    public long? SubAccountId { get; set; }
    public string BaseUrl { get; set; } = "https://testnet-api.phemex.com";
    public int RateLimitPerSecond { get; set; } = 10;

    public void ConfigureEndpoints()
    {
        if (IsTestnet)
        {
            BaseUrl = "https://testnet-api.phemex.com";
            UseHighRateLimitApi = false;
        }
        else if (UseHighRateLimitApi)
        {
            BaseUrl = "https://vapi.phemex.com";
            if (RateLimitPerSecond == 10)
                RateLimitPerSecond = 100;
        }
        else
        {
            BaseUrl = "https://api.phemex.com";
        }
    }
}

#endregion

#region API Models

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

    [JsonProperty("initMarginReqEr")]
    public long InitMarginReqEr { get; set; }

    [JsonProperty("posCostEv")]
    public long PosCostEv { get; set; }

    [JsonProperty("positionMarginEv")]
    public long PositionMarginEv { get; set; }

    [JsonProperty("positionValueEv")]
    public long PositionValueEv { get; set; }

    [JsonProperty("unrealisedPnlEv")]
    public long UnrealisedPnlEv { get; set; }

    [JsonProperty("avgEntryPriceEp")]
    public long AvgEntryPriceEp { get; set; }

    [JsonProperty("cumRealisedPnlEv")]
    public long CumRealisedPnlEv { get; set; }

    [JsonProperty("size")]
    public long Size { get; set; }

    [JsonProperty("markPriceEp")]
    public long MarkPriceEp { get; set; }

    [JsonProperty("markValueEv")]
    public long MarkValueEv { get; set; }

    [JsonProperty("riskLimitEv")]
    public long RiskLimitEv { get; set; }
}

public class PhemexSubAccount
{
    [JsonProperty("userId")]
    public long UserId { get; set; }

    [JsonProperty("nickName")]
    public string NickName { get; set; } = string.Empty;

    [JsonProperty("passwordState")]
    public int PasswordState { get; set; }

    [JsonProperty("totp")]
    public int Totp { get; set; }

    [JsonProperty("status")]
    public int Status { get; set; }
}

public class PhemexTicker
{
    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("lastEp")]
    public long LastEp { get; set; }

    [JsonProperty("bidEp")]
    public long BidEp { get; set; }

    [JsonProperty("askEp")]
    public long AskEp { get; set; }

    [JsonProperty("openEp")]
    public long OpenEp { get; set; }

    [JsonProperty("highEp")]
    public long HighEp { get; set; }

    [JsonProperty("lowEp")]
    public long LowEp { get; set; }

    [JsonProperty("volume")]
    public long Volume { get; set; }

    [JsonProperty("turnoverEv")]
    public long TurnoverEv { get; set; }

    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }
}

public class PhemexOrderResponse
{
    [JsonProperty("orderID")]
    public string OrderId { get; set; } = string.Empty;

    [JsonProperty("clOrdID")]
    public string ClientOrderId { get; set; } = string.Empty;

    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("side")]
    public string Side { get; set; } = string.Empty;

    [JsonProperty("orderType")]
    public string OrderType { get; set; } = string.Empty;

    [JsonProperty("priceEp")]
    public long PriceEp { get; set; }

    [JsonProperty("orderQty")]
    public long OrderQty { get; set; }

    [JsonProperty("ordStatus")]
    public string OrderStatus { get; set; } = string.Empty;

    [JsonProperty("createTime")]
    public long CreateTime { get; set; }
}

#endregion

#region Command Classes

[Area("phemex")]
[Description("Check Phemex account balance", Name = "balance")]
public class PhemexBalanceCommand : JasperFxAsyncCommand<PhemexBalanceInput>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PhemexBalanceCommand>? _logger;

    public PhemexBalanceCommand(IConfiguration configuration, ILogger<PhemexBalanceCommand>? logger = null)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Explicitly define no positional arguments
        Usage("Check Phemex account balance");
    }

    public override async Task<bool> Execute(PhemexBalanceInput input)
    {

        try
        {
            // Build options from configuration and command line
            var options = new PhemexOptions();
            _configuration.GetSection("Phemex").Bind(options);

            // Override with command line options
            if (!string.IsNullOrEmpty(input.ApiKeyFlag))
                options.ApiKey = input.ApiKeyFlag;
            if (!string.IsNullOrEmpty(input.ApiSecretFlag))
                options.ApiSecret = input.ApiSecretFlag;
            if (input.TestnetFlag)
                options.IsTestnet = true;
            if (input.HighRateFlag)
                options.UseHighRateLimitApi = true;
            if (input.SubaccountFlag.HasValue)
                options.SubAccountId = input.SubaccountFlag;

            // Configure endpoints
            options.ConfigureEndpoints();

            if (string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.ApiSecret))
            {
                AnsiConsole.MarkupLine("[red]Error: API credentials not configured![/]");
                AnsiConsole.WriteLine("Please set the following in your appsettings.json or environment variables:");
                AnsiConsole.WriteLine("  Phemex__ApiKey=your_api_key");
                AnsiConsole.WriteLine("  Phemex__ApiSecret=your_api_secret");
                return false;
            }

            if (input.VerboseFlag)
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
                    var result = await GetAccountBalance(options, _logger);
                    if (result != null)
                    {
                        DisplayAccountBalance(result);
                    }
                });

            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            _logger?.LogError(ex, "Failed to fetch Phemex balance");
            return false;
        }
    }

    private async Task<PhemexPositionData?> GetAccountBalance(PhemexOptions options, ILogger? logger)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl) };
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var path = "/accounts/accountPositions?currency=USD";
        if (options.SubAccountId.HasValue)
            path += $"&accountID={options.SubAccountId}";

        var expires = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60;
        var signatureData = $"{path}{expires}";
        var signature = CreateSignature(signatureData, options.ApiSecret);

        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("x-phemex-access-token", options.ApiKey);
        request.Headers.Add("x-phemex-request-expiry", expires.ToString());
        request.Headers.Add("x-phemex-request-signature", signature);

        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            AnsiConsole.MarkupLine($"[red]HTTP Error {response.StatusCode}: {responseContent}[/]");
            return null;
        }

        var apiResponse = JsonConvert.DeserializeObject<PhemexApiResponse<PhemexPositionData>>(responseContent);

        if (apiResponse?.Code != 0)
        {
            AnsiConsole.MarkupLine($"[red]API Error {apiResponse?.Code}: {apiResponse?.Message}[/]");
            return null;
        }

        return apiResponse.Data;
    }

    private void DisplayAccountBalance(PhemexPositionData data)
    {
        var account = data.Account;

        var table = new Table();
        table.Title = new TableTitle($"Phemex Account Balance (ID: {account.AccountId})");
        table.AddColumn("Metric");
        table.AddColumn(new TableColumn("Value").RightAligned());

        // Convert from scaled values (Ev = scaled by 10^8 for USD)
        var scaleFactor = account.Currency == "USD" ? 100_000_000m : 100_000_000m;

        table.AddRow("Currency", account.Currency);
        table.AddRow("Account Balance", $"{account.AccountBalanceEv / scaleFactor:N2} {account.Currency}");
        table.AddRow("Available Balance", $"{account.AvailableBalanceEv / scaleFactor:N2} {account.Currency}");
        table.AddRow("Margin Balance", $"{account.MarginBalanceEv / scaleFactor:N2} {account.Currency}");
        table.AddRow("Position Margin", $"{account.PositionMarginEv / scaleFactor:N2} {account.Currency}");
        table.AddRow("Order Margin", $"{account.OrderMarginEv / scaleFactor:N2} {account.Currency}");
        table.AddRow("Unrealized PnL", $"{account.UnrealisedPnlEv / scaleFactor:N2} {account.Currency}");
        table.AddRow("Total Used Balance", $"{account.TotalUsedBalanceEv / scaleFactor:N2} {account.Currency}");

        AnsiConsole.Write(table);

        if (data.Positions.Any())
        {
            AnsiConsole.WriteLine();
            DisplayPositions(data.Positions, account.Currency);
        }
    }

    private void DisplayPositions(List<PhemexPosition> positions, string currency)
    {
        var table = new Table();
        table.Title = new TableTitle($"Open Positions ({positions.Count})");
        table.AddColumn("Symbol");
        table.AddColumn(new TableColumn("Side").Centered());
        table.AddColumn(new TableColumn("Size").RightAligned());
        table.AddColumn(new TableColumn("Entry Price").RightAligned());
        table.AddColumn(new TableColumn("Mark Price").RightAligned());
        table.AddColumn(new TableColumn("Unrealized PnL").RightAligned());
        table.AddColumn(new TableColumn("Leverage").RightAligned());

        var scaleFactor = currency == "USD" ? 100_000_000m : 100_000_000m;
        var priceScale = 10_000m; // Ep = scaled by 10^4

        foreach (var pos in positions)
        {
            var sideColor = pos.Side == "Buy" ? "green" : "red";
            var pnlColor = pos.UnrealisedPnlEv >= 0 ? "green" : "red";

            table.AddRow(
                pos.Symbol,
                $"[{sideColor}]{pos.Side}[/]",
                pos.Size.ToString(),
                $"{pos.AvgEntryPriceEp / priceScale:N2}",
                $"{pos.MarkPriceEp / priceScale:N2}",
                $"[{pnlColor}]{pos.UnrealisedPnlEv / scaleFactor:N2} {currency}[/]",
                $"{pos.Leverage:N1}x"
            );
        }

        AnsiConsole.Write(table);
    }

    private static string CreateSignature(string data, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}

[Area("phemex")]
[Description("List Phemex subaccounts", Name = "subaccounts")]
public class PhemexSubaccountsCommand : JasperFxAsyncCommand<PhemexSubaccountsInput>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PhemexSubaccountsCommand>? _logger;

    public PhemexSubaccountsCommand(IConfiguration configuration, ILogger<PhemexSubaccountsCommand>? logger = null)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Explicitly define no positional arguments
        Usage("List Phemex subaccounts");
    }

    public override async Task<bool> Execute(PhemexSubaccountsInput input)
    {

        try
        {
            var options = new PhemexOptions();
            _configuration.GetSection("Phemex").Bind(options);

            if (input.TestnetFlag)
                options.IsTestnet = true;

            options.ConfigureEndpoints();

            if (string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.ApiSecret))
            {
                AnsiConsole.MarkupLine("[red]Error: API credentials not configured![/]");
                return false;
            }

            await AnsiConsole.Status()
                .StartAsync("Fetching subaccounts...", async ctx =>
                {
                    var subaccounts = await GetSubaccounts(options, _logger);
                    if (subaccounts != null)
                    {
                        DisplaySubaccounts(subaccounts);
                    }
                });

            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            _logger?.LogError(ex, "Failed to fetch Phemex subaccounts");
            return false;
        }
    }

    private async Task<List<PhemexSubAccount>?> GetSubaccounts(PhemexOptions options, ILogger? logger)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl) };
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var path = "/phemex-user/users/children";
        var expires = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60;
        var signatureData = $"{path}{expires}";
        var signature = CreateSignature(signatureData, options.ApiSecret);

        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("x-phemex-access-token", options.ApiKey);
        request.Headers.Add("x-phemex-request-expiry", expires.ToString());
        request.Headers.Add("x-phemex-request-signature", signature);

        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            AnsiConsole.MarkupLine($"[red]HTTP Error {response.StatusCode}: {responseContent}[/]");
            return null;
        }

        var apiResponse = JsonConvert.DeserializeObject<PhemexApiResponse<List<PhemexSubAccount>>>(responseContent);

        if (apiResponse?.Code != 0)
        {
            AnsiConsole.MarkupLine($"[red]API Error {apiResponse?.Code}: {apiResponse?.Message}[/]");
            return null;
        }

        return apiResponse.Data;
    }

    private void DisplaySubaccounts(List<PhemexSubAccount> subaccounts)
    {
        if (!subaccounts.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No subaccounts found[/]");
            return;
        }

        var table = new Table();
        table.Title = new TableTitle($"Phemex Subaccounts ({subaccounts.Count})");
        table.AddColumn("User ID");
        table.AddColumn("Nickname");
        table.AddColumn("Status");
        table.AddColumn("2FA");

        foreach (var account in subaccounts)
        {
            var statusText = account.Status == 1 ? "[green]Active[/]" : "[red]Inactive[/]";
            var totpText = account.Totp == 1 ? "[green]Enabled[/]" : "[yellow]Disabled[/]";

            table.AddRow(
                account.UserId.ToString(),
                account.NickName,
                statusText,
                totpText
            );
        }

        AnsiConsole.Write(table);
    }

    private static string CreateSignature(string data, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}

[Area("phemex")]
[Description("Show Phemex positions", Name = "positions")]
public class PhemexPositionsCommand : JasperFxAsyncCommand<PhemexPositionsInput>
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<PhemexPositionsCommand>? _logger;

    public PhemexPositionsCommand(IConfiguration configuration, ILogger<PhemexPositionsCommand>? logger = null)
    {
        _configuration = configuration;
        _logger = logger;
        
        // Explicitly define no positional arguments
        Usage("Show Phemex positions");
    }

    public override async Task<bool> Execute(PhemexPositionsInput input)
    {

        try
        {
            var options = new PhemexOptions();
            _configuration.GetSection("Phemex").Bind(options);

            if (input.TestnetFlag)
                options.IsTestnet = true;
            if (input.SubaccountFlag.HasValue)
                options.SubAccountId = input.SubaccountFlag;

            options.ConfigureEndpoints();

            if (string.IsNullOrEmpty(options.ApiKey) || string.IsNullOrEmpty(options.ApiSecret))
            {
                AnsiConsole.MarkupLine("[red]Error: API credentials not configured![/]");
                return false;
            }

            await AnsiConsole.Status()
                .StartAsync("Fetching positions...", async ctx =>
                {
                    var result = await GetPositions(options, input.CurrencyFlag, _logger);
                    if (result != null)
                    {
                        DisplayPositionsOnly(result);
                    }
                });

            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            _logger?.LogError(ex, "Failed to fetch Phemex positions");
            return false;
        }
    }

    private async Task<PhemexPositionData?> GetPositions(PhemexOptions options, string currency, ILogger? logger)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl) };
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var path = $"/accounts/accountPositions?currency={currency}";
        if (options.SubAccountId.HasValue)
            path += $"&accountID={options.SubAccountId}";

        var expires = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 60;
        var signatureData = $"{path}{expires}";
        var signature = CreateSignature(signatureData, options.ApiSecret);

        var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Add("x-phemex-access-token", options.ApiKey);
        request.Headers.Add("x-phemex-request-expiry", expires.ToString());
        request.Headers.Add("x-phemex-request-signature", signature);

        var response = await httpClient.SendAsync(request);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            AnsiConsole.MarkupLine($"[red]HTTP Error {response.StatusCode}: {responseContent}[/]");
            return null;
        }

        var apiResponse = JsonConvert.DeserializeObject<PhemexApiResponse<PhemexPositionData>>(responseContent);

        if (apiResponse?.Code != 0)
        {
            AnsiConsole.MarkupLine($"[red]API Error {apiResponse?.Code}: {apiResponse?.Message}[/]");
            return null;
        }

        return apiResponse.Data;
    }

    private void DisplayPositionsOnly(PhemexPositionData data)
    {
        if (!data.Positions.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No open positions[/]");
            return;
        }

        var table = new Table();
        table.Title = new TableTitle($"Open Positions ({data.Positions.Count})");
        table.AddColumn("Symbol");
        table.AddColumn(new TableColumn("Side").Centered());
        table.AddColumn(new TableColumn("Size").RightAligned());
        table.AddColumn(new TableColumn("Entry Price").RightAligned());
        table.AddColumn(new TableColumn("Mark Price").RightAligned());
        table.AddColumn(new TableColumn("Unrealized PnL").RightAligned());
        table.AddColumn(new TableColumn("Realized PnL").RightAligned());
        table.AddColumn(new TableColumn("Leverage").RightAligned());
        table.AddColumn(new TableColumn("Margin").RightAligned());

        var currency = data.Account.Currency;
        var scaleFactor = currency == "USD" ? 100_000_000m : 100_000_000m;
        var priceScale = 10_000m;

        decimal totalUnrealizedPnl = 0;
        decimal totalRealizedPnl = 0;

        foreach (var pos in data.Positions)
        {
            var sideColor = pos.Side == "Buy" ? "green" : "red";
            var unrealizedPnl = pos.UnrealisedPnlEv / scaleFactor;
            var realizedPnl = pos.CumRealisedPnlEv / scaleFactor;
            var pnlColor = unrealizedPnl >= 0 ? "green" : "red";
            var realizedPnlColor = realizedPnl >= 0 ? "green" : "red";

            totalUnrealizedPnl += unrealizedPnl;
            totalRealizedPnl += realizedPnl;

            table.AddRow(
                pos.Symbol,
                $"[{sideColor}]{pos.Side}[/]",
                pos.Size.ToString(),
                $"{pos.AvgEntryPriceEp / priceScale:N2}",
                $"{pos.MarkPriceEp / priceScale:N2}",
                $"[{pnlColor}]{unrealizedPnl:N2}[/]",
                $"[{realizedPnlColor}]{realizedPnl:N2}[/]",
                $"{pos.Leverage:N1}x",
                $"{pos.PositionMarginEv / scaleFactor:N2}"
            );
        }

        // Add totals row
        table.AddEmptyRow();
        var totalUnrealizedColor = totalUnrealizedPnl >= 0 ? "green" : "red";
        var totalRealizedColor = totalRealizedPnl >= 0 ? "green" : "red";
        
        table.AddRow(
            "[bold]TOTAL[/]",
            "",
            "",
            "",
            "",
            $"[bold][{totalUnrealizedColor}]{totalUnrealizedPnl:N2}[/][/]",
            $"[bold][{totalRealizedColor}]{totalRealizedPnl:N2}[/][/]",
            "",
            ""
        );

        AnsiConsole.Write(table);

        // Show account summary
        AnsiConsole.WriteLine();
        var summaryTable = new Table();
        summaryTable.Title = new TableTitle("Account Summary");
        summaryTable.AddColumn("Metric");
        summaryTable.AddColumn(new TableColumn("Value").RightAligned());

        summaryTable.AddRow("Currency", currency);
        summaryTable.AddRow("Account Balance", $"{data.Account.AccountBalanceEv / scaleFactor:N2} {currency}");
        summaryTable.AddRow("Available Balance", $"{data.Account.AvailableBalanceEv / scaleFactor:N2} {currency}");
        summaryTable.AddRow("Position Margin", $"{data.Account.PositionMarginEv / scaleFactor:N2} {currency}");

        AnsiConsole.Write(summaryTable);
    }

    private static string CreateSignature(string data, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }
}

[Area("phemex")]
[Description("Get current ticker price for a symbol")]
public class PhemexTickerCommand : JasperFxAsyncCommand<PhemexTickerInput>
{
    private ILogger<PhemexTickerCommand>? _logger;

    public PhemexTickerCommand()
    {
        Usage("Get ticker for symbol").Arguments(x => x.Symbol);
    }

    public override async Task<bool> Execute(PhemexTickerInput input)
    {
        try
        {
            var host = input.BuildHost();
            _logger = host.Services.GetService<ILogger<PhemexTickerCommand>>();
            var config = host.Services.GetRequiredService<IConfiguration>();

            var options = LoadConfiguration(config);
            
            // Override with command line flags
            options.IsTestnet = input.TestnetFlag;
            options.ConfigureEndpoints();

            if (input.VerboseFlag)
            {
                AnsiConsole.MarkupLine($"[grey]Fetching ticker for: {input.Symbol}[/]");
                AnsiConsole.MarkupLine($"[grey]Endpoint: {options.BaseUrl}[/]");
                AnsiConsole.MarkupLine($"[grey]Testnet: {options.IsTestnet}[/]");
            }

            var ticker = await GetTicker(options, input.Symbol, _logger);
            if (ticker != null)
            {
                DisplayTicker(ticker);
            }

            return true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            _logger?.LogError(ex, "Failed to fetch ticker");
            return false;
        }
    }

    private PhemexOptions LoadConfiguration(IConfiguration config)
    {
        var options = new PhemexOptions();
        
        // Load from configuration
        options.ApiKey = config["PHEMEX_API_KEY"] ?? 
                        config["Phemex:ApiKey"] ?? 
                        Environment.GetEnvironmentVariable("PHEMEX_API_KEY") ?? "";
        options.ApiSecret = config["PHEMEX_API_SECRET"] ?? 
                           config["Phemex:ApiSecret"] ?? 
                           Environment.GetEnvironmentVariable("PHEMEX_API_SECRET") ?? "";
        options.IsTestnet = config.GetValue<bool?>("PHEMEX_TESTNET") ?? 
                           config.GetValue<bool?>("Phemex:IsTestnet") ?? true;

        return options;
    }

    private async Task<PhemexTicker?> GetTicker(PhemexOptions options, string symbol, ILogger? logger)
    {
        using var httpClient = new HttpClient { BaseAddress = new Uri(options.BaseUrl) };
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Public endpoint - no authentication needed
        var path = $"/md/ticker/24hr?symbol={symbol}";
        
        var response = await httpClient.GetAsync(path);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            AnsiConsole.MarkupLine($"[red]HTTP Error {response.StatusCode}: {responseContent}[/]");
            return null;
        }

        var apiResponse = JsonConvert.DeserializeObject<PhemexApiResponse<PhemexTicker>>(responseContent);

        if (apiResponse?.Code != 0)
        {
            AnsiConsole.MarkupLine($"[red]API Error {apiResponse?.Code}: {apiResponse?.Message}[/]");
            return null;
        }

        return apiResponse.Data;
    }

    private void DisplayTicker(PhemexTicker ticker)
    {
        var table = new Table();
        table.Title = new TableTitle($"Ticker: {ticker.Symbol}");
        table.AddColumn("Metric");
        table.AddColumn(new TableColumn("Value").RightAligned());

        // Price scale for spot markets (typically 10^4 for USDT pairs)
        var priceScale = 10000m;
        
        var last = ticker.LastEp / priceScale;
        var bid = ticker.BidEp / priceScale;
        var ask = ticker.AskEp / priceScale;
        var open = ticker.OpenEp / priceScale;
        var high = ticker.HighEp / priceScale;
        var low = ticker.LowEp / priceScale;
        
        var change = last - open;
        var changePercent = open > 0 ? (change / open) * 100 : 0;
        var changeColor = change >= 0 ? "green" : "red";

        table.AddRow("Last Price", $"{last:N2} USDT");
        table.AddRow("Bid", $"{bid:N2} USDT");
        table.AddRow("Ask", $"{ask:N2} USDT");
        table.AddRow("Spread", $"{(ask - bid):N2} USDT ({((ask - bid) / bid * 100):N3}%)");
        table.AddRow("24h Open", $"{open:N2} USDT");
        table.AddRow("24h High", $"{high:N2} USDT");
        table.AddRow("24h Low", $"{low:N2} USDT");
        table.AddRow("24h Change", $"[{changeColor}]{change:+0.00;-0.00} ({changePercent:+0.00;-0.00}%)[/]");
        table.AddRow("24h Volume", $"{ticker.Volume:N0}");
        
        var turnover = ticker.TurnoverEv / 100_000_000m; // Ev = scaled by 10^8
        table.AddRow("24h Turnover", $"{turnover:N2} USDT");
        
        var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(ticker.Timestamp / 1000);
        table.AddRow("Last Update", timestamp.ToString("yyyy-MM-dd HH:mm:ss"));

        AnsiConsole.Write(table);
    }
}

[Area("phemex")]
[Description("Place an order (paper or live)")]
public class PhemexPlaceOrderCommand : JasperFxAsyncCommand<PhemexOrderInput>
{
    private ILogger<PhemexPlaceOrderCommand>? _logger;

    public PhemexPlaceOrderCommand()
    {
        Usage("Place an order").Arguments(x => x.Symbol, x => x.Side, x => x.Quantity);
    }

    public override async Task<bool> Execute(PhemexOrderInput input)
    {
        try
        {
            // Validate input
            if (string.IsNullOrEmpty(input.Symbol))
            {
                AnsiConsole.MarkupLine("[red]Error: Symbol is required[/]");
                return false;
            }

            if (string.IsNullOrEmpty(input.Side) || (input.Side.ToUpper() != "BUY" && input.Side.ToUpper() != "SELL"))
            {
                AnsiConsole.MarkupLine("[red]Error: Side must be BUY or SELL[/]");
                return false;
            }

            if (input.Quantity <= 0)
            {
                AnsiConsole.MarkupLine("[red]Error: Quantity must be positive[/]");
                return false;
            }

            if (input.TypeFlag.ToUpper() == "LIMIT" && !input.Price.HasValue)
            {
                AnsiConsole.MarkupLine("[red]Error: Price is required for LIMIT orders[/]");
                return false;
            }

            var host = input.BuildHost();
            _logger = host.Services.GetService<ILogger<PhemexPlaceOrderCommand>>();

            if (input.PaperFlag)
            {
                return await ExecutePaperOrder(input);
            }
            else
            {
                AnsiConsole.MarkupLine("[yellow]Warning: Live trading not yet implemented. Using paper mode.[/]");
                return await ExecutePaperOrder(input);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error: {ex.Message}[/]");
            _logger?.LogError(ex, "Failed to place order");
            return false;
        }
    }

    private async Task<bool> ExecutePaperOrder(PhemexOrderInput input)
    {
        AnsiConsole.MarkupLine("[yellow]PAPER TRADING MODE[/]");
        
        var table = new Table();
        table.Title = new TableTitle("Simulated Order");
        table.AddColumn("Field");
        table.AddColumn("Value");

        var orderId = Guid.NewGuid().ToString("N").Substring(0, 12);
        var timestamp = DateTimeOffset.UtcNow;

        table.AddRow("Order ID", orderId);
        table.AddRow("Symbol", input.Symbol);
        table.AddRow("Side", input.Side.ToUpper());
        table.AddRow("Type", input.TypeFlag.ToUpper());
        table.AddRow("Quantity", input.Quantity.ToString("N8"));
        
        if (input.Price.HasValue)
        {
            table.AddRow("Price", $"{input.Price.Value:N2} USDT");
            table.AddRow("Total Value", $"{(input.Price.Value * input.Quantity):N2} USDT");
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
        _logger?.LogInformation("Paper order placed: {OrderId} {Symbol} {Side} {Quantity} @ {Price}", 
            orderId, input.Symbol, input.Side, input.Quantity, input.Price);

        await Task.Delay(100); // Simulate network delay
        return true;
    }
}

#endregion