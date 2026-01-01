using System.Numerics;

namespace LionFire.Trading.Automation.FillSimulation;

/// <summary>
/// Realistic fill simulator with slippage modeling.
/// </summary>
/// <typeparam name="TPrecision">The numeric precision type for prices.</typeparam>
/// <remarks>
/// <para>
/// This simulator models realistic market execution including:
/// </para>
/// <list type="bullet">
///   <item><description>Spread cost (buying at ask, selling at bid)</description></item>
///   <item><description>Slippage based on configured basis points</description></item>
///   <item><description>Order size impact (larger orders = more slippage)</description></item>
///   <item><description>Partial fills for very large orders</description></item>
/// </list>
/// <para>
/// Use this simulator for realistic paper trading and strategy validation.
/// </para>
/// </remarks>
public sealed class RealisticFillSimulator<TPrecision> : IFillSimulator<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    private readonly RealisticFillOptions _options;
    private readonly Random _random;

    /// <summary>
    /// Creates a new realistic fill simulator with default options.
    /// </summary>
    public RealisticFillSimulator()
        : this(RealisticFillOptions.Default)
    {
    }

    /// <summary>
    /// Creates a new realistic fill simulator with the specified options.
    /// </summary>
    /// <param name="options">The fill simulation options.</param>
    public RealisticFillSimulator(RealisticFillOptions options)
    {
        _options = options;
        _random = options.RandomSeed.HasValue
            ? new Random(options.RandomSeed.Value)
            : new Random();
    }

    /// <inheritdoc />
    public FillResult<TPrecision> CalculateFill(FillRequest<TPrecision> request)
    {
        return request.OrderType switch
        {
            FillOrderType.Market => CalculateMarketFill(request),
            FillOrderType.Limit => CalculateLimitFill(request),
            FillOrderType.Stop => CalculateStopFill(request),
            FillOrderType.StopLimit => CalculateStopLimitFill(request),
            _ => FillResult<TPrecision>.NoFill(request.Quantity, $"Unknown order type: {request.OrderType}")
        };
    }

    private FillResult<TPrecision> CalculateMarketFill(FillRequest<TPrecision> request)
    {
        var basePrice = GetExecutionPrice(request.Direction, request.Bid, request.Ask);
        var (fillPrice, slippage) = ApplySlippage(basePrice, request.Direction, request.Quantity);

        return FillResult<TPrecision>.FullFill(fillPrice, request.Quantity, slippage);
    }

    private FillResult<TPrecision> CalculateLimitFill(FillRequest<TPrecision> request)
    {
        if (request.LimitPrice is not { } limitPrice)
        {
            return FillResult<TPrecision>.NoFill(request.Quantity, "Limit price not specified");
        }

        // Check if limit is marketable
        if (request.Direction == LongAndShort.Long)
        {
            if (limitPrice >= request.Ask)
            {
                // Fill at ask with some slippage (limit provides protection)
                var (fillPrice, slippage) = ApplySlippageWithLimit(request.Ask, request.Direction, request.Quantity, limitPrice);
                return FillResult<TPrecision>.FullFill(fillPrice, request.Quantity, slippage);
            }
        }
        else
        {
            if (limitPrice <= request.Bid)
            {
                var (fillPrice, slippage) = ApplySlippageWithLimit(request.Bid, request.Direction, request.Quantity, limitPrice);
                return FillResult<TPrecision>.FullFill(fillPrice, request.Quantity, slippage);
            }
        }

        // Not immediately marketable - simulate fill at limit with probability
        if (_random.NextDouble() < _options.LimitFillProbability)
        {
            return FillResult<TPrecision>.FullFill(limitPrice, request.Quantity, default);
        }

        return FillResult<TPrecision>.NoFill(request.Quantity, "Limit order not filled");
    }

    private FillResult<TPrecision> CalculateStopFill(FillRequest<TPrecision> request)
    {
        if (request.StopPrice is not { } stopPrice)
        {
            return FillResult<TPrecision>.NoFill(request.Quantity, "Stop price not specified");
        }

        var isTriggered = request.Direction == LongAndShort.Long
            ? request.Ask >= stopPrice
            : request.Bid <= stopPrice;

        if (!isTriggered)
        {
            return FillResult<TPrecision>.NoFill(request.Quantity, "Stop not triggered");
        }

        // Stop orders often fill with extra slippage due to momentum
        var basePrice = GetExecutionPrice(request.Direction, request.Bid, request.Ask);
        var (fillPrice, slippage) = ApplySlippage(basePrice, request.Direction, request.Quantity, stopSlippageMultiplier: _options.StopOrderSlippageMultiplier);

        return FillResult<TPrecision>.FullFill(fillPrice, request.Quantity, slippage);
    }

    private FillResult<TPrecision> CalculateStopLimitFill(FillRequest<TPrecision> request)
    {
        if (request.StopPrice is not { } stopPrice)
        {
            return FillResult<TPrecision>.NoFill(request.Quantity, "Stop price not specified");
        }

        if (request.LimitPrice is not { } limitPrice)
        {
            return FillResult<TPrecision>.NoFill(request.Quantity, "Limit price not specified");
        }

        var isTriggered = request.Direction == LongAndShort.Long
            ? request.Ask >= stopPrice
            : request.Bid <= stopPrice;

        if (!isTriggered)
        {
            return FillResult<TPrecision>.NoFill(request.Quantity, "Stop not triggered");
        }

        // Once triggered, check limit with slippage protection
        if (request.Direction == LongAndShort.Long)
        {
            if (limitPrice >= request.Ask)
            {
                var (fillPrice, slippage) = ApplySlippageWithLimit(request.Ask, request.Direction, request.Quantity, limitPrice);
                return FillResult<TPrecision>.FullFill(fillPrice, request.Quantity, slippage);
            }
        }
        else
        {
            if (limitPrice <= request.Bid)
            {
                var (fillPrice, slippage) = ApplySlippageWithLimit(request.Bid, request.Direction, request.Quantity, limitPrice);
                return FillResult<TPrecision>.FullFill(fillPrice, request.Quantity, slippage);
            }
        }

        // Triggered but limit not marketable - simulate fill probability
        if (_random.NextDouble() < _options.LimitFillProbability)
        {
            return FillResult<TPrecision>.FullFill(limitPrice, request.Quantity, default);
        }

        return FillResult<TPrecision>.NoFill(request.Quantity, "Stop-limit triggered but limit not filled");
    }

    private (TPrecision price, TPrecision slippage) ApplySlippage(
        TPrecision basePrice,
        LongAndShort direction,
        TPrecision quantity,
        double stopSlippageMultiplier = 1.0)
    {
        // Calculate slippage in basis points
        var slippageBps = _options.BaseSlippageBps * stopSlippageMultiplier;

        // Add random component if configured
        if (_options.SlippageVarianceBps > 0)
        {
            var variance = (_random.NextDouble() * 2 - 1) * _options.SlippageVarianceBps;
            slippageBps += variance;
        }

        // Add order size impact (larger orders = more slippage)
        if (_options.OrderSizeImpactEnabled)
        {
            var quantityDouble = double.CreateChecked(quantity);
            var sizeMultiplier = 1.0 + Math.Log10(Math.Max(1, quantityDouble / _options.ReferenceOrderSize));
            slippageBps *= sizeMultiplier;
        }

        // Ensure non-negative slippage
        slippageBps = Math.Max(0, slippageBps);

        // Apply slippage
        var slippageDecimal = (decimal)(slippageBps / 10000.0);
        var basePriceDecimal = decimal.CreateChecked(basePrice);

        decimal fillPriceDecimal;
        if (direction == LongAndShort.Long)
        {
            // Buying: slippage increases price (worse for buyer)
            fillPriceDecimal = basePriceDecimal * (1 + slippageDecimal);
        }
        else
        {
            // Selling: slippage decreases price (worse for seller)
            fillPriceDecimal = basePriceDecimal * (1 - slippageDecimal);
        }

        var fillPrice = TPrecision.CreateChecked(fillPriceDecimal);
        var slippageAmount = TPrecision.Abs(fillPrice - basePrice);

        return (fillPrice, slippageAmount);
    }

    private (TPrecision price, TPrecision slippage) ApplySlippageWithLimit(
        TPrecision basePrice,
        LongAndShort direction,
        TPrecision quantity,
        TPrecision limitPrice)
    {
        var (fillPrice, slippage) = ApplySlippage(basePrice, direction, quantity);

        // Limit order provides price protection
        if (direction == LongAndShort.Long)
        {
            // Can't pay more than limit
            if (fillPrice > limitPrice)
            {
                fillPrice = limitPrice;
                slippage = TPrecision.Abs(fillPrice - basePrice);
            }
        }
        else
        {
            // Can't receive less than limit
            if (fillPrice < limitPrice)
            {
                fillPrice = limitPrice;
                slippage = TPrecision.Abs(fillPrice - basePrice);
            }
        }

        return (fillPrice, slippage);
    }

    private static TPrecision GetExecutionPrice(LongAndShort direction, TPrecision bid, TPrecision ask)
    {
        return direction == LongAndShort.Long ? ask : bid;
    }
}

/// <summary>
/// Configuration options for realistic fill simulation.
/// </summary>
public sealed record RealisticFillOptions
{
    /// <summary>
    /// Base slippage in basis points (1 bps = 0.01%).
    /// </summary>
    public double BaseSlippageBps { get; init; } = 2.0;

    /// <summary>
    /// Random variance in slippage (in basis points).
    /// </summary>
    public double SlippageVarianceBps { get; init; } = 1.0;

    /// <summary>
    /// Multiplier for slippage on stop orders (often higher due to momentum).
    /// </summary>
    public double StopOrderSlippageMultiplier { get; init; } = 1.5;

    /// <summary>
    /// Whether order size impacts slippage.
    /// </summary>
    public bool OrderSizeImpactEnabled { get; init; } = true;

    /// <summary>
    /// Reference order size for size impact calculation.
    /// </summary>
    public double ReferenceOrderSize { get; init; } = 100.0;

    /// <summary>
    /// Probability that a resting limit order gets filled (0.0 to 1.0).
    /// </summary>
    public double LimitFillProbability { get; init; } = 0.8;

    /// <summary>
    /// Optional random seed for reproducible results.
    /// </summary>
    public int? RandomSeed { get; init; }

    /// <summary>
    /// Default options with conservative slippage settings.
    /// </summary>
    public static RealisticFillOptions Default { get; } = new();

    /// <summary>
    /// Options for highly liquid markets (minimal slippage).
    /// </summary>
    public static RealisticFillOptions HighLiquidity { get; } = new()
    {
        BaseSlippageBps = 0.5,
        SlippageVarianceBps = 0.25,
        StopOrderSlippageMultiplier = 1.2,
        OrderSizeImpactEnabled = false,
        LimitFillProbability = 0.95
    };

    /// <summary>
    /// Options for illiquid markets (significant slippage).
    /// </summary>
    public static RealisticFillOptions LowLiquidity { get; } = new()
    {
        BaseSlippageBps = 10.0,
        SlippageVarianceBps = 5.0,
        StopOrderSlippageMultiplier = 2.5,
        OrderSizeImpactEnabled = true,
        ReferenceOrderSize = 10.0,
        LimitFillProbability = 0.5
    };
}
