using FluentAssertions;
using LionFire.Trading.Automation.FillSimulation;

namespace LionFire.Trading.Automation.Tests.FillSimulation;

/// <summary>
/// Unit tests for <see cref="SimpleFillSimulator{TPrecision}"/>.
/// </summary>
public class SimpleFillSimulatorTests
{
    private readonly SimpleFillSimulator<decimal> _simulator = new();

    #region Market Order Tests

    [Fact]
    public void MarketOrder_Long_FillsAtAsk()
    {
        // Arrange
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().Be(100.05m); // Long market order fills at Ask
        result.FilledQuantity.Should().Be(1.0m);
        result.Slippage.Should().Be(0m); // No slippage in simple mode
    }

    [Fact]
    public void MarketOrder_Short_FillsAtBid()
    {
        // Arrange
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Short,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().Be(100.00m); // Short market order fills at Bid
        result.FilledQuantity.Should().Be(1.0m);
        result.Slippage.Should().Be(0m);
    }

    [Fact]
    public void MarketOrder_AlwaysCompleteFill()
    {
        // Arrange - large order
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 1000000m, // Very large quantity
            Bid = 100.00m,
            Ask = 100.05m
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.FilledQuantity.Should().Be(1000000m); // Complete fill even for large orders
        result.UnfilledQuantity.Should().Be(0m);
    }

    #endregion

    #region Limit Order Tests

    [Fact]
    public void LimitOrder_Long_FillsAtAsk_WhenLimitAboveAsk()
    {
        // Arrange - limit is above ask (marketable)
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Limit,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m,
            LimitPrice = 100.10m // Above ask
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().Be(100.05m); // Gets better price at Ask
        result.FilledQuantity.Should().Be(1.0m);
    }

    [Fact]
    public void LimitOrder_Long_FillsAtLimit_WhenLimitBelowAsk()
    {
        // Arrange - limit is below ask (resting order)
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Limit,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m,
            LimitPrice = 99.95m // Below ask
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert - SimpleFillSimulator fills at limit (assumes eventual fill)
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().Be(99.95m);
    }

    [Fact]
    public void LimitOrder_Short_FillsAtBid_WhenLimitBelowBid()
    {
        // Arrange - limit is below bid (marketable for short)
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Limit,
            Direction = LongAndShort.Short,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m,
            LimitPrice = 99.90m // Below bid
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().Be(100.00m); // Gets better price at Bid
    }

    [Fact]
    public void LimitOrder_Short_FillsAtLimit_WhenLimitAboveBid()
    {
        // Arrange - limit is above bid (resting order)
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Limit,
            Direction = LongAndShort.Short,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m,
            LimitPrice = 100.10m // Above bid
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert - SimpleFillSimulator fills at limit
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().Be(100.10m);
    }

    [Fact]
    public void LimitOrder_NoLimitPrice_Fails()
    {
        // Arrange
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Limit,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m,
            LimitPrice = null
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeFalse();
        result.Reason.Should().Contain("Limit price not specified");
    }

    #endregion

    #region Stop Order Tests

    [Fact]
    public void StopOrder_Long_TriggeredWhenAskReachesStopPrice()
    {
        // Arrange - buy stop triggers when ask >= stop
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Stop,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 101.00m, // At or above stop
            StopPrice = 101.00m
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().Be(101.00m); // Fills at Ask
    }

    [Fact]
    public void StopOrder_Long_NotTriggeredWhenAskBelowStop()
    {
        // Arrange
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Stop,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 99.00m,
            Ask = 99.50m, // Below stop
            StopPrice = 101.00m
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeFalse();
        result.Reason.Should().Contain("Stop not triggered");
    }

    [Fact]
    public void StopOrder_Short_TriggeredWhenBidReachesStopPrice()
    {
        // Arrange - sell stop triggers when bid <= stop
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Stop,
            Direction = LongAndShort.Short,
            Quantity = 1.0m,
            Bid = 99.00m, // At or below stop
            Ask = 99.50m,
            StopPrice = 99.00m
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().Be(99.00m); // Fills at Bid
    }

    [Fact]
    public void StopOrder_Short_NotTriggeredWhenBidAboveStop()
    {
        // Arrange
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Stop,
            Direction = LongAndShort.Short,
            Quantity = 1.0m,
            Bid = 101.00m, // Above stop
            Ask = 101.50m,
            StopPrice = 99.00m
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeFalse();
        result.Reason.Should().Contain("Stop not triggered");
    }

    [Fact]
    public void StopOrder_NoStopPrice_Fails()
    {
        // Arrange
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Stop,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m,
            StopPrice = null
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeFalse();
        result.Reason.Should().Contain("Stop price not specified");
    }

    #endregion

    #region Stop-Limit Order Tests

    [Fact]
    public void StopLimitOrder_Long_TriggeredAndMarketable()
    {
        // Arrange
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.StopLimit,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 101.00m, // Triggered (>= stop)
            StopPrice = 101.00m,
            LimitPrice = 101.50m // Above ask (marketable)
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().Be(101.00m); // Gets better price at Ask
    }

    [Fact]
    public void StopLimitOrder_Long_TriggeredButLimitNotMarketable()
    {
        // Arrange
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.StopLimit,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 102.00m, // Triggered (>= stop)
            StopPrice = 101.00m,
            LimitPrice = 101.50m // Below ask (not immediately marketable)
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert - SimpleFillSimulator fills at limit
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().Be(101.50m);
    }

    [Fact]
    public void StopLimitOrder_NotTriggered()
    {
        // Arrange
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.StopLimit,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 99.00m,
            Ask = 99.50m, // Not triggered (< stop)
            StopPrice = 101.00m,
            LimitPrice = 101.50m
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeFalse();
        result.Reason.Should().Contain("Stop not triggered");
    }

    #endregion

    #region Zero Slippage Tests

    [Theory]
    [InlineData(FillOrderType.Market)]
    [InlineData(FillOrderType.Limit)]
    [InlineData(FillOrderType.Stop)]
    [InlineData(FillOrderType.StopLimit)]
    public void AllOrderTypes_HaveZeroSlippage(FillOrderType orderType)
    {
        // Arrange
        var request = new FillRequest<decimal>
        {
            OrderType = orderType,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 101.00m,
            LimitPrice = 101.50m,
            StopPrice = 101.00m
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert
        if (result.IsFilled)
        {
            result.Slippage.Should().Be(0m, "SimpleFillSimulator should never apply slippage");
        }
    }

    #endregion

    #region Double Precision Tests

    [Fact]
    public void DoublePrecision_MarketOrder_Works()
    {
        // Arrange
        var simulator = new SimpleFillSimulator<double>();
        var request = new FillRequest<double>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 1.0,
            Bid = 100.00,
            Ask = 100.05
        };

        // Act
        var result = simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().Be(100.05);
    }

    #endregion

    #region Unknown Order Type Tests


    [Fact]
    public void UnknownOrderType_ReturnsNoFill()
    {
        // Arrange
        var request = new FillRequest<decimal>
        {
            OrderType = (FillOrderType)999, // Invalid order type
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m
        };

        // Act
        var result = _simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeFalse();
        result.Reason.Should().Contain("Unknown order type");
    }

    #endregion
}
