using FluentAssertions;
using LionFire.Trading.Automation.PriceMonitoring;
using LionFire.Trading.PriceMonitoring;

namespace LionFire.Trading.Automation.Tests.PriceMonitoring;

/// <summary>
/// Tests for <see cref="PendingOrder{TPrecision}"/> trigger logic.
/// </summary>
public class PendingOrderTests
{
    private static ExchangeSymbol TestSymbol => new("Binance", "spot", "BTCUSDT");

    #region Long Position - StopLoss

    [Fact]
    public void LongStopLoss_TriggersBelowTriggerPrice()
    {
        var order = CreateOrder(LongAndShort.Long, SimulatedOrderType.StopLoss, 100m);

        // Bid at 99 (below 100) should trigger
        order.ShouldTrigger(99m, 101m).Should().BeTrue();
    }

    [Fact]
    public void LongStopLoss_TriggersAtTriggerPrice()
    {
        var order = CreateOrder(LongAndShort.Long, SimulatedOrderType.StopLoss, 100m);

        // Bid at exactly 100 should trigger
        order.ShouldTrigger(100m, 102m).Should().BeTrue();
    }

    [Fact]
    public void LongStopLoss_DoesNotTriggerAboveTriggerPrice()
    {
        var order = CreateOrder(LongAndShort.Long, SimulatedOrderType.StopLoss, 100m);

        // Bid at 101 (above 100) should not trigger
        order.ShouldTrigger(101m, 103m).Should().BeFalse();
    }

    #endregion

    #region Long Position - TakeProfit

    [Fact]
    public void LongTakeProfit_TriggersAboveTriggerPrice()
    {
        var order = CreateOrder(LongAndShort.Long, SimulatedOrderType.TakeProfit, 100m);

        // Bid at 101 (above 100) should trigger
        order.ShouldTrigger(101m, 103m).Should().BeTrue();
    }

    [Fact]
    public void LongTakeProfit_TriggersAtTriggerPrice()
    {
        var order = CreateOrder(LongAndShort.Long, SimulatedOrderType.TakeProfit, 100m);

        // Bid at exactly 100 should trigger
        order.ShouldTrigger(100m, 102m).Should().BeTrue();
    }

    [Fact]
    public void LongTakeProfit_DoesNotTriggerBelowTriggerPrice()
    {
        var order = CreateOrder(LongAndShort.Long, SimulatedOrderType.TakeProfit, 100m);

        // Bid at 99 (below 100) should not trigger
        order.ShouldTrigger(99m, 101m).Should().BeFalse();
    }

    #endregion

    #region Short Position - StopLoss

    [Fact]
    public void ShortStopLoss_TriggersAboveTriggerPrice()
    {
        var order = CreateOrder(LongAndShort.Short, SimulatedOrderType.StopLoss, 100m);

        // Ask at 101 (above 100) should trigger
        order.ShouldTrigger(99m, 101m).Should().BeTrue();
    }

    [Fact]
    public void ShortStopLoss_TriggersAtTriggerPrice()
    {
        var order = CreateOrder(LongAndShort.Short, SimulatedOrderType.StopLoss, 100m);

        // Ask at exactly 100 should trigger
        order.ShouldTrigger(98m, 100m).Should().BeTrue();
    }

    [Fact]
    public void ShortStopLoss_DoesNotTriggerBelowTriggerPrice()
    {
        var order = CreateOrder(LongAndShort.Short, SimulatedOrderType.StopLoss, 100m);

        // Ask at 99 (below 100) should not trigger
        order.ShouldTrigger(97m, 99m).Should().BeFalse();
    }

    #endregion

    #region Short Position - TakeProfit

    [Fact]
    public void ShortTakeProfit_TriggersBelowTriggerPrice()
    {
        var order = CreateOrder(LongAndShort.Short, SimulatedOrderType.TakeProfit, 100m);

        // Ask at 99 (below 100) should trigger
        order.ShouldTrigger(97m, 99m).Should().BeTrue();
    }

    [Fact]
    public void ShortTakeProfit_TriggersAtTriggerPrice()
    {
        var order = CreateOrder(LongAndShort.Short, SimulatedOrderType.TakeProfit, 100m);

        // Ask at exactly 100 should trigger
        order.ShouldTrigger(98m, 100m).Should().BeTrue();
    }

    [Fact]
    public void ShortTakeProfit_DoesNotTriggerAboveTriggerPrice()
    {
        var order = CreateOrder(LongAndShort.Short, SimulatedOrderType.TakeProfit, 100m);

        // Ask at 101 (above 100) should not trigger
        order.ShouldTrigger(99m, 101m).Should().BeFalse();
    }

    #endregion

    #region Execution Price

    [Fact]
    public void LongPosition_ExecutesAtBid()
    {
        var order = CreateOrder(LongAndShort.Long, SimulatedOrderType.StopLoss, 100m);

        // Long exits at bid
        order.GetExecutionPrice(99m, 101m).Should().Be(99m);
    }

    [Fact]
    public void ShortPosition_ExecutesAtAsk()
    {
        var order = CreateOrder(LongAndShort.Short, SimulatedOrderType.StopLoss, 100m);

        // Short exits at ask
        order.GetExecutionPrice(99m, 101m).Should().Be(101m);
    }

    #endregion

    #region Helper Methods

    private static PendingOrder<decimal> CreateOrder(
        LongAndShort direction,
        SimulatedOrderType orderType,
        decimal triggerPrice)
    {
        return new PendingOrder<decimal>
        {
            Id = $"test-{Guid.NewGuid()}",
            PositionId = 1,
            Symbol = TestSymbol,
            OrderType = orderType,
            TriggerPrice = triggerPrice,
            Direction = direction
        };
    }

    #endregion
}
