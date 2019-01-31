using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Statistics;

namespace TimeSeries
{
    public class Analyzer
    {
        private readonly Vector<double>[] _train;
        private readonly Vector<double>[] _test;

        public Analyzer(Vector<double>[] train, Vector<double>[] test)
        {
            if (train == null || test == null) throw new Exception("Null input data!");
            _train = train;
            _test = test;
        }

        /// <summary>
        /// Get results for initialized train and test data
        /// </summary>
        /// <returns>result of analysis</returns>
        public Matrix<double> Analyze()
        {
            if (_train.Length == 0 || _test.Length == 0) return Matrix.Build.DenseIdentity(0);

            Matrix<double> d = CalculateD();
            Matrix<double> invB = CalculateB(d).PseudoInverse();
            Matrix<double> c = CalculateC(d);
            Matrix<double> w = (invB * c).NormalizeColumns(1.0d);
          
            return d * w;
        }


        private Matrix<double> CalculateD()
        {
            if (_train.Length == 0) return null;

            // get matrix with 5-"good" matrix for each dimensional of vector
            var dArrays =
                Matrix<double>.Build.DenseOfRowVectors(_train)
                    .EnumerateColumns()
                    .Select(col => col.FiveNumberSummary());

            return Matrix.Build.DenseOfRowArrays(dArrays);
        }

        private Matrix<double> CalculateB(Matrix<double> d)
        {
            return Matrix<double>.Build.Dense(
                d.ColumnCount, d.ColumnCount,
                d.EnumerateColumns()
                    .SelectMany(d1 => d.EnumerateColumns(), GetSimilarity) // GetSimilarity on CrossProduct of d and d
                    .ToArray());
        }

        private Matrix<double> CalculateC(Matrix<double> d)
        {
            return Matrix<double>.Build.Dense(
                d.ColumnCount, _test.Length,
                d.EnumerateColumns()
                    .SelectMany(d1 => _test, GetSimilarity) // GetSimilarity on CrossProduct of d and _test
                    .ToArray()
                );

        }

        private double GetSimilarity(Vector<double> a, Vector<double> b)
        {
            double dist = (a-b).PointwisePower(2).Sum(); //calculate distance between vectors
            double res = Math.Pow(Math.E, -dist); //sum >=0, due to squares; get coef of difference, exp needs for function domain (0,1]
            return res;
        }
    }
}
