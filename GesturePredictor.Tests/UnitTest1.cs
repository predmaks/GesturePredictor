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

            //-
            foreach (var feature in features.Take(7))
            {
                var featureVector = string.Join("\t", feature.FeatureVector.Select(d => d.ToString("F2")));
                Console.WriteLine(featureVector, $"{feature.PredictorValue} |\t{featureVector}\r\n");
            }
            //-

            return;

            var trainingData = Helpers.SplitForTraining(features);

            //-
            foreach (var outer in trainingData.TrainingInput)
            {
                Console.WriteLine();
                foreach(var inner in outer)
                {
                    Console.Write(inner);
                }
            }
            
            return;
            //-

            // SVM
            IPredictor svmPredictor = new SvmPredictor();
            //svmPredictor.NumberOfFeatures = trainingData.TrainingInput[0].Length;
            svmPredictor.CreateModel();
            svmPredictor.StartTraining(trainingData.TrainingInput, trainingData.TrainingLabels);
            var svmEvaluationResult = svmPredictor.EvaluateModel(trainingData.ValidationInput, trainingData.ValidationLabels);
            var svmClassificationError = svmEvaluationResult.Item3 * 100;
            Console.WriteLine($"SVM evaluation error: {svmClassificationError}%");

            // kNN
            IPredictor knnPredictor = new KnnPredictor();
            //knnPredictor.NumberOfFeatures = trainingData.TrainingInput[0].Length;
            knnPredictor.CreateModel();
            knnPredictor.StartTraining(trainingData.TrainingInput, trainingData.TrainingLabels);
            var knnEvaluationResult = knnPredictor.EvaluateModel(trainingData.ValidationInput, trainingData.ValidationLabels);
            var knnClassificationError = knnEvaluationResult.Item3 * 100;
            Console.WriteLine($"kNN evaluation error: {knnClassificationError}%");

            // NaiveBayes
            IPredictor naiveBayesPredictor = new NbPredictor();
            //naiveBayesPredictor.NumberOfFeatures = trainingData.TrainingInput[0].Length;
            naiveBayesPredictor.CreateModel();
            naiveBayesPredictor.StartTraining(trainingData.TrainingInput, trainingData.TrainingLabels);
            var naiveBayesEvaluationResult = naiveBayesPredictor.EvaluateModel(trainingData.ValidationInput, trainingData.ValidationLabels);
            var naiveBayesClassificationError = naiveBayesEvaluationResult.Item3 * 100;
            Console.WriteLine($"NB evaluation error: {naiveBayesClassificationError}%");

            // DBN
            IPredictor dbnPredictor = new DbnPredictor();
            dbnPredictor.NumberOfFeatures = trainingData.TrainingInput[0].Length;
            dbnPredictor.CreateModel();
            dbnPredictor.StartTraining(trainingData.TrainingInput, trainingData.TrainingLabels);
            var dbnEvaluationResult = dbnPredictor.EvaluateModel(trainingData.ValidationInput, trainingData.ValidationLabels);
            var dbnClassificationError = dbnEvaluationResult.Item3 * 100;
            Console.WriteLine($"DBN evaluation error: {dbnClassificationError}%");

            // HMM
            IPredictor hmmPredictor = new HmmPredictor();
            hmmPredictor.NumberOfFeatures = trainingData.TrainingInput[0].Length;
            hmmPredictor.CreateModel();
            hmmPredictor.StartTraining(trainingData.TrainingInput, trainingData.TrainingLabels);
            var hmmEvaluationResult = hmmPredictor.EvaluateModel(trainingData.ValidationInput, trainingData.ValidationLabels);
            var hmmClassificationError = hmmEvaluationResult.Item3 * 100;
            Console.WriteLine($"HMM evaluation error: {hmmClassificationError}%");
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