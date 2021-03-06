﻿namespace SoundFingerprinting.NeuralHasher.Tests
{
    using System.Collections.Generic;

    using Encog.ML;
    using Encog.ML.Data.Basic;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Moq;

    using SoundFingerprinting.Tests;

    [TestClass]
    public class DynamicReorderingAlgorithmTest : AbstractTest
    {
        private readonly DynamicReorderingAlgorithm dynamicReorderingAlgorithm = new DynamicReorderingAlgorithm();

        [TestMethod]
        public void TestComputeAm()
        {
            var network = new Mock<IMLRegression>();
            var songSnippets = new List<double[][]>
                {
                    new[] { new double[] { 1 }, new double[] { 2 } },
                    new[] { new double[] { 3 }, new double[] { 4 } }
                };
            var networkOutput = new Queue<double[]>();
            networkOutput.Enqueue(new[] { 0, 1d });
            networkOutput.Enqueue(new[] { 0, 1d });
            networkOutput.Enqueue(new[] { 0.5, 0 });
            networkOutput.Enqueue(new[] { 0.5, 0 });
            network.Setup(n => n.Compute(It.IsAny<BasicMLData>())).Returns(() => new BasicMLData(networkOutput.Dequeue()));

            double[][] am = dynamicReorderingAlgorithm.ComputeAm(network.Object, songSnippets, 2);

            Assert.AreEqual(2, am.Length);
            AssertArraysAreEqual(new[] { 0, 1d }, am[0]);
            AssertArraysAreEqual(new[] { 0.5, 0 }, am[1]);
        }

        [TestMethod]
        public void TestL2NormPairCalculation()
        {
            double[][] binaryOutputs = new[] { new[] { 0d }, new[] { 1d } };
            double[][] am = new[] { new[] { 1d }, new[] { 0d } };

            var normPairs = dynamicReorderingAlgorithm.CalculateL2NormPairs(binaryOutputs, am);

            Assert.AreEqual(4, normPairs.Count);
            AssertL2PairIsEqual(1, 0, 0, normPairs[0]);
            AssertL2PairIsEqual(0, 0, 1, normPairs[1]);
            AssertL2PairIsEqual(0, 1, 0, normPairs[2]);
            AssertL2PairIsEqual(1, 1, 1, normPairs[3]);
        }

        [TestMethod]
        public void TestFindBestReorderingPairsInEntireDataSet()
        {
            var normPairs = new List<L2NormPair>()
                {
                    new L2NormPair { L2Norm = 1, BinaryOutputIndex = 0, SnippetIndex = 0 }, 
                    new L2NormPair { L2Norm = 2, BinaryOutputIndex = 0, SnippetIndex = 1 }, 
                    new L2NormPair { L2Norm = 3, BinaryOutputIndex = 1, SnippetIndex = 0 }, 
                    new L2NormPair { L2Norm = 4, BinaryOutputIndex = 1, SnippetIndex = 1 }
                };

            var bestPairs = dynamicReorderingAlgorithm.FindBestReorderingPairs(normPairs);

            Assert.AreEqual(2, bestPairs.Count);
            AssertBestReorderingPair(0, 0, bestPairs[0]);
            AssertBestReorderingPair(1, 1, bestPairs[1]);
        }

        private static void AssertBestReorderingPair(int binaryOutputIndex, int snippetIndex, BestReorderingPair bestPair)
        {
            Assert.AreEqual(binaryOutputIndex, bestPair.BinaryOutputIndex);
            Assert.AreEqual(snippetIndex, bestPair.SnippetIndex);
        }

        private void AssertL2PairIsEqual(double norm, int binaryIndex, int snippetIndex, L2NormPair pair)
        {
            Assert.AreEqual(norm, pair.L2Norm);
            Assert.AreEqual(binaryIndex, pair.BinaryOutputIndex);
            Assert.AreEqual(snippetIndex, pair.SnippetIndex);
        }
    }
}
