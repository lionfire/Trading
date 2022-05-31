#nullable enable

namespace LionFire.Trading.Algos.Modular.Signals;

/// <summary>
/// Decides what to do with trade signals.  
/// The simplest case would be to open a full size position as soon as a signal is received.
///
/// These interpreters can
/// </summary>
public interface ITradeSignalInterpreter
{
// Signal interpreter

// Simple: 
// e.g. Require multiple signals within an n bars window.
// 
}

/// <summary>
/// Execute an entry/exit immediately for a full-size trade (which may be smaller or equal to a full-size position).
/// </summary>
public class SimpleTradeSignalInterpreter
{

}

/// <summary>
/// 
/// </summary>
public class TradeSignalInterpreter
{
}

public class DesiredPositionSizeTradeSignalInterpreter
{
}

public enum FunctionType
{
    Unspecified,
    Linear,
    /// <summary>
    /// Round to 1.0 or 0.0
    /// </summary>
    Round,
    /// <summary>
    /// If above zero, translate to 1, otherwise 0.
    /// </summary>
    Nonzero,

    /// <summary>
    /// Specify a mapping Function
    /// </summary>
    Custom,
}

public class TradeSignalInterpreterParameters
{
    public FunctionType BiasFunctionType { get; set; }
    public Func<float, float>? BiasFunction { get; set; }

    /// <summary>
    /// e.g.:
    ///  - Linear: Close/open {signalstrength}% based on the strength of the signal
    /// </summary>
    public FunctionType SignalFunctionType { get; set; }
    public Func<float,float>? SignalFunction { get; set; }

    public int MinBarsBetweenEntries { get; set; }
    public int MinBarsBetweenExits { get; set; }
}