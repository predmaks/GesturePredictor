namespace GesturePredictor.PreProcessing
{
    public interface IPreProcessor
    {
        IEnumerable<GestureDataSnapshot> RectifyData(IEnumerable<GestureDataSnapshot> input);
        IEnumerable<GestureDataSnapshot> SmoothData(IEnumerable<GestureDataSnapshot> input, int windowSize);
        IEnumerable<GestureDataSnapshot> NormalizeData(IEnumerable<GestureDataSnapshot> input);
        IEnumerable<GestureDataSnapshot> ExtractActiveSegments(IEnumerable<GestureDataSnapshot> input, int windowSize);
    }
}
