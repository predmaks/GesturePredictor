using Accord.IO;
using Accord.MachineLearning.VectorMachines;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math.Optimization.Losses;
using Accord.Statistics.Kernels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GesturePredictor.Classification.AccordNET
{
    public class SvmPredictor : IPredictor
    {
        const string modelRelativePath = @"Model/svm_model.accord";

        double[][] input;
        int[] output;
        MulticlassSupportVectorLearning<Linear> teacher;
        MulticlassSupportVectorMachine<Linear> machine;

        private string ModelFullPath => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), modelRelativePath);

        public int? NumberOfFeatures { get; set; }

        //public MachineLearningAlgorithm Algorithm { get => MachineLearningAlgorithm.SupportVectorMachines; }

        public void CreateModel()
        {
            // Create the multi-class learning algorithm for the machine
            teacher = new MulticlassSupportVectorLearning<Linear>()
            {
                // Configure the learning algorithm to use SMO to train the
                //  underlying SVMs in each of the binary class subproblems.
                Learner = (param) => new LinearCoordinateDescent()
                {
                    // Estimate a suitable guess for the Gaussian kernel's parameters.
                    // This estimate can serve as a starting point for a grid search.
                    //UseKernelEstimation = true,
                    //Loss = Loss.L2,
                    Complexity = 2,
                    Tolerance = 1.0e-4
                }
            };

            // The following line is only needed to ensure reproducible results. Please remove it to enable full parallelization
            teacher.ParallelOptions.MaxDegreeOfParallelism = 1; // (Remove, comment, or change this line to enable full parallelism)
        }

        public void StartTraining(double[][] input, int[] output)
        {
            this.input = input;
            this.output = output;

            // Learn a machine
            machine = teacher.Learn(input, output);
        }

        public Tuple<int[], double[], double> EvaluateModel(double[][] inputArray, int[] outputArray)
        {
            // Obtain class predictions for each sample
            int[] predicted = machine.Decide(inputArray);
            // Get class scores for each sample
            double[] scores = machine.Score(inputArray);
            // Compute classification error
            double error = new ZeroOneLoss(outputArray)
                .Loss(predicted);
            return Tuple.Create(predicted, scores, error);
        }

        public int Predict(double[] input)
        {
            if (machine == null)
                LoadModel();

            return machine.Decide(input);
        }

        public void LoadModel()
        {
            machine = Serializer
                .Load<MulticlassSupportVectorMachine<Linear>>(ModelFullPath);

            if (machine == null)
                throw new Exception("Model does not exist!");
        }

        public void SaveModel()
        {
            machine.Save(ModelFullPath);
        }

        public TrainingData SplitForTraining(List<FeatureTransposed> features)
        {
            // TODO: move to separate ITrainier interface and class and do better random split
            var result = new TrainingData();

            // TODO 1: improve this to do really 70:30 or 60:40 in random way!!!
            for (int i = 0; i < features.Count; i++)
            {
                if (i % 10 < 6)
                {
                    result.Training.Add(features.ElementAt(i));
                }
                else
                {
                    result.Validation.Add(features.ElementAt(i));
                }
            }

            return result;
        }
    }
}
