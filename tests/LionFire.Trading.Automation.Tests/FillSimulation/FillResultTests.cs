using FluentAssertions;
using LionFire.Trading.Automation.FillSimulation;

namespace LionFire.Trading.Automation.Tests.FillSimulation;

/// <summary>
/// Unit tests for <see cref="FillResult{TPrecision}"/> and <see cref="FillRequest{TPrecision}"/>.
/// </summary>
public class FillResultTests
{
    #region FillResult Factory Methods

    [Fact]
    public void FullFill_SetsCorrectProperties()
    {
        // Act
        var result = FillResult<decimal>.FullFill(100.50m, 10m, 0.05m);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().Be(100.50m);
        result.FilledQuantity.Should().Be(10m);
        result.UnfilledQuantity.Should().Be(0m);
        result.Slippage.Should().Be(0.05m);
        result.Reason.Should().BeNull();
    }

    [Fact]
    public void FullFill_WithDefaultSlippage()
    {
        // Act
        var result = FillResult<decimal>.FullFill(100.50m, 10m);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.Slippage.Should().Be(0m);
    }

    [Fact]
    public void NoFill_SetsCorrectProperties()
    {
        // Act
        var result = FillResult<decimal>.NoFill(10m, "Order rejected");

        // Assert
        result.IsFilled.Should().BeFalse();
        result.ExecutionPrice.Should().Be(0m);
        result.FilledQuantity.Should().Be(0m);
        result.UnfilledQuantity.Should().Be(10m);
        result.Reason.Should().Be("Order rejected");
    }

    #endregion

    #region FillRequest Tests

    [Fact]
    public void FillRequest_RequiredProperties()
    {
        // Act
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m
        };

        // Assert
        request.OrderType.Should().Be(FillOrderType.Market);
        request.Direction.Should().Be(LongAndShort.Long);
        request.Quantity.Should().Be(1.0m);
        request.Bid.Should().Be(100.00m);
        request.Ask.Should().Be(100.05m);
        request.LimitPrice.Should().BeNull();
        request.StopPrice.Should().BeNull();
        request.Symbol.Should().BeNull();
    }

    [Fact]
    public void FillRequest_OptionalProperties()
    {
        // Act
        var symbol = new ExchangeSymbol("Binance", "Spot", "BTCUSDT");
        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.StopLimit,
            Direction = LongAndShort.Short,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m,
            LimitPrice = 99.90m,
            StopPrice = 99.95m,
            Symbol = symbol
        };

        // Assert
        request.LimitPrice.Should().Be(99.90m);
        request.StopPrice.Should().Be(99.95m);
        request.Symbol.Should().Be(symbol);
    }

    #endregion

    #region FillOrderType Tests

    [Fact]
    public void FillOrderType_HasExpectedValues()
    {
        // Assert
        ((int)FillOrderType.Market).Should().Be(0);
        ((int)FillOrderType.Limit).Should().Be(1);
        ((int)FillOrderType.Stop).Should().Be(2);
        ((int)FillOrderType.StopLimit).Should().Be(3);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void FillResult_ZeroQuantity()
    {
        // Act
        var result = FillResult<decimal>.FullFill(100.00m, 0m);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.FilledQuantity.Should().Be(0m);
    }

    [Fact]
    public void FillResult_NegativeSlippage_Allowed()
    {
        // Negative slippage means price improvement
        // Act
        var result = FillResult<decimal>.FullFill(100.00m, 10m, -0.05m);

        // Assert - negative slippage is allowed (price improvement)
        result.Slippage.Should().Be(-0.05m);
    }

    [Fact]
    public void FillResult_LargeQuantity()
    {
        // Act
        var result = FillResult<decimal>.FullFill(100.00m, decimal.MaxValue / 2);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.FilledQuantity.Should().Be(decimal.MaxValue / 2);
    }

    #endregion

    #region RealisticFillOptions Tests

    [Fact]
    public void RealisticFillOptions_DefaultValues()
    {
        // Act
        var options = RealisticFillOptions.Default;

        // Assert
        options.BaseSlippageBps.Should().Be(2.0);
        options.SlippageVarianceBps.Should().Be(1.0);
        options.StopOrderSlippageMultiplier.Should().Be(1.5);
        options.OrderSizeImpactEnabled.Should().BeTrue();
        options.ReferenceOrderSize.Should().Be(100.0);
        options.LimitFillProbability.Should().Be(0.8);
        options.RandomSeed.Should().BeNull();
    }

    [Fact]
    public void RealisticFillOptions_HighLiquidity()
    {
        // Act
        var options = RealisticFillOptions.HighLiquidity;

        // Assert
        options.BaseSlippageBps.Should().Be(0.5);
        options.SlippageVarianceBps.Should().Be(0.25);
        options.StopOrderSlippageMultiplier.Should().Be(1.2);
        options.OrderSizeImpactEnabled.Should().BeFalse();
        options.LimitFillProbability.Should().Be(0.95);
    }

    [Fact]
    public void RealisticFillOptions_LowLiquidity()
    {
        // Act
        var options = RealisticFillOptions.LowLiquidity;

        // Assert
        options.BaseSlippageBps.Should().Be(10.0);
        options.SlippageVarianceBps.Should().Be(5.0);
        options.StopOrderSlippageMultiplier.Should().Be(2.5);
        options.OrderSizeImpactEnabled.Should().BeTrue();
        options.ReferenceOrderSize.Should().Be(10.0);
        options.LimitFillProbability.Should().Be(0.5);
    }

    [Fact]
    public void RealisticFillOptions_CanBeModifiedWithWith()
    {
        // Act
        var options = RealisticFillOptions.Default with
        {
            BaseSlippageBps = 15.0,
            RandomSeed = 123
        };

        // Assert
        options.BaseSlippageBps.Should().Be(15.0);
        options.RandomSeed.Should().Be(123);
        options.SlippageVarianceBps.Should().Be(1.0); // Unchanged
    }

    #endregion

    #region Generic Precision Tests

    [Fact]
    public void FillResult_Double_Works()
    {
        // Act
        var result = FillResult<double>.FullFill(100.50, 10.0, 0.05);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().Be(100.50);
        result.FilledQuantity.Should().Be(10.0);
    }

    [Fact]
    public void FillRequest_Double_Works()
    {
        // Act
        var request = new FillRequest<double>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 1.0,
            Bid = 100.00,
            Ask = 100.05
        };

        // Assert
        request.Quantity.Should().Be(1.0);
        request.Ask.Should().Be(100.05);
    }

    #endregion
}
