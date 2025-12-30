using Orleans;

namespace LionFire.Trading.Grains.Bots;

/// <summary>
/// Configuration for a realtime bot harness, specifying the bot type, account,
/// markets to subscribe to, and bot-specific parameters.
/// </summary>
/// <remarks>
/// This configuration supports:
/// - Multiple markets per bot (for pairs trading or portfolio strategies)
/// - Flexible parameter system using Dictionary for bot-specific settings
/// - Orleans serialization for grain method calls
/// </remarks>
[GenerateSerializer]
[Alias("realtime-bot-config")]
public class RealtimeBotConfiguration
{
    /// <summary>
    /// Fully qualified type name of the bot to instantiate (e.g., "MyNamespace.MyBot").
    /// Must be a valid type that can be resolved and instantiated.
    /// </summary>
    [Id(0)]
    public string BotTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Account identifier for the trading account this bot should use.
    /// Used for position tracking, order execution, and P&L calculations.
    /// </summary>
    [Id(1)]
    public string AccountId { get; set; } = string.Empty;

    /// <summary>
    /// List of markets to subscribe to. Each market specifies exchange, area, symbol, and timeframe.
    /// Must contain at least one market. Supports multiple markets for portfolio strategies.
    /// </summary>
    [Id(2)]
    public List<MarketSubscription> Markets { get; set; }

    /// <summary>
    /// Bot-specific parameters as key-value pairs.
    /// The bot implementation determines which parameters are required and how they're used.
    /// </summary>
    /// <remarks>
    /// Common parameter types:
    /// - Numeric values for indicators (e.g., "FastPeriod" = 12)
    /// - Boolean flags (e.g., "EnableTrailing" = true)
    /// - String settings (e.g., "OrderType" = "Market")
    ///
    /// Values are stored as objects to support any serializable type.
    /// </remarks>
    [Id(3)]
    public Dictionary<string, object> Parameters { get; set; }

    /// <summary>
    /// Initializes a new instance of RealtimeBotConfiguration with empty collections.
    /// </summary>
    public RealtimeBotConfiguration()
    {
        Markets = new List<MarketSubscription>();
        Parameters = new Dictionary<string, object>();
    }

    /// <summary>
    /// Validates the configuration to ensure all required fields are properly set.
    /// </summary>
    /// <returns>Validation result with success status and error messages if validation fails</returns>
    public ValidationResult Validate()
    {
        var errors = new List<string>();

        // Check BotTypeName
        if (string.IsNullOrWhiteSpace(BotTypeName))
        {
            errors.Add("BotTypeName is required and cannot be empty");
        }

        // Check AccountId
        if (string.IsNullOrWhiteSpace(AccountId))
        {
            errors.Add("AccountId is required and cannot be empty");
        }

        // Check Markets list
        if (Markets == null || Markets.Count == 0)
        {
            errors.Add("Markets list must contain at least one market subscription");
        }
        else
        {
            // Validate each market subscription
            for (int i = 0; i < Markets.Count; i++)
            {
                var market = Markets[i];
                if (string.IsNullOrWhiteSpace(market.Symbol))
                {
                    errors.Add($"Market[{i}].Symbol is required and cannot be empty");
                }
                if (string.IsNullOrWhiteSpace(market.TimeFrame))
                {
                    errors.Add($"Market[{i}].TimeFrame is required and cannot be empty");
                }
                if (string.IsNullOrWhiteSpace(market.Exchange))
                {
                    errors.Add($"Market[{i}].Exchange is required and cannot be empty");
                }
                if (string.IsNullOrWhiteSpace(market.ExchangeArea))
                {
                    errors.Add($"Market[{i}].ExchangeArea is required and cannot be empty");
                }
            }
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}

/// <summary>
/// Result of configuration validation.
/// </summary>
[GenerateSerializer]
[Alias("validation-result")]
public class ValidationResult
{
    /// <summary>
    /// True if validation passed, false if there are errors.
    /// </summary>
    [Id(0)]
    public bool IsValid { get; set; }

    /// <summary>
    /// List of validation error messages. Empty if IsValid is true.
    /// </summary>
    [Id(1)]
    public List<string> Errors { get; set; } = new List<string>();
}
