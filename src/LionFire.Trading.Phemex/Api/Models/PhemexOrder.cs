using Newtonsoft.Json;

namespace LionFire.Trading.Phemex.Api.Models;

public class PhemexOrder
{
    [JsonProperty("orderID")]
    public string OrderId { get; set; } = string.Empty;
    
    [JsonProperty("clOrdID")]
    public string ClientOrderId { get; set; } = string.Empty;
    
    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;
    
    [JsonProperty("side")]
    public string Side { get; set; } = string.Empty; // Buy or Sell
    
    [JsonProperty("orderType")]
    public string OrderType { get; set; } = string.Empty; // Market, Limit, etc.
    
    [JsonProperty("orderQty")]
    public decimal Quantity { get; set; }
    
    [JsonProperty("price")]
    public decimal? Price { get; set; }
    
    [JsonProperty("stopPx")]
    public decimal? StopPrice { get; set; }
    
    [JsonProperty("ordStatus")]
    public string Status { get; set; } = string.Empty; // New, PartiallyFilled, Filled, Canceled, Rejected
    
    [JsonProperty("cumQty")]
    public decimal FilledQuantity { get; set; }
    
    [JsonProperty("avgPx")]
    public decimal? AveragePrice { get; set; }
    
    [JsonProperty("transactTimeNs")]
    public long TransactionTimeNanos { get; set; }
    
    [JsonProperty("text")]
    public string? Text { get; set; }
    
    public DateTime TransactionTime => DateTimeOffset.FromUnixTimeMilliseconds(TransactionTimeNanos / 1_000_000).DateTime;
}

public class PhemexOrderRequest
{
    [JsonProperty("symbol")]
    public string Symbol { get; set; } = string.Empty;
    
    [JsonProperty("clOrdID")]
    public string? ClientOrderId { get; set; }
    
    [JsonProperty("side")]
    public string Side { get; set; } = string.Empty; // Buy or Sell
    
    [JsonProperty("orderQty")]
    public long Quantity { get; set; } // In contracts
    
    [JsonProperty("priceEp")]
    public long? PriceEp { get; set; } // Price in scaled format
    
    [JsonProperty("orderType")]
    public string OrderType { get; set; } = "Market";
    
    [JsonProperty("stopPxEp")]
    public long? StopPriceEp { get; set; }
    
    [JsonProperty("takeProfitEp")]
    public long? TakeProfitEp { get; set; }
    
    [JsonProperty("stopLossEp")]
    public long? StopLossEp { get; set; }
    
    [JsonProperty("timeInForce")]
    public string TimeInForce { get; set; } = "GoodTillCancel";
    
    [JsonProperty("reduceOnly")]
    public bool ReduceOnly { get; set; }
    
    [JsonProperty("closeOnTrigger")]
    public bool CloseOnTrigger { get; set; }
}

public class PhemexOrderResponse
{
    [JsonProperty("code")]
    public int Code { get; set; }
    
    [JsonProperty("msg")]
    public string Message { get; set; } = string.Empty;
    
    [JsonProperty("data")]
    public PhemexOrder? Data { get; set; }
}