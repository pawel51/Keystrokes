using KeystrokesData.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeystrokesData.Entities
{
    public class KnnGraphEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public DistanceMetric Metric { get; set; }
        public List<KnnNodeEntity> Nodes { get; set; }

        public List<KnnEdgeEntity> Edges { get; set; }
    }
}
