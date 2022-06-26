using Accord.IO;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Distributions.Fitting;
using Accord.Statistics.Distributions.Multivariate;
using Accord.Statistics.Models.Markov;
using Accord.Statistics.Models.Markov.Learning;
using Accord.Statistics.Models.Markov.Topology;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GesturePredictor.Classification.AccordNET
{
    public class HmmPredictor : IPredictor
    {
        const string modelRelativePath = @"Model/hmm_model.accord";

        double[][] input;
        int[] output;

        //BaumWelchLearning<NormalDistribution, double> teacher;
        HiddenMarkovClassifierLearning<MultivariateNormalDistribution, double[]> teacher;
        HiddenMarkovClassifier<MultivariateNormalDistribution, double[]> classifier;

        private string ModelFullPath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), modelRelativePath);

        //public MachineLearningAlgorithm Algorithm => MachineLearningAlgorithm.HiddenMarkovModel;

        public int? NumberOfFeatures { get; set; }

        public void CreateModel()
        {
            if (!NumberOfFeatures.HasValue)
                throw new Exception("The number of input features needs to be set before creating model!");

            // Create one base Normal distribution with 5 features to be replicated accross the states
            var initialDensity = new MultivariateNormalDistribution(NumberOfFeatures.Value);

            // Creates a sequence classifier containing 5 hidden Markov Models with 2 states
            // and an underlying multivariate mixture of Normal distributions as density.
            classifier = new HiddenMarkovClassifier<MultivariateNormalDistribution, double[]>(
                classes: Helpers.NumberOfClasses, topology: new Forward(2), initial: initialDensity);

            // Configure the learning algorithms to train the sequence classifier
            teacher = new HiddenMarkovClassifierLearning<MultivariateNormalDistribution, double[]>(classifier)
            {
                // Train each model until the log-likelihood changes less than 0.0001
                Learner = modelIndex => new BaumWelchLearning<MultivariateNormalDistribution, double[], NormalOptions>(classifier.Models[modelIndex])
                {
                    Tolerance = 0.0001,
                    MaxIterations = 0,

                    FittingOptions = new NormalOptions()
                    {
                        Diagonal = true,      // only diagonal covariance matrices
                        Regularization = 1e-5 // avoid non-positive definite errors
                    }
                }
            };
        }

        public void StartTraining(double[][][] input, int[] output)
        {

        }

        public void StartTraining(double[][] input, int[] output)
        {
            this.input = input;
            this.output = output;

            var labels = output.Distinct().OrderBy(data => data).ToArray();

            //double[][][] inputData = new double[output.Length][][];

            //var labelGroups = output
            //    .Select((v, i) => new { Value = v, Index = i })
            //    .GroupBy(item => item.Value, item => item.Index, (key, g) => new { Label = key, Indexes = g.ToArray() })
            //    .OrderBy(g => g.Label);

            //foreach (var labelGroup in labelGroups)
            //{
            //    inputData[labelGroup.Label] = labelGroup.Indexes.Select(i => input[i]).ToArray();
            //}
            List<double[][]> inputData = new List<double[][]>();
            foreach (var featureVector in input)
            {
                var sensors = new double[][] { featureVector };

                inputData.Add(sensors);
            }

            // Train the sequence classifier 
            teacher.Learn(inputData.ToArray(), output);
        }

        public Tuple<int[], double[], double> EvaluateModel(double[][] input, int[] output)
        {
            List<double[][]> inputData = new List<double[][]>();
            foreach (var featureVector in input)
            {
                var sensors = new double[][] { featureVector };

                inputData.Add(sensors);
            }

            var evaluationData = inputData.ToArray();

            // Obtain class predictions for each sample

            // NOTE!!! don't use this method, but rather one which returns int[] as a result,
            // due to wrong validation error calculated when new ZeroOneLoss(output).Loss(predicted) is used

            //double[] predicted = new double[input.Length];
            //classifier.Decide(evaluationData, predicted);
            //var predictedResult = predicted.Select(d => (int)d).ToArray();

            var predicted = classifier.Decide(evaluationData);
            //double logLikelihood = teacher.LogLikelihood;

            // Get class scores for each sample
            double[] scores = classifier.Score(evaluationData);

            // Compute classification error
            double error = new ZeroOneLoss(output).Loss(predicted);

            return Tuple.Create(predicted, scores, error);
        }

        public int Predict(double[] input)
        {
            if (classifier == null)
                LoadModel();

            return 0;
            //return classifier.Decide(input);
        }

        public void LoadModel()
        {
            classifier = Serializer
                .Load<HiddenMarkovClassifier<MultivariateNormalDistribution, double[]>>(ModelFullPath);

            if (classifier == null)
                throw new Exception("Model does not exist!");
        }

        public void SaveModel()
        {
            var assembly = Assembly.GetExecutingAssembly();

            classifier.Save(ModelFullPath);
        }
    }
}
