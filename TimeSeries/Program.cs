using System;
using MathNet.Numerics.LinearAlgebra;

namespace TimeSeries
{
    class Program
    {
        static void Main(string[] args)
        {
            Generator.Generate(out var train, out var test, trainSize:8, testSize:2);

            Analyzer analyzer = new Analyzer(train, test);

            Matrix<double> result = analyzer.Analyze();

            Console.WriteLine("Average coefficient of similarity:\n" +  result);
            Console.Read();

        }


    }
}
