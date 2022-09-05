namespace GesturePredictor.FeatureProcessing
{
    public class FeatureProcessor : IFeatureProcessor
    {
        private Dictionary<FeatureTypes, Func<double[], double>> featureTypeFuncMappings;
        private Dictionary<string, int> labels;

        public FeatureProcessor()
        {
            PopulateFeatureTypeFuncMappings();
        }

        public IEnumerable<FeatureRecord> ExtractFeatures(IEnumerable<GestureDataSnapshot> input, List<FeatureTypes> featureTypes)
        {
            var normalizedDataGrouped = input.GroupBy(
                emg => (emg.GestureId, emg.GestureValue), (key, g) => new { Gesture = key, Records = g.ToList() });

            var features = new List<FeatureRecord>();

            foreach (var gestureGroup in normalizedDataGrouped)
            {
                foreach (var funcMapping in featureTypeFuncMappings.Where(m => featureTypes.Contains(m.Key)))
                {
                    var feature = new FeatureRecord
                    {
                        FeatureType = funcMapping.Key,
                        GestureId = gestureGroup.Gesture.GestureId,
                        GestureValue = gestureGroup.Gesture.GestureValue
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

            // TODO: initialize constructor with dataset, and move this to constructor
            var gestureValues = input.Select(l => l.GestureValue).Distinct().ToList();
            labels = EncodeCategoricalLabels(gestureValues);

            return features;
        }

        public IEnumerable<FeatureTransposed> MergeFeatures(IEnumerable<FeatureRecord> input1, IEnumerable<FeatureRecord> input2)
        {
            var features = from emg in input1
                           join imu in input2
                           on new { emg.GestureId, emg.FeatureType } equals new { imu.GestureId, imu.FeatureType } //into details
                           //from d in details
                           select new
                           {
                               emg.GestureId,
                               GestureValue = emg.GestureValue,
                               emg.FeatureType,
                               EmgVector = emg.SensorValues,
                               ImuVector = imu.SensorValues
                           };

            var groups = features.GroupBy(f => new { f.GestureId, f.GestureValue }, (key, g) => new { KeyPair = key, Records = g });

            var result = new List<FeatureTransposed>();

            foreach (var featureGroup in groups)
            {
                var mavFeature = featureGroup.Records.SingleOrDefault(r => r.FeatureType == FeatureTypes.MeanAbsoluteValue);
                var wlFeature = featureGroup.Records.SingleOrDefault(r => r.FeatureType == FeatureTypes.WaveformLength);
                var rmsFeature = featureGroup.Records.SingleOrDefault(r => r.FeatureType == FeatureTypes.RootMeanSquare);
                var varFeature = featureGroup.Records.SingleOrDefault(r => r.FeatureType == FeatureTypes.Variance);
                var sscFeature = featureGroup.Records.SingleOrDefault(r => r.FeatureType == FeatureTypes.SlopeSignChange);

                var featureVector = (mavFeature?.EmgVector ?? Enumerable.Empty<double>())
                    .Concat(wlFeature?.EmgVector ?? Enumerable.Empty<double>())
                    .Concat(rmsFeature?.EmgVector ?? Enumerable.Empty<double>())
                    .Concat(varFeature?.EmgVector ?? Enumerable.Empty<double>())
                    .Concat(sscFeature?.EmgVector ?? Enumerable.Empty<double>())
                    .Concat(mavFeature?.ImuVector ?? Enumerable.Empty<double>())
                    .Concat(wlFeature?.ImuVector ?? Enumerable.Empty<double>())
                    .Concat(rmsFeature?.ImuVector ?? Enumerable.Empty<double>())
                    .Concat(varFeature?.ImuVector ?? Enumerable.Empty<double>())
                    .Concat(sscFeature?.ImuVector ?? Enumerable.Empty<double>());

                var featureTransposed = new FeatureTransposed
                {
                    PredictorValue = labels[featureGroup.KeyPair.GestureValue],
                    FeatureVector = featureVector.ToArray()
                };

                result.Add(featureTransposed);
            }

            return result;
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

        public Dictionary<string, int> EncodeCategoricalLabels(List<string> labels)
        {
            var result = new Dictionary<string, int>();

            return labels.OrderBy(label => label)
                .Select((label, idx) => new { Id = label, Value = idx })
                .ToDictionary(label => label.Id, label => label.Value);
        }

        public double CalculateMeanValue(double[] sensorData)
        {
            return sensorData.Average();
        }

        public double CalculateWaveformLength(double[] sensorData)
        {
            double result = 0;

            for (int i = 0; i < sensorData.Length - 1; i++)
            {
                result += Math.Abs(sensorData[i + 1] - sensorData[i]);
            }

            return result;
        }

        public double CalculateRootMeanSquare(double[] sensorData)
        {
            double squaresSum = GetSquaresSum(sensorData);

            return Math.Sqrt(squaresSum / sensorData.Count());
        }

        public double CalculateVariance(double[] sensorData)
        {
            double squaresSum = GetSquaresSum(sensorData);

            return squaresSum / (sensorData.Count() - 1);
        }

        public double CalculateSlopeSignChange(double[] sensorData)
        {
            double result = 0;

            for (int i = 2; i < sensorData.Length - 2; i++)
            {
                if ((sensorData[i] < sensorData[i - 1] && sensorData[i] < sensorData[i + 1]) ||
                    (sensorData[i] > sensorData[i - 1] && sensorData[i] > sensorData[i + 1]))
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
