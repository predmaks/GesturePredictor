using Accord.IO;
using Accord.MachineLearning;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Analysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GesturePredictor.Classification.AccordNET
{
    public class KnnPredictor : IPredictor
    {
        const string modelRelativePath = @"Model/knn_model.accord";

        double[][] input;
        int[] output;
        KNearestNeighbors knn;

        private string ModelFullPath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), modelRelativePath);

        public int? NumberOfFeatures { get; set; }

        public void CreateModel()
        {
            knn = new KNearestNeighbors(k: 5);
        }

        public void StartTraining(double[][] input, int[] output)
        {
            this.input = input;
            this.output = output;

            // Learn a machine
            knn.Learn(input, output);
        }

        public Tuple<int[], double[], double> EvaluateModel(double[][] inputArray, int[] outputArray)
        {
            // Obtain class predictions for each sample
            int[] predicted = knn.Decide(inputArray);
            // Get class scores for each sample
            double[] scores = knn.Score(inputArray);
            // Compute classification error
            double error = new ZeroOneLoss(outputArray)
                .Loss(predicted);
            return Tuple.Create(predicted, scores, error);
        }

        public int Predict(double[] input)
        {
            if (knn == null)
                LoadModel();

            return knn.Decide(input);
        }

        public void LoadModel()
        {
            knn = Serializer.Load<KNearestNeighbors>(ModelFullPath);

            if (knn == null)
                throw new Exception("Model does not exist!");
        }

        public void SaveModel()
        {
            var cm = GeneralConfusionMatrix.Estimate(knn, input, output);

            // We can use it to estimate measures such as 
            double error = cm.Error;  // should be 0
            double acc = cm.Accuracy; // should be 1
            double kappa = cm.Kappa;  // should be 1

            knn.Save(ModelFullPath);
        }
    }
}
