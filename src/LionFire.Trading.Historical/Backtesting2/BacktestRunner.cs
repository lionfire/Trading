using LionFire.Trading.Optimizing2;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LionFire.Trading.Backtesting2;

public class BacktestRunner
{
    public ILogger<BacktestRunner> Logger { get; }

    public BacktestRunner(ILogger<BacktestRunner> logger)
    {
        Logger = logger;
    }

    public Task Run(BacktestParameters job)
    {
        Logger.LogInformation("TODO");
        return Task.CompletedTask;
    }
}

