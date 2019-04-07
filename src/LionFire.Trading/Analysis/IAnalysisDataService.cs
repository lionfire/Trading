using System.Threading;
using System.Threading.Tasks;
using LionFire.Trading.Backtesting;
using LionFire.Trading.Portfolios;
using Microsoft.Extensions.Configuration;

namespace LionFire.Trading.Analysis
{
    public interface IAnalysisDataService
    {

        int BacktestResultsCountInDatabase();
        Task<int> InjestBacktests(InjestOptions options = null, CancellationToken? token = null);
        Task LoadPortfolio(Portfolio portfolio, PortfolioAnalysisOptions options, CancellationToken? token = null);
        object Entities { get; }
    }
}