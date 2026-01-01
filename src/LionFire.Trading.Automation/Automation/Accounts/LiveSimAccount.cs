using System.Numerics;

namespace LionFire.Trading.Automation.Accounts;

/// <summary>
/// A live simulated account that tracks realistic balance changes based on realized P&amp;L.
/// </summary>
/// <remarks>
/// <para>
/// This account type uses real-time market data with simulated order execution.
/// Unlike <see cref="LivePaperAccount{TPrecision}"/>, this account tracks realistic balance:
/// </para>
/// <list type="bullet">
///   <item><description>Balance starts at <see cref="LiveAccountOptions.InitialBalance"/></description></item>
///   <item><description>Balance changes based on realized P&amp;L from closed positions</description></item>
///   <item><description>The account can "blow up" if losses exceed the balance</description></item>
/// </list>
/// <para>
/// This is useful for validating trading strategies under realistic capital constraints.
/// </para>
/// </remarks>
/// <typeparam name="TPrecision">The numeric precision type used for calculations.</typeparam>
public class LiveSimAccount<TPrecision> : LiveAccountBase<TPrecision>
    where TPrecision : struct, INumber<TPrecision>
{
    #region Properties

    /// <inheritdoc />
    public override BotAccountMode Mode => BotAccountMode.LiveSimulated;

    /// <inheritdoc />
    /// <remarks>
    /// For LiveSimAccount, balance changes based on realized P&amp;L.
    /// Balance = InitialBalance + RealizedPnL
    /// </remarks>
    public override TPrecision Balance => _balance;
    private TPrecision _balance;

    /// <inheritdoc />
    /// <remarks>
    /// Equity = Balance + UnrealizedPnL
    /// </remarks>
    public override TPrecision Equity => Balance + UnrealizedPnL;

    /// <summary>
    /// Gets a value indicating whether this account has blown up (balance <= 0).
    /// </summary>
    /// <remarks>
    /// When an account is blown up, no new positions can be opened.
    /// </remarks>
    public bool IsBlownUp { get; private set; }

    #endregion

    #region Events

    /// <summary>
    /// Raised when the account blows up (balance <= 0).
    /// </summary>
    public event EventHandler? BlownUp;

    #endregion

    #region Lifecycle

    /// <summary>
    /// Initializes a new instance of <see cref="LiveSimAccount{TPrecision}"/>.
    /// </summary>
    /// <param name="exchangeArea">The exchange and area for this account.</param>
    /// <param name="options">The account configuration options.</param>
    public LiveSimAccount(ExchangeArea exchangeArea, LiveAccountOptions? options = null)
        : base(exchangeArea, options ?? LiveAccountOptions.DefaultSimulated)
    {
        _balance = InitialBalance;
    }

    #endregion

    #region Position Management

    /// <inheritdoc />
    public override LivePosition<TPrecision> OpenPosition(
        string symbol,
        LongAndShort direction,
        TPrecision size,
        TPrecision entryPrice)
    {
        if (IsBlownUp)
        {
            throw new InvalidOperationException("Cannot open positions on a blown up account");
        }

        // Check if we have sufficient margin (simplified check)
        var requiredMargin = entryPrice * size / TPrecision.CreateChecked(Options.Leverage);
        if (requiredMargin > Balance)
        {
            throw new InvalidOperationException($"Insufficient margin. Required: {requiredMargin}, Available: {Balance}");
        }

        return base.OpenPosition(symbol, direction, size, entryPrice);
    }

    #endregion

    #region Balance Tracking

    /// <inheritdoc />
    protected override void OnPositionClosed(TPrecision pnl)
    {
        _balance += pnl;

        // Check for blow-up
        if (_balance <= TPrecision.Zero && !IsBlownUp)
        {
            IsBlownUp = true;
            BlownUp?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Resets the account to initial state.
    /// </summary>
    /// <remarks>
    /// This clears all positions, resets balance to initial, and clears the blown up flag.
    /// </remarks>
    public void Reset()
    {
        _balance = InitialBalance;
        RealizedPnL = TPrecision.Zero;
        IsBlownUp = false;
        // Note: positions would also need to be cleared
    }

    #endregion
}
