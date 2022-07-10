using GesturePredictor.Classification;
using GesturePredictor.Classification.AccordNET;
using GesturePredictor.DataLoading;
using GesturePredictor.FeatureProcessing;
using GesturePredictor.PreProcessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace GesturePredictor.Tests
{
    [TestClass]
    public class DataTrainingTests
    {
        private FeatureProcessor featureProcessor;
        private string emgTestFilePath;
        private string imuTestFilePath;

        public DataTrainingTests()
        {
            featureProcessor = new FeatureProcessor();
            var rootFolder = "C:/Temp";
            var subfolder = "6Words1980Samples";
#if !DEBUG
            string configFile = $"{Assembly.GetExecutingAssembly().Location}.config";
            string outputConfigFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).FilePath;
            File.Copy(configFile, outputConfigFile, true);

            rootFolder = ConfigurationManager.AppSettings["TestDatasetsFolder"];
#endif
            emgTestFilePath = $"{rootFolder}/{subfolder}/emg_training_data.csv";
            imuTestFilePath = $"{rootFolder}/{subfolder}/imu_training_data.csv";
        }

        [TestMethod]
        public void Test_Extract_EMG_Features_FeaturesExtracted()
        {
            IDataLoader csvLoader = new CsvDataLoader();
            var emgRawRecords = csvLoader.LoadData(emgTestFilePath, 11);
            Assert.IsTrue(emgRawRecords.Count() > 0);

            var emgNormalizedData = PreProcessData(emgRawRecords, 50);
            Assert.IsTrue(emgNormalizedData.Count() > 0);

            var features = ExtractFeatures(emgNormalizedData);
            Assert.IsTrue(features.Count() > 0);
        }

        [TestMethod]
        public void Test_Extract_IMU_Features_FeaturesExtracted()
        {
            IDataLoader csvLoader = new CsvDataLoader();

            var imuRawRecords = csvLoader.LoadData(imuTestFilePath, 13);
            Assert.IsTrue(imuRawRecords.Count() > 0);

            var imuNormalizedData = PreProcessData(imuRawRecords, 10);
            Assert.IsTrue(imuNormalizedData.Count() > 0);

            var features = ExtractFeatures(imuNormalizedData);
            Assert.IsTrue(features.Count() > 0);
        }

        [TestMethod]
        public void Test_PerformTraining_DataTrainedAndEvaluated()
        {
            IDataLoader csvLoader = new CsvDataLoader();

            // EMG data
            var emgRawRecords = csvLoader.LoadData(emgTestFilePath, 11);
            var emgNormalizedData = PreProcessData(emgRawRecords, 50); // 250 ms interval
            var emgFeatures = ExtractFeatures(emgNormalizedData);

            // IMU data
            var imuRawRecords = csvLoader.LoadData(imuTestFilePath, 13);
            var imuNormalizedData = PreProcessData(imuRawRecords, 10); // 200 ms interval
            var imuFeatures = ExtractFeatures(imuNormalizedData);

            var features = featureProcessor.MergeFeatures(emgFeatures, imuFeatures).ToList();

            foreach (var feature in features.Take(7))
            {
                var featureVector = string.Join("\t", feature.FeatureVector.Select(d => d.ToString("F2")));
                Console.WriteLine(featureVector, $"{feature.PredictorValue} |\t{featureVector}\r\n");
            }

            var trainingData = Helpers.SplitForTraining(features);

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

        private IEnumerable<RawDataSnapshot> PreProcessData(IEnumerable<RawDataSnapshot> rawRecords, int windowSize)
        {
            var preProcessor = new PreProcessor();

            var rectifiedData = preProcessor.RectifyData(rawRecords);
            var smoothedData = preProcessor.SmoothData(rectifiedData, windowSize);
            var activeSegments = preProcessor.ExtractActiveSegments(smoothedData, windowSize);

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