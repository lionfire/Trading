using LionFire.Trading.Indicators.Parameters;
using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.ValueWindows;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Automation.Bots;

/// <summary>
/// Parameters for the Chandelier Exit Bot.
/// This bot always maintains a position: long when price is above the exit line, short when below.
/// </summary>
/// <typeparam name="TValue">The numeric type for calculations (typically double or decimal)</typeparam>
public class PChandelierExitBot<TValue> : PStandardBot2<PChandelierExitBot<TValue>, TValue>
    , IPBot2Static
    where TValue : struct, INumber<TValue>
{
    #region Static

    [JsonIgnore]
    public override Type MaterializedType => typeof(ChandelierExitBot<TValue>);
    public static Type StaticMaterializedType => typeof(ChandelierExitBot<TValue>);

    #endregion

    #region Indicator Parameters

    /// <summary>
    /// Chandelier Exit indicator parameters.
    /// The indicator produces ExitLong (for trailing stop on long positions) and ExitShort (for short positions).
    /// This bot uses ExitLong as the trend line: above = long, below = short.
    /// </summary>
    [PSignal]
    public PChandelierExit<double, TValue>? ChandelierExitLong { get; set; }

    /// <summary>
    /// Optional separate ATR indicator for position sizing and take profit calculations.
    /// If not provided, the bot will estimate ATR from the Chandelier Exit distance.
    /// </summary>
    [PSignal]
    public PAverageTrueRange<double, TValue>? ATR { get; set; }

    #endregion

    #region Signal Filtering Parameters

    /// <summary>
    /// If true, use the previous bar's close for comparison (avoids same-bar whipsaws).
    /// Default is false (use current bar's close).
    /// </summary>
    [TradingParameter(OptimizePriority = -10, DefaultValue = false)]
    public bool UsePreviousBar { get; set; } = false;

    /// <summary>
    /// Minimum ATR distance required to flip direction.
    /// Helps avoid whipsaws when price is hovering near the exit line.
    /// 0 = no minimum distance required.
    /// </summary>
    [TradingParameter(
        OptimizePriority = -10,
        HardValueMin = 0.0,
        DefaultMin = 0.0,
        DefaultMax = 1.0,
        HardValueMax = 3.0,
        Step = 0.1,
        DefaultValue = 0.0)]
    public TValue MinAtrDistanceToFlip { get; set; } = TValue.Zero;

    #endregion

    #region Stop Loss Parameters

    /// <summary>
    /// Whether to use the Chandelier Exit as a trailing stop loss.
    /// When enabled, stop losses are set at the exit line level.
    /// </summary>
    [TradingParameter(OptimizePriority = -15, DefaultValue = true)]
    public bool UseAsTrailingStop { get; set; } = true;

    /// <summary>
    /// Additional buffer (in ATR units) to add beyond the Chandelier Exit line for stop loss.
    /// Helps avoid getting stopped out by wicks that briefly pierce the exit line.
    /// 0 = stop exactly at exit line.
    /// </summary>
    [TradingParameter(
        OptimizePriority = -16,
        HardValueMin = 0.0,
        DefaultMin = 0.0,
        DefaultMax = 1.0,
        HardValueMax = 2.0,
        Step = 0.1,
        DefaultValue = 0.0)]
    public TValue StopLossBuffer { get; set; } = TValue.Zero;

    #endregion

    #region Take Profit Parameters

    /// <summary>
    /// Whether to use ATR-based take profit levels.
    /// </summary>
    [TradingParameter(OptimizePriority = -20, DefaultValue = false)]
    public bool UseTakeProfit { get; set; } = false;

    /// <summary>
    /// ATR multiplier for take profit distance from entry price.
    /// Take Profit = Entry ± (ATR × TakeProfitAtrMultiplier)
    /// </summary>
    [TradingParameter(
        OptimizePriority = -21,
        HardValueMin = 0.5,
        DefaultMin = 1.0,
        DefaultMax = 5.0,
        HardValueMax = 10.0,
        Step = 0.25,
        DefaultValue = 2.0)]
    public TValue TakeProfitAtrMultiplier { get; set; } = TValue.CreateChecked(2.0);

    /// <summary>
    /// Whether to trail the take profit as the position moves in our favor.
    /// When enabled, take profit is recalculated on each bar based on current price.
    /// </summary>
    [TradingParameter(OptimizePriority = -22, DefaultValue = false)]
    public bool TrailTakeProfit { get; set; } = false;

    #endregion

    #region Position Sizing Parameters

    /// <summary>
    /// Whether to use ATR-based position sizing.
    /// When enabled, position size is calculated based on risk percentage and ATR.
    /// </summary>
    [TradingParameter(OptimizePriority = -30, DefaultValue = false)]
    public bool UseAtrPositionSizing { get; set; } = false;

    /// <summary>
    /// Percentage of account balance to risk per trade (0.01 = 1%).
    /// Used when UseAtrPositionSizing is enabled.
    /// Position Size = (Balance × RiskPercentage) / (ATR × ChandelierMultiplier)
    /// </summary>
    [TradingParameter(
        OptimizePriority = -31,
        HardValueMin = 0.001,
        DefaultMin = 0.005,
        DefaultMax = 0.05,
        HardValueMax = 0.10,
        Step = 0.005,
        DefaultValue = 0.01)]
    public double RiskPercentage { get; set; } = 0.01;

    /// <summary>
    /// Maximum position size as a multiple of the base PositionSize.
    /// Prevents oversized positions when volatility is very low.
    /// </summary>
    [TradingParameter(
        OptimizePriority = -32,
        HardValueMin = 1.0,
        DefaultMin = 1.0,
        DefaultMax = 10.0,
        HardValueMax = 20.0,
        Step = 0.5,
        DefaultValue = 3.0)]
    public double MaxPositionSizeMultiple { get; set; } = 3.0;

    /// <summary>
    /// Minimum position size as a multiple of the base PositionSize.
    /// Prevents tiny positions when volatility is very high.
    /// </summary>
    [TradingParameter(
        OptimizePriority = -32,
        HardValueMin = 0.1,
        DefaultMin = 0.1,
        DefaultMax = 1.0,
        HardValueMax = 1.0,
        Step = 0.1,
        DefaultValue = 0.25)]
    public double MinPositionSizeMultiple { get; set; } = 0.25;

    #endregion

    #region Lookback Configuration

    [JsonIgnore]
    private const int Lookback = 1;

    #endregion

    #region Lifecycle

    public PChandelierExitBot() { }

    public PChandelierExitBot(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, int period = 22, double atrMultiplier = 3.0)
        : base(exchangeSymbolTimeFrame)
    {
        ChandelierExitLong = new PChandelierExit<double, TValue>
        {
            Period = period,
            AtrMultiplier = TValue.CreateChecked(atrMultiplier),
            Lookback = Lookback,
        };

        // Add ATR indicator with same period for position sizing/take profit
        ATR = new PAverageTrueRange<double, TValue>
        {
            Period = period,
            Lookback = Lookback,
        };

        Init();
    }

    protected override void InferMissingParameters()
    {
        var lookbacks = new List<int>
        {
            Lookback, // Bars - need [0] and [1] when UsePreviousBar is true
            Lookback  // Chandelier Exit - need [0] for current value
        };

        if (ChandelierExitLong != null)
        {
            ChandelierExitLong.Lookback = Lookback;
        }

        // Add ATR lookback if ATR is configured
        if (ATR != null)
        {
            ATR.Lookback = Lookback;
            lookbacks.Add(Lookback);
        }

        InputLookbacks = lookbacks.ToArray();

        base.InferMissingParameters();
    }

    #endregion

    #region Validation

    public void ThrowIfInvalid()
    {
        ArgumentNullException.ThrowIfNull(ChandelierExitLong, nameof(ChandelierExitLong));
    }

    #endregion
}

/// <summary>
/// Chandelier Exit trend-following bot.
///
/// Strategy:
/// - Go LONG when price is above the Chandelier Exit Long line
/// - Go SHORT when price is below the Chandelier Exit Long line
/// - Always maintains a position (flips between long and short)
///
/// Features:
/// - ATR-based take profit levels
/// - ATR-based position sizing (risk percentage of account)
/// - Trailing stop loss at the Chandelier Exit line
///
/// The Chandelier Exit Long line is calculated as: Highest High(N) - ATR(N) × Multiplier
/// It "hangs" from the ceiling (recent highs) like a chandelier.
///
/// In uptrends, price stays above this line. When price breaks below, it signals trend reversal.
/// </summary>
/// <typeparam name="TValue">The numeric type for calculations</typeparam>
[Bot(Direction = BotDirection.Bidirectional)]
public class ChandelierExitBot<TValue> : StandardBot2<PChandelierExitBot<TValue>, TValue>
    where TValue : struct, INumber<TValue>
{
    public static Type ParametersType => typeof(PChandelierExitBot<TValue>);

    #region Inputs

    /// <summary>
    /// Chandelier Exit Long values - the trend line we use for direction decisions.
    /// When price is above this line, we're in an uptrend (long).
    /// When price is below this line, we're in a downtrend (short).
    /// </summary>
    [Signal(0)]
    public IReadOnlyValuesWindow<TValue> ChandelierExitLong { get; set; } = null!;

    /// <summary>
    /// ATR values for position sizing and take profit calculations.
    /// Optional - if not provided, position sizing uses base PositionSize.
    /// </summary>
    [Signal(1)]
    public IReadOnlyValuesWindow<TValue>? ATR { get; set; }

    #endregion

    #region State

    /// <summary>
    /// Current position direction. Null if no position yet.
    /// </summary>
    private LongAndShort? CurrentDirection { get; set; }

    /// <summary>
    /// Entry price of the current position.
    /// </summary>
    private TValue? EntryPrice { get; set; }

    /// <summary>
    /// ATR value at the time of entry.
    /// </summary>
    private TValue? EntryAtr { get; set; }

    /// <summary>
    /// Best price achieved during the current position (for trailing take profit).
    /// </summary>
    private TValue? BestPrice { get; set; }

    #endregion

    #region Lifecycle

    private ILogger Logger { get; }

    public ChandelierExitBot(ILogger<ChandelierExitBot<TValue>> logger)
    {
        Logger = logger;
    }

    #endregion

    #region Event Handling

    public override void OnBar()
    {
        // Need at least 1 Chandelier Exit value (or 2 if using previous bar comparison)
        var requiredSize = TypedParameters.UsePreviousBar ? 2 : 1;
        if (ChandelierExitLong == null || ChandelierExitLong.Size < requiredSize)
        {
            Logger.LogDebug("[ChandelierExitBot] OnBar - Not enough data. Size={Size}, Required={Required}",
                ChandelierExitLong?.Size ?? 0, requiredSize);
            return;
        }

        var typedParams = TypedParameters;

        // Get the exit line value
        var exitLine = ChandelierExitLong[0];

        // Get current ATR value (if available)
        var currentAtr = GetCurrentAtr(typedParams);

        // Get the price to compare - current close or previous close based on settings
        var compareBar = typedParams.UsePreviousBar ? Bars[1] : Bars[0];
        var currentBar = Bars[0];
        var closePrice = compareBar.Close;

        // Update best price for trailing take profit
        UpdateBestPrice(currentBar);

        // Determine the signal direction
        LongAndShort signalDirection;

        if (closePrice > exitLine)
        {
            signalDirection = LongAndShort.Long;
        }
        else if (closePrice < exitLine)
        {
            signalDirection = LongAndShort.Short;
        }
        else
        {
            // Price exactly at exit line - maintain current direction or wait
            Logger.LogDebug("[ChandelierExitBot] Price at exit line, maintaining direction");
            UpdateStopsAndTargets(typedParams, exitLine, currentAtr, currentBar);
            return;
        }

        // Check minimum distance requirement to flip
        if (CurrentDirection.HasValue && CurrentDirection.Value != signalDirection)
        {
            var minDistance = typedParams.MinAtrDistanceToFlip;
            if (minDistance > TValue.Zero)
            {
                var distance = TValue.Abs(closePrice - exitLine);
                if (distance < minDistance)
                {
                    Logger.LogDebug("[ChandelierExitBot] Distance {Distance} below minimum {MinDistance}, not flipping",
                        distance, minDistance);
                    UpdateStopsAndTargets(typedParams, exitLine, currentAtr, currentBar);
                    return;
                }
            }
        }

        // Execute the trade if direction changed
        if (!CurrentDirection.HasValue || CurrentDirection.Value != signalDirection)
        {
            Logger.LogInformation("[ChandelierExitBot] Direction change: {Old} -> {New}. Close={Close}, ExitLine={ExitLine}, ATR={ATR}",
                CurrentDirection?.ToString() ?? "None", signalDirection, closePrice, exitLine, currentAtr);

            // Close existing position if any
            if (CurrentDirection.HasValue)
            {
                TryClose();
            }

            // Update direction
            Direction = signalDirection;
            CurrentDirection = signalDirection;

            // Calculate position size
            var positionSize = CalculatePositionSize(typedParams, currentAtr);

            // Record entry state
            EntryPrice = currentBar.Close;
            EntryAtr = currentAtr;
            BestPrice = currentBar.Close;

            // Open new position with calculated size
            TryOpen(positionSize / typedParams.PositionSize);

            Logger.LogInformation("[ChandelierExitBot] Opened {Direction} position. Size={Size}, Entry={Entry}",
                signalDirection, positionSize, EntryPrice);
        }

        // Update stops and targets
        UpdateStopsAndTargets(typedParams, exitLine, currentAtr, currentBar);

        Logger.LogDebug("[ChandelierExitBot] OnBar complete. Direction={Direction}, Close={Close}, ExitLine={ExitLine}",
            CurrentDirection, closePrice, exitLine);
    }

    #endregion

    #region Position Sizing

    /// <summary>
    /// Calculate position size based on ATR and risk parameters.
    /// </summary>
    private double CalculatePositionSize(PChandelierExitBot<TValue> typedParams, TValue? currentAtr)
    {
        var baseSize = typedParams.PositionSize;

        if (!typedParams.UseAtrPositionSizing || currentAtr == null || currentAtr == TValue.Zero)
        {
            return baseSize;
        }

        // Get account balance
        var balance = DoubleAccount.Balance;
        if (balance <= 0)
        {
            return baseSize;
        }

        // Risk amount = Balance × Risk Percentage
        var riskAmount = balance * typedParams.RiskPercentage;

        // Stop distance = ATR × Chandelier Multiplier (distance to stop loss)
        var atrDouble = Convert.ToDouble(currentAtr);
        var chandelierMultiplier = Convert.ToDouble(typedParams.ChandelierExitLong?.AtrMultiplier ?? TValue.CreateChecked(3.0));
        var stopDistance = atrDouble * chandelierMultiplier;

        // Add buffer if configured
        var bufferDouble = Convert.ToDouble(typedParams.StopLossBuffer);
        stopDistance += atrDouble * bufferDouble;

        if (stopDistance <= 0)
        {
            return baseSize;
        }

        // Position size = Risk Amount / Stop Distance
        // This gives us the notional position size where a move of stopDistance would lose riskAmount
        var calculatedSize = riskAmount / stopDistance;

        // Apply min/max constraints relative to base size
        var minSize = baseSize * typedParams.MinPositionSizeMultiple;
        var maxSize = baseSize * typedParams.MaxPositionSizeMultiple;

        calculatedSize = Math.Max(minSize, Math.Min(maxSize, calculatedSize));

        Logger.LogDebug("[ChandelierExitBot] Position sizing: Balance={Balance}, Risk={RiskPct}%, ATR={ATR}, StopDist={StopDist}, Size={Size}",
            balance, typedParams.RiskPercentage * 100, atrDouble, stopDistance, calculatedSize);

        return calculatedSize;
    }

    #endregion

    #region Stop Loss and Take Profit

    /// <summary>
    /// Update stop loss and take profit levels.
    /// </summary>
    private void UpdateStopsAndTargets(PChandelierExitBot<TValue> typedParams, TValue exitLine, TValue? currentAtr, HLC<TValue> currentBar)
    {
        if (!CurrentDirection.HasValue)
        {
            return;
        }

        // Calculate stop loss with optional buffer
        if (typedParams.UseAsTrailingStop)
        {
            var stopLoss = CalculateStopLoss(typedParams, exitLine, currentAtr);
            Account.SetStopLosses(Symbol, CurrentDirection.Value, stopLoss, StopLossFlags.TightenOnly);
        }

        // Calculate and set take profit
        if (typedParams.UseTakeProfit && EntryPrice.HasValue)
        {
            var takeProfit = CalculateTakeProfit(typedParams, currentBar);
            if (takeProfit.HasValue)
            {
                Account.SetTakeProfits(Symbol, CurrentDirection.Value, takeProfit.Value, StopLossFlags.Unspecified);
            }
        }
    }

    /// <summary>
    /// Calculate stop loss level with optional buffer.
    /// </summary>
    private TValue CalculateStopLoss(PChandelierExitBot<TValue> typedParams, TValue exitLine, TValue? currentAtr)
    {
        var stopLoss = exitLine;

        // Add buffer if ATR is available
        if (typedParams.StopLossBuffer > TValue.Zero && currentAtr.HasValue && currentAtr.Value > TValue.Zero)
        {
            var buffer = currentAtr.Value * typedParams.StopLossBuffer;

            stopLoss = CurrentDirection!.Value switch
            {
                LongAndShort.Long => exitLine - buffer,  // For longs, move stop further down
                LongAndShort.Short => exitLine + buffer, // For shorts, move stop further up
                _ => exitLine
            };
        }

        return stopLoss;
    }

    /// <summary>
    /// Calculate take profit level based on ATR.
    /// </summary>
    private TValue? CalculateTakeProfit(PChandelierExitBot<TValue> typedParams, HLC<TValue> currentBar)
    {
        if (!EntryPrice.HasValue)
        {
            return null;
        }

        // Use entry ATR or current ATR
        var atrForTp = EntryAtr ?? GetCurrentAtr(typedParams);
        if (atrForTp == null || atrForTp == TValue.Zero)
        {
            return null;
        }

        var tpDistance = atrForTp.Value * typedParams.TakeProfitAtrMultiplier;

        TValue takeProfit;
        if (typedParams.TrailTakeProfit && BestPrice.HasValue)
        {
            // Trail from best price achieved
            takeProfit = CurrentDirection!.Value switch
            {
                LongAndShort.Long => BestPrice.Value + tpDistance,
                LongAndShort.Short => BestPrice.Value - tpDistance,
                _ => EntryPrice.Value + tpDistance
            };
        }
        else
        {
            // Fixed take profit from entry
            takeProfit = CurrentDirection!.Value switch
            {
                LongAndShort.Long => EntryPrice.Value + tpDistance,
                LongAndShort.Short => EntryPrice.Value - tpDistance,
                _ => EntryPrice.Value + tpDistance
            };
        }

        return takeProfit;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Get the current ATR value from the ATR indicator or estimate from Chandelier Exit.
    /// </summary>
    private TValue? GetCurrentAtr(PChandelierExitBot<TValue> typedParams)
    {
        // Use ATR indicator if available
        if (ATR != null && ATR.Size > 0)
        {
            return ATR[0];
        }

        // No ATR available - could estimate from Chandelier Exit distance, but that would require
        // access to the indicator's internal state. For now, return null.
        return null;
    }

    /// <summary>
    /// Update the best price achieved during the current position.
    /// </summary>
    private void UpdateBestPrice(HLC<TValue> currentBar)
    {
        if (!CurrentDirection.HasValue || !BestPrice.HasValue)
        {
            return;
        }

        BestPrice = CurrentDirection.Value switch
        {
            LongAndShort.Long => TValue.Max(BestPrice.Value, currentBar.High),
            LongAndShort.Short => TValue.Min(BestPrice.Value, currentBar.Low),
            _ => BestPrice.Value
        };
    }

    #endregion
}
