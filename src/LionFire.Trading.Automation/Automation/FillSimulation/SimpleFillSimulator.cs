using System.Numerics;

namespace LionFire.Trading.Automation.FillSimulation;

/// <summary>
/// Simple fill simulator that executes at current market prices without slippage.
/// </summary>
/// <typeparam name="TPrecision">The numeric precision type for prices.</typeparam>
/// <remarks>
/// <para>
/// This simulator provides deterministic, instant fills for all orders:
/// </para>
/// <list type="bullet">
///   <item><description>Buy orders fill at the ask price</description></item>
///   <item><description>Sell orders fill at the bid price</description></item>
///   <item><description>Limit orders fill at their limit price if marketable</description></item>
///   <item><description>Stop orders fill at the bid/ask once triggered</description></item>
/// </list>
/// <para>
/// No slippage, partial fills, or order book depth is considered.
/// Ideal for basic backtesting and development scenarios.
/// </para>
/// </remarks>
public sealed class SimpleFillSimulator<TPrecision> : IFillSimulator<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
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

    private static FillResult<TPrecision> CalculateMarketFill(FillRequest<TPrecision> request)
    {
        // Market orders fill at current bid/ask
        var price = GetExecutionPrice(request.Direction, request.Bid, request.Ask);
        return FillResult<TPrecision>.FullFill(price, request.Quantity);
    }

    private static FillResult<TPrecision> CalculateLimitFill(FillRequest<TPrecision> request)
    {
        if (request.LimitPrice is not { } limitPrice)
        {
            return FillResult<TPrecision>.NoFill(request.Quantity, "Limit price not specified");
        }

        // Check if limit is marketable
        if (request.Direction == LongAndShort.Long)
        {
            // Buying: limit must be >= ask to fill immediately
            if (limitPrice >= request.Ask)
            {
                // Fill at ask (not limit) - we get a better price
                return FillResult<TPrecision>.FullFill(request.Ask, request.Quantity);
            }
        }
        else
        {
            // Selling: limit must be <= bid to fill immediately
            if (limitPrice <= request.Bid)
            {
                // Fill at bid (not limit) - we get a better price
                return FillResult<TPrecision>.FullFill(request.Bid, request.Quantity);
            }
        }

        // Not marketable - in real trading this would rest on the book
        // For simulation, we can either reject or fill at limit (configurable behavior)
        // Simple simulator just fills at limit price (assuming it eventually gets filled)
        return FillResult<TPrecision>.FullFill(limitPrice, request.Quantity);
    }

    private static FillResult<TPrecision> CalculateStopFill(FillRequest<TPrecision> request)
    {
        if (request.StopPrice is not { } stopPrice)
        {
            return FillResult<TPrecision>.NoFill(request.Quantity, "Stop price not specified");
        }

        // Check if stop is triggered
        var isTriggered = request.Direction == LongAndShort.Long
            ? request.Ask >= stopPrice  // Buy stop triggers when ask rises to stop
            : request.Bid <= stopPrice; // Sell stop triggers when bid falls to stop

        if (!isTriggered)
        {
            return FillResult<TPrecision>.NoFill(request.Quantity, "Stop not triggered");
        }

        // Once triggered, fill at market
        var price = GetExecutionPrice(request.Direction, request.Bid, request.Ask);
        return FillResult<TPrecision>.FullFill(price, request.Quantity);
    }

    private static FillResult<TPrecision> CalculateStopLimitFill(FillRequest<TPrecision> request)
    {
        if (request.StopPrice is not { } stopPrice)
        {
            return FillResult<TPrecision>.NoFill(request.Quantity, "Stop price not specified");
        }

        if (request.LimitPrice is not { } limitPrice)
        {
            return FillResult<TPrecision>.NoFill(request.Quantity, "Limit price not specified");
        }

        // Check if stop is triggered
        var isTriggered = request.Direction == LongAndShort.Long
            ? request.Ask >= stopPrice
            : request.Bid <= stopPrice;

        if (!isTriggered)
        {
            return FillResult<TPrecision>.NoFill(request.Quantity, "Stop not triggered");
        }

        // Once triggered, check if limit is marketable
        if (request.Direction == LongAndShort.Long)
        {
            if (limitPrice >= request.Ask)
            {
                return FillResult<TPrecision>.FullFill(request.Ask, request.Quantity);
            }
        }
        else
        {
            if (limitPrice <= request.Bid)
            {
                return FillResult<TPrecision>.FullFill(request.Bid, request.Quantity);
            }
        }

        // Triggered but limit not marketable - fill at limit in simple mode
        return FillResult<TPrecision>.FullFill(limitPrice, request.Quantity);
    }

    private static TPrecision GetExecutionPrice(LongAndShort direction, TPrecision bid, TPrecision ask)
    {
        // Buy at ask, sell at bid
        return direction == LongAndShort.Long ? ask : bid;
    }
}
