﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GesturePredictor.Classification
{
    public interface IPredictor
    {
        public int? NumberOfFeatures { get; set; }

        //MachineLearningAlgorithm Algorithm { get; }
        void StartTraining(double[][] input, int[] output);
        // TODO: create result class
        //Tuple<int[], double[], double> EvaluateModel();
        Tuple<int[], double[], double> EvaluateModel(double[][] input, int[] output);
        int Predict(double[] input);
        void CreateModel();
        void LoadModel();
    }
}
