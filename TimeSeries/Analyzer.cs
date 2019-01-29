﻿using System;
using System.Collections;
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

            //bit array for marking used columns
            BitArray used = new BitArray(_train.Length, false);
                     
            //select indexes of min, mid, max rows in that column
            IEnumerable<int> Selector(Vector<double> col)
            {
                var components = col.EnumerateIndexed().OrderBy(n => n.Item2).ToArray();
                int len = components.Length;
                return new[] {components[0].Item1, components[(len - 1) / 2].Item1, components[len - 1].Item1};
            }

            // Function on results of SelectMany operation (use for making 2d array of results to 1d array)
            int ResultSelector(Vector<double> col, int i) => i;
          
            // predicate for not used rows
            bool Predicate(int t) => !used[t];

            // get row with marking as used
            Vector<double> ChoseRow(int i)
            {
                used[i] = true;
                return _train[i];
            }

            var dVectors = 
                Matrix<double>.Build.DenseOfRowVectors(_train) //make array of vectors to matrix
                .EnumerateColumns() //get columns array
                .SelectMany(Selector, ResultSelector) //select indexes of rows, which contain min, mid, max in any column
                .Where(Predicate) // check for row have been chosen or not
                .Select(ChoseRow);  // final selection of rows and mark this row as used

            return Matrix.Build.DenseOfColumnVectors(dVectors);
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
