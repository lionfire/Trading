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


        this.WhenAnyValue(x => x.MultiSimContext.Parameters.PBotType)
            .Select(BotParameterPropertiesInfo.SafeGet)
            .Subscribe(properties =>
            {
                Debug.WriteLine($"POptimization Rx bind: PBotType => parameters");

                parameters.Edit(u =>
                {
                    u.Clear();
                    if (properties != null)
                    {
                        u.AddOrUpdate(properties.PathDictionary.Values
                            .Where(info => info.IsOptimizable
                                && info.LastPropertyInfo!.PropertyType != typeof(bool) // NOTIMPLEMENTED yet
                            )
                            .Select(info =>
                            {
                                var poo = CreatePoo(info);
                                poo.PropertyChanged += (s, e) => RaiseParametersChanged();
                                return poo;
                            }));
                    }
                    this.RaisePropertyChanged(nameof(Parameters));
                });
            })
            .DisposeWith(disposables);
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

    #region Bot Parameters

    /// <summary>
    /// Initialize ParameterOptimizationOptions if needed, from various sources, in the following order:
    /// - ParameterAttribute
    /// //- EnableOptimization from this.MinParameterPriority
    /// //- POptimizationStrategy.PMultiSim
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    public static IParameterOptimizationOptions CreatePoo(HierarchicalPropertyInfo info)
    {
        //ParameterOptimizationOptions ??= new();

        //var fromPOptimization = ParameterOptimizationOptions.TryGetValue(info.Path)
        //?? ParameterOptimizationOptions?.TryGetValue(info.Key)
        ;
        //if (fromPOptimization != null)
        //{
        //    return fromPOptimization;
        //}

        IParameterOptimizationOptions parameterOptimizationOptions = LionFire.Trading.ParameterOptimizationOptions.Create(info);


        //ParameterOptimizationOptions.Create(info.ValueType, "<AttributePrototype>");

        using var _ = parameterOptimizationOptions.SuppressChangeNotifications();

        AssignFromExtensions.AssignNonDefaultPropertiesFrom(parameterOptimizationOptions!, info.ParameterAttribute);

        #region Attribute

        //IParameterOptimizationOptions fromAttribute = info.ParameterAttribute.GetParameterOptimizationOptions(info.LastPropertyInfo!.PropertyType);
        //ArgumentNullException.ThrowIfNull(fromAttribute);

        //var clone = fromAttribute.Clone();

        #endregion

        //if (!clone.EnableOptimization.HasValue)
        //{
        //    clone.EnableOptimization = info.ParameterAttribute.OptimizePriorityInt >= MinParameterPriority;
        //}

        #region POptimizationStrategy

        //IParameterOptimizationOptions? fromOptimizationParameters = POptimizationStrategy.PMultiSim.TryGetValue(info.Path);

        //// FUTURE: Clone per-strategy options somehow 
        ////clone.FitnessOfInterest ??= gridSearchStrategy.PMultiSim.FitnessOfInterest;

        //if (fromOptimizationParameters != null)
        //{
        //    AssignFromExtensions.AssignNonDefaultPropertiesFrom(clone, fromOptimizationParameters);
        //}

        #endregion

        #region ParameterOptimizationOptions

        //clone.Path = info.Path;
        //ParameterOptimizationOptions.TryAdd(info.Path, clone);

        //var fromPOptimization = ParameterOptimizationOptions?.TryGetValue(info.Path) ?? ParameterOptimizationOptions?.TryGetValue(info.Key);
        //if (fromPOptimization != null)
        //{
        //    AssignFromExtensions.AssignNonDefaultPropertiesFrom(clone, fromPOptimization);
        //}

        #endregion

        return parameterOptimizationOptions;
        //return ParameterOptimizationOptions[info.Path];
    }

    #endregion
}
