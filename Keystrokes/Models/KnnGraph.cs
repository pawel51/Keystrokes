using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keystrokes.Models.KnnGraph
{
    public class KnnGraph
    {
        public string Name { get; set; }

        public Dictionary<string, KnnNode> Nodes { get; set; }

        public Dictionary<string, KnnEdge> Edges { get; set; }
    }
}
