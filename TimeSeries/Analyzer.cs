using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

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

            List<Vector<double>> dSeries = new List<Vector<double>>(3 * _paramsLength);

            for (int i = 0; i < _paramsLength; i++)
            {
                var res = _train.OrderBy(n => n[i]).ToList();

                dSeries.Add(res[0]); //min
                dSeries.Add(res[(len - 1) / 2]); //mid
                dSeries.Add(res[len - 1]); //max
            }      

            return Matrix.Build.DenseOfColumnVectors(dSeries.Distinct());
        }

        private Matrix<double> CalculateB(Matrix<double> d)
        {
            Matrix<double> core  = Matrix<double>.Build.Dense(d.ColumnCount, d.ColumnCount);

            for (int i = 0; i < d.ColumnCount; i++)
            {
                for (int j = 0; j < d.ColumnCount; j++)
                    core[i, j] = GetSimilarity(d.Column(i), d.Column(j));
            } 
            
            return core;
        }

        private Matrix<double> CalculateC(Matrix<double> d)
        {
            Matrix<double> cMatrix = Matrix<double>.Build.Dense(d.ColumnCount, _test.Length);

            for (int i = 0; i < d.ColumnCount; i++)
            {
                for (int j = 0; j < _test.Length; j++)
                    cMatrix[i, j] = GetSimilarity(d.Column(i), _test[j]);
            }

            return cMatrix;
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
