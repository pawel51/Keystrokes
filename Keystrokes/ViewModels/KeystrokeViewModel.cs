using Keystrokes.Models;
using Keystrokes.Models.KnnGraph;
using Keystrokes.Services;
using KeystrokesData.Entities;
using KeystrokesData.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Keystrokes.ViewModels
{
    public class KeystrokeViewModel : BaseViewModel
    {

        private string _testSentence1 = "Apollo 11 was the first space flight that landed first two people on the moon";

        public string TestSentence1
        {
            get { return _testSentence1; }
            set { _testSentence1 = value; }
        }

        private string _testSentence2 = 
            "Aldrin joined him 20 minutes later";

        public string TestSentence2
        {
            get { return _testSentence2; }
            set { _testSentence2 = value; }
        }

        private string _testSentence3 =
            "Armstrong became the first person to step onto the lunar surface six hours after landing on July 21";

        public string TestSentence3
        {
            get { return _testSentence3; }
            set { _testSentence3 = value; }
        }

        private string _textBlock = "";

        public string TextBlock
        {
            get { return _textBlock; }
            set { _textBlock = value; OnPropertyChanged(TextBlock); }
        }

        private Brush _borderColor = Brushes.DimGray;

        public Brush BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; OnPropertyChanged(nameof(BorderColor)); }
        }

        private Boolean _canSaveTrainingSample = false;

        public Boolean CanSaveTrainingSample
        {
            get { return _canSaveTrainingSample; }
            set { _canSaveTrainingSample = value; OnPropertyChanged(nameof(CanSaveTrainingSample)); }
        }



        private readonly IKeystrokeService service;
        private readonly IGraphService graphService;

        public KnnGraph GraphModel { get; set; }

        private ObservableCollection<TrainSample> trainSamples;

        public ObservableCollection<TrainSample> TrainingSamples
        {
            get { return trainSamples; }
            set { trainSamples = value; OnPropertyChanged(nameof(TrainingSamples)); }
        }

        private ObservableCollection<TestSample> testSamples;

        public ObservableCollection<TestSample> TestSamples
        {
            get { return testSamples; }
            set { testSamples = value; OnPropertyChanged(nameof(TestSamples)); }
        }

        private List<SingleRowModel> keyStrokeData = new List<SingleRowModel>();

        public List<SingleRowModel> KeyStrokeData
        {
            get { return keyStrokeData; }
            set { keyStrokeData = value; OnPropertyChanged(nameof(KeyStrokeData)); }
        }


        public KeystrokeViewModel(IKeystrokeService service, IGraphService graphService)
        {
            this.service = service;
            this.graphService = graphService;
            
            
            TrainingSamples = new ObservableCollection<TrainSample>(service.GetTrainSamples());
            TestSamples = new ObservableCollection<TestSample>(service.GetTestSamples());


            
            GraphModel = graphService.GetKnnGraph();


            if (GraphModel.Nodes == null || GraphModel.Edges == null)
            {
                GraphModel = graphService.CreateGraph(TrainingSamples.ToList(), DistanceMetric.EUCLIDIAN);
            }
            else
            {
                graphService.UpdateGraphToDb(GraphModel, DistanceMetric.EUCLIDIAN);
            }
        }
    }
}
