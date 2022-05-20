using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keystrokes.Models
{
    public class TreeDairy
    {
        public class Split
        {
            public int GroupId { get; set; }
                    
            // all classes and their probability in this specific decission node
            public Dictionary<string, double> ClassProbList { get; set; }

            public bool WasPicked { get; set; } = false;

        }

        // Decission nodes on each Level of a tree start from 0
        public Dictionary<int, List<Split>> SplitList { get; set; } = new Dictionary<int, List<Split>>();
    }
}
