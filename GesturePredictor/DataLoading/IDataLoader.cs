namespace GesturePredictor.DataLoading
{
    internal interface IDataLoader
    {
        IEnumerable<RawDataSnapshot> LoadData(string path);
    }
}