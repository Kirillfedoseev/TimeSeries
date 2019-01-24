using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace TimeSeries
{
    public class Generator
    {
        /// <summary>
        /// Generate random TimeSeries structures of length paramsLength for train and test 
        /// </summary>
        /// <param name="train">Train vector of TimeSeries, out parameter</param>
        /// <param name="test">Test vector of TimeSeries, out parameter</param>
        /// <param name="paramsLength">Amount of params in each TimeSeries</param>
        /// <param name="testSize">the size of train set</param>
        /// <param name="trainSize">the size of test set</param>     
        public static void Generate(out Vector<double>[] train, out Vector<double>[] test, int paramsLength = 3, int trainSize = 80, int testSize = 20)
        {
            train = new Vector<double>[trainSize];
            test = new Vector<double>[testSize];

            for (int i = 0; i < trainSize; i++)
                train[i] = Vector.Build.Random(paramsLength);

            for (int i = 0; i < testSize; i++)
                test[i] = Vector.Build.Random(paramsLength);

        }

    }
}
