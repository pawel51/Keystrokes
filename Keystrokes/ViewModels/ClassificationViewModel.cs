using Keystrokes.Models.KnnGraph;
using Keystrokes.Services.Interfaces;
using KeystrokesData.Entities;
using KeystrokesData.Enums;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Keystrokes.ViewModels
{
    public class ClassificationViewModel : BaseViewModel
    {
        
        public ClassificationViewModel(IKeystrokeService keystrokeService, IGraphService graphService, IKnnClassificatorService classService)
        {
            this.classService = classService;
            this.graphService = graphService;
            this.keystrokeService = keystrokeService;


            MetricList = new ObservableCollection<DistanceMetric>();
            MetricList.Add(DistanceMetric.HAMING);
            MetricList.Add(DistanceMetric.EUCLIDIAN);
            MetricList.Add(DistanceMetric.COSINE);
            MetricList.Add(DistanceMetric.MANHATAN);
            MetricList.Add(DistanceMetric.MINKOWSKI);
            MetricList.Add(DistanceMetric.CHEBYSHEV);

            AlgorithmsList = new ObservableCollection<Algorithm>();
            AlgorithmsList.Add(Algorithm.KNN);
            AlgorithmsList.Add(Algorithm.KMEANS);
            AlgorithmsList.Add(Algorithm.BAYES);
            AlgorithmsList.Add(Algorithm.DECISSIONTREES);

            AccuracyValues = new ChartValues<ObservableValue>();

            AccuracySeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Accuracy",
                    Values = AccuracyValues,
                    Fill = Brushes.Red
                }
            };

            Labels = new[] { "KNN", "KMEANS", "BAYES", "DECISSIONTREES" };
            Formatter = value => value.ToString("N");

            double[] accVals = new double[4] { 0, 0, 0, 0 };
            AccuracySeries[0].Values.AddRange(accVals.ToList().Select(v => new ObservableValue(v)));

            TestSamples = new ObservableCollection<TestSample>(keystrokeService.GetTestSamples());
            GraphModel = graphService.GetKnnGraph();

        }

        #region COLLECTIONS AND SELECTED VALUES

        private ObservableCollection<Algorithm> algorithmsList;

        public ObservableCollection<Algorithm> AlgorithmsList 
        {
            get { return algorithmsList; }
            set { algorithmsList = value; OnPropertyChanged(nameof(AlgorithmsList)); }
        }

        private Algorithm selectedAlgorithm;

        public Algorithm SelectedAlgorithm
        {
            get { return selectedAlgorithm; }
            set 
            { 
                selectedAlgorithm = value; 
                OnPropertyChanged(nameof(SelectedAlgorithm)); 
                switch (selectedAlgorithm)
                {
                    case Algorithm.KNN:
                        MetricVisibility = Visibility.Visible;
                        KParamVisibility = Visibility.Visible;
                        ProbThresholdVisibility = Visibility.Hidden;
                        FlightTimeVisibility = Visibility.Hidden;
                        DwellTimeVisibility = Visibility.Hidden;
                        break;
                    case Algorithm.KMEANS:
                        MetricVisibility = Visibility.Visible;
                        KParamVisibility = Visibility.Visible;
                        ProbThresholdVisibility = Visibility.Hidden;
                        FlightTimeVisibility = Visibility.Hidden;
                        DwellTimeVisibility = Visibility.Hidden;
                        break;
                    case Algorithm.BAYES:
                        MetricVisibility = Visibility.Visible;
                        KParamVisibility = Visibility.Hidden;
                        ProbThresholdVisibility = Visibility.Hidden;
                        FlightTimeVisibility = Visibility.Visible;
                        DwellTimeVisibility = Visibility.Visible;
                        break;
                    case Algorithm.DECISSIONTREES:
                        MetricVisibility = Visibility.Hidden;
                        KParamVisibility = Visibility.Hidden;
                        ProbThresholdVisibility = Visibility.Visible;
                        FlightTimeVisibility = Visibility.Hidden;
                        DwellTimeVisibility = Visibility.Hidden;
                        break;

                }
            }
        }


        private ObservableCollection<DistanceMetric> metricList;

        public ObservableCollection<DistanceMetric> MetricList
        {
            get { return metricList; }
            set { metricList = value; OnPropertyChanged(nameof(MetricList)); }
        }

        

        private DistanceMetric selectedMetric;

        public DistanceMetric SelectedMetric
        {
            get { return selectedMetric; }
            set { selectedMetric = value; OnPropertyChanged(nameof(SelectedMetric)); }
        }

        #endregion

        #region PARAMETERS

        private int kParam = 3;

        public int KParam
        {
            get { return kParam; }
            set { kParam = value; OnPropertyChanged(nameof(KParam)); }
        }

        private int flightBayes = 5;

        public int FlightBayes
        {
            get { return flightBayes; }
            set { flightBayes = value; OnPropertyChanged(nameof(FlightBayes)); }
        }

        private int dwellBayes = 5;

        public int DwellBayes
        {
            get { return dwellBayes; }
            set { dwellBayes = value; OnPropertyChanged(nameof(DwellBayes)); }
        }

        private int probThreshold;

        public int ProbThreshold
        {
            get { return probThreshold; }
            set { probThreshold = value; OnPropertyChanged(nameof(ProbThreshold)); }
        }

        #endregion

        #region VISIBILITY

        private Visibility metricVisibility = Visibility.Visible;

        public Visibility MetricVisibility
        {
            get { return metricVisibility; }
            set { metricVisibility = value; OnPropertyChanged(nameof(MetricVisibility)); }
        }

        private Visibility kparamVisibility = Visibility.Visible;

        public Visibility KParamVisibility
        {
            get { return kparamVisibility; }
            set { kparamVisibility = value; OnPropertyChanged(nameof(KParamVisibility)); }
        }

        private Visibility dwellTimeVisibility = Visibility.Hidden;

        public Visibility DwellTimeVisibility
        {
            get { return dwellTimeVisibility; }
            set { dwellTimeVisibility = value; OnPropertyChanged(nameof(DwellTimeVisibility)); }
        }

        private Visibility flightTimeVisibility = Visibility.Hidden;

        public Visibility FlightTimeVisibility
        {
            get { return flightTimeVisibility; }
            set { flightTimeVisibility = value; OnPropertyChanged(nameof(FlightTimeVisibility)); }
        }

        private Visibility probThresholdVisibility = Visibility.Hidden;

        public Visibility ProbThresholdVisibility
        {
            get { return probThresholdVisibility; }
            set { probThresholdVisibility = value; OnPropertyChanged(nameof(ProbThresholdVisibility)); }
        }

        #endregion

        #region CLASSIFICATION

        public KnnGraph GraphModel { get; set; }

        private ObservableCollection<TestSample> testSamples;

        public ObservableCollection<TestSample> TestSamples
        {
            get { return testSamples; }
            set { testSamples = value; OnPropertyChanged(nameof(TestSamples)); }
        }

        private Dictionary<string, double> knnResults;

        public Dictionary<string, double> KnnResults
        {
            get { return knnResults; }
            set { knnResults = value; OnPropertyChanged(nameof(KnnResults)); }
        }

        private Dictionary<string, double> kmeansResults;

        public Dictionary<string, double> KmeansResults
        {
            get { return kmeansResults; }
            set { kmeansResults = value; OnPropertyChanged(nameof(KmeansResults)); }
        }

        private Dictionary<string, double> bayesResults;

        public Dictionary<string, double> BayesResults
        {
            get { return bayesResults; }
            set { bayesResults = value; OnPropertyChanged(nameof(BayesResults)); }
        }

        private Dictionary<string, double> treeResults;

        public Dictionary<string, double> TreeResults
        {
            get { return treeResults; }
            set { treeResults = value; OnPropertyChanged(nameof(TreeResults)); }
        }


        #endregion

        #region CHART

        public Func<double, string> Formatter { get; set; }

        public string[] Labels { get; set; }

        private SeriesCollection accuracySeries;

        public SeriesCollection AccuracySeries
        {
            get { return accuracySeries; }
            set { accuracySeries = value; OnPropertyChanged(nameof(AccuracySeries)); }
        }


        private ChartValues<ObservableValue> accuracyValues;
        
        public ChartValues<ObservableValue> AccuracyValues
        {
            get { return accuracyValues; }
            set { accuracyValues = value; OnPropertyChanged(nameof(AccuracyValues)); }
        }

        #endregion

        private readonly IKnnClassificatorService classService;
        private readonly IGraphService graphService;
        private readonly IKeystrokeService keystrokeService;
    }
}
