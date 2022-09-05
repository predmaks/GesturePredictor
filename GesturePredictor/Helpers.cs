using GesturePredictor.Classification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GesturePredictor
{
    internal static class Helpers
    {
        public const int NumberOfClasses = 6;
        public const int NumberOfFeatures = 90; // 40 EMG (8 x 5) and 50 IMU (10 x 5) features 

        /*public static Dictionary<string, int> GetColumns(Type t)
        {
            var _dict = new Dictionary<string, int>();

            PropertyInfo[] props = t.GetProperties();
            Console.Write(props.Count());
            foreach (PropertyInfo prop in props)
            {
                object[] attrs = prop.GetCustomAttributes(true);
                foreach (object attr in attrs)
                {
                    AuthorAttribute authAttr = attr as AuthorAttribute;
                    if (authAttr != null)
                    {
                        string propName = prop.Name;
                        //string auth = authAttr.Name;
                        int v = authAttr.Value;

                        _dict.Add(propName, v);
                    }
                }
            }

            return _dict;
        }*/

        /// <summary>
        /// https://www.mathsisfun.com/data/standard-deviation-formulas.html
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double CalculateStandardDeviation(IEnumerable<double> values)
        {
            // 1. Calculate Mean (the simple average of the numbers)
            double mean = values.Sum() / values.Count();

            // 2. For each number: subtract the Mean and square the result
            var squaredDifferences = from value in values
                                     select (value - mean) * (value - mean);

            // 3. Work out the Mean of squared differences
            double squaredDifferencesMean = squaredDifferences.Sum() / values.Count();

            // 4. Take and return the square root of the squared differences Mean value
            return Convert.ToInt32(Math.Sqrt(squaredDifferencesMean));
        }

        public static TrainingData SplitForTraining(List<FeatureTransposed> features)
        {
            // TODO: move to separate ITrainier interface and class and do better random split
            var result = new TrainingData();

            // TODO 1: improve this to do really 70:30 or 60:40 in random way!!!
            for (int i = 0; i < features.Count; i++)
            {
                if (i % 10 < 7)
                {
                    result.Training.Add(features.ElementAt(i));
                }
                else
                {
                    result.Validation.Add(features.ElementAt(i));
                }
            }

            return result;
        }
    }
}
