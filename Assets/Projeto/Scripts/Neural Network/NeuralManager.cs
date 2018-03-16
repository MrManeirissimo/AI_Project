using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeuralNetwork
{

    public class NeuralManager : MonoBehaviour
    {

        #region Constant Fields

        private const int InputSize = 4;           // Map Length | Qt of turns | Speed | Time
        private const int SampleSize = 3;          // Amount of samples that will be provided for the network
        private const int HiddenLayerSize = 13;    // Amount of hidden layers
        private const int OutputSize = 1;           // Size of the output matrix

        #endregion

        #region Private Fields

        //X - Matrix that stores sample data.  Rows = Amount of input the network receives | Columns = Amount of samples the network has to train
        private double[,] _sampleInput = new double[SampleSize, InputSize];

        // y - Sample output values
        private double[,] _sampleOutput = new double[SampleSize, OutputSize];

        //W¹ - Stores weights. Columns = One column for each Input value received | Rows = One row for each neuron in the hidden layer 
        private double[,] _weights = new double[InputSize, HiddenLayerSize];

        //Z² - Activity of the hidden layer
        private double[,] _activity = new double[SampleSize, HiddenLayerSize];

        //W² - One weight for each synapse | Rows = Amount of weights for each synapse
        private double[,] _w2 = new double[HiddenLayerSize, OutputSize];

        // ==FORMULAS==
        // Z² = X.W¹            - Initial activity
        // (a²) = Sigmoid(Z²)   - Normalized activity
        // Z³ = a².W²           - Output activity. One row for each sample
        // Y = Sigmoid(Z³)      - Output activity normalized
        // J = Sum 1/2 (y - Y)² - Cost
        //Ref: https://www.youtube.com/watch?v=UJwK6jAStmg
        #endregion

        private void Initialize()
        {
            _sampleInput = new double[,] {
            { 10, 4, 30, 60 },
            { 20, 7, 25, 60 },
            { 35, 10, 15, 60 },
            };

            _sampleOutput = new double[,]
            {
                { 8 },
                { 9.3f },
                { 4 },
            };

            for (int i = 0; i < _weights.GetLength(0); i++)
            {
                for (int j = 0; j < _weights.GetLength(1); j++)
                {
                    _weights[i, j] = UnityEngine.Random.Range(-50, 50);
                }
            }

            for (int i = 0; i < _w2.GetLength(0); i++)
            {
                for (int j = 0; j < _w2.GetLength(1); j++)
                {
                    _w2[i,j] = UnityEngine.Random.Range(-50, 50);
                }
            }
        }

        // Use this for initialization
        void Start()
        {
            Initialize();

            PropagateForward();
        }

        // Update is called once per frame
        void Update()
        {

        }


        private void PropagateForward()
        {
            double[,] _activity = MultiplyMatrix(_sampleInput, _weights);
            
            for (int i = 0; i < _activity.GetLength(0); i++)
            {
                for (int j = 0; j < _activity.GetLength(1); j++)
                {
                    _activity[i,j] = Sigmoid(_activity[i,j]);
                }
            }

            _activity = MultiplyMatrix(_activity, _w2);

            for (int i = 0; i < _activity.GetLength(0); i++)
            {
                for (int j = 0; j < _activity.GetLength(1); j++)
                {
                    _activity[i, j] = Sigmoid(_activity[i, j]);
                }
            }
        }

        private double[,] MultiplyMatrix(double[,] a, double[,] b)
        {
            double[,] __c = new double[a.GetLength(0), b.GetLength(1)];
            if (a.GetLength(1) == b.GetLength(0))
            {
                for (int i = 0; i < __c.GetLength(0); i++)
                {
                    for (int j = 0; j < __c.GetLength(1); j++)
                    {
                        __c[i, j] = 0;
                        for (int k = 0; k < a.GetLength(1); k++)
                            __c[i, j] = __c[i, j] + a[i, k] * b[k, j];
                    }
                }
            }


            return __c;
        }

        public double Sigmoid(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }
    }
}
//Neural networks são um conjunto de neurônios que, através de uma operação entre dada entrada e um peso ajustável (W), produzem uma saída.
//Um perceptron é uma unidade que recebe entradas, aplica pesos e comprara os resultados para tentar chegar à uma resposta.
//Para o perceptron aprender, os pesos precisam ser ajustados em uma quantidade proporcional à diferença entre a saída desejada e a saída atual do meu sistema.
// W = W + (Delta)W, onde Delta = n(t-o)X
// W = Peso     n = Learning Rate       t = saída desejada      o = Saída atual     X = entrada

