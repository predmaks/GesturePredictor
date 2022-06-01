using GesturePredictor.FeatureProcessing;

namespace GesturePredictor
{
    public class RawDataSnapshot : GestureRecordBase
    {
        public DateTime Timestamp { get; set; }
    }

    public class GestureRecordBase
    {
        public int GestureId { get; set; }
        public string GestureValue { get; set; }
        public List<double> SensorValues { get; set; }
    }

    public class FeatureRecord : GestureRecordBase
    {
        public FeatureTypes FeatureType { get; set; }
    }
}
