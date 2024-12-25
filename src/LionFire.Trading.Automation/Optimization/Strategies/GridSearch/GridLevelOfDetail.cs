using System.Collections;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace LionFire.Trading.Automation.Optimization.Strategies.GridSpaces;

public class GridLevelOfDetail : ILevelOfDetail
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

        foreach ((var info, var options) in OptimizerLevelsOfDetail.AllParameters.Where(p => p.options.IsEligibleForOptimization && (p.options.EnableOptimization != false)))
        {
            var state = ParameterLevelOfDetailInfo.Create(level, info, options);
            Parameters.Add(state);
        }
    }

    #endregion

    #region State

    public List<IParameterLevelOfDetailInfo> Parameters { get; set; } = new();

    #region Derived

    public double TestPermutationCount => Parameters.Aggregate(1.0, (acc, x) => acc * x.TestCount);

    #endregion

    #endregion

    #region IEnumerable<int[]>

    public IEnumerator<int[]> GetEnumerator()
    {
        return new StepEnumerator(this);
    }
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
            int i = 0;
            foreach (var p in gridLevelOfDetailState.Parameters)
            {
                Debug.WriteLine($"Parameter: {gridLevelOfDetailState.OptimizerLevelsOfDetail.OptimizableParameters[i].info.Key}  TestCount: {p.TestCount}");
                max[i++] = (int)p.TestCount;
            }
        }

        public int[] Current => current;
        int[] current;
        object IEnumerator.Current => current;


        public void Dispose() { current = null!; max = null!; }

        public bool MoveNext()
        {
            for (int i = 0; i < current.Length; i++)
            {
                if (current[i] < max[i] - 1)
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


