using LionFire.Trading.Indicators.QuantConnect_;
using LionFire.Trading.Indicators;
using LionFire.Trading.ValueWindows;
using LionFire.Trading.DataFlow;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace LionFire.Trading.Automation.Bots;

/// <summary>
/// ATR Moving Average Crossover Bot - Test bot for verifying derived indicator signal wiring.
/// Buy when ATR > ATR_MA (volatility expanding)
/// Close when ATR &lt;= ATR_MA (volatility contracting)
/// </summary>
public class PAtrMaBot<TValue> : PStandardBot2<PAtrMaBot<TValue>, TValue>
    , IPBot2Static
    where TValue : struct, INumber<TValue>
{
    #region Static

    [JsonIgnore]
    public override Type MaterializedType => typeof(AtrMaBot<TValue>);
    public static Type StaticMaterializedType => typeof(AtrMaBot<TValue>);

    #endregion

    #region Indicator Parameters

    /// <summary>
    /// ATR indicator - measures volatility
    /// </summary>
    [PSignal]
    public PAverageTrueRange<double, TValue>? ATR { get; set; }

    /// <summary>
    /// Simple Moving Average of ATR - smooths the volatility measure
    /// This is a derived indicator that uses ATR output as its input
    /// </summary>
    [PSignal]
    public PSimpleMovingAverage<double>? ATR_MA { get; set; }

    // TODO: Specify that ATR_MA uses ATR as its input source (SlotSource mechanism not fully implemented yet)
    // [JournalIgnore]
    // public SlotSource ATR_MASource { get; set; }

    #endregion

    [JsonIgnore]
    const int Lookback = 1;

    #region Lifecycle

    public PAtrMaBot() { }

    public PAtrMaBot(ExchangeSymbolTimeFrame exchangeSymbolTimeFrame, int atrPeriod = 14, int maPeriod = 20)
        : base(exchangeSymbolTimeFrame)
    {
        ATR = new PAverageTrueRange<double, TValue>
        {
            Period = atrPeriod,
            Lookback = Lookback,
            MovingAverageType = QuantConnect.Indicators.MovingAverageType.Wilders,
        };

        ATR_MA = new PSimpleMovingAverage<double>
        {
            Period = maPeriod,
            Lookback = Lookback,
        };

        Init();
    }

    protected override void InferMissingParameters()
    {
        InputLookbacks = [
            0,  // Bars
            Lookback, // ATR
            Lookback  // ATR_MA
        ];

        if (ATR != null) ATR.Lookback = Lookback;
        if (ATR_MA != null) ATR_MA.Lookback = Lookback;

        base.InferMissingParameters();
    }

    #endregion
}

/// <summary>
/// ATR Moving Average Crossover Bot implementation.
/// This bot tests derived indicator wiring by using ATR and a moving average of ATR.
/// </summary>
[Bot(Direction = BotDirection.Unidirectional)]
public class AtrMaBot<TValue> : StandardBot2<PAtrMaBot<TValue>, TValue>
    where TValue : struct, INumber<TValue>
{
    public static Type ParametersType => typeof(PAtrMaBot<TValue>);

    #region Inputs

    /// <summary>
    /// ATR values window
    /// </summary>
    [Signal(0)]
    public IReadOnlyValuesWindow<TValue> ATR { get; set; } = null!;

    /// <summary>
    /// Moving Average of ATR values window
    /// </summary>
    [Signal(1)]
    public IReadOnlyValuesWindow<double> ATR_MA { get; set; } = null!;

    #endregion

    ILogger Logger { get; }

    public AtrMaBot(ILogger<AtrMaBot<TValue>> logger)
    {
        Logger = logger;
    }

    #region Event Handling

    public override void OnBar()
    {
        // Trace logging to verify indicator values on every bar
        var atrValue = ATR?.Size > 0 ? ATR[0].ToString() : "N/A";
        var atrMaValue = ATR_MA?.Size > 0 ? ATR_MA[0].ToString("F6") : "N/A";

        Logger.LogTrace("[AtrMaBot] OnBar - ATR={ATR} (Size={ATRSize}), ATR_MA={ATR_MA} (Size={ATR_MASize})",
            atrValue, ATR?.Size ?? 0, atrMaValue, ATR_MA?.Size ?? 0);

        // Need at least 1 value from each indicator
        if (ATR == null || ATR_MA == null || ATR.Size < 1 || ATR_MA.Size < 1)
        {
            Logger.LogTrace("[AtrMaBot] No action: waiting for indicators - ATR ready: {ATRReady}, ATR_MA ready: {ATR_MAReady}",
                ATR?.Size > 0, ATR_MA?.Size > 0);
            return;
        }

        var atr = double.CreateChecked(ATR[0]);
        var atrMa = ATR_MA[0];

        // Crossover logic:
        // ATR > ATR_MA: Volatility expanding - potential breakout, go long
        // ATR <= ATR_MA: Volatility contracting - exit
        var hasPosition = DoubleAccount.Positions.Count > 0;
        if (atr > atrMa)
        {
            if (!hasPosition)
            {
                Logger.LogInformation("[AtrMaBot] BUY signal - ATR ({ATR:F6}) > ATR_MA ({ATR_MA:F6})", atr, atrMa);
                try
                {
                    TryOpen();
                    Logger.LogInformation("[AtrMaBot] Position opened. Positions count: {Count}, Balance: {Balance:F2}",
                        DoubleAccount.Positions.Count, DoubleAccount.Balance);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "[AtrMaBot] Failed to open position");
                }
            }
            else
            {
                Logger.LogTrace("[AtrMaBot] No action: ATR expanding ({ATR:F6} > {ATR_MA:F6}) but already in position",
                    atr, atrMa);
            }
        }
        else // ATR <= ATR_MA
        {
            if (hasPosition)
            {
                Logger.LogInformation("[AtrMaBot] CLOSE signal - ATR ({ATR:F6}) <= ATR_MA ({ATR_MA:F6})", atr, atrMa);
                try
                {
                    TryClose();
                    Logger.LogInformation("[AtrMaBot] Position closed. Positions count: {Count}, Balance: {Balance:F2}",
                        DoubleAccount.Positions.Count, DoubleAccount.Balance);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "[AtrMaBot] Failed to close position");
                }
            }
            else
            {
                Logger.LogTrace("[AtrMaBot] No action: ATR contracting ({ATR:F6} <= {ATR_MA:F6}) and no position to close",
                    atr, atrMa);
            }
        }
    }

    #endregion
}
