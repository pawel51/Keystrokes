using CsvHelper;
using CsvHelper.Configuration;
using Keystrokes.Helpers;
using Keystrokes.Models;
using Keystrokes.Models.KnnGraph;
using Keystrokes.Services;
using Keystrokes.Services.Interfaces;
using Keystrokes.ViewModels;
using KeystrokesData;
using KeystrokesData.Entities;
using KeystrokesData.Enums;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Keystrokes.Views
{
    /// <summary>
    /// Interaction logic for KeystrokeView.xaml
    /// </summary>
    public partial class KeystrokeView : UserControl
    {
        public KeystrokeView(IKeystrokeService keystrokeService, IGraphService graphService, IKnnClassificatorService classificationService)
        {
            InitializeComponent();
            this.keystrokeService = keystrokeService;
            this.graphService = graphService;
            this.classificationService = classificationService;
            DataContext = new KeystrokeViewModel(keystrokeService, graphService);
            Log.Logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.File(@"C:\logs\keystrokes\log.log").WriteTo.Console().CreateLogger();
            Log.Information("Logger created");
        }


        private readonly IKeystrokeService keystrokeService;
        private readonly IGraphService graphService;
        private readonly IKnnClassificatorService classificationService;
        private KeystrokesDbContext context;


        private long lastKeyUp = 0;



        internal class Timings
        {
            public Key key = Key.A;
            public long prevKeyUp = 0L;
            public long keyDown = 0L;
            public long keyUp = 0L;
            public Timings(Key key)
            {
                this.key = key;
            }

            public double GetDwellTime()
            {
                double ret = keyUp - keyDown;
                if (ret < 0)
                {
                    Log.Error("Negative dwell time value registered");
                }
                return ret;
            }

            public double GetFlightTime()
            {
                double ret = keyDown - prevKeyUp;
                if (ret < 0)
                {
                    Log.Error("Negative flight time value registered");
                }
                return ret;
            }
            public long RegKeyDown(long lastKeyUp)
            {
                if (lastKeyUp == 0L)
                {
                    lastKeyUp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    keyDown = lastKeyUp;
                    prevKeyUp = lastKeyUp;
                }
                else
                {
                    prevKeyUp = lastKeyUp;
                    keyDown = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                }
                return lastKeyUp;
            }
            public long RegKeyUp()
            {
                keyUp = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                return keyUp;
            }
        }

        private Dictionary<Key, Timings> tDict = new Dictionary<Key, Timings>();

        private Dictionary<string, List<(double flight, double dwell)>> probe
            = new Dictionary<string, List<(double flight, double dwell)>>();
        

        private void MyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Log.Information("down");

            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
                return;



            Timings t = new Timings(e.Key);

            lastKeyUp = t.RegKeyDown(lastKeyUp);

            if (!tDict.ContainsKey(e.Key))
                tDict.Add(e.Key, t);
            else
                return;

        }

        private void MyTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            //Log.Information("up");

            if (!tDict.ContainsKey(e.Key)) return;

            Timings t = tDict[e.Key];
            if (t == null) return;

            lastKeyUp = t.RegKeyUp();

            double flight = t.GetFlightTime();
            double dwell = t.GetDwellTime();

            string charKey = KeyToCharHelper.KeyToString(e.Key, false);
            

            if (probe.ContainsKey(charKey))
            {
                probe[charKey].Add((flight, dwell));
            }
            else
            {
                probe.Add(charKey, new List<(double flight, double dwell)>() { (flight, dwell) });
            }

            tDict.Remove(e.Key);


        }

        private void Train_BtnClicked(object sender, RoutedEventArgs e)
        {
            KeystrokeViewModel dc = (KeystrokeViewModel)DataContext;
            TrainSample? sample = keystrokeService.AddTrainSample(probe, CategoryNameTxtBox.Text);
            if (sample != null)
            {
                dc.TrainingSamples.Add(sample);
                MyTextBox.IsEnabled = false;
                if (graphService.UpdateGraph(sample, dc.GraphModel))
                {
                    graphService.UpdateGraphToDb(dc.GraphModel, DistanceMetric.EUCLIDIAN);
                }
            }
            else
            {
                MessageBox.Show("Couldn't add new training sample check if category already exists.");
            }

        }

        private void Classify_BtnClicked(object sender, RoutedEventArgs e)
        {
            KeystrokeViewModel dc = (KeystrokeViewModel)DataContext;
            TestSample testSample = keystrokeService.AddTestSample(probe, CategoryNameTxtBox.Text);
            KnnNode knnNode = graphService.TestSampleToKnnNode(testSample);
            Dictionary<string, double> keyProbList = classificationService.treeDecisions(dc.GraphModel, knnNode, 0.4);
            keyProbList.ToList().ForEach(item =>
            {
                Log.Information($"Test Sample '{testSample.Category}' was classified to\n{item.Key} with probability = '{item.Value}'\n");
            });
            
        }

        private void CreateKnnGraph_BtnClicked(object sender, RoutedEventArgs e)
        {
            KnnGraph graph = graphService.CreateGraph(keystrokeService.GetTrainSamples(), DistanceMetric.EUCLIDIAN);
            graphService.AddGraphToDb(graph, DistanceMetric.EUCLIDIAN);

        }

        private void StartWritting_BtnClicked(object sender, RoutedEventArgs e)
        {
            probe.Clear();
            tDict.Clear();
            lastKeyUp = 0L;
            MyTextBox.Text = "";
            MyTextBox.IsEnabled = true;
            MyTextBox.Focus();
        }

        private void MyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //KeystrokeViewModel dc = (KeystrokeViewModel)DataContext;

            //if (dc.TextBlock.Equals(dc.TestSentence))
            //{
            //    dc.BorderColor = Brushes.Green;
            //    dc.CanSaveTrainingSample = true;
            //}
            //else
            //{
            //    dc.BorderColor = Brushes.DimGray;
            //    dc.CanSaveTrainingSample = false;
            //}
        }

        private void TrainData_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string? headername = e.Column.Header.ToString();
            if (headername != null && headername == "Probes")
            {
                e.Cancel = true;
            }
        }


        private void ReadTrainingData_Click(object sender, RoutedEventArgs e)
        {
            KeystrokeViewModel dc = (KeystrokeViewModel)DataContext;
            List<TrainSample> samples = keystrokeService.ReadTreningData("ss");
            foreach (TrainSample sample in samples)
            {
                dc.TrainingSamples.Add(sample);
                graphService.UpdateGraph(sample, dc.GraphModel);
            }
            graphService.UpdateGraphToDb(dc.GraphModel, DistanceMetric.EUCLIDIAN);
        }

        private void ReadTestData_Click(object sender, RoutedEventArgs e)
        {
            //KeystrokeViewModel dc = (KeystrokeViewModel)DataContext;
            //List<TestSample> samples = keystrokeService.Read("ss");
            //foreach (TrainSample sample in samples)
            //{
            //    dc.TrainingSamples.Add(sample);
            //    graphService.UpdateGraph(sample, dc.GraphModel);
            //}
            //graphService.UpdateGraphToDb(dc.GraphModel, DistanceMetric.EUCLIDIAN);
        }
    }
}
