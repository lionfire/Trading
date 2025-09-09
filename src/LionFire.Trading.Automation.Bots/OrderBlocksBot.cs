using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Proprietary.Abstractions.OrderBlocks;
using LionFire.Trading.ValueWindows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Automation.Bots;

/// <summary>
/// Parameters for Order Blocks Bot
/// </summary>
public class POrderBlocksBot<TValue> : PStandardBot2<POrderBlocksBot<TValue>, TValue>
    , IPBot2Static
    where TValue : struct, INumber<TValue>
{
    #region Static

    [JsonIgnore]
    public override Type MaterializedType => typeof(OrderBlocksBot<TValue>);
    public static Type StaticMaterializedType => typeof(OrderBlocksBot<TValue>);

    #endregion

    /// <summary>
    /// Order Blocks indicator parameters
    /// </summary>
    [PSignal]
    public POrderBlocks<Kline, OrderBlocksState>? OrderBlocks { get; set; }

    /// <summary>
    /// ATR indicator for dynamic stop loss and approaching distance
    /// </summary>
    [PSignal]
    public PAverageTrueRange<double, TValue>? ATR { get; set; }

    /// <summary>
    /// Unidirectional trading settings
    /// </summary>
    public PUnidirectionalBot? Unidirectional { get; set; }

    /// <summary>
    /// Minimum confidence to enter a trade (0-1)
    /// </summary>
    [Parameter(
        HardValueMin = 0.1,
        DefaultMin = 0.3,
        DefaultMax = 0.8,
        ValueMax = 1.0,
        HardValueMax = 1.0,
        Step = 0.05,
        DefaultValue = 0.5)]
    public double MinConfidence { get; set; } = 0.5;

    /// <summary>
    /// Risk/reward ratio threshold for taking trades
    /// </summary>
    [Parameter(
        HardValueMin = 0.5,
        DefaultMin = 1.0,
        DefaultMax = 3.0,
        ValueMax = 5.0,
        HardValueMax = 10.0,
        Step = 0.25,
        DefaultValue = 1.5)]
    public double MinRiskReward { get; set; } = 1.5;

    /// <summary>
    /// ATR multiplier for stop loss placement
    /// </summary>
    [Parameter(
        HardValueMin = 0.5,
        DefaultMin = 1.0,
        DefaultMax = 3.0,
        ValueMax = 5.0,
        HardValueMax = 10.0,
        Step = 0.25,
        DefaultValue = 1.5)]
    public double StopLossATRMultiplier { get; set; } = 1.5;

    /// <summary>
    /// ATR multiplier for take profit placement
    /// </summary>
    [Parameter(
        HardValueMin = 0.5,
        DefaultMin = 1.5,
        DefaultMax = 4.0,
        ValueMax = 8.0,
        HardValueMax = 20.0,
        Step = 0.25,
        DefaultValue = 2.5)]
    public double TakeProfitATRMultiplier { get; set; } = 2.5;

    /// <summary>
    /// Trade on approaching signals
    /// </summary>
    [Parameter]
    public bool TradeOnApproaching { get; set; } = false;

    /// <summary>
    /// Trade on touch signals
    /// </summary>
    [Parameter]
    public bool TradeOnTouch { get; set; } = true;

    /// <summary>
    /// Trade on failed breakout signals
    /// </summary>
    [Parameter]
    public bool TradeOnFailedBreakout { get; set; } = true;

    #region Lifecycle

    public POrderBlocksBot() { }

    public POrderBlocksBot(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame) : base(exchangeSymbolTimeFrame)
    {
        OrderBlocks = new POrderBlocks<Kline, OrderBlocksState>
        {
            SwingLookback = 10,
            MaxBullishBlocks = 5,
            MaxBearishBlocks = 5,
            EnableConfidenceScoring = true,
            EnableGradients = false,
            GenerateApproachingSignals = true,
            ApproachingDistanceATR = 0.5,
            UseBody = true
        };

        ATR = new PAverageTrueRange<double, TValue>
        {
            Period = 14,
            Lookback = 1,
            MovingAverageType = QuantConnect.Indicators.MovingAverageType.Wilders
        };

        Init();
    }

    protected override void InferMissingParameters()
    {
        InputLookbacks = [
            0,  // Bars
            OrderBlocks!.SwingLookback * 2, // OrderBlocks
            1 // ATR
        ];

        base.InferMissingParameters();
    }

    #endregion

    #region Validation

    public void ThrowIfInvalid()
    {
        ArgumentNullException.ThrowIfNull(OrderBlocks, nameof(OrderBlocks));
        ArgumentNullException.ThrowIfNull(ATR, nameof(ATR));
        ArgumentNullException.ThrowIfNull(Unidirectional, nameof(Unidirectional));
    }

    #endregion
}

/// <summary>
/// Bot that trades based on Order Blocks patterns
/// </summary>
[Bot(Direction = BotDirection.Unidirectional)]
public class OrderBlocksBot<TValue> : StandardBot2<POrderBlocksBot<TValue>, TValue>
    where TValue : struct, INumber<TValue>
{
    public static Type ParametersType => typeof(POrderBlocksBot<TValue>);

    #region Inputs

    [Signal(0)]
    public OrderBlocksState OrderBlocksState { get; set; } = null!;

    [Signal(1)]
    public IReadOnlyValuesWindow<TValue> ATR { get; set; } = null!;

    #endregion

    #region State

    private OrderBlockSignal? lastSignal;
    private int barsSinceEntry = 0;
    private bool inPosition = false;
    private OrderBlock? currentTradeBlock;
    private double entryPrice;

    #endregion

    #region Event Handling

    public override void OnBar()
    {
        if (OrderBlocksState == null || ATR.Size == 0) return;

        var typedParams = (POrderBlocksBot<TValue>)Parameters;

        // Update bars since entry
        if (inPosition)
        {
            barsSinceEntry++;
        }

        // Get current price from the latest bar
        var currentBar = Bars[0];
        double currentPrice = currentBar.Close;

        // Check for exit conditions first
        if (inPosition && currentTradeBlock.HasValue)
        {
            CheckExitConditions(currentBar, typedParams);
        }

        // Check for entry conditions
        if (!inPosition)
        {
            CheckEntryConditions(currentBar, typedParams);
        }

        // Update stop loss and take profit based on ATR
        if (inPosition)
        {
            UpdateStopsAndTargets(currentBar, typedParams);
        }
    }

    private void CheckEntryConditions(IKline currentBar, POrderBlocksBot<TValue> parameters)
    {
        // Get the most recent signals from the order blocks indicator
        var signals = GetRecentSignals();
        
        foreach (var signal in signals)
        {
            // Check if signal meets our criteria
            if (signal.Confidence < parameters.MinConfidence) continue;

            bool shouldTrade = false;
            LongAndShort direction = LongAndShort.None;

            switch (signal.Type)
            {
                case OrderBlockSignalType.Approaching:
                    if (parameters.TradeOnApproaching)
                    {
                        shouldTrade = true;
                        direction = signal.Block.Type == OrderBlockType.Bullish ? 
                            LongAndShort.Long : LongAndShort.Short;
                    }
                    break;

                case OrderBlockSignalType.Touch:
                    if (parameters.TradeOnTouch)
                    {
                        shouldTrade = true;
                        direction = signal.Block.Type == OrderBlockType.Bullish ? 
                            LongAndShort.Long : LongAndShort.Short;
                    }
                    break;

                case OrderBlockSignalType.FailedBreakout:
                    if (parameters.TradeOnFailedBreakout)
                    {
                        shouldTrade = true;
                        // Trade in the direction of the block (failed breakout reversal)
                        direction = signal.Block.Type == OrderBlockType.Bullish ? 
                            LongAndShort.Long : LongAndShort.Short;
                    }
                    break;
            }

            if (shouldTrade && direction != LongAndShort.None)
            {
                // Calculate risk/reward
                double riskReward = CalculateRiskReward(currentBar.Close, signal.Block);
                
                if (riskReward >= parameters.MinRiskReward)
                {
                    // Enter trade
                    Direction = direction;
                    if (TryOpen())
                    {
                        inPosition = true;
                        currentTradeBlock = signal.Block;
                        entryPrice = currentBar.Close;
                        barsSinceEntry = 0;
                        lastSignal = signal;
                        
                        Debug.WriteLine($"OrderBlocksBot: Entered {direction} at {entryPrice} on {signal.Type} signal, " +
                            $"Block: {signal.Block.Bottom:F2}-{signal.Block.Top:F2}, Confidence: {signal.Confidence:F2}");
                        
                        break; // Only enter one trade at a time
                    }
                }
            }
        }
    }

    private void CheckExitConditions(IKline currentBar, POrderBlocksBot<TValue> parameters)
    {
        bool shouldExit = false;
        string exitReason = "";

        // Check if the order block has been invalidated
        if (currentTradeBlock.HasValue)
        {
            var block = currentTradeBlock.Value;
            
            if (Direction == LongAndShort.Long)
            {
                // Exit long if price breaks below the order block
                if (currentBar.Close < block.Bottom)
                {
                    shouldExit = true;
                    exitReason = "Block invalidated (price below support)";
                }
            }
            else if (Direction == LongAndShort.Short)
            {
                // Exit short if price breaks above the order block
                if (currentBar.Close > block.Top)
                {
                    shouldExit = true;
                    exitReason = "Block invalidated (price above resistance)";
                }
            }
        }

        // Check for opposing order block signal
        var signals = GetRecentSignals();
        foreach (var signal in signals)
        {
            if (signal.Type == OrderBlockSignalType.Touch && signal.Confidence >= parameters.MinConfidence)
            {
                if ((Direction == LongAndShort.Long && signal.Block.Type == OrderBlockType.Bearish) ||
                    (Direction == LongAndShort.Short && signal.Block.Type == OrderBlockType.Bullish))
                {
                    shouldExit = true;
                    exitReason = $"Opposing {signal.Block.Type} block touched";
                    break;
                }
            }
        }

        // Time-based exit (optional)
        if (barsSinceEntry > 50) // Exit after 50 bars if still in position
        {
            shouldExit = true;
            exitReason = "Time-based exit";
        }

        if (shouldExit)
        {
            if (TryClose())
            {
                Debug.WriteLine($"OrderBlocksBot: Exited {Direction} position. Reason: {exitReason}");
                inPosition = false;
                currentTradeBlock = null;
                barsSinceEntry = 0;
                Direction = LongAndShort.None;
            }
        }
    }

    private void UpdateStopsAndTargets(IKline currentBar, POrderBlocksBot<TValue> parameters)
    {
        if (ATR.Size == 0) return;

        TValue atrValue = ATR[0];
        double atr = Convert.ToDouble(atrValue);

        if (Direction == LongAndShort.Long)
        {
            // For longs: stop loss below entry, take profit above
            double stopLoss = entryPrice - (atr * parameters.StopLossATRMultiplier);
            double takeProfit = entryPrice + (atr * parameters.TakeProfitATRMultiplier);

            // Optionally use the order block boundaries
            if (currentTradeBlock.HasValue)
            {
                // Use the bottom of the order block as stop loss if it's tighter
                stopLoss = Math.Max(stopLoss, currentTradeBlock.Value.Bottom - (atr * 0.1));
            }

            Account.SetStopLosses(Symbol, Direction, stopLoss, StopLossFlags.TightenOnly);
            Account.SetTakeProfits(Symbol, Direction, takeProfit, StopLossFlags.Unspecified);
        }
        else if (Direction == LongAndShort.Short)
        {
            // For shorts: stop loss above entry, take profit below
            double stopLoss = entryPrice + (atr * parameters.StopLossATRMultiplier);
            double takeProfit = entryPrice - (atr * parameters.TakeProfitATRMultiplier);

            // Optionally use the order block boundaries
            if (currentTradeBlock.HasValue)
            {
                // Use the top of the order block as stop loss if it's tighter
                stopLoss = Math.Min(stopLoss, currentTradeBlock.Value.Top + (atr * 0.1));
            }

            Account.SetStopLosses(Symbol, Direction, stopLoss, StopLossFlags.TightenOnly);
            Account.SetTakeProfits(Symbol, Direction, takeProfit, StopLossFlags.Unspecified);
        }
    }

    private double CalculateRiskReward(double entryPrice, OrderBlock block)
    {
        if (ATR.Size == 0) return 0;

        TValue atrValue = ATR[0];
        double atr = Convert.ToDouble(atrValue);
        var parameters = (POrderBlocksBot<TValue>)Parameters;

        double risk, reward;

        if (block.Type == OrderBlockType.Bullish)
        {
            // Long trade
            risk = atr * parameters.StopLossATRMultiplier;
            reward = atr * parameters.TakeProfitATRMultiplier;
        }
        else
        {
            // Short trade
            risk = atr * parameters.StopLossATRMultiplier;
            reward = atr * parameters.TakeProfitATRMultiplier;
        }

        return risk > 0 ? reward / risk : 0;
    }

    private List<OrderBlockSignal> GetRecentSignals()
    {
        // In a real implementation, this would get signals from the OrderBlocksState
        // For now, we'll analyze the current state to generate signals
        var signals = new List<OrderBlockSignal>();
        
        if (OrderBlocksState == null) return signals;

        var currentBar = Bars[0];
        double currentPrice = currentBar.Close;

        // Check for blocks near current price
        foreach (var block in OrderBlocksState.AllBlocks)
        {
            // Simple proximity check
            double distance = block.Type == OrderBlockType.Bullish ? 
                block.Top - currentPrice : currentPrice - block.Bottom;

            if (Math.Abs(distance) < Convert.ToDouble(ATR[0]) * 0.5)
            {
                signals.Add(new OrderBlockSignal
                {
                    Type = OrderBlockSignalType.Approaching,
                    Block = block,
                    Price = currentPrice,
                    Timestamp = currentBar.Time,
                    Confidence = block.Confidence
                });
            }

            // Check for touch
            if (block.ContainsPrice(currentPrice))
            {
                signals.Add(new OrderBlockSignal
                {
                    Type = OrderBlockSignalType.Touch,
                    Block = block,
                    Price = currentPrice,
                    Timestamp = currentBar.Time,
                    Confidence = block.Confidence
                });
            }
        }

        return signals;
    }

    #endregion
}