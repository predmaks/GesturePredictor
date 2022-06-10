using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GesturePredictor.Classification
{
    public class TrainingData
    {
        private double[][] trainingInput;
        private double[][] validationInput;
        private int[] trainingLabels;
        private int[] validationLabels;

        public TrainingData()
        {
            Training = new List<FeatureTransposed>();
            Validation = new List<FeatureTransposed>();
        }

        public List<FeatureTransposed> Training { get; set; }
        public List<FeatureTransposed> Validation { get; set; }

        public double[][] TrainingInput
        {
            get
            {
                if (trainingInput == null)
                {
                    trainingInput = Training.Select(f => f.FeatureVector).ToArray();
                }

                return trainingInput;
            }
        }

        public double[][] ValidationInput
        {
            get
            {
                if (validationInput == null)
                {
                    validationInput = Validation.Select(f => f.FeatureVector).ToArray();
                }

                return validationInput;
            }
        }

        public int[] TrainingLabels
        {
            get
            {
                if (trainingLabels == null)
                {
                    trainingLabels = Training.Select(f => f.PredictorValue).ToArray();
                }

                return trainingLabels;
            }
        }

        public int[] ValidationLabels
        {
            get
            {
                if (validationLabels == null)
                {
                    validationLabels = Validation.Select(f => f.PredictorValue).ToArray();
                }

                return validationLabels;
            }
        }
    }
}
