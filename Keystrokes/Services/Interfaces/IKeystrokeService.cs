using KeystrokesData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keystrokes.Services
{
    public interface IKeystrokeService
    {
        public TrainSample? AddTrainSample(Dictionary<string, List<(double flight, double dwell)>> probe, string categoryName);

        public List<TrainSample> GetTrainSamples();

        public List<TrainSample> ReadTreningData(string testDataFileName);
    }
}
