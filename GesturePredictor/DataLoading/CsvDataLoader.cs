namespace GesturePredictor.DataLoading
{
    internal class CsvDataLoader : IDataLoader
    {
        public IEnumerable<RawDataSnapshot> LoadData(string path)
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
                record.SensorValues.Add(Convert.ToDouble(columns[3]));
                record.SensorValues.Add(Convert.ToDouble(columns[4]));
                record.SensorValues.Add(Convert.ToDouble(columns[5]));
                record.SensorValues.Add(Convert.ToDouble(columns[6]));
                record.SensorValues.Add(Convert.ToDouble(columns[7]));
                record.SensorValues.Add(Convert.ToDouble(columns[8]));
                record.SensorValues.Add(Convert.ToDouble(columns[9]));
                record.SensorValues.Add(Convert.ToDouble(columns[10]));

                result.Add(record);
            }

            return result;
        }
    }
}