using KeystrokesData.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeystrokesData.Entities
{
    public class TestSample
    {
        public int Id { get; set; }
        public string Category { get; set; }

        public List<SingleProbe> Probes { get; set; }

    }
}
