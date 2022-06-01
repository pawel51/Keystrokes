using Keystrokes.Models.KnnGraph;
using Keystrokes.Services.Interfaces;
using KeystrokesData;
using KeystrokesData.Entities;
using KeystrokesData.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Keystrokes.Services.Impl
{
    public class GraphService : IGraphService
    {
        private readonly KeystrokesDbContext context;
        private readonly List<DistanceMetric> distanceMetricList;
        public GraphService(KeystrokesDbContext context)
        {
            this.context = context;
            distanceMetricList = new List<DistanceMetric>();
            distanceMetricList.Add(DistanceMetric.HAMING);
            distanceMetricList.Add(DistanceMetric.EUCLIDIAN);
            distanceMetricList.Add(DistanceMetric.COSINE);
            distanceMetricList.Add(DistanceMetric.MANHATAN);
            distanceMetricList.Add(DistanceMetric.MINKOWSKI);
            distanceMetricList.Add(DistanceMetric.CHEBYSHEV);
            distanceMetricList.Add(DistanceMetric.JACCARD);
            distanceMetricList.Add(DistanceMetric.CHISQUARE);
        }

        /// <summary>
        /// Saves InMemory KnnGraph model to database
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="metric"></param>
        /// <returns></returns>
        public bool AddGraphToDb(KnnGraph graph, DistanceMetric metric)
        {
            KnnGraphEntity knnGraph = ConvertToEntity(graph, metric);
            KnnGraphEntity graphEntity = context.GraphData.FirstOrDefault(m => m.Metric == metric);
            if (graphEntity != null)
            {
                MessageBox.Show("Graph with that metric already exists, try to change metric");
                return false;
            }
            context.GraphData.Add(knnGraph);
                                    
            context.SaveChanges();
            return true;
        }

        
        public bool UpdateGraphToDb(KnnGraph graph, DistanceMetric metric)
        {
            KnnGraphEntity? knnGraph = null;
            distanceMetricList.ForEach(m =>
            {
                knnGraph = ConvertToEntity(graph, m);
                if (knnGraph != null)
                {
                    context.GraphData.Update(knnGraph);
                }
            });
            
            context.SaveChanges();
            return true;
        }

        public bool UpdateGraph(TrainSample trainSample, KnnGraph graph, DistanceMetric metric)
        {
            if (graph.Nodes == null || graph.Edges == null) return false;
            if (graph.Nodes.Any(node => node.Key == trainSample.Category)) return false;

            Dictionary<string, (double dwell, double flight)> keystrokes = new Dictionary<string, (double dwell, double flight)>();

            KnnNode newNode = TrainSampleToKnnNode(trainSample, keystrokes);
            if (newNode == null) return false;


            graph.Nodes.ToList().ForEach(node =>
            {
                if (!(graph.Edges.ContainsKey(node.Key + newNode.Category) || graph.Edges.ContainsKey(newNode.Category + node.Key)))
                {
                    graph.Edges.Add(node.Key + newNode.Category, new KnnEdge()
                    {
                        Distance = CalcDistance(newNode, node.Value, metric)
                    });
                }
            });

            if (!graph.Nodes.ContainsKey(trainSample.Category))
            {
                graph.Nodes.Add(trainSample.Category, newNode);
            }

            return true;
        }

        private KnnGraphEntity ConvertToEntity(KnnGraph graph, DistanceMetric metric)
        {
            List<KnnEdgeEntity> edges = new List<KnnEdgeEntity>();
            List<KnnNodeEntity> nodes = new List<KnnNodeEntity>();

            graph.Edges.ToList().ForEach(edge =>
            {
                edges.Add(new KnnEdgeEntity()
                {
                    Distance = edge.Value.Distance,
                    Key = edge.Key
                });
            });

            graph.Nodes.ToList().ForEach(node =>
            {
                nodes.Add(new KnnNodeEntity() { Category = node.Value.Category });
            });

            KnnGraphEntity graphEntity = context.GraphData.Include(g => g.Nodes).Include(g => g.Edges).FirstOrDefault(m => m.Metric == metric);
            if (graphEntity == null)
            {
                graphEntity = new KnnGraphEntity()
                {
                    Name = "MainModel" + metric.ToString(),
                    Edges = edges,
                    Nodes = nodes,
                    Metric = metric
                };
            }
            else
            {
                graphEntity.Edges.Clear();
                graphEntity.Edges.AddRange(edges);
                graphEntity.Nodes.Clear();
                graphEntity.Nodes.AddRange(nodes);
            }

            return graphEntity;
        }

        /// <summary>
        /// Gets saved KnnGraph model from database
        /// </summary>
        /// <returns></returns>
        public KnnGraph GetKnnGraph(DistanceMetric metric)
        {
            KnnGraph knnGraph = new KnnGraph();
            Dictionary<string, KnnNode> nodes = new Dictionary<string, KnnNode>();
            Dictionary<string, KnnEdge> edges = new Dictionary<string, KnnEdge>();

            KnnGraphEntity? entity = context.GraphData.Include(m => m.Edges).Include(m => m.Nodes).FirstOrDefault(graph => graph.Metric == metric);
            if (entity == null) return null;

            entity.Edges.ForEach(edge =>
            {
                edges.Add(edge.Key, new KnnEdge() { Distance = edge.Distance });
            });

            List<TrainSample> trainData = context.TrainData.Include(m => m.Probes).ToList();

            entity.Nodes.ForEach(node =>
            {
                Dictionary<string, (double dwell, double flight)> keystrokes = new Dictionary<string, (double dwell, double flight)>();

                trainData.Where(e => e.Category == node.Category).ToList().ForEach(sample =>
                {
                    sample.Probes.ForEach(probe =>
                    {
                        if (!keystrokes.ContainsKey(probe.AsciiSign))
                            keystrokes.Add(probe.AsciiSign, (probe.Dwell, probe.Flight));
                    });
                });

                if (!nodes.ContainsKey(node.Category))
                {
                    nodes.Add(node.Category, new KnnNode()
                    {
                        Category = node.Category,
                        Keystrokes = keystrokes
                    });
                }
            });

            knnGraph.Nodes = nodes;
            knnGraph.Edges = edges;

            return knnGraph;
        }

        #region KNN GRAPH

        /// <summary>
        /// Creates InMemory Graph from training samples from database
        /// </summary>
        /// <param name="trainData"></param>
        /// <param name="metric"></param>
        /// <returns></returns>
        public KnnGraph CreateGraph(List<TrainSample> trainData, DistanceMetric metric)
        {
            
            // Graph:
            // Nodes:
            Dictionary<string, KnnNode> nodes = new Dictionary<string, KnnNode>();

            // Edges:
            Dictionary<string, KnnEdge> edges = new Dictionary<string, KnnEdge>();

            // prepare nodes
            trainData.ForEach(sample =>
            {
                if (!nodes.ContainsKey(sample.Category))
                {
                    Dictionary<string, (double dwell, double flight)> keystrokes = new Dictionary<string, (double dwell, double flight)>();
                    if (sample.Probes != null)
                    {
                        sample.Probes.ForEach(probe =>
                        {
                            keystrokes.Add(probe.AsciiSign, (probe.Dwell, probe.Flight));
                        });
                    }

                    if (sample.Category == null)
                        sample.Category = "undefined";

                    nodes.Add(sample.Category, new KnnNode()
                    {
                        Category = sample.Category,
                        Keystrokes = keystrokes
                    });
                }
            });

            //prepare edges
            nodes.ToList().ForEach(node1 =>
            {
                nodes.ToList().ForEach(node2 =>
                {

                    // jeżeli nie ma jeszcze takie krawędzi i nie licz krawędzi sam do siebie
                    if (!(edges.ContainsKey(node1.Key + node2.Key) || edges.ContainsKey(node2.Key + node1.Key)) 
                          && node1.Key != node2.Key)
                    {
                        edges.Add(node1.Key + node2.Key, new KnnEdge()
                        {
                            Distance = CalcDistance(node1.Value, node2.Value, metric)
                        });
                    }
                });
            });

            KnnGraph newGraph = new KnnGraph()
            {
                Name = "MainModel" + metric.ToString(),
                Nodes = nodes,
                Edges = edges,
               
            };

            AddGraphToDb(newGraph, metric);

            return newGraph;
        }

        private double CalcDistance(KnnNode node1, KnnNode node2, DistanceMetric metric)
        {

            switch (metric)
            {
                case DistanceMetric.EUCLIDIAN:
                    return CalcEuclidianDistance(node1.Keystrokes, node2.Keystrokes);
                case (DistanceMetric.MANHATAN):
                    return CalcManhatanDistance(node1.Keystrokes, node2.Keystrokes);
                case (DistanceMetric.CHEBYSHEV):
                    return CalcChebyshevDistance(node1.Keystrokes, node2.Keystrokes);
                case (DistanceMetric.MINKOWSKI):
                    return CalcMinkowskiDistance(node1.Keystrokes, node2.Keystrokes);
                case (DistanceMetric.HAMING):
                    return CalcHammingDistance(node1.Keystrokes, node2.Keystrokes);
                case (DistanceMetric.COSINE):
                    return CalcCosineDistance(node1.Keystrokes, node2.Keystrokes);
                case (DistanceMetric.JACCARD):
                    return CalcJakardDistance(node1.Keystrokes, node2.Keystrokes);
                case (DistanceMetric.CHISQUARE):
                    return CalcChiSquareDistance(node1.Keystrokes, node2.Keystrokes);
                default:
                    return CalcEuclidianDistance(node1.Keystrokes, node2.Keystrokes);
            }
        }


        private double CalcEuclidianDistance(Dictionary<string, (double dwell, double flight)> keystrokes1, Dictionary<string, (double dwell, double flight)> keystrokes2)
        {


            double sum = 0;
            int counter = 0;
            keystrokes1.ToList().ForEach(k1 =>
            {
                (double dwell, double flight) v1 = k1.Value;

                (double dwell, double flight) v2;
                if (keystrokes2.TryGetValue(k1.Key, out v2))
                {
                    double d, f;
                    d = Math.Pow(v1.dwell - v2.dwell, 2);
                    f = Math.Pow(v1.flight - v2.flight, 2);
                    sum += Math.Sqrt(d + f);
                    counter++;
                }
            });

            return sum / counter;

        }

        private double CalcManhatanDistance(Dictionary<string, (double dwell, double flight)> keystrokes1, Dictionary<string, (double dwell, double flight)> keystrokes2)
        {


            double sum = 0;
            int counter = 0;
            keystrokes1.ToList().ForEach(k1 =>
            {
                (double dwell, double flight) v1 = k1.Value;

                (double dwell, double flight) v2;
                if (keystrokes2.TryGetValue(k1.Key, out v2))
                {
                    sum += Math.Abs(v1.dwell - v2.dwell);
                    sum += Math.Abs(v1.flight - v2.flight);
                    counter++;
                }
            });

            return sum / counter;

        }

        private double CalcMinkowskiDistance(Dictionary<string, (double dwell, double flight)> keystrokes1, Dictionary<string, (double dwell, double flight)> keystrokes2)
        {


            double sum = 0;
            int counter = 0;
            keystrokes1.ToList().ForEach(k1 =>
            {
                (double dwell, double flight) v1 = k1.Value;

                (double dwell, double flight) v2;
                if (keystrokes2.TryGetValue(k1.Key, out v2))
                {
                    double d, f;
                    d = Math.Pow(Math.Abs(v1.dwell - v2.dwell), 3);
                    f = Math.Pow(Math.Abs(v1.flight - v2.flight), 3);
                    sum += Math.Pow(sum, 1.0 / 3);
                    counter++;
                }
            });

            return sum / counter;

        }

        private double CalcCosineDistance(Dictionary<string, (double dwell, double flight)> keystrokes1, Dictionary<string, (double dwell, double flight)> keystrokes2)
        {


            double sum = 0;
            double counter = 0;
            keystrokes1.ToList().ForEach(k1 =>
            {
                (double dwell, double flight) v1 = k1.Value;

                (double dwell, double flight) v2;
                if (keystrokes2.TryGetValue(k1.Key, out v2))
                {
                    double sumX = 0, sumY = 0, numerator = 0;
                    numerator = v1.dwell * v2.dwell + v1.flight * v2.flight;
                    sumX = Math.Pow(v1.dwell, 2) + Math.Pow(v1.flight, 2);
                    sumY = Math.Pow(v2.dwell, 2) + Math.Pow(v2.flight, 2);
                    double denominator = Math.Round(Math.Sqrt(sumX * sumY));
                    sum += numerator / denominator;
                    counter++;
                }
            });

            return sum / counter;

        }

        private double CalcHammingDistance(Dictionary<string, (double dwell, double flight)> keystrokes1, Dictionary<string, (double dwell, double flight)> keystrokes2)
        {


            double sum = 0;
            int counter = 0;
            keystrokes1.ToList().ForEach(k1 =>
            {
                (double dwell, double flight) v1 = k1.Value;

                (double dwell, double flight) v2;
                if (keystrokes2.TryGetValue(k1.Key, out v2))
                {
                    double sumi = 0;
                    if ((int)Math.Round(v1.dwell) == (int)Math.Round(v2.dwell))
                        sumi++;
                    if ((int)Math.Round(v1.flight) == (int)Math.Round(v2.flight))
                        sumi++;
                    sumi /= 2;
                    sum += 1.01 - sumi;
                    counter++;
                }
            });

            return sum / counter;

        }

        private double CalcChebyshevDistance(Dictionary<string, (double dwell, double flight)> keystrokes1, Dictionary<string, (double dwell, double flight)> keystrokes2)
        {


            double sum = 0;
            int counter = 0;
            keystrokes1.ToList().ForEach(k1 =>
            {
                (double dwell, double flight) v1 = k1.Value;

                (double dwell, double flight) v2;
                if (keystrokes2.TryGetValue(k1.Key, out v2))
                {
                    sum += Math.Max(Math.Abs(v1.dwell - v2.dwell), Math.Abs(v1.flight - v2.flight));
                    counter++;
                }
            });

            return sum / counter;

        }

        private double CalcJakardDistance(Dictionary<string, (double dwell, double flight)> keystrokes1, Dictionary<string, (double dwell, double flight)> keystrokes2)
        {


            double sum = 0;
            int counter = 0;
            keystrokes1.ToList().ForEach(k1 =>
            {
                (double dwell, double flight) v1 = k1.Value;

                (double dwell, double flight) v2;
                if (keystrokes2.TryGetValue(k1.Key, out v2))
                {
                    double sumi = 0;
                    if ((int)Math.Round(v1.dwell) == (int)Math.Round(v2.dwell))
                        sumi++;
                    else if ((int)Math.Round(v1.dwell) == (int)Math.Round(v2.flight))
                        sumi++;
                    if((int)Math.Round(v1.flight) == (int)Math.Round(v2.flight))
                        sumi++;
                    else if((int)Math.Round(v1.flight) == (int)Math.Round(v2.dwell))
                        sumi++;
                    sum += 1.01 - 2 / sumi;
                }
            });

            return sum / counter;

        }

        private double CalcChiSquareDistance(Dictionary<string, (double dwell, double flight)> keystrokes1, Dictionary<string, (double dwell, double flight)> keystrokes2)
        {


            double sum = 0;
            int counter = 0;
            keystrokes1.ToList().ForEach(k1 =>
            {
                (double dwell, double flight) v1 = k1.Value;

                (double dwell, double flight) v2;
                if (keystrokes2.TryGetValue(k1.Key, out v2))
                {
                    double d, f;
                    d = Math.Pow(v1.dwell - v2.dwell, 2) / (v1.dwell + v2.dwell);
                    f = Math.Pow(v1.flight - v2.flight, 2) / (v1.flight + v2.flight);
                    
                    sum += 0.5 * (d + f);
                    counter++;
                }
            });

            return sum / counter;

        }

        public KnnNode TrainSampleToKnnNode(TrainSample trainSample)
        {
            Dictionary<string, (double dwell, double flight)> keystrokes = new Dictionary<string, (double dwell, double flight)>();

            return TrainSampleToKnnNode(trainSample, keystrokes);
        }

        public KnnNode TestSampleToKnnNode(TestSample testSample)
        {
            Dictionary<string, (double dwell, double flight)> keystrokes = new Dictionary<string, (double dwell, double flight)>();

            return TestSampleToKnnNode(testSample, keystrokes);
        }

        

        private KnnNode TrainSampleToKnnNode(TrainSample trainSample, Dictionary<string, (double dwell, double flight)> keystrokes)
        {
            if (trainSample.Probes == null) return null;
            trainSample.Probes.ForEach(probe =>
            {
                keystrokes.Add(probe.AsciiSign, (probe.Dwell, probe.Flight));
            });

            if (trainSample.Category == null)
                trainSample.Category = "undefined";

            return new KnnNode()
            {
                Category = trainSample.Category,
                Keystrokes = keystrokes
            };
        }

        private KnnNode TestSampleToKnnNode(TestSample testSample, Dictionary<string, (double dwell, double flight)> keystrokes)
        {
            if (testSample.Probes == null) return null;
            testSample.Probes.ForEach(probe =>
            {
                keystrokes.Add(probe.AsciiSign, (probe.Dwell, probe.Flight));
            });

            if (testSample.Category == null)
                testSample.Category = "undefined";

            return new KnnNode()
            {
                Category = testSample.Category,
                Keystrokes = keystrokes
            };
        }


        #endregion
    }
}
