using Keystrokes.Helpers;
using Keystrokes.Models;
using Keystrokes.Models.KnnGraph;
using Keystrokes.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Keystrokes.Services.Impl
{
    public class KnnClassificationService : IKnnClassificatorService
    {
        public List<string> FindMostCommonKnn(KnnGraph graph, KnnNode node, int k)
        {
            throw new NotImplementedException();
        }

        public List<(string klass, double prob)> FindMostLikelyClassBayes(KnnGraph graph, KnnNode node, int d1, int d2)
        {
            throw new NotImplementedException();
        }

        public List<string> FindNearestMean(KnnGraph graph, KnnNode node, int k)
        {
            throw new NotImplementedException();
        }


        internal class Group{

            public int Id { get; set; }
            public double Min { get; set; }
            public double Max { get; set; }
            public List<(string klass, double value)> Items { get; set; }
        }

        public Dictionary<string, double> TreeDecisions(KnnGraph graph, KnnNode node, double probThreshold, Canvas canvasDwell, Canvas canvasFlight)
        {
            

            // dictionary of all distinct classes and count of probes in it
            // should be greater than 3, otherway algorithm will not make progress in probability while going down the tree
            Dictionary<string, int> klasses = new Dictionary<string, int>();

            List<(string key, double dwell)> meanDwellList = CalcMeanDwells(graph, klasses);
            List<(string key, double flight)> meanFlightList = CalcMeanFlights(graph, klasses);

            meanDwellList.Sort((x, y) => x.dwell.CompareTo(y.dwell));
            meanFlightList.Sort((x, y) => x.flight.CompareTo(y.flight));


            TreeDairy dairyDwell = new TreeDairy();
            TreeDairy dairyFlight = new TreeDairy();

            int maxDeepLevel = 3;

            int deep = 1;
            // devide on even groups
            Dictionary<string, double> klassProbList = DoDecisionTree(node, probThreshold, klasses, meanDwellList, maxDeepLevel, deep, dairyDwell);
            Dictionary<string, double> klassProbList2 = DoDecisionTree(node, probThreshold, klasses, meanFlightList, maxDeepLevel, deep, dairyFlight);

            Dictionary<string, double> concatenatedDict = new Dictionary<string, double>();
            int dwellTimeMoreImportantRate = 2;
            klassProbList.ToList().ForEach(item =>
            {
                concatenatedDict.Add(item.Key, item.Value * dwellTimeMoreImportantRate);
            });

            klassProbList2.ToList().ForEach(item =>
            {
                if (concatenatedDict.ContainsKey(item.Key))
                {
                    double value = concatenatedDict[item.Key];
                    concatenatedDict.Remove(item.Key);
                    concatenatedDict.Add(item.Key, (value + item.Value) / (dwellTimeMoreImportantRate + 1));
                }
                else
                {
                    concatenatedDict.Add(item.Key, item.Value / (dwellTimeMoreImportantRate + 1));
                }
            });

            VisualiseDairy(dairyDwell, canvasDwell);
            VisualiseDairy(dairyFlight, canvasFlight);


            return klassProbList;
        }

        

        private Dictionary<string, double> DoDecisionTree(KnnNode node, double probThreshold, Dictionary<string, int> klasses, List<(string key, double dwell)> meanValueList, int maxDeepLevel, int deep, TreeDairy dairy)
        {
            // prepare returning list
            Dictionary<string, double> klassProbList = new Dictionary<string, double>();

            

            Dictionary<int, Group> groups = DevideOnEvenGroups(klasses, meanValueList);

            while (deep < maxDeepLevel)
            {


                Group pickedGroup = ClassifyToGroup(groups, node);

                dairy.SplitList.Add(deep, WriteToDairy(groups, pickedGroup));

                var grouped = pickedGroup.Items.GroupBy(x => x.klass).OrderByDescending(group => group.Count());

                if (grouped.First().Count() / pickedGroup.Items.Count() > probThreshold)
                {
                    foreach (var item in grouped.ToList())
                    {
                        if (!klassProbList.ContainsKey(item.Key))
                            klassProbList.Add(item.Key, Math.Round((double)item.Count() / pickedGroup.Items.Count(), 2));
                    }
                    return klassProbList;
                }

                // prepare for next split decission
                deep++;
                klasses = GetKlassesInGroup(pickedGroup);
                groups = DevideOnEvenGroups(klasses, pickedGroup.Items);

                // if it is too deep
                if (deep >= maxDeepLevel)
                {
                    foreach (var item in grouped.ToList())
                    {
                        if (!klassProbList.ContainsKey(item.Key))
                            klassProbList.Add(item.Key, Math.Round((double)item.Count() / pickedGroup.Items.Count(), 2));
                    }
                    return klassProbList;
                }
            }
            return klassProbList;
        }

        private List<TreeDairy.Split> WriteToDairy(Dictionary<int, Group> groups, Group pickedGroup)
        {
            List<TreeDairy.Split> splitList = new List<TreeDairy.Split>();
            int groupNumber = 0;
            groups.ToList().ForEach(groupItem =>
            {
                TreeDairy.Split split = new TreeDairy.Split()
                {
                    GroupId = groupNumber++,
                    ClassProbList = new Dictionary<string, double>(),
                    WasPicked = groupItem.Value.Id == pickedGroup.Id ? true : false
                };
                var groupedItem = groupItem.Value.Items.GroupBy(x => x.klass).OrderByDescending(group => group.Count());
                groupedItem.ToList().ForEach(g =>
                {
                    if (!split.ClassProbList.ContainsKey(g.Key))
                        split.ClassProbList.Add(g.Key, Math.Round((double)groupItem.Value.Items.Where(e => e.klass == g.Key).Count() / groupItem.Value.Items.Count(), 2));
                });
                splitList.Add(split);
            });
            return splitList;
        }

        private Group ClassifyToGroup(Dictionary<int, Group> groups, KnnNode node)
        {
            bool classified = false;
            double meanDwellNode = node.Keystrokes.Sum(s => s.Value.dwell) / node.Keystrokes.Count;

            for (int i = 0; groups.ContainsKey(i); i++)
            {
                if (meanDwellNode > groups[i].Min && meanDwellNode < groups[i].Max)
                {
                    Group group = new Group()
                    {
                        Items = new List<(string klass, double value)>(),
                        Min = i > 1 ? groups[i - 1].Min : groups[i].Min,
                        Max = i < groups.Count - 1 ? groups[i + 1].Max : groups[i].Max
                    };
                    group.Items.AddRange(groups[i].Items);
                    if (i > 1)
                    {
                        group.Min = groups[i - 1].Min;
                        group.Items.AddRange(groups[i - 1].Items);
                    }
                    else
                    {
                        group.Min = groups[i].Min;
                    }
                    if (i < groups.Count - 1)
                    {
                        group.Max = groups[i + 1].Max;
                        group.Items.AddRange(groups[i + 1].Items);
                    }
                    else
                    {
                        group.Max = groups[i].Max;
                    }
                    return group;
                }
                
            }
            if (meanDwellNode < groups[0].Min)
            {
                return groups[0];
            }
            else
            {
                return groups[groups.Count - 1];
            }
        }

        private Dictionary<int, Group> DevideOnEvenGroups(Dictionary<string, int> klasses, List<(string key, double dwell)> meanDwellList)
        {
            Dictionary<int, Group> groups = new Dictionary<int, Group>();

            int groupCapacity = (int)Math.Ceiling((double)(klasses.Count / 2));
            int i = 0;
            int classId = 0;
            while (i < meanDwellList.Count)
            {
                groups.Add(classId, new Group()
                {
                    Id = classId,
                    Items = new List<(string klass, double value)>(),
                    Min = 100000,
                    Max = 0,
                });
                for (int j = 0; j < groupCapacity && i < meanDwellList.Count; j++, i++)
                {
                    groups[classId].Items.Add((meanDwellList[i].key.Split('_')[0], meanDwellList[i].dwell));
                    groups[classId].Max = meanDwellList[i].dwell > groups[classId].Max?
                        meanDwellList[i].dwell : groups[classId].Max;
                    groups[classId].Min = groups[classId].Min > meanDwellList[i].dwell ?
                        meanDwellList[i].dwell : groups[classId].Min;
                }
                classId++;
            }

            return groups;
        }

        private List<(string key, double dwell)> CalcMeanDwells(KnnGraph graph, Dictionary<string, int> klasses)
        {
            List<(string key, double dwell)> meanDwellList = new List<(string, double)>();

            // calculate mean dwells
            graph.Nodes.ToList().ForEach(nod =>
            {
                double sum = nod.Value.Keystrokes.Select(k => k.Value.dwell).Sum();

                meanDwellList.Add((nod.Key, sum / nod.Value.Keystrokes.Count));

                string classKey = nod.Key.Split('_')[0];
                if (!klasses.ContainsKey(classKey))
                {
                    klasses.Add(classKey, 1);
                }
                else
                {
                    klasses[classKey]++;
                }
            });
            return meanDwellList;
        }

        private List<(string key, double flight)> CalcMeanFlights(KnnGraph graph, Dictionary<string, int> klasses)
        {
            List<(string key, double flight)> meanFlightList = new List<(string, double)>();

            // calculate mean dwells
            graph.Nodes.ToList().ForEach(nod =>
            {
                double sum = nod.Value.Keystrokes.Select(keystroke => keystroke.Value.flight).Sum();

                meanFlightList.Add((nod.Key, sum / nod.Value.Keystrokes.Count));

                string classKey = nod.Key.Split('_')[0];
                if (!klasses.ContainsKey(classKey))
                {
                    klasses.Add(classKey, 1);
                }
                else
                {
                    klasses[classKey]++;
                }
            });
            return meanFlightList;
        }

        private Dictionary<string, int> GetKlassesInGroup(Group group)
        {
            Dictionary<string, int> klasses = new Dictionary<string, int>();

            group.Items.ForEach(item =>
            {
                if (!klasses.ContainsKey(item.klass))
                    klasses.Add(item.klass, 1);
                else
                    klasses[item.klass]++;
            });

            return klasses;
        }

        

        private void VisualiseDairy(TreeDairy dairy, Canvas canvas)
        {
            Drawer d = new Drawer();

            canvas.Children.Clear();

            int rootLeft = 2200;
            int rootTop = 50;

            int nodeWidth = 50;
            int nodeHeight = 50;

            int labelWidth = 100;
            int labelHeight = 20;

            Brush color = Brushes.SteelBlue;

            Ellipse root = d.CreateEllipse(canvas, nodeWidth, nodeHeight, rootLeft, rootTop, color);

            int curLevel = 0;
            int curGroup = 0;

            List<KeyValuePair<int, List<TreeDairy.Split>>> tree = dairy.SplitList.ToList();


            int curLeft = rootLeft - tree[0].Value.Count() * (nodeWidth + 25);
            int curTop = rootTop + 100;
            int curRootLeft = rootLeft;
            int curRootTop = rootTop;
            int proxyRootLeft = rootLeft;
            int proxyRootTop = rootTop;

            (double x, double y) rootPoint = (rootLeft, rootTop);
            (double x, double y)[] childrenPoints = new (double x, double y)[tree[0].Value.Count()];


            for (int i = 0; i < tree.Count; i++)
            {
                for (int j = 0; j < tree[i].Value.Count(); j++)
                {
                    d.CreateLine(canvas, curRootLeft, curRootTop, curLeft, curTop, Brushes.Azure);

                    List<KeyValuePair<string, double>> classProbList = tree[i].Value[j].ClassProbList.ToList();
                    Brush elColor = Brushes.SteelBlue;
                    if (tree[i].Value[j].WasPicked)
                    {
                        elColor = Brushes.LightGoldenrodYellow;
                        proxyRootLeft = curLeft;
                        proxyRootTop = curTop;
                    }

                    Ellipse newEllipse = d.CreateEllipse(canvas, nodeWidth, nodeHeight, curLeft, curTop, elColor);
                    
                    for (int k = 0; k < classProbList.Count(); k++)
                    {
                        d.CreateLabel(canvas, labelWidth, labelHeight,
                            curLeft,
                            curTop + nodeHeight + k * (labelHeight + 5), 
                            Brushes.Black,
                            $"{classProbList[k].Key} -> {classProbList[k].Value}");
                    }

                    curLeft += 200;
                }
                if (i >= tree.Count - 1) break;
                curRootLeft = proxyRootLeft;
                curRootTop = proxyRootTop;
                curLeft = curRootLeft - tree[i + 1].Value.Count() * (nodeWidth + 25);
                curTop = curTop + 200;
            }

        }
    }
}
