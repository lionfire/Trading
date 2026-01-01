using FluentAssertions;
using LionFire.Trading.Automation.PriceMonitoring;
using LionFire.Trading.PriceMonitoring;
using Moq;

namespace LionFire.Trading.Automation.Tests.PriceMonitoring;

/// <summary>
/// Tests for <see cref="PendingOrderManager{TPrecision}"/>.
/// </summary>
public class PendingOrderManagerTests : IAsyncDisposable
{
    private readonly Mock<ILivePriceMonitor> _mockPriceMonitor;
    private readonly PendingOrderManager<decimal> _manager;
    private readonly List<(PendingOrder<decimal> Order, decimal Price)> _triggeredOrders;

    private static ExchangeSymbol TestSymbol => new("Binance", "spot", "BTCUSDT");
    private static ExchangeSymbol TestSymbol2 => new("Binance", "spot", "ETHUSDT");

    public PendingOrderManagerTests()
    {
        _mockPriceMonitor = new Mock<ILivePriceMonitor>();
        _manager = new PendingOrderManager<decimal>(_mockPriceMonitor.Object);
        _triggeredOrders = new List<(PendingOrder<decimal>, decimal)>();

        _manager.OnOrderTriggered += (order, price) => _triggeredOrders.Add((order, price));
    }

    public async ValueTask DisposeAsync()
    {
        await _manager.DisposeAsync();
    }

    #region Registration Tests

    [Fact]
    public async Task RegisterStopLoss_CreatesOrderWithCorrectProperties()
    {
        var position = CreateMockPosition(1, LongAndShort.Long);

        var order = await _manager.RegisterStopLossAsync(position.Object, 100m);

        order.Should().NotBeNull();
        order.OrderType.Should().Be(SimulatedOrderType.StopLoss);
        order.TriggerPrice.Should().Be(100m);
        order.Direction.Should().Be(LongAndShort.Long);
        order.PositionId.Should().Be(1);
    }

    [Fact]
    public async Task RegisterTakeProfit_CreatesOrderWithCorrectProperties()
    {
        var position = CreateMockPosition(2, LongAndShort.Short);

        var order = await _manager.RegisterTakeProfitAsync(position.Object, 150m);

        order.Should().NotBeNull();
        order.OrderType.Should().Be(SimulatedOrderType.TakeProfit);
        order.TriggerPrice.Should().Be(150m);
        order.Direction.Should().Be(LongAndShort.Short);
        order.PositionId.Should().Be(2);
    }

    [Fact]
    public async Task RegisterOrder_SubscribesToPriceMonitor()
    {
        var position = CreateMockPosition(1, LongAndShort.Long);

        await _manager.RegisterStopLossAsync(position.Object, 100m);

        _mockPriceMonitor.Verify(m => m.SubscribeAsync(TestSymbol, It.IsAny<CancellationToken>()), Moq.Times.Once);
    }

    [Fact]
    public async Task RegisterMultipleOrdersForSameSymbol_SubscribesOnce()
    {
        var position1 = CreateMockPosition(1, LongAndShort.Long);
        var position2 = CreateMockPosition(2, LongAndShort.Long);

        await _manager.RegisterStopLossAsync(position1.Object, 100m);
        await _manager.RegisterTakeProfitAsync(position2.Object, 150m);

        // Should only subscribe once for the same symbol
        _mockPriceMonitor.Verify(m => m.SubscribeAsync(TestSymbol, It.IsAny<CancellationToken>()), Moq.Times.Once);
    }

    [Fact]
    public async Task RegisterOrdersForDifferentSymbols_SubscribesToEach()
    {
        var position1 = CreateMockPosition(1, LongAndShort.Long);
        var position2 = CreateMockPosition(2, LongAndShort.Long, TestSymbol2);

        await _manager.RegisterStopLossAsync(position1.Object, 100m);
        await _manager.RegisterStopLossAsync(position2.Object, 2000m);

        _mockPriceMonitor.Verify(m => m.SubscribeAsync(TestSymbol, It.IsAny<CancellationToken>()), Moq.Times.Once);
        _mockPriceMonitor.Verify(m => m.SubscribeAsync(TestSymbol2, It.IsAny<CancellationToken>()), Moq.Times.Once);
    }

    #endregion

    #region Order Count Tests

    [Fact]
    public async Task OrderCount_ReflectsRegisteredOrders()
    {
        var position = CreateMockPosition(1, LongAndShort.Long);

        _manager.OrderCount.Should().Be(0);

        await _manager.RegisterStopLossAsync(position.Object, 100m);
        _manager.OrderCount.Should().Be(1);

        await _manager.RegisterTakeProfitAsync(position.Object, 150m);
        _manager.OrderCount.Should().Be(2);
    }

    [Fact]
    public async Task ActiveSymbolCount_ReflectsActiveSymbols()
    {
        var position1 = CreateMockPosition(1, LongAndShort.Long);
        var position2 = CreateMockPosition(2, LongAndShort.Long, TestSymbol2);

        _manager.ActiveSymbolCount.Should().Be(0);

        await _manager.RegisterStopLossAsync(position1.Object, 100m);
        _manager.ActiveSymbolCount.Should().Be(1);

        await _manager.RegisterStopLossAsync(position2.Object, 2000m);
        _manager.ActiveSymbolCount.Should().Be(2);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateOrder_ChangesTriggerPrice()
    {
        var position = CreateMockPosition(1, LongAndShort.Long);
        var order = await _manager.RegisterStopLossAsync(position.Object, 100m);

        await _manager.UpdateOrderAsync(order.Id, 95m);

        var updatedOrder = _manager.GetOrder(order.Id);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.TriggerPrice.Should().Be(95m);
        updatedOrder.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateOrder_ThrowsIfNotFound()
    {
        Func<Task> act = () => _manager.UpdateOrderAsync("nonexistent", 100m);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    #endregion

    #region Cancel Tests

    [Fact]
    public async Task CancelOrder_RemovesOrder()
    {
        var position = CreateMockPosition(1, LongAndShort.Long);
        var order = await _manager.RegisterStopLossAsync(position.Object, 100m);

        var result = await _manager.CancelOrderAsync(order.Id);

        result.Should().BeTrue();
        _manager.GetOrder(order.Id).Should().BeNull();
        _manager.OrderCount.Should().Be(0);
    }

    [Fact]
    public async Task CancelOrder_ReturnsFalseIfNotFound()
    {
        var result = await _manager.CancelOrderAsync("nonexistent");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CancelLastOrderForSymbol_UnsubscribesFromPriceMonitor()
    {
        var position = CreateMockPosition(1, LongAndShort.Long);
        var order = await _manager.RegisterStopLossAsync(position.Object, 100m);

        await _manager.CancelOrderAsync(order.Id);

        _mockPriceMonitor.Verify(m => m.UnsubscribeAsync(TestSymbol, It.IsAny<CancellationToken>()), Moq.Times.Once);
    }

    [Fact]
    public async Task ClearOrdersForPosition_RemovesAllPositionOrders()
    {
        var position = CreateMockPosition(1, LongAndShort.Long);

        await _manager.RegisterStopLossAsync(position.Object, 100m);
        await _manager.RegisterTakeProfitAsync(position.Object, 150m);

        var count = await _manager.ClearOrdersForPositionAsync(1);

        count.Should().Be(2);
        _manager.OrderCount.Should().Be(0);
    }

    #endregion

    #region Query Tests

    [Fact]
    public async Task GetOrder_ReturnsOrderById()
    {
        var position = CreateMockPosition(1, LongAndShort.Long);
        var order = await _manager.RegisterStopLossAsync(position.Object, 100m);

        var result = _manager.GetOrder(order.Id);

        result.Should().NotBeNull();
        result.Should().BeSameAs(order);
    }

    [Fact]
    public async Task GetOrdersForSymbol_ReturnsSymbolOrders()
    {
        var position1 = CreateMockPosition(1, LongAndShort.Long);
        var position2 = CreateMockPosition(2, LongAndShort.Long, TestSymbol2);

        await _manager.RegisterStopLossAsync(position1.Object, 100m);
        await _manager.RegisterStopLossAsync(position2.Object, 2000m);
        await _manager.RegisterTakeProfitAsync(position1.Object, 150m);

        var orders = _manager.GetOrdersForSymbol(TestSymbol).ToList();

        orders.Should().HaveCount(2);
        orders.Should().OnlyContain(o => o.Symbol == TestSymbol);
    }

    [Fact]
    public async Task GetOrdersForPosition_ReturnsPositionOrders()
    {
        var position = CreateMockPosition(1, LongAndShort.Long);

        var sl = await _manager.RegisterStopLossAsync(position.Object, 100m);
        var tp = await _manager.RegisterTakeProfitAsync(position.Object, 150m);

        var orders = _manager.GetOrdersForPosition(1).ToList();

        orders.Should().HaveCount(2);
        orders.Should().Contain(sl);
        orders.Should().Contain(tp);
    }

    #endregion

    #region Price Tick Trigger Tests

    [Fact]
    public async Task PriceTick_TriggersMatchingOrder()
    {
        // Setup - capture the OnPriceTick handler
        Action<ExchangeSymbol, decimal, decimal>? capturedHandler = null;
        _mockPriceMonitor.SetupAdd(m => m.OnPriceTick += It.IsAny<Action<ExchangeSymbol, decimal, decimal>>())
            .Callback<Action<ExchangeSymbol, decimal, decimal>>(h => capturedHandler = h);

        // Create a new manager to capture the handler
        await using var manager = new PendingOrderManager<decimal>(_mockPriceMonitor.Object);
        var triggered = new List<(PendingOrder<decimal>, decimal)>();
        manager.OnOrderTriggered += (o, p) => triggered.Add((o, p));

        var position = CreateMockPosition(1, LongAndShort.Long);
        await manager.RegisterStopLossAsync(position.Object, 100m);

        // Simulate price tick that triggers the stop
        capturedHandler?.Invoke(TestSymbol, 99m, 101m);

        triggered.Should().HaveCount(1);
        triggered[0].Item2.Should().Be(99m); // Long exits at bid
    }

    [Fact]
    public async Task PriceTick_DoesNotTriggerNonMatchingOrders()
    {
        Action<ExchangeSymbol, decimal, decimal>? capturedHandler = null;
        _mockPriceMonitor.SetupAdd(m => m.OnPriceTick += It.IsAny<Action<ExchangeSymbol, decimal, decimal>>())
            .Callback<Action<ExchangeSymbol, decimal, decimal>>(h => capturedHandler = h);

        await using var manager = new PendingOrderManager<decimal>(_mockPriceMonitor.Object);
        var triggered = new List<(PendingOrder<decimal>, decimal)>();
        manager.OnOrderTriggered += (o, p) => triggered.Add((o, p));

        var position = CreateMockPosition(1, LongAndShort.Long);
        await manager.RegisterStopLossAsync(position.Object, 100m);

        // Price is above stop, should not trigger
        capturedHandler?.Invoke(TestSymbol, 101m, 103m);

        triggered.Should().BeEmpty();
    }

    [Fact]
    public async Task TriggeredOrder_IsRemovedFromManager()
    {
        Action<ExchangeSymbol, decimal, decimal>? capturedHandler = null;
        _mockPriceMonitor.SetupAdd(m => m.OnPriceTick += It.IsAny<Action<ExchangeSymbol, decimal, decimal>>())
            .Callback<Action<ExchangeSymbol, decimal, decimal>>(h => capturedHandler = h);

        await using var manager = new PendingOrderManager<decimal>(_mockPriceMonitor.Object);
        var position = CreateMockPosition(1, LongAndShort.Long);
        var order = await manager.RegisterStopLossAsync(position.Object, 100m);

        // Trigger the order
        capturedHandler?.Invoke(TestSymbol, 99m, 101m);

        // Order should be removed
        manager.GetOrder(order.Id).Should().BeNull();
        manager.OrderCount.Should().Be(0);
    }

    #endregion

    #region Event Tests

    [Fact]
    public async Task OnOrderRegistered_FiredWhenOrderCreated()
    {
        PendingOrder<decimal>? registeredOrder = null;
        _manager.OnOrderRegistered += o => registeredOrder = o;

        var position = CreateMockPosition(1, LongAndShort.Long);
        var order = await _manager.RegisterStopLossAsync(position.Object, 100m);

        registeredOrder.Should().BeSameAs(order);
    }

    [Fact]
    public async Task OnOrderCancelled_FiredWhenOrderCancelled()
    {
        PendingOrder<decimal>? cancelledOrder = null;
        _manager.OnOrderCancelled += o => cancelledOrder = o;

        var position = CreateMockPosition(1, LongAndShort.Long);
        var order = await _manager.RegisterStopLossAsync(position.Object, 100m);

        await _manager.CancelOrderAsync(order.Id);

        cancelledOrder.Should().NotBeNull();
        cancelledOrder!.Id.Should().Be(order.Id);
    }

    [Fact]
    public async Task OnOrderUpdated_FiredWhenOrderUpdated()
    {
        (PendingOrder<decimal>? order, decimal oldPrice) updated = default;
        _manager.OnOrderUpdated += (o, p) => updated = (o, p);

        var position = CreateMockPosition(1, LongAndShort.Long);
        var order = await _manager.RegisterStopLossAsync(position.Object, 100m);

        await _manager.UpdateOrderAsync(order.Id, 95m);

        updated.order.Should().NotBeNull();
        updated.oldPrice.Should().Be(100m);
        updated.order!.TriggerPrice.Should().Be(95m);
    }

    #endregion

    #region Helper Methods

    private Mock<IPosition<decimal>> CreateMockPosition(
        int id,
        LongAndShort direction,
        ExchangeSymbol? symbol = null)
    {
        var effectiveSymbol = symbol ?? TestSymbol;
        var mockPosition = new Mock<IPosition<decimal>>();

        mockPosition.Setup(p => p.Id).Returns(id);
        mockPosition.Setup(p => p.LongOrShort).Returns(direction);
        mockPosition.Setup(p => p.Symbol).Returns(effectiveSymbol.Symbol);
        mockPosition.Setup(p => p.SymbolId).Returns(new SymbolId
        {
            Exchange = effectiveSymbol.Exchange,
            ExchangeArea = effectiveSymbol.Area,
            Symbol = effectiveSymbol.Symbol
        });

        return mockPosition;
    }

    #endregion
}
