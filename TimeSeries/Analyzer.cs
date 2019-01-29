using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly int _paramsLength;

        public Analyzer(Vector<double>[] train, Vector<double>[] test)
        {
            if(test.Length != 0) _paramsLength = test[0].Count;

            _train = train;
            _test = test;
        }

        /// <summary>
        /// Get results for initialized train and test data
        /// </summary>
        /// <returns>result of analysis</returns>
        public Matrix<double> Analyze()
        {
            Matrix<double> d = CalculateD();
            Matrix<double> invB = CalculateB(d).PseudoInverse();
            Matrix<double> c = CalculateC(d);
            Matrix<double> w = (invB * c).NormalizeColumns(1.0d);
          
            return d * w;
        }


        private Matrix<double> CalculateD()
        {



            int len = _train.Length;
            if (len == 0) return null;

            BitArray used = new BitArray(_train.Length, false);

            List<Vector<double>> dSeries = new List<Vector<double>>(_train.Length);
            Matrix<double> matrix = Matrix<double>.Build.DenseOfRowVectors(_train);

            Vector<double> ChoseRow(int i)
            {
                used[i] = true;
                return _train[i];
            }

            bool Predicate(int t) => !used[t];

            int MedianIndex(Vector<double> vector)
            {
                int lastMin = 0;
                for (int i = 0; i < vector.Count / 2; i++)
                {
                    int min = 0;
                    for (int j = 0; i < vector.Count; i++)
                    {
                        if (vector[i] < vector[min] && vector[i] > vector[lastMin])
                            min = i;
                    }

                    min = from v in vector
                          
                    


                    lastMin = min;
                }

                //from i in Enumerable.Range(0, vector.Count / 2)
                //let min = 0
                //select from j in Enumerable.Range(0, vector.Count)
                //       let m = vector[i] < vector[min] && vector[i] > vector[lastMin] ? i : min
                //       select min
                
    



                return lastMin;
            }

            dSeries.Min();

            dSeries.AddRange(matrix.EnumerateColumns()
                .SelectMany(col => new[] {col.MinimumIndex(), MedianIndex(col), col.MaximumIndex()}, (col, i) => i)
                .Where(Predicate)
                .Select(ChoseRow));



            return Matrix.Build.DenseOfColumnVectors(dSeries1.Distinct());
        }

        private Matrix<double> CalculateB(Matrix<double> d)
        {
            return Matrix<double>.Build.Dense(
                d.ColumnCount, d.ColumnCount,
                d.EnumerateColumns().SelectMany(d1 => d.EnumerateColumns(), GetSimilarity).ToArray());
        }

        private Matrix<double> CalculateC(Matrix<double> d)
        {
            return Matrix<double>.Build.Dense(
                d.ColumnCount, _test.Length,
                d.EnumerateColumns().SelectMany(d1 => _test, GetSimilarity).ToArray()
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
