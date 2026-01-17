using DynamicData;
using DynamicData.Binding;
using LionFire.Hosting;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Automation.Optimization.Strategies.GridSpaces;

public class GridLevelOfDetail : ILevelOfDetail, IEnumerable<int[]>
{
    #region Identity

    public int Level { get; }

    #region Parent

    [JsonIgnore]
    public OptimizerLevelsOfDetail OptimizerLevelsOfDetail { get; }

#if UNUSED
        [JsonIgnore]
        public LevelOfDetail ParentLevel => Level == 0 ? this : State.GetLevel(Math.Sign(Level) * (Math.Abs(Level) - 1));
#endif

    #endregion

    #endregion

    #region Lifecycle

    public GridLevelOfDetail(int level, OptimizerLevelsOfDetail optimizerLevelsOfDetail)
    {
        Level = level;
        OptimizerLevelsOfDetail = optimizerLevelsOfDetail;
        //foreach (var options in OptimizerLevelsOfDetail.POptimization.OptimizableParameters.KeyValues.Values)
        ////foreach ((var info, var options) in OptimizerLevelsOfDetail.AllParameters
        ////.Where(p => optimizerLevelsOfDetail.POptimization.EffectiveEnableOptimization(p.options))
        ////)
        //{
        //    var state = ParameterLevelOfDetailInfo.Create(level, options.Info, options);
        //    ParametersList.Add(state);
        //}

        parameters = new List<IParameterLevelOfDetailInfo>(OptimizerLevelsOfDetail.Optimizable.ToArray().Select(options => ParameterLevelOfDetailInfo.Create(level, options.Info, options)));


        //Task.Run(() =>
        //{
        //    OptimizerLevelsOfDetail.POptimization.OptimizableParameters
        //  .ToObservableChangeSet<IObservableCollection<IParameterOptimizationOptions>, IParameterOptimizationOptions>()
        //  //.Filter(options => options.IsEligibleForOptimization)
        //  .Transform(options => ParameterLevelOfDetailInfo.Create(level, options.Info, options))
        //  .Bind(out parameters)
        //  .Subscribe()
        //  .DisposeWith(disposables);
        //})
        //    //.Wait()
        //    ;
    }

    //CompositeDisposable disposables = new();

    public void Dispose()
    {
        //disposables?.Dispose();
    }

    #endregion

    #region State

    //public ReadOnlyObservableCollection<IParameterLevelOfDetailInfo> PMultiSim => parameters;
    //private ReadOnlyObservableCollection<IParameterLevelOfDetailInfo> parameters;
    public IReadOnlyList<IParameterLevelOfDetailInfo> Parameters => parameters;
    private List<IParameterLevelOfDetailInfo> parameters;

    #region Derived

    public double TestPermutationCount => (Parameters == null || !Parameters.Any()) ? 0.0 : Parameters.Aggregate(1.0, (acc, x) => acc * x.TestCount);

    #endregion

    #endregion

    #region IEnumerable<int[]>

    public IEnumerator<int[]> GetEnumerator()
    {
        return new StepEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    //IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private class StepEnumerator : IEnumerator<int[]>
    {
        private GridLevelOfDetail gridLevelOfDetailState;

        int[] max;

        public StepEnumerator(GridLevelOfDetail gridLevelOfDetailState)
        {
            this.gridLevelOfDetailState = gridLevelOfDetailState;

            current = new int[gridLevelOfDetailState.Parameters.Count];
            max = new int[gridLevelOfDetailState.Parameters.Count];

            //for(int d = 0; d < gridLevelOfDetailState.PMultiSim.Count; d++) { 
            //}
            int i = 0;
            foreach (var p in gridLevelOfDetailState.Parameters)
            {
                current[i] = i == 0 ? -1 : 0;
                max[i++] = (int)p.TestCount - 1;
            }
        }

        // Return a copy to prevent mutation issues when consumers store the array
        public int[] Current => (int[])current.Clone();
        int[] current;
        object IEnumerator.Current => (int[])current.Clone();


        public void Dispose() { current = null!; max = null!; }

        public bool MoveNext()
        {
            for (int i = 0; i < current.Length; i++)
            {
                if (current[i] < max[i])
                {
                    current[i]++;
                    //Debug.WriteLine("Iterator: " + string.Join(", ", current));
                    return true;
                }
                current[i] = 0;
            }
            return false;
        }

        public void Reset() => current = new int[gridLevelOfDetailState.Parameters.Count];
    }

    #endregion
}


