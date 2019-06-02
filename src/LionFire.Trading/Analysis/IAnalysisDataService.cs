using System.Threading;
using System.Threading.Tasks;
using LionFire.Trading.Backtesting;
using LionFire.Trading.Portfolios;
using Microsoft.Extensions.Configuration;

namespace LionFire.Trading.Analysis
{
    public interface IAnalysisDataService : IBacktestInjestService // Move IBacktestInjestService to its own service?
    {

        int BacktestResultsCountInDatabase();
        Task<int> InjestBacktests(InjestOptions options = null, CancellationToken? token = null);

        /// <summary>
        /// Load backtest results
        /// Load trades if options.Needstrades
        /// </summary>
        Task LoadPortfolio(Portfolio portfolio, PortfolioAnalysisOptions options, CancellationToken? token = null);

        object Entities { get; }


        
    }

    public interface IBacktestInjestService
    {
        bool AnyInjestsAvailable { get; }
        int InjestsAvailable { get; }
    }
}