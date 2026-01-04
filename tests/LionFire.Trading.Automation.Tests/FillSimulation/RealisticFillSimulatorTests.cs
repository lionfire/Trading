using FluentAssertions;
using LionFire.Trading.Automation.FillSimulation;

namespace LionFire.Trading.Automation.Tests.FillSimulation;

/// <summary>
/// Unit tests for <see cref="RealisticFillSimulator{TPrecision}"/>.
/// </summary>
public class RealisticFillSimulatorTests
{
    #region Market Order Slippage Tests

    [Fact]
    public void MarketOrder_Long_HasPositiveSlippage()
    {
        // Arrange - use fixed seed for reproducibility
        var options = new RealisticFillOptions
        {
            BaseSlippageBps = 5.0,
            SlippageVarianceBps = 0, // No randomness
            OrderSizeImpactEnabled = false,
            RandomSeed = 42
        };
        var simulator = new RealisticFillSimulator<decimal>(options);

        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m
        };

        // Act
        var result = simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().BeGreaterThan(100.05m); // Price increases (worse for buyer)
        result.Slippage.Should().BeGreaterThan(0m);
    }

    [Fact]
    public void MarketOrder_Short_HasPositiveSlippage()
    {
        // Arrange
        var options = new RealisticFillOptions
        {
            BaseSlippageBps = 5.0,
            SlippageVarianceBps = 0,
            OrderSizeImpactEnabled = false,
            RandomSeed = 42
        };
        var simulator = new RealisticFillSimulator<decimal>(options);

        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Short,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m
        };

        // Act
        var result = simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().BeLessThan(100.00m); // Price decreases (worse for seller)
        result.Slippage.Should().BeGreaterThan(0m);
    }

    [Theory]
    [InlineData(5.0, 100.0, 100.05)] // 5 bps = 0.05%
    [InlineData(10.0, 100.0, 100.10)] // 10 bps = 0.10%
    [InlineData(20.0, 100.0, 100.20)] // 20 bps = 0.20%
    public void MarketOrder_SlippageCalculation_Correct(double bps, decimal baseAsk, decimal expectedMin)
    {
        // Arrange
        var options = new RealisticFillOptions
        {
            BaseSlippageBps = bps,
            SlippageVarianceBps = 0, // No randomness
            OrderSizeImpactEnabled = false,
            RandomSeed = 42
        };
        var simulator = new RealisticFillSimulator<decimal>(options);

        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = baseAsk - 0.05m,
            Ask = baseAsk
        };

        // Act
        var result = simulator.CalculateFill(request);

        // Assert
        // Price should be approximately base + (base * bps / 10000)
        var expectedSlippage = baseAsk * (decimal)(bps / 10000.0);
        result.ExecutionPrice.Should().BeApproximately(baseAsk + expectedSlippage, 0.0001m);
    }

    #endregion

    #region Stop Order Slippage Tests

    [Fact]
    public void StopOrder_HasExtraSlippage()
    {
        // Arrange
        var options = new RealisticFillOptions
        {
            BaseSlippageBps = 5.0,
            SlippageVarianceBps = 0,
            StopOrderSlippageMultiplier = 2.0, // 2x slippage on stops
            OrderSizeImpactEnabled = false,
            RandomSeed = 42
        };
        var simulator = new RealisticFillSimulator<decimal>(options);

        var stopRequest = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Stop,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 101.00m, // Triggered
            StopPrice = 101.00m
        };

        var marketRequest = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 101.00m
        };

        // Act
        var stopResult = simulator.CalculateFill(stopRequest);
        var marketResult = simulator.CalculateFill(marketRequest);

        // Assert - stop order should have ~2x the slippage
        stopResult.Slippage.Should().BeGreaterThan(marketResult.Slippage);
    }

    #endregion

    #region Limit Order Protection Tests

    [Fact]
    public void LimitOrder_ProvidesSlippageProtection()
    {
        // Arrange
        var options = new RealisticFillOptions
        {
            BaseSlippageBps = 50.0, // Large slippage
            SlippageVarianceBps = 0,
            OrderSizeImpactEnabled = false,
            RandomSeed = 42
        };
        var simulator = new RealisticFillSimulator<decimal>(options);

        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Limit,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m,
            LimitPrice = 100.10m // Limit protects against slippage above 100.10
        };

        // Act
        var result = simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().BeLessThanOrEqualTo(100.10m); // Never pays more than limit
    }

    [Fact]
    public void LimitOrder_Short_ProvidesSlippageProtection()
    {
        // Arrange
        var options = new RealisticFillOptions
        {
            BaseSlippageBps = 50.0, // Large slippage
            SlippageVarianceBps = 0,
            OrderSizeImpactEnabled = false,
            RandomSeed = 42
        };
        var simulator = new RealisticFillSimulator<decimal>(options);

        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Limit,
            Direction = LongAndShort.Short,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m,
            LimitPrice = 99.95m // Limit protects against slippage below 99.95
        };

        // Act
        var result = simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().BeGreaterThanOrEqualTo(99.95m); // Never sells below limit
    }

    #endregion

    #region Order Size Impact Tests

    [Fact]
    public void LargeOrder_HasMoreSlippage()
    {
        // Arrange
        var options = new RealisticFillOptions
        {
            BaseSlippageBps = 5.0,
            SlippageVarianceBps = 0,
            OrderSizeImpactEnabled = true,
            ReferenceOrderSize = 100.0, // Normal order size
            RandomSeed = 42
        };
        var simulator = new RealisticFillSimulator<decimal>(options);

        var smallRequest = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 10m, // Small
            Bid = 100.00m,
            Ask = 100.05m
        };

        var largeRequest = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 10000m, // Large
            Bid = 100.00m,
            Ask = 100.05m
        };

        // Act
        var smallResult = simulator.CalculateFill(smallRequest);
        var largeResult = simulator.CalculateFill(largeRequest);

        // Assert - large order should have more slippage
        largeResult.Slippage.Should().BeGreaterThan(smallResult.Slippage);
    }

    [Fact]
    public void OrderSizeImpact_CanBeDisabled()
    {
        // Arrange
        var optionsWithImpact = new RealisticFillOptions
        {
            BaseSlippageBps = 5.0,
            SlippageVarianceBps = 0,
            OrderSizeImpactEnabled = true,
            ReferenceOrderSize = 100.0,
            RandomSeed = 42
        };

        var optionsWithoutImpact = new RealisticFillOptions
        {
            BaseSlippageBps = 5.0,
            SlippageVarianceBps = 0,
            OrderSizeImpactEnabled = false,
            RandomSeed = 42
        };

        var simulatorWithImpact = new RealisticFillSimulator<decimal>(optionsWithImpact);
        var simulatorWithoutImpact = new RealisticFillSimulator<decimal>(optionsWithoutImpact);

        var largeRequest = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 10000m,
            Bid = 100.00m,
            Ask = 100.05m
        };

        // Act
        var resultWithImpact = simulatorWithImpact.CalculateFill(largeRequest);
        var resultWithoutImpact = simulatorWithoutImpact.CalculateFill(largeRequest);

        // Assert
        resultWithImpact.Slippage.Should().BeGreaterThan(resultWithoutImpact.Slippage);
    }

    #endregion

    #region Limit Fill Probability Tests

    [Fact]
    public void LimitOrder_NotMarketable_MayNotFill()
    {
        // Arrange
        var options = new RealisticFillOptions
        {
            LimitFillProbability = 0.0, // Never fill resting orders
            RandomSeed = 42
        };
        var simulator = new RealisticFillSimulator<decimal>(options);

        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Limit,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m,
            LimitPrice = 99.90m // Below ask - resting order
        };

        // Act
        var result = simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeFalse();
    }

    [Fact]
    public void LimitOrder_NotMarketable_MayFill_WithHighProbability()
    {
        // Arrange
        var options = new RealisticFillOptions
        {
            LimitFillProbability = 1.0, // Always fill resting orders
            RandomSeed = 42
        };
        var simulator = new RealisticFillSimulator<decimal>(options);

        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Limit,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m,
            LimitPrice = 99.90m // Below ask - resting order
        };

        // Act
        var result = simulator.CalculateFill(request);

        // Assert
        result.IsFilled.Should().BeTrue();
        result.ExecutionPrice.Should().Be(99.90m); // Fills at limit price
        result.Slippage.Should().Be(0m); // No slippage on resting fills
    }

    #endregion

    #region Variance Tests

    [Fact]
    public void SlippageVariance_ProducesVariation()
    {
        // Arrange
        var options = new RealisticFillOptions
        {
            BaseSlippageBps = 5.0,
            SlippageVarianceBps = 5.0, // Up to 5 bps variance
            OrderSizeImpactEnabled = false
            // No random seed = random behavior
        };
        var simulator = new RealisticFillSimulator<decimal>(options);

        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m
        };

        // Act - run multiple times
        var prices = new HashSet<decimal>();
        for (int i = 0; i < 100; i++)
        {
            var result = simulator.CalculateFill(request);
            prices.Add(result.ExecutionPrice);
        }

        // Assert - should have multiple different prices
        prices.Count.Should().BeGreaterThan(1, "Variance should produce different prices");
    }

    [Fact]
    public void DeterministicWithSeed()
    {
        // Arrange
        var options = new RealisticFillOptions
        {
            BaseSlippageBps = 5.0,
            SlippageVarianceBps = 5.0,
            RandomSeed = 42
        };

        var simulator1 = new RealisticFillSimulator<decimal>(options);
        var simulator2 = new RealisticFillSimulator<decimal>(options);

        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m
        };

        // Act
        var result1 = simulator1.CalculateFill(request);
        var result2 = simulator2.CalculateFill(request);

        // Assert - same seed should produce same results
        result1.ExecutionPrice.Should().Be(result2.ExecutionPrice);
    }

    #endregion

    #region Preset Configuration Tests

    [Fact]
    public void HighLiquidityPreset_HasLowSlippage()
    {
        // Arrange
        var simulator = new RealisticFillSimulator<decimal>(
            RealisticFillOptions.HighLiquidity with { RandomSeed = 42, SlippageVarianceBps = 0 });

        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m
        };

        // Act
        var result = simulator.CalculateFill(request);

        // Assert - slippage should be small (0.5 bps = 0.005%)
        var expectedSlippage = 100.05m * 0.00005m;
        result.Slippage.Should().BeLessThan(0.01m); // Less than 1 cent
    }

    [Fact]
    public void LowLiquidityPreset_HasHighSlippage()
    {
        // Arrange
        var simulator = new RealisticFillSimulator<decimal>(
            RealisticFillOptions.LowLiquidity with { RandomSeed = 42, SlippageVarianceBps = 0 });

        var request = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m
        };

        // Act
        var result = simulator.CalculateFill(request);

        // Assert - slippage should be significant (10 bps = 0.1%)
        result.Slippage.Should().BeGreaterThan(0.05m);
    }

    #endregion

    #region Double Precision Tests

    [Fact]
    public void DoublePrecision_Works()
    {
        // Arrange
        var options = new RealisticFillOptions
        {
            BaseSlippageBps = 5.0,
            SlippageVarianceBps = 0,
            RandomSeed = 42
        };

        // Note: RealisticFillOptions works with double internally,
        // but the simulator is generic
        var decimalSimulator = new RealisticFillSimulator<decimal>(options);
        var doubleSimulator = new RealisticFillSimulator<double>(options);

        var decimalRequest = new FillRequest<decimal>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 1.0m,
            Bid = 100.00m,
            Ask = 100.05m
        };

        var doubleRequest = new FillRequest<double>
        {
            OrderType = FillOrderType.Market,
            Direction = LongAndShort.Long,
            Quantity = 1.0,
            Bid = 100.00,
            Ask = 100.05
        };

        // Act
        var decimalResult = decimalSimulator.CalculateFill(decimalRequest);
        var doubleResult = doubleSimulator.CalculateFill(doubleRequest);

        // Assert - both should fill with similar results
        decimalResult.IsFilled.Should().BeTrue();
        doubleResult.IsFilled.Should().BeTrue();
        ((double)decimalResult.ExecutionPrice).Should().BeApproximately(doubleResult.ExecutionPrice, 0.01);
    }

    #endregion
}
