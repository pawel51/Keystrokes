using Keystrokes.Models.KnnGraph;
using KeystrokesData;
using KeystrokesData.Entities;
using KeystrokesData.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keystrokes.Services
{
    public class GraphService : IGraphService
    {
        private readonly KeystrokesDbContext context;

        public GraphService(KeystrokesDbContext context)
        {
            this.context = context;
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

            context.GraphData.Add(knnGraph);
                                    
            context.SaveChanges();
            return true;
        }

        
        public bool UpdateGraphToDb(KnnGraph graph, DistanceMetric metric)
        {
            KnnGraphEntity? knnGraph = ConvertToEntity(graph, metric);
            if (knnGraph == null) return false;

            context.GraphData.Update(knnGraph);
            context.SaveChanges();
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

            KnnGraphEntity graphEntity = context.GraphData.FirstOrDefault(m => m.Name == "MainModel" + metric.ToString());
            if (graphEntity == null)
            {
                graphEntity = new KnnGraphEntity()
                {
                    Name = "MainModel" + metric.ToString(),
                    Edges = edges,
                    Nodes = nodes
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
        public KnnGraph GetKnnGraph()
        {
            KnnGraph knnGraph = new KnnGraph();
            Dictionary<string, KnnNode> nodes = new Dictionary<string, KnnNode>();
            Dictionary<string, KnnEdge> edges = new Dictionary<string, KnnEdge>();

            KnnGraphEntity? entity = context.GraphData.Include(m => m.Edges).Include(m => m.Nodes).FirstOrDefault();
            if (entity == null) return knnGraph;

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
                Edges = edges
            };

            AddGraphToDb(newGraph, DistanceMetric.EUCLIDIAN);

            return newGraph;
        }

        private double CalcDistance(KnnNode node1, KnnNode node2, DistanceMetric metric)
        {

            switch (metric)
            {
                case DistanceMetric.EUCLIDIAN:
                    return CalcEuclidianDistance(node1.Keystrokes, node2.Keystrokes);
                    break;
                //case (DistanceMetric.MANHATAN):

                //    break;
                //case (DistanceMetric.CHEBYSHEV):

                //    break;
                //case (DistanceMetric.MINKOWSKI):

                //    break;
                //case (DistanceMetric.HAMING):

                //    break;
                //case (DistanceMetric.COSINE):

                //    break;
                default:
                    return 0;
                    break;
            }
        }


        private double CalcEuclidianDistance(Dictionary<string, (double dwell, double flight)> keystrokes1, Dictionary<string, (double dwell, double flight)> keystrokes2)
        {


            double sum = 0;
            keystrokes1.ToList().ForEach(k1 =>
            {
                (double dwell, double flight) v1 = k1.Value;

                (double dwell, double flight) v2;
                if (keystrokes2.TryGetValue(k1.Key, out v2))
                {
                    sum += Math.Pow(v1.dwell - v2.dwell, 2);
                    //sum += Math.Pow(v1.flight - v2.flight, 2);
                }
            });

            return Math.Sqrt(sum);

        }





        public bool UpdateGraph(TrainSample trainSample, KnnGraph graph)
        {
            if (graph.Nodes == null || graph.Edges == null) return false;

            Dictionary<string, (double dwell, double flight)> keystrokes = new Dictionary<string, (double dwell, double flight)>();

            if (trainSample.Probes == null) return false;
            trainSample.Probes.ForEach(probe =>
            {
                keystrokes.Add(probe.AsciiSign, (probe.Dwell, probe.Flight));
            });

            if (trainSample.Category == null)
                trainSample.Category = "undefined";

            KnnNode newNode = new KnnNode()
            {
                Category = trainSample.Category,
                Keystrokes = keystrokes
            };

            graph.Nodes.ToList().ForEach(node =>
            {
                if (!(graph.Edges.ContainsKey(node.Key + newNode.Category) || graph.Edges.ContainsKey(newNode.Category + node.Key)))
                {
                    graph.Edges.Add(node.Key + newNode.Category, new KnnEdge()
                    {
                        Distance = CalcDistance(newNode, node.Value, DistanceMetric.EUCLIDIAN)
                    });
                }
            });

            if (!graph.Nodes.ContainsKey(trainSample.Category))
            {
                graph.Nodes.Add(trainSample.Category, newNode);
            }
                
            return true;
        }


        #endregion
    }
}
