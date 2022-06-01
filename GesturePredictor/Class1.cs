using Microsoft.ML;
using Microsoft.ML.Data;

namespace GesturePredictor
{
    public class Class1
    {
        void Test()
        {
            /*
            //Step 1. Create an ML Context
            var ctx = new MLContext();

            //Step 2. Read in the input data from a text file for model training
            IDataView trainingData = ctx.Data
                .LoadFromTextFile<ModelInput>(dataPath, hasHeader: true);

            //Step 3. Build your data processing and training pipeline
            var pipeline = ctx.Transforms.Text
                .FeaturizeText("Features", nameof(SentimentIssue.Text))
                .Append(ctx.BinaryClassification.Trainers
                    .LbfgsLogisticRegression("Label", "Features"));

            //Step 4. Train your model
            Microsoft.ML.ITransformer trainedModel = pipeline.Fit(trainingData);

            //Step 5. Make predictions using your trained model
            var predictionEngine = ctx.Model
                .CreatePredictionEngine<ModelInput, ModelOutput>(trainedModel);

            var sampleStatement = new ModelInput() { Text = "This is a horrible movie" };

            var prediction = predictionEngine.Predict(sampleStatement);

            /*var model = new Predictor()
                .LoadRawData<>()
                .PreProcess()
                .TrainModel()
                .SaveModel*/

            //var modelBuilder = new ModelBuilder()
            //    .LoadData().*/
        }
    }

    public enum DataLoaderType
    {
        CsvFileLoader
    }

    public interface IModelBuilder
    {
        //IDataLoader WithRawDataLoader(DataLoaderType loaderType);
        DataModel Build();
    }

    /*public class ModelBuilder : IDataLoader, IPreProcessor
    {
        private readonly DataModel model;

        private ModelBuilder()
        {
            model = new DataModel();
        }

        public IPreProcessor LoadData(string path)
        {
            model.DataFilePath = path;
            return this;
        }

        public DataModel Build()
        {
            return model;
        }

        public IEnumerable<T> RectifyData<T>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> SmoothData<T>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> NormalizeData<T>()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> ExtractActiveSegments<T>()
        {
            throw new NotImplementedException();
        }
    }*/

    public class DataModel
    {
        public string DataFilePath { get; set; }
    }

    public class ModelInput
    {
        [LoadColumn(2)]
        public float PassengerCount;
        [LoadColumn(3)]
        public float TripTime;
        [LoadColumn(4)]
        public float TripDistance;
        [LoadColumn(5)]
        public string PaymentType;
        [LoadColumn(6)]
        public float FareAmount;
    }
}