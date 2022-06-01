namespace GesturePredictor.FeatureProcessing
{
    public class FeatureProcessor : IFeatureProcessor
    {
        private Dictionary<FeatureTypes, Func<double[], double>> featureTypeFuncMappings;

        public FeatureProcessor()
        {
            PopulateFeatureTypeFuncMappings();
        }

        public IEnumerable<FeatureRecord> ExtractFeatures(IEnumerable<RawDataSnapshot> input, List<FeatureTypes> featureTypes)
        {
            var normalizedDataGrouped = input.GroupBy(
                emg => emg.GestureId, (key, g) => new { GestureId = key, Records = g.ToList() });

            var features = new List<FeatureRecord>();

            foreach (var gestureGroup in normalizedDataGrouped)
            {
                foreach (var funcMapping in featureTypeFuncMappings.Where(m => featureTypes.Contains(m.Key)))
                {
                    var feature = new FeatureRecord
                    {
                        FeatureType = funcMapping.Key,
                        GestureId = gestureGroup.GestureId
                    };

                    var sensorValuesCount = gestureGroup.Records.ElementAt(0).SensorValues.Count();

                    feature.SensorValues = new List<double>();

                    for (int i = 0; i < sensorValuesCount; i++)
                    {
                        var inputValues = gestureGroup.Records.Select(r => r.SensorValues.ElementAt(i)).ToArray();
                        feature.SensorValues.Add(funcMapping.Value(inputValues));
                    }

                    features.Add(feature);
                }
            }

            return features;
        }

        public IEnumerable<RawDataSnapshot> MergeFeatures()
        {
            throw new NotImplementedException();
        }

        private void PopulateFeatureTypeFuncMappings()
        {
            featureTypeFuncMappings = new Dictionary<FeatureTypes, Func<double[], double>>
            {
                { FeatureTypes.MeanAbsoluteValue, CalculateMeanValue },
                { FeatureTypes.WaveformLength, CalculateWaveformLength },
                { FeatureTypes.RootMeanSquare, CalculateRootMeanSquare },
                { FeatureTypes.Variance, CalculateVariance },
                { FeatureTypes.SlopeSignChange, CalculateSlopeSignChange }
            };
        }

        public double CalculateMeanValue(double[] emgSensorData)
        {
            return emgSensorData.Average();
        }

        public double CalculateWaveformLength(double[] emgSensorData)
        {
            double result = 0;

            for (int i = 0; i < emgSensorData.Length - 1; i++)
            {
                result += Math.Abs(emgSensorData[i + 1] - emgSensorData[i]);
            }

            return result;
        }

        public double CalculateRootMeanSquare(double[] emgSensorData)
        {
            double squaresSum = GetSquaresSum(emgSensorData);

            return Math.Sqrt(squaresSum / emgSensorData.Count());
        }

        public double CalculateVariance(double[] emgSensorData)
        {
            double squaresSum = GetSquaresSum(emgSensorData);

            return squaresSum / (emgSensorData.Count() - 1);
        }

        public double CalculateSlopeSignChange(double[] emgSensorData)
        {
            double result = 0;

            for (int i = 2; i < emgSensorData.Length - 2; i++)
            {
                if ((emgSensorData[i] < emgSensorData[i - 1] && emgSensorData[i] < emgSensorData[i + 1]) ||
                    (emgSensorData[i] > emgSensorData[i - 1] && emgSensorData[i] > emgSensorData[i + 1]))
                {
                    result += 1;
                }
            }

            return result;
        }

        private double GetSquaresSum(double[] input)
        {
            double result = 0;

            for (int i = 0; i < input.Length - 1; i++)
            {
                result += Math.Pow(input[i], 2);
            }

            return result;
        }
    }
}
