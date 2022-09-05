using Accord.Neuro;
using Accord.Neuro.ActivationFunctions;
using Accord.Neuro.Learning;
using Accord.Neuro.Networks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GesturePredictor.Classification.AccordNET
{
    public class DbnPredictor : IPredictor
    {
        const string modelRelativePath = @"Model/dbn_model.accord";

        private const double LearningRate = 0.1;
        private const double Momentum = 0.9;
        private const double WeightDecay = 0.001;
        //private const int Epochs = 630;
        //private const int BatchSize = 100;
        private DeepBeliefNetwork network;
        private BackPropagationLearning teacher;

        //public MachineLearningAlgorithm Algorithm => MachineLearningAlgorithm.DeepBeliefNetwork;

        public int? NumberOfFeatures { get; set; }

        private string ModelFullPath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), modelRelativePath);

        public void CreateModel()
        {
            if (!NumberOfFeatures.HasValue)
                throw new Exception("The number of input features needs to be set before creating model!");

            network = new DeepBeliefNetwork(new BernoulliFunction(), NumberOfFeatures.Value, 48, Helpers.NumberOfClasses);

            teacher = new BackPropagationLearning(network)
            {
                LearningRate = LearningRate,
                Momentum = Momentum
            };

            new GaussianWeights(network).Randomize();
        }

        public void StartTraining(double[][] input, int[] output)
        {
            if (input.Length != output.Length)
                throw new Exception("Number of output labels does not correspond to the number of items in the input array!");

            var labels = output.Select(item => Enumerable.Repeat(0d, item)
                .Concat(new double[] { 1 })
                .Concat(Enumerable.Repeat(0d, Helpers.NumberOfClasses - 1 - item))
                .ToArray()).ToArray();

            // Start running the learning procedure
            for (int i = 0; i < input.Length; i++)
            {
                double error = teacher.RunEpoch(input, labels);
            }

            network.UpdateVisibleWeights();
        }

        public Tuple<int[], double[], double> EvaluateModel(double[][] input, int[] output)
        {
            List<int> predictions = new List<int>();
            List<double> scores = new List<double>();

            int correct = 0;
            for (int i = 0; i < input.Length; i++)
            {
                var predicted = network.Compute(input[i]);
                var maxValue = predicted.Max();
                var predictedIndex = predicted.ToList().IndexOf(maxValue);
                var label = output[i];

                if (predictedIndex == label)
                    correct++;

                predictions.Add(predictedIndex);
                scores.Add(maxValue);
            }

            var error = Math.Round(1 - ((double)correct / input.Length), 2);

            return Tuple.Create(predictions.ToArray(), scores.ToArray(), error);
        }

        public int Predict(double[] input)
        {
            if (network == null)
                LoadModel();

            var predicted = network.Compute(input);
            var maxValue = predicted.Max();

            return predicted.ToList().IndexOf(maxValue);
        }

        public void LoadModel()
        {
            network = DeepBeliefNetwork.Load(ModelFullPath);

            if (network == null)
                throw new Exception("Model does not exist!");
        }

        public void SaveModel()
        {
            network.Save(ModelFullPath);
        }
    }
}
