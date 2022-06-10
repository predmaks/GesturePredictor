using GesturePredictor.Classification;
using GesturePredictor.Classification.AccordNET;
using GesturePredictor.DataLoading;
using GesturePredictor.FeatureProcessing;
using GesturePredictor.PreProcessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace GesturePredictor.Tests
{
    [TestClass]
    public class UnitTest1
    {
        private FeatureProcessor featureProcessor;

        public UnitTest1()
        {
            featureProcessor = new FeatureProcessor();
        }

        [TestMethod]
        public void TestMethod1()
        {
            IDataLoader csvLoader = new CsvDataLoader();
            var emgRawRecords = csvLoader.LoadData("C:/Temp/emg_training_data.csv", 11);
            Assert.IsTrue(emgRawRecords.Count() > 0);

            var emgNormalizedData = NormalizeData(emgRawRecords);
            Assert.IsTrue(emgNormalizedData.Count() > 0);

            var features = ExtractFeatures(emgNormalizedData);
            Assert.IsTrue(features.Count() > 0);
        }

        [TestMethod]
        public void Test_ProcessImuRawData_FeaturesExtracted()
        {
            IDataLoader csvLoader = new CsvDataLoader();

            var imuRawRecords = csvLoader.LoadData("C:/Temp/imu_training_data.csv", 13);
            Assert.IsTrue(imuRawRecords.Count() > 0);

            var imuNormalizedData = NormalizeData(imuRawRecords);
            Assert.IsTrue(imuNormalizedData.Count() > 0);

            var features = ExtractFeatures(imuNormalizedData);
            Assert.IsTrue(features.Count() > 0);
        }

        [TestMethod]
        public void Test_PerformTraining_DataTrainedAndEvaluated()
        {
            IDataLoader csvLoader = new CsvDataLoader();

            // EMG data
            var emgRawRecords = csvLoader.LoadData("C:/Temp/emg_training_data.csv", 11);
            var emgNormalizedData = NormalizeData(emgRawRecords);
            var emgFeatures = ExtractFeatures(emgNormalizedData);

            // IMU data
            var imuRawRecords = csvLoader.LoadData("C:/Temp/imu_training_data.csv", 13);
            var imuNormalizedData = NormalizeData(imuRawRecords);
            var imuFeatures = ExtractFeatures(imuNormalizedData);

            var features = featureProcessor.MergeFeatures(emgFeatures, imuFeatures).ToList();

            IPredictor predictor = new SvmPredictor();
            var trainingData = predictor.SplitForTraining(features);
            predictor.NumberOfFeatures = trainingData.TrainingInput[0].Length;
            predictor.CreateModel();
            predictor.StartTraining(trainingData.TrainingInput, trainingData.TrainingLabels);
            var evaluationResult = predictor.EvaluateModel(trainingData.ValidationInput, trainingData.ValidationLabels);
            var classificationError = evaluationResult.Item3 * 100;

        }

        private IEnumerable<RawDataSnapshot> NormalizeData(IEnumerable<RawDataSnapshot> rawRecords)
        {
            var preProcessor = new PreProcessor();

            var rectifiedData = preProcessor.RectifyData(rawRecords);
            var smoothedData = preProcessor.SmoothData(rectifiedData);
            var activeSegments = preProcessor.ExtractActiveSegments(smoothedData);

            return preProcessor.NormalizeData(activeSegments);
        }

        private IEnumerable<FeatureRecord> ExtractFeatures(IEnumerable<RawDataSnapshot> normalizedRecords)
        {
            var allFeatureTypes = Enum.GetValues(
                        typeof(FeatureTypes)).Cast<FeatureTypes>().ToList();
            
            return featureProcessor.ExtractFeatures(normalizedRecords, allFeatureTypes);
        }
    }
}