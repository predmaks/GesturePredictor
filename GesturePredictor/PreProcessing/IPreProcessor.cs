namespace GesturePredictor.PreProcessing
{
    public interface IPreProcessor
    {
        IEnumerable<RawDataSnapshot> RectifyData(IEnumerable<RawDataSnapshot> input);
        IEnumerable<RawDataSnapshot> SmoothData(IEnumerable<RawDataSnapshot> input, int windowSize);
        IEnumerable<RawDataSnapshot> NormalizeData(IEnumerable<RawDataSnapshot> input);
        IEnumerable<RawDataSnapshot> ExtractActiveSegments(IEnumerable<RawDataSnapshot> input, int windowSize);
    }
}
