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

        public IEnumerable<RawDataSnapshot> SmoothData(IEnumerable<RawDataSnapshot> input)
        {
            var windowSize = 50;
            var result = new List<RawDataSnapshot>();

            var gesturesGrouped = input.GroupBy(
                emg => emg.GestureId, (key, g) => new { GestureId = key, Records = g.ToList() });

            foreach (var gesture in gesturesGrouped)
            {
                for (int i = 0; i < gesture.Records.Count - windowSize + 1; i++)
                {
                    var sensor1Sum = 0d;
                    var sensor2Sum = 0d;
                    var sensor3Sum = 0d;
                    var sensor4Sum = 0d;
                    var sensor5Sum = 0d;
                    var sensor6Sum = 0d;
                    var sensor7Sum = 0d;
                    var sensor8Sum = 0d;

                    // TODO: List<double> sensorSums = new() { 0d, 0d, 0d, 0d, 0d, 0d, 0d, 0d };
                    // TODO: https://stackoverflow.com/questions/10284133/sum-range-of-ints-in-listint

                    for (int j = i; j < i + windowSize - 1; j++)
                    {
                        var rectifiedRecord = gesture.Records.ElementAt(j);
                        sensor1Sum += rectifiedRecord.SensorValues[0];
                        sensor2Sum += rectifiedRecord.SensorValues[1];
                        sensor3Sum += rectifiedRecord.SensorValues[2];
                        sensor4Sum += rectifiedRecord.SensorValues[3];
                        sensor5Sum += rectifiedRecord.SensorValues[4];
                        sensor6Sum += rectifiedRecord.SensorValues[5];
                        sensor7Sum += rectifiedRecord.SensorValues[6];
                        sensor8Sum += rectifiedRecord.SensorValues[7];
                    }

                    var baseRectifiedRecord = gesture.Records.ElementAt(i);

                    var sensorValues = new List<double>
                    {
                        sensor1Sum / windowSize,
                        sensor2Sum / windowSize,
                        sensor3Sum / windowSize,
                        sensor4Sum / windowSize,
                        sensor5Sum / windowSize,
                        sensor6Sum / windowSize,
                        sensor7Sum / windowSize,
                        sensor8Sum / windowSize
                    };

                    result.Add(new RawDataSnapshot
                    {
                        Timestamp = baseRectifiedRecord.Timestamp,
                        GestureId = baseRectifiedRecord.GestureId,
                        SensorValues = sensorValues 
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

                var gesturesGrouped = input.GroupBy(
                    emg => emg.GestureId, (key, g) => new { GestureId = key, Records = g.ToList() });

                foreach (var gesture in gesturesGrouped)
                {
                    var sensor1Max = (double)gesture.Records.Max(r => r.SensorValues[0]);
                    var sensor2Max = (double)gesture.Records.Max(r => r.SensorValues[1]);
                    var sensor3Max = (double)gesture.Records.Max(r => r.SensorValues[2]);
                    var sensor4Max = (double)gesture.Records.Max(r => r.SensorValues[3]);
                    var sensor5Max = (double)gesture.Records.Max(r => r.SensorValues[4]);
                    var sensor6Max = (double)gesture.Records.Max(r => r.SensorValues[5]);
                    var sensor7Max = (double)gesture.Records.Max(r => r.SensorValues[6]);
                    var sensor8Max = (double)gesture.Records.Max(r => r.SensorValues[7]);

                    result.AddRange(gesture.Records.Select(
                        r => new RawDataSnapshot
                        {
                            GestureId = r.GestureId,
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
                        }));
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

        public IEnumerable<RawDataSnapshot> ExtractActiveSegments(IEnumerable<RawDataSnapshot> input)
        {
            var windowSize = 50; // 250ms interval
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
