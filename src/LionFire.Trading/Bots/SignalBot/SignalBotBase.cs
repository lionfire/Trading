#if DEBUG
#define NULLCHECKS
#define TRACE_RISK
#define TRACE_CLOSE
#define TRACE_OPEN
#define TRACE_EVALUATE
#endif
#if cAlgo
using cAlgo.API;
using cAlgo.API.Internals;
#else 

#endif
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using LionFire.Trading.Indicators;
using LionFire.Extensions.Logging;
using LionFire.Trading;
using System.IO;
using Newtonsoft.Json;
using LionFire.Trading.Backtesting;

namespace LionFire.Trading.Bots
{

    public partial class SignalBotBase<TIndicator, TConfig, TIndicatorConfig> : BotBase<TConfig>, ISignalBot
    where TIndicator : class, ISignalIndicator, new()
    where TConfig : TSignalBot<TIndicatorConfig>, new()
        where TIndicatorConfig : class, ITIndicator, new()
    {
        public ISignalIndicator Indicator { get; protected set; }

        protected virtual void OnEvaluated()
        {
            Evaluated?.Invoke();
        }

        public event Action Evaluated;
    }


  }
