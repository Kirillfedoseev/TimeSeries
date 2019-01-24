using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace TimeSeries
{
    public class Analyzer
    {
        private Vector<double>[] _train;
        private Vector<double>[] _test;
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
            if(_train.Length == 0) return null;

            int[] min = new int[_paramsLength];
            int[] mid = new int[_paramsLength];
            int[] max = new int[_paramsLength];


            for (int i = 0; i < _paramsLength; i++)
            {
                //min[i] = 0;
                //mid[i] = 0;
                //max[i] = 0;

                for (int j = 1; j < _train.Length; j++)
                {
                    if (_train[min[i]][i] > _train[j][i]) min[i] = j;
                    if (_train[max[i]][i] < _train[j][i]) max[i] = j;
                }

                double median = (_train[min[i]][i] + _train[max[i]][i]) / 2; //arithmetic median

                for (int j = 0; j < _train.Length; j++)
                {
                    if (Math.Abs(median - _train[j][i]) < Math.Abs(median - _train[mid[i]][i]))
                        mid[i] = j; //index of real median
                }
            }

            List<int> indexes = new List<int>(_train.Length);
            for (int i = 0; i < _paramsLength; i++)
            {
                if (!indexes.Contains(min[i])) indexes.Add(min[i]);
                if (!indexes.Contains(mid[i])) indexes.Add(mid[i]);
                if (!indexes.Contains(max[i])) indexes.Add(max[i]);
            }
            Vector<double>[] dSeries = new Vector<double>[indexes.Count];
            for (int i = 0; i < dSeries.Length; i++)
                dSeries[i] = _train[indexes[i]];

            return Matrix.Build.DenseOfColumnVectors(dSeries);
        }

        private Matrix<double> CalculateB(Matrix<double> d)
        {
            double[,] core = new double[d.ColumnCount, d.ColumnCount];
            for (int i = 0; i < d.ColumnCount; i++)
            {
                for (int j = 0; j < d.ColumnCount; j++)
                    core[i, j] = GetSimilarity(d.Column(i), d.Column(j));
            }            
            return Matrix.Build.DenseOfArray(core);
        }

        private Matrix<double> CalculateC(Matrix<double> d)
        {
            double[,] cMatrix = new double[d.ColumnCount, _test.Length];

            for (int i = 0; i < d.ColumnCount; i++)
            {
                for (int j = 0; j < _test.Length; j++)
                    cMatrix[i, j] = GetSimilarity(d.Column(i), _test[j]);
            }

            return Matrix.Build.DenseOfArray(cMatrix);
        }

        private double GetSimilarity(Vector<double> a, Vector<double> b)
        {

            double dist = (a-b).PointwisePower(2).Sum(); //calculate distance between vectors

            //sum >=0, due to squares
            double res = Math.Pow(Math.E, -dist); // get coef of difference, exp needs for function domain (0,1]
            return res;
        }
    }
}
