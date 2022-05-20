using Keystrokes.Models.KnnGraph;
using Keystrokes.Services.Interfaces;
using Keystrokes.ViewModels;
using KeystrokesData.Entities;
using KeystrokesData.Enums;
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

            canvasBorder.Height = canvas.Height + 4;
            canvasBorder.Width = canvas.Width + 4;

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
            
            KnnNode knnNode = graphService.TestSampleToKnnNode(dc.TestSamples.Last());
            Dictionary<string, double> keyProbList = classificationService.TreeDecisions(dc.GraphModel, knnNode, 0.4, canvas);
            keyProbList.ToList().ForEach(item =>
            {
                Log.Information($"Test Sample '{dc.TestSamples.Last().Category}' was classified to\n{item.Key} with probability = '{item.Value}'\n");
            });

        }
    }
}
