using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeystrokesData.Entities
{
    public class SingleProbe
    {
        public int Id { get; set; }

        public string AsciiSign { get; set; }

        public double Flight { get; set; }

        public double Dwell { get; set; }

    }
}
