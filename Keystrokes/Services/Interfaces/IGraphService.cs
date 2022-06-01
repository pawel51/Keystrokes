using Keystrokes.Models.KnnGraph;
using KeystrokesData.Entities;
using KeystrokesData.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keystrokes.Services.Interfaces
{
    public interface IGraphService
    {
        public bool AddGraphToDb(KnnGraph graph, DistanceMetric metric);
        public KnnGraph GetKnnGraph(DistanceMetric metric);

        public KnnGraph CreateGraph(List<TrainSample> trainData, DistanceMetric metric);

        public bool UpdateGraphToDb(KnnGraph graph, DistanceMetric metric);

        public bool UpdateGraph(TrainSample trainSample, KnnGraph knnGraph, DistanceMetric metric);

        public KnnNode TrainSampleToKnnNode(TrainSample trainSample);

        public KnnNode TestSampleToKnnNode(TestSample testSample);

    }
}
