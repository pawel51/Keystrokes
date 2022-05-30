using KeystrokesData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeystrokesData.Entities
{
    public class TrainSample
    {
        public int Id { get; set; }
        public string Category { get; set; }

        public List<SingleProbe> Probes { get; set; }

        public TrainSample()
        {

        }

        public TrainSample(TestSample sample)
        {
            Category = sample.Category;
            Probes = sample.Probes;
        }
    }
}
