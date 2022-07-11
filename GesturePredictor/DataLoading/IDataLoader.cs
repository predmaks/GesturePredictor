namespace GesturePredictor.DataLoading
{
    public interface IDataLoader
    {
        IEnumerable<GestureDataSnapshot> LoadData(string path, int columnCount);
    }
}