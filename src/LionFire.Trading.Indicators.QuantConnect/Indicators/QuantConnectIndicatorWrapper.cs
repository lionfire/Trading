using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LionFire.Trading;
using QuantConnect.Data.Market;

namespace LionFire.Trading.Indicators.QuantConnect_;

public abstract class QuantConnectIndicatorWrapper<TConcrete, TQuantConnectIndicator, TParameters, TInput, TOutput> : SingleInputIndicatorBase<TConcrete, TParameters, TInput, TOutput>
    where TConcrete : IndicatorBase<TConcrete, TParameters, TInput, TOutput>, IIndicator<TConcrete, TParameters, TInput, TOutput>
{
    #region State

    public TQuantConnectIndicator WrappedIndicator { get; protected set; }

    #endregion


    public DateTime LastEndTime { get; protected set; }
}

