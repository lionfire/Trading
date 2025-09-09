using Newtonsoft.Json;

namespace LionFire.Trading.Phemex.Api.Models;

public class PhemexPosition
{
    [JsonProperty("accountID")]
    public long AccountId { get; set; }
    
    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;
    
    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;
    
    [JsonProperty("side")]
    public string Side { get; set; } = string.Empty; // Buy or Sell
    
    [JsonProperty("positionStatus")]
    public string Status { get; set; } = string.Empty;
    
    [JsonProperty("size")]
    public decimal Size { get; set; }
    
    [JsonProperty("avgEntryPriceEp")]
    public long AverageEntryPriceEp { get; set; }
    
    [JsonProperty("posCostEv")]
    public long PositionCostEv { get; set; }
    
    [JsonProperty("posBalanceEv")]
    public long PositionBalanceEv { get; set; }
    
    [JsonProperty("posMarginEv")]
    public long PositionMarginEv { get; set; }
    
    [JsonProperty("posPnlEv")]
    public long UnrealizedPnlEv { get; set; }
    
    [JsonProperty("realizedPnlEv")]
    public long RealizedPnlEv { get; set; }
    
    [JsonProperty("markPriceEp")]
    public long MarkPriceEp { get; set; }
    
    [JsonProperty("leverage")]
    public decimal Leverage { get; set; }
    
    // Helper properties to convert scaled values
    public decimal AverageEntryPrice => ConvertFromScaledPrice(AverageEntryPriceEp, Symbol);
    public decimal MarkPrice => ConvertFromScaledPrice(MarkPriceEp, Symbol);
    public decimal UnrealizedPnl => ConvertFromScaledValue(UnrealizedPnlEv, Currency);
    public decimal RealizedPnl => ConvertFromScaledValue(RealizedPnlEv, Currency);
    
    private static decimal ConvertFromScaledPrice(long scaledPrice, string symbol)
    {
        // Phemex uses different scale factors for different symbols
        // For BTC perpetuals: scale factor is 10000 (4 decimal places)
        // For USD perpetuals: scale factor is 10000
        // This is simplified - actual implementation should look up symbol specs
        return scaledPrice / 10000m;
    }
    
    private static decimal ConvertFromScaledValue(long scaledValue, string currency)
    {
        // For USD values: scale factor is typically 10000
        return scaledValue / 10000m;
    }
}

public class PhemexAccountPosition
{
    [JsonProperty("account")]
    public PhemexAccountInfo Account { get; set; } = new();
    
    [JsonProperty("positions")]
    public List<PhemexPosition> Positions { get; set; } = new();
}

public class PhemexAccountInfo
{
    [JsonProperty("accountID")]
    public long AccountId { get; set; }
    
    [JsonProperty("currency")]
    public string Currency { get; set; } = string.Empty;
    
    [JsonProperty("accountBalanceEv")]
    public long AccountBalanceEv { get; set; }
    
    [JsonProperty("totalUsedBalanceEv")]
    public long TotalUsedBalanceEv { get; set; }
    
    [JsonProperty("availableBalanceEv")]
    public long AvailableBalanceEv { get; set; }
    
    public decimal AccountBalance => AccountBalanceEv / 10000m;
    public decimal TotalUsedBalance => TotalUsedBalanceEv / 10000m;
    public decimal AvailableBalance => AvailableBalanceEv / 10000m;
}