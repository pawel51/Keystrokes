using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Keystrokes.Models.KnnGraph
{
    public class KnnGraph
    {
        /// <summary>
        /// unikatowa nazwa grafu
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Każdy wierzchołek posiada klucz string, który jest unikatowy np. pawel_1
        /// Natomiast nazwa kategorii zapamiętanej w klasie KnnNode to "pawel" 
        /// </summary>
        public Dictionary<string, KnnNode> Nodes { get; set; }

        /// <summary>
        /// Odległości między wierzchołkami
        /// Każda krawędź posiada klucz string, który jest złączeniem kluczy wierzchołków
        /// np. dla wierzchołków o kluczach A_1 i B_2 , klucz krawędzi będzie równy A_1B_2 lub B_2A_1
        /// algorytm musi wykluczać obie możliwości, natomiast nie możliwe jest wystąpienie obu kluczy  A_1B_2 i B_2A_1
        /// </summary>
        public Dictionary<string, KnnEdge> Edges { get; set; }
    }
}
