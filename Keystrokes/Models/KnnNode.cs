using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keystrokes.Models.KnnGraph
{
    public class KnnNode
    {
        public string Category { get; set; }

        public Dictionary<string, (double dwell, double flight)> Keystrokes { get; set; }

    }
}
