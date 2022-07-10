namespace GesturePredictor.PreProcessing
{
    internal class PreProcessor : IPreProcessor
    {
        public IEnumerable<RawDataSnapshot> RectifyData(IEnumerable<RawDataSnapshot> input)
        {
            try
            {
                return input.Select(
                        r => new RawDataSnapshot
                        {
                            GestureId = r.GestureId,
                            Timestamp = r.Timestamp,
                            GestureValue = r.GestureValue,
                            SensorValues = r.SensorValues.Select(v => Math.Abs(v)).ToList()
                        }).ToList();
            }
            catch (Exception ex)
            {
                var temp = ex.Message;
                return null;
                //throw;
            }
        }

        /*public IEnumerable<RawDataSnapshot> SmoothData(IEnumerable<RawDataSnapshot> input)
        {
            var windowSize = 50; // TODO: move to config
            var result = new List<RawDataSnapshot>();

            var sensorCount = input.ElementAt(0).SensorValues.Count;

            var gesturesGrouped = input.GroupBy(
                emg => emg.GestureId, (key, g) => new { GestureId = key, Records = g.ToList() });

            foreach (var gesture in gesturesGrouped)
            {
                var split = gesture.Records.Select((x, i) => new { Index = i, Value = x.SensorValues })
                    .GroupBy(x => x.Index / windowSize)
                    .Select(x => x.Select(v => v.Value).ToList())
                    .ToList();
            }
        }*/

        public IEnumerable<RawDataSnapshot> SmoothData(IEnumerable<RawDataSnapshot> input, int windowSize)
        {
            //var windowSize = 50; // TODO: move to config
            var result = new List<RawDataSnapshot>();

            var sensorCount = input.ElementAt(0).SensorValues.Count;

            var gesturesGrouped = input.GroupBy(
                emg => emg.GestureId, (key, g) => new { GestureId = key, Records = g.ToList() });

            foreach (var gesture in gesturesGrouped)
            {
                for (int i = 0; i < gesture.Records.Count - windowSize + 1; i++)
                {
                    var sensorSums = new double[sensorCount];

                    for (int sensorIndex = 0; sensorIndex < sensorCount; sensorIndex++)
                    {
                        sensorSums[sensorIndex] = 0d;
                    }

                    // TODO: https://stackoverflow.com/questions/10284133/sum-range-of-ints-in-listint

                    for (int j = i; j < i + windowSize - 1; j++)
                    {
                        var gestureRecord = gesture.Records.ElementAt(j);

                        for (int k = 0; k < sensorCount; k++)
                        {
                            sensorSums[k] += gestureRecord.SensorValues.ElementAt(k);
                        }
                    }

                    var baseRectifiedRecord = gesture.Records.ElementAt(i);

                    for (int sensorIndex = 0; sensorIndex < sensorCount; sensorIndex++)
                    {
                        sensorSums[sensorIndex] /= windowSize;
                    }

                    result.Add(new RawDataSnapshot
                    {
                        Timestamp = baseRectifiedRecord.Timestamp,
                        GestureId = baseRectifiedRecord.GestureId,
                        GestureValue = baseRectifiedRecord.GestureValue,
                        SensorValues = sensorSums.ToList()
                    });
                }
            }

            return result;
        }

        public IEnumerable<RawDataSnapshot> NormalizeData(IEnumerable<RawDataSnapshot> input)
        {
            try
            {
                var result = new List<RawDataSnapshot>();

                var sensorCount = input.ElementAt(0).SensorValues.Count;

                var gesturesGrouped = input.GroupBy(
                    emg => emg.GestureId, (key, g) => new { GestureId = key, Records = g.ToList() });

                foreach (var gesture in gesturesGrouped)
                {
                    var sensorMaxValues = new double[sensorCount];

                    for (int sensorIndex = 0; sensorIndex < sensorCount; sensorIndex++)
                    {
                        sensorMaxValues[sensorIndex] = (double)gesture.Records.Max(r => r.SensorValues[sensorIndex]);
                    }

                    foreach (var record in gesture.Records)
                    {
                        var dataSnapshot = new RawDataSnapshot
                        {
                            GestureId = record.GestureId,
                            GestureValue = record.GestureValue,
                            Timestamp = record.Timestamp
                        };

                        var sensorNormalizedValues = new double[sensorCount];

                        for (int sensorIndex = 0; sensorIndex < sensorCount; sensorIndex++)
                        {
                            sensorNormalizedValues[sensorIndex] = record.SensorValues[sensorIndex] / sensorMaxValues[sensorIndex];
                        }

                        dataSnapshot.SensorValues = sensorNormalizedValues.ToList();
                        
                        result.Add(dataSnapshot);
                    }

                    /*result.AddRange(gesture.Records.Select(
                        r => new RawDataSnapshot
                        {
                            GestureId = r.GestureId,
                            GestureValue = r.GestureValue,
                            Timestamp = r.Timestamp,
                            SensorValues = new List<double>
                            {
                                r.SensorValues[0] / sensor1Max,
                                r.SensorValues[1] / sensor2Max,
                                r.SensorValues[2] / sensor3Max,
                                r.SensorValues[3] / sensor4Max,
                                r.SensorValues[4] / sensor5Max,
                                r.SensorValues[5] / sensor6Max,
                                r.SensorValues[6] / sensor7Max,
                                r.SensorValues[7] / sensor8Max
                            }
                        }));*/
                }

                return result;
            }
            catch (Exception ex)
            {
                var temp = ex.Message;
                return null;
                //throw;
            }
        }

        public IEnumerable<RawDataSnapshot> ExtractActiveSegments(IEnumerable<RawDataSnapshot> input, int windowSize)
        {
            //var windowSize = 50; // 250ms interval
            var result = new List<RawDataSnapshot>();

            var gesturesGrouped = input.GroupBy(
                emg => emg.GestureId, (key, g) => new { GestureId = key, Records = g.ToList() });

            foreach (var gesture in gesturesGrouped)
            {
                var segments = new Dictionary<int, double>();

                for (int i = 0; i < gesture.Records.Count - windowSize + 1; i++)
                {
                    var average = 0d;
                    for (int j = i; j < i + windowSize - 1; j++)
                    {
                        var record = gesture.Records.ElementAt(j);
                        var sum = record.SensorValues.Sum();
                        average += sum;
                    }

                    segments.Add(i, average);
                }

                var values = segments.Select(item => item.Value);
                var sd = Helpers.CalculateStandardDeviation(values);
                var segmentStart = segments.First(s => s.Value >= sd).Key;
                var segmentEnd = segments.Last(s => s.Value >= sd).Key;

                for (int i = segmentStart; i <= segmentEnd; i++)
                {
                    result.Add(gesture.Records.ElementAt(i));
                }
            }

            return result;
        }
    }
}
