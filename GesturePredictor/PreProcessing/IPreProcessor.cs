namespace GesturePredictor.PreProcessing
{
    internal interface IPreProcessor
    {
        IEnumerable<RawDataSnapshot> RectifyData(IEnumerable<RawDataSnapshot> input);
        IEnumerable<RawDataSnapshot> SmoothData(IEnumerable<RawDataSnapshot> input);
        IEnumerable<RawDataSnapshot> NormalizeData(IEnumerable<RawDataSnapshot> input);
        IEnumerable<RawDataSnapshot> ExtractActiveSegments(IEnumerable<RawDataSnapshot> input);
    }
}
