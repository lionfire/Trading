//#define ConcurrentInjest
//using BlazorComponents.ChartJS;
using System;

namespace LionFire.Trading.Analysis
{
    [Serializable]
    public class AnalysisException : Exception
    {
        public AnalysisException() { }
        public AnalysisException(string message) : base(message) { }
        public AnalysisException(string message, Exception inner) : base(message, inner) { }
    }
}
