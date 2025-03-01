using LionFire.ExtensionMethods.Collections;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using System.Threading;

namespace LionFire.Trading.Automation.Optimization;

public class BotOptimizationStatusRepository
{
    public string BacktestsDir { get; set; }
    public IOptimizationRepository OptimizationRepository { get; }

    public BotOptimizationStatusRepository(IOptionsMonitor<BacktestOptions> backtestOptions, IOptimizationRepository optimizationRepository)
    {
        BacktestsDir = backtestOptions.CurrentValue.Dir;
        OptimizationRepository = optimizationRepository;
    }


    public Task<IEnumerable<BotOptimizationStatusItem>> GetItemsAsync(bool refresh = false, CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            var result = new List<BotOptimizationStatusItem>();

            foreach (var directory in Directory.GetDirectories(BacktestsDir))
            {
                var optRunDirs = Directory.GetDirectories(directory);

                var bot = Path.GetFileName(directory);

                var runs = await OptimizationRepository.GetRuns(bot);

                var item = new BotOptimizationStatusItem
                {
                    Bot = bot,
                    OptimizationRunCount = optRunDirs.Count(),
                };

                var statsTask = Task.Run(async () =>
                {
                    List<OptimizationRunStats> statsList = new();

                    foreach (var batch in runs.Batch(3))
                    {
                        await Task.WhenAll(batch.Select(async run =>
                        {
                            var stats = await OptimizationRepository.GetStats(run.Id, cancellationToken: cancellationToken);
                            if (stats != null)
                            {
                                statsList.Add(stats);
                            } else
                            {
                                throw new Exception("Failed to get stats");
                            }
                        }));
                    }
                    item.StatsList = statsList;
                });

                var dataPath = Path.Combine(directory, typeof(BotOptimizationStatusData).Name + ".json");
                if (File.Exists(dataPath))
                {
                    var data = JsonConvert.DeserializeObject<BotOptimizationStatusData>(await File.ReadAllTextAsync(dataPath));
                    item.Data = data;
                }

                await statsTask;

                result.Add(item);
            }
            return (IEnumerable<BotOptimizationStatusItem>)result;
        });
    }
}

