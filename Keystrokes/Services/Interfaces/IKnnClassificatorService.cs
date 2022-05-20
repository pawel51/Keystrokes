using Keystrokes.Models.KnnGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Keystrokes.Services.Interfaces
{
    public interface IKnnClassificatorService
    {

        /// <summary>
        ///  metoda rozpatrująca k najbliższych sąsiadów wierzchołka c
        ///  zwraca klasę wierzchołka najczęściej występującą w rozpatrywanym zbiorze sąsiadów (o wielkości k)
        ///  jeżeli jest kilka najczęściej występującyh klas zwraca listę klas tych wierzchołków
        ///
        /// 
        /// </summary>
        /// <param name="graph">Model grafu</param>
        /// <param name="node">Wierzchołek który próbujemy sklasyfikować</param>
        /// <param name="k">Liczba rozpatrywanych sąsiadów</param>
        /// <returns>Listę </returns>
        public List<string> FindMostCommonKnn(KnnGraph graph, KnnNode node, int k);


        /// <summary>
        /// metoda rozpatrująca odległości pomiędzy średnimi 
        /// 
        /// </summary>
        /// <param name="graph">Model grafu</param>
        /// <param name="node">Wierzchołek który próbujemy sklasyfikować</param>
        /// <param name="k">Liczba zwracanych średnich</param>
        /// <returns>zwraca listę klas od długości np. 5 posortowaną od najbliższej klasy do najdalszej</returns>
        public List<string> FindNearestMean(KnnGraph graph, KnnNode node, int k);


        /// <summary>
        /// metoda rozpatrująca próbki w pewnej odległości d od klasyfikowanego wierzchołka
        /// zlicza próbki należące do każdej z klas w odległości d1 (dwelltime) od klasyfikowanego wierzchołka
        /// zlicza próbki należące do każdej z klas w odległości d2 (flighttime) od klasyfikowanego wierzchołka
        /// oblicza prawdopodobieństwo złożone przynależności do danej klasy 
        /// </summary>
        /// <param name="graph">Model grafu</param>
        /// <param name="node">Wierzchołek który próbujemy sklasyfikować</param>
        /// <returns></returns>
        public List<(string klass, double prob)> FindMostLikelyClassBayes(KnnGraph graph, KnnNode node, int d1, int d2);


        /// <summary>
        /// Krok 1.
        /// sortuje próbki ze względu na dwell time i dzieli na k (tyle ile klas w grafie) równych grup
        /// klasyfikuje próbkę ze względu na dwell time do odpowiedniej grupy
        /// 
        /// Krok 2.
        /// zlicza jakie jest prawdopodobieństwo przynależności do pozostałych klas
        /// jeżeli jest mniejsze od parametru probThreshold powtarza krok 1.
        /// 
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="node"></param>
        /// <returns>Zwraca listę klas i prawdopodobieństw przynależności do każdej z nich</returns>
        public Dictionary<string, double> TreeDecisions(KnnGraph graph, KnnNode node, double probThreshold, Canvas canvas);


    }
}
