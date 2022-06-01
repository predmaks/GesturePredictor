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
        [TestMethod]
        public void TestMethod1()
        {
            IDataLoader l = new CsvDataLoader();
            var rawRecords = l.LoadData("C:/Temp/training_data.csv");
            Assert.IsTrue(rawRecords.Count() > 0);

            var preProcessor = new PreProcessor();
            var rectifiedData = preProcessor.RectifyData(rawRecords);
            var smoothedData = preProcessor.SmoothData(rectifiedData);
            var activeSegments = preProcessor.ExtractActiveSegments(smoothedData);
            var normalizedData = preProcessor.NormalizeData(activeSegments);
            Assert.IsTrue(normalizedData.Count() > 0);

            var allFeatureTypes = Enum.GetValues(
                        typeof(FeatureTypes)).Cast<FeatureTypes>().ToList();

            var featureProcessor = new FeatureProcessor();
            var features = featureProcessor.ExtractFeatures(normalizedData, allFeatureTypes);
            Assert.IsTrue(features.Count() > 0);
        }
    }
}