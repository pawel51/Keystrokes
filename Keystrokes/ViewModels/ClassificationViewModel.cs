using KeystrokesData.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Keystrokes.ViewModels
{
    public class ClassificationViewModel : BaseViewModel
    {
        public ClassificationViewModel()
        {
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
            AlgorithmsList.Add(Algorithm.DECISIONTREES);

        }

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
                    case Algorithm.DECISIONTREES:
                        MetricVisibility = Visibility.Visible;
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

    }
}
