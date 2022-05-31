namespace LionFire.Trading.Optimizing2
{
    
    public class ParameterRange<T>
    {
        public T Low { get; set; }
        public T High { get; set; }

        public T Step { get; set; }

        /// <summary>
        /// Starting point for Stepping.  If unset, Low is used.
        /// </summary>
        public T Anchor { get; set; }

        public Func<T, T, T> StepFactor = (x,y) => throw new NotImplementedException();

    }

    public delegate T StepFactor<T>(T step);
}
