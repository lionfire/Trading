using LionFire.Applications.Trading;
using LionFire.ExtensionMethods.Validation;
using LionFire.ReactiveUI_;
using LionFire.Trading.Automation.Optimization;
using LionFire.Validation;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.AccessControl;
using YamlDotNet.Core.Tokens;

namespace LionFire.Trading.Automation;

public class PBatch : DisposableBaseViewModel
//, IValidatable
{
    //public ValidationContext ValidateThis(ValidationContext v) =>
    //    v
    //        .Validate(PMultiSim)
    //        .Validate(POptimization)
    //    ;

    //public PSimContext<TPrecision> CreatePSimContext<TPrecision>()
    //    where TPrecision : struct, INumber<TPrecision>
    //{
    //    var first = PBacktests.First();

    //    return new PSimContext<TPrecision>
    //    {
    //        DefaultExchangeArea = first.ExchangeSymbolTimeFrame,
    //        DefaultSymbol = first.DefaultSymbol,
    //        Start = first.Start,
    //        EndExclusive = first.EndExclusive,
    //    };
    //}

    #region Lifecycle

    //public static implicit operator PSimContext(PMultiSim p) => new(p);

    //[Obsolete]
    //public PMultiBacktestContext(Type pBotType, ExchangeSymbol? exchangeSymbol = null, DateTimeOffset? start = null, DateTimeOffset? endExclusive = null)
    //{
    //    if (start.HasValue)
    //    {
    //        PMultiSim.Start = start.Value;
    //    }
    //    if (endExclusive.HasValue)
    //    {
    //        PMultiSim.EndExclusive = endExclusive.Value;
    //    }

    //    PMultiSim.ExchangeSymbol = ExchangeSymbol;
    //    PMultiSim.PBotType = pBotType;
    //    POptimization = new POptimization(this);
    //}

    public PBatch(IEnumerable<PBotWrapper>? pBacktestTask2 = null)
    {
        ArgumentNullException.ThrowIfNull(pMultiSim);

        pBacktestTasks = pBacktestTask2;
        var first = pBacktestTask2.First();
        var pBotType = first.PBot!.GetType();

    }

    #endregion

    #region State


    public PMultiSim PMultiSim
    {
        get => pMultiSim;
        set => RaiseAndSetNestedViewModelIfChanged(ref pMultiSim, value);
    }
    private PMultiSim pMultiSim;

    #endregion

    #region Backtests

    public IEnumerable<PBotWrapper> PBacktests => pBacktestTasks ?? [];
    IEnumerable<PBotWrapper>? pBacktestTasks;

    #endregion

    public POptimization? POptimization => PMultiSim.POptimization;
    
    #region Derived

    #region Convenience

    public Type? PBotType => PMultiSim.PBotType;
    //public ExchangeSymbol ExchangeSymbol => PMultiSim.ExchangeSymbolTimeFrame!;  // REVIEW nullability

    #endregion

    public Type? BotType => PMultiSim.BotType;
    //public Type? BotType => botType ??= PBotType == null ? null : BotTyping.TryGetBotType(PBotType);
    //private Type? botType;

    #endregion

    public TimeFrame? TimeFrame => PMultiSim.DefaultTimeFrame;

}


