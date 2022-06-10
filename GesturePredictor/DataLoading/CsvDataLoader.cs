namespace GesturePredictor.DataLoading
{
    internal class CsvDataLoader : IDataLoader
    {
        public IEnumerable<RawDataSnapshot> LoadData(string path, int columnCount)
        {
            var result = new List<RawDataSnapshot>();

            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                var record = new RawDataSnapshot();
                
                string[] columns = line.Split(',');
                record.GestureId = Convert.ToInt32(columns[0]);
                record.GestureValue = columns[1];
                record.Timestamp = DateTime.Parse(columns[2]);
                
                record.SensorValues = new List<double>();
                for (int i = 3; i < columnCount; i++)
                {
                    record.SensorValues.Add(Convert.ToDouble(columns[i]));
                }

                result.Add(record);
            }

            return result;
        }
    }
}