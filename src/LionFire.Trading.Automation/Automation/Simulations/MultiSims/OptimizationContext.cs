using Durian;
using Hjson;
using LionFire.ExtensionMethods.Copying;
using LionFire.Serialization.Csv;
using LionFire.Trading.Automation.Journaling.Trades;
using LionFire.Trading.Automation.Optimization;
using LionFire.Validation;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ReactiveUI;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace LionFire.Trading.Automation;

[FriendClass(typeof(MultiSimContext))]
public sealed class OptimizationContext : IValidatable
{
    #region Dependencies

    public BacktestsRepository BacktestsRepository { get; }

    private IServiceProvider ServiceProvider => MultiSimContext.ServiceProvider;

    public BestJournalsTracker BestJournalsTracker => bestJournalsTracker ??= new(MultiSimContext);
    private BestJournalsTracker? bestJournalsTracker;

    #endregion

    #region Relationships

    // Parent
    public MultiSimContext MultiSimContext { get; }

    #endregion

    #region Parameters

    public POptimization? POptimization => MultiSimContext.Parameters.POptimization;

    public OptimizationRunInfo? OptimizationRunInfo { get; set; }

    #endregion

    #region Validation

    public ValidationContext ValidateThis(ValidationContext validationContext)
    {
        return validationContext
            .PropertyNotNull(nameof(OptimizationRunInfo), OptimizationRunInfo)
            .PropertyNotNull(nameof(BacktestsRepository), BacktestsRepository)
            .Validate(POptimization)
            ;
    }

    #endregion

    #region Lifecycle

    CompositeDisposable disposables = new();

    internal OptimizationContext(MultiSimContext multiSimContext)
    {
        MultiSimContext = multiSimContext;

        BacktestsRepository = ServiceProvider.GetRequiredService<BacktestsRepository>();



    }

    public async Task Init()
    {
        await WriteOptimizationRunInfo();
    }

    #endregion 

    #region State

    public OptimizationTask? OptimizationTask { get; set; }


    #region Cancel state


    #endregion

    #endregion

    private async Task WriteOptimizationRunInfo(
    //Func<OptimizationRunInfo> getter
    )
    {
        ArgumentNullException.ThrowIfNull(OptimizationRunInfo);

        //lock (optimizationRunInfoLock)
        //{
        //    if (optimizationRunInfo != null) return;
        //    else optimizationRunInfo = getter();
        //}

        //await Task.Yield(); // Hjson is synchronous
        var json = JsonConvert.SerializeObject(OptimizationRunInfo, new JsonSerializerSettings
        {
            DefaultValueHandling = DefaultValueHandling.Ignore,
        });

        var hjsonValue = Hjson.JsonValue.Parse(json);
        var hjson = hjsonValue.ToString(hjsonOptions);

        if (MultiSimContext.FilesystemRetryPipeline != null)
        {
            await MultiSimContext.FilesystemRetryPipeline.ExecuteAsync(async _ =>
            {
                await write().ConfigureAwait(false);
                return ValueTask.CompletedTask;
            }).ConfigureAwait(false);
        }
        else { await write().ConfigureAwait(false); }

        async ValueTask write() => await File.WriteAllBytesAsync(Path.Combine(MultiSimContext.OutputDirectory, "OptimizationRunInfo.hjson"), System.Text.Encoding.UTF8.GetBytes(hjson));

    }

    HjsonOptions hjsonOptions = new HjsonOptions() { EmitRootBraces = false };


}
