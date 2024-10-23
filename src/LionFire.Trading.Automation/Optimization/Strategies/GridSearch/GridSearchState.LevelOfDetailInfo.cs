using System.Collections;

namespace LionFire.Trading.Automation.Optimization.Strategies.GridSpaces;

public partial class GridSearchState
{
    public class GridLevelOfDetailState : IEnumerable<int[]>
    {
        #region Identity

        public int Level { get; }

        #region Parent

        public GridSearchState GridSearchState { get; }

        #endregion

        #endregion

        #region Lifecycle

        public GridLevelOfDetailState(int level, GridSearchState gridSearchState)
        {
            Level = level;
            GridSearchState = gridSearchState;
            foreach (var kvp in gridSearchState.dict)
            {
                var state = ParameterLevelOfDetailInfo.Create(level, kvp.Value.info, kvp.Value.options);
                Parameters.Add(state);
            }
        }

        #endregion

        #region State

        public List<IParameterLevelOfDetailInfo> Parameters = new();



        #region Derived

        public int TestCount => Parameters.Aggregate(1, (acc, x) => acc * x.TestCount);

        #endregion

        #endregion

        public IEnumerator<int[]> GetEnumerator()
        {
            return new OptimizationEnumerator(this);
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        private class OptimizationEnumerator : IEnumerator<int[]>
        {
            private GridLevelOfDetailState gridLevelOfDetailState;

            int[] max;

            public OptimizationEnumerator(GridLevelOfDetailState gridLevelOfDetailState)
            {
                this.gridLevelOfDetailState = gridLevelOfDetailState;
                current = new int[gridLevelOfDetailState.Parameters.Count];
                max = new int[gridLevelOfDetailState.Parameters.Count];
                int i = 0;
                foreach (var p in gridLevelOfDetailState.Parameters)
                {
                    max[i++] = p.TestCount;
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
                        return true;
                    }
                    current[i] = 0;
                }
                return false;
            }

            public void Reset() => current = new int[gridLevelOfDetailState.Parameters.Count];
        }
    }
}



