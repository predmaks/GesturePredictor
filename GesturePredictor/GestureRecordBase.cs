namespace GesturePredictor
{
    public class GestureRecordBase
    {
        public int GestureId { get; set; }
        public string GestureValue { get; set; }
        public List<double> SensorValues { get; set; }
    }
}
