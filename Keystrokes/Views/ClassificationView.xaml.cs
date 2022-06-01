using Keystrokes.Models.KnnGraph;
using Keystrokes.Services.Interfaces;
using Keystrokes.ViewModels;
using KeystrokesData.Entities;
using KeystrokesData.Enums;
using LiveCharts;
using LiveCharts.Defaults;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Keystrokes.Views
{
    /// <summary>
    /// Logika interakcji dla klasy ClassificationView.xaml
    /// </summary>
    public partial class ClassificationView : UserControl
    {
        public List<Ellipse> Ellipses { get; set; }
        public ClassificationView(IKeystrokeService keystrokeService, IGraphService graphService, IKnnClassificatorService classificationService)
        {
            InitializeComponent();
            DataContext = new ClassificationViewModel(keystrokeService, graphService, classificationService);
            this.keystrokeService = keystrokeService;
            this.graphService = graphService;
            this.classificationService = classificationService;

            canvasBorderDwell.Height = canvasDwell.Height + 4;
            canvasBorderDwell.Width = canvasDwell.Width + 4;

            canvasBorderFlight.Height = canvasFlight.Height + 4;
            canvasBorderFlight.Width = canvasFlight.Width + 4;

            Scroll1.ScrollToHorizontalOffset(canvasDwell.Width / 2);
            Scroll2.ScrollToHorizontalOffset(canvasFlight.Width / 2);

            //Label label = new Label();

            //List<Ellipse> Ellipses = new List<Ellipse>();
            //for (int i = 0; i < 100; i++)
            //{
            //    Ellipses.Add(new Ellipse()
            //    {
            //        Width = 60,
            //        Height = 40,
            //        Fill = Brushes.Red
            //    });
            //}
            //for (int i = 0; i < 10; i++)
            //{
            //    for (int j = 0; j < 10; j++)
            //    {

            //        Canvas.SetLeft(Ellipses[i * 10 + j], j * 80);
            //        Canvas.SetTop(Ellipses[i * 10 + j], i * 60);

            //        canvas.Children.Add(Ellipses[i * 10 + j]);

            //        Label label1 = new Label()
            //        {
            //            Content = $"E = {i * 10 + j}",
            //            Foreground = Brushes.Black
            //        };
            //        Canvas.SetLeft(label1, j * 80);
            //        Canvas.SetTop(label1, i * 60);

            //        canvas.Children.Add(label1);
            //    }
            //}
        }

        private readonly IKeystrokeService keystrokeService;
        private readonly IGraphService graphService;
        private readonly IKnnClassificatorService classificationService;

        private void ReadTestData_Click(object sender, RoutedEventArgs e)
        {
            ClassificationViewModel dc = (ClassificationViewModel)DataContext;
            dc.TestSamples.Clear();
            List<TestSample> samples = keystrokeService.ReadTestingData("ss");
            foreach (TestSample sample in samples)
            {
                dc.TestSamples.Add(sample);
            }
        }


        private void TrainData_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string? headername = e.Column.Header.ToString();
            if (headername != null && headername == "Probes")
            {
                e.Cancel = true;
            }
        }

        private void Classify_BtnClicked(object sender, RoutedEventArgs e)
        {
            ClassificationViewModel dc = (ClassificationViewModel)DataContext;

            if (!Validation(dc)) return;

            switch (dc.SelectedAlgorithm)
            {
                case Algorithm.KNN:
                    KnnClassification(dc);
                    break;
                case Algorithm.KMEANS:
                    KMeansClassification(dc);
                    break;
                case Algorithm.BAYES:
                    BayesClassification(dc);
                    break;
                case Algorithm.DECISSIONTREES:
                    TreeClassification(dc);
                    break;
            }

            

        }

        private void KMeansClassification(ClassificationViewModel dc)
        {
            List<TestSample> testSamples = TestDataGrid.SelectedItems.Cast<TestSample>().ToList();
            List<Dictionary<string, double>> outcomes = new List<Dictionary<string, double>>();

            int correctClassifications = 0;
            testSamples.ForEach(sample =>
            {
                KnnNode knnNode = graphService.TestSampleToKnnNode(sample);
                graphService.UpdateGraph(new TrainSample(sample), dc.GraphModel, dc.SelectedMetric);
                Dictionary<string, double> keyProbList = classificationService.FindNearestMean(dc.GraphModel, knnNode, dc.KParam, canvasKnn);
                // key of the greatest value
                correctClassifications = CheckAccuracy(sample, correctClassifications, keyProbList);

                outcomes.Add(keyProbList);

            });

            // updates treedecission's accuracy on the chart 
            UpdateChart(dc, outcomes, correctClassifications, 1);

            LogOutcomes(dc, outcomes);

            dc.KmeansResults = outcomes[0];
        }

        private void BayesClassification(ClassificationViewModel dc)
        {
            List<TestSample> testSamples = TestDataGrid.SelectedItems.Cast<TestSample>().ToList();
            List<Dictionary<string, double>> outcomes = new List<Dictionary<string, double>>();

            int correctClassifications = 0;
            testSamples.ForEach(sample =>
            {
                KnnNode knnNode = graphService.TestSampleToKnnNode(sample);
                graphService.UpdateGraph(new TrainSample(sample), dc.GraphModel, dc.SelectedMetric);
                Dictionary<string, double> keyProbList = classificationService.FindMostLikelyClassBayes(dc.GraphModel, knnNode, dc.DwellBayes, dc.FlightBayes, canvasKnn);
                // key of the greatest value
                correctClassifications = CheckAccuracy(sample, correctClassifications, keyProbList);

                outcomes.Add(keyProbList);

            });

            // updates treedecission's accuracy on the chart 
            UpdateChart(dc, outcomes, correctClassifications, 2);

            LogOutcomes(dc, outcomes);

            dc.BayesResults = outcomes[0];
        }

        private void TreeClassification(ClassificationViewModel dc)
        {
            List<TestSample> testSamples = TestDataGrid.SelectedItems.Cast<TestSample>().ToList();
            List<Dictionary<string, double>> outcomes = new List<Dictionary<string, double>>();

            int correctClassifications = 0;
            testSamples.ForEach(sample =>
            {
                KnnNode knnNode = graphService.TestSampleToKnnNode(sample);
                graphService.UpdateGraph(new TrainSample(sample), dc.GraphModel, dc.SelectedMetric);
                Dictionary<string, double> keyProbList = classificationService.TreeDecisions(dc.GraphModel, knnNode, dc.ProbThreshold * 1.0 / 100, canvasDwell, canvasFlight);
                // key of the greatest value
                correctClassifications = CheckAccuracy(sample, correctClassifications, keyProbList);

                outcomes.Add(keyProbList);

            });

            // updates treedecission's accuracy on the chart 
            UpdateChart(dc, outcomes, correctClassifications, 3);

            LogOutcomes(dc, outcomes);

            dc.TreeResults = outcomes[0];
        }

        private void ClassifyKnn_BtnClicked(object sender, RoutedEventArgs e)
        {
            ClassificationViewModel dc = (ClassificationViewModel)DataContext;

            if (!Validation(dc)) return;

            KnnClassification(dc);

        }

        private void KnnClassification(ClassificationViewModel dc)
        {
            List<TestSample> testSamples = TestDataGrid.SelectedItems.Cast<TestSample>().ToList();

            List<Dictionary<string, double>> outcomes = new List<Dictionary<string, double>>();

            int correctClassifications = 0;
            testSamples.ForEach(sample =>
            {
                KnnNode knnNode = graphService.TestSampleToKnnNode(sample);
                TrainSample trainSample = new TrainSample(sample);
                graphService.UpdateGraph(trainSample, dc.GraphModel, dc.SelectedMetric);
                Dictionary<string, double> keyProbList = classificationService.FindMostCommonKnn(dc.GraphModel, knnNode, dc.KParam, canvasKnn);
                // key of the greatest value
                correctClassifications = CheckAccuracy(sample, correctClassifications, keyProbList);

                outcomes.Add(keyProbList);

            });

            // updates treedecission's accuracy on the chart 
            UpdateChart(dc, outcomes, correctClassifications, 0);

            LogOutcomes(dc, outcomes);

            dc.KnnResults = outcomes[0];
        }

        private int CheckAccuracy(TestSample sample, int correctClassifications, Dictionary<string, double> keyProbList)
        {
            if (keyProbList.Count == 0)
            {
                MessageBox.Show("Graph model was not updated with node to classify");
                return correctClassifications;
            }

            string classString = keyProbList.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            string categoryToVerify = sample.Category.Split('_')[0];

            if (categoryToVerify != null && String.Equals(categoryToVerify, classString))
                correctClassifications++;
            return correctClassifications;
        }

        private static void UpdateChart(ClassificationViewModel dc, List<Dictionary<string, double>> outcomes, int correctClassifications, int barNumber)
        {
            dc.AccuracySeries[0].Values.Cast<ObservableValue>().ToList()[barNumber].Value
                            = Math.Round(correctClassifications * 1.0 / outcomes.Count, 2);
        }

        private bool Validation(ClassificationViewModel dc)
        {
            if (dc.GraphModel == null) return false;

            if (TestDataGrid.SelectedItems == null || TestDataGrid.SelectedItems.Count == 0)
            {
                MessageBox.Show("You must select samples first");
                return false;
            }
            return true;
        }

        private void LogOutcomes(ClassificationViewModel dc, List<Dictionary<string, double>> outcomes)
        {
            outcomes.ForEach(outcome =>
            {
                outcome.ToList().ForEach(keyprobe =>
                {
                    Log.Information($"Algorithm KNN: Test Sample '{dc.TestSamples.Last().Category}' was classified to\n{keyprobe.Key} with probability = '{keyprobe.Value}'\n");
                });
            });
        }
    }
}
