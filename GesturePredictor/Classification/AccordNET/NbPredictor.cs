using Accord.IO;
using Accord.MachineLearning.Bayes;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Distributions.Fitting;
using Accord.Statistics.Distributions.Univariate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GesturePredictor.Classification.AccordNET
{
    public class NbPredictor : IPredictor
    {
        private double[][] input;
        private int[] output;
        private NaiveBayesLearning<NormalDistribution> teacher;
        private NaiveBayes<NormalDistribution> model;

        private string ModelFullPath => Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            @"Model/naivebayes_model.accord");

        public int? NumberOfFeatures { get; set ; }

        public void CreateModel()
        {
            // Create a new Gaussian distribution naive Bayes learner
            teacher = new NaiveBayesLearning<NormalDistribution>();

            // Set options for the component distributions
            teacher.Options.InnerOption = new NormalOptions
            {
                Regularization = 1e-5 // to avoid zero variances
            };
        }

        public void StartTraining(double[][] input, int[] output)
        {
            this.input = input;
            this.output = output;

            // Learn the naive Bayes model
            model = teacher.Learn(input, output);
        }

        public Tuple<int[], double[], double> EvaluateModel(double[][] input, int[] output)
        {
            // Use the model to predict class labels
            int[] predicted = model.Decide(input);

            // Get class scores for each sample
            double[] scores = model.Score(input);

            // Compute classification error
            double error = new ZeroOneLoss(output).Loss(predicted);

            return Tuple.Create(predicted, scores, error);
        }

        public int Predict(double[] input)
        {
            if (model == null)
                LoadModel();

            return model.Decide(input);
        }

        public void LoadModel()
        {
            model = Serializer
                .Load<NaiveBayes<NormalDistribution>>(ModelFullPath);

            if (model == null)
                throw new Exception("Model does not exist!");
        }

        public void SaveModel()
        {
            Serializer.Save(model, ModelFullPath);
        }
    }
}
