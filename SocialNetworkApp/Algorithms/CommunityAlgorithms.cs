using System;
using System.Collections.Generic;
using System.Linq;
using SocialNetworkApp.Models;

namespace SocialNetworkApp.Algorithms
{
    public static class CommunityAlgorithms
    {
        /// <summary>
        /// Grafta baðlý bileþenleri (connected components) bulur.
        /// Her bileþen, birbirine baðlý olan düðümlerin bir listesidir.
        /// DFS algoritmasý kullanarak baðlantýsýz bileþenleri tespit eder.
        /// </summary>
        /// <param name="graph">Hedef graf</param>
        /// <returns>
        /// Baðlý bileþenlerin listesi. Her bileþen, düðüm ID'lerinin bir listesidir.
        /// Eðer graf boþ ise, boþ bir liste döner.
        /// </returns>
        public static List<List<int>> ConnectedComponents(Graph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var result = new List<List<int>>();
            var visited = new HashSet<int>();

            foreach (var node in graph.GetAllNodes())
            {
                if (visited.Contains(node.Id)) continue;

                var comp = new List<int>();
                var stack = new Stack<int>();
                stack.Push(node.Id);
                visited.Add(node.Id);

                while (stack.Count > 0)
                {
                    var u = stack.Pop();
                    comp.Add(u);

                    foreach (var v in graph.GetNeighbors(u))
                    {
                        if (!visited.Contains(v))
                        {
                            visited.Add(v);
                            stack.Push(v);
                        }
                    }
                }

                result.Add(comp);
            }

            return result;
        }

        /// <summary>
        /// Baðlý bileþenlerin sayýsýný döner.
        /// </summary>
        /// <param name="graph">Hedef graf</param>
        /// <returns>Baðlý bileþen sayýsý</returns>
        public static int GetComponentCount(Graph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            return ConnectedComponents(graph).Count;
        }

        /// <summary>
        /// Baðlý bileþenlerin özet bilgisini döner (UI'de göstermek için).
        /// Format: "Toplam 3 bileþen bulundu. En büyük bileþen: 5 düðüm (Bileþen-1)"
        /// </summary>
        /// <param name="graph">Hedef graf</param>
        /// <returns>Bileþenlerin özeti</returns>
        public static string GetComponentSummary(Graph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var components = ConnectedComponents(graph);

            if (components.Count == 0)
                return "Hiçbir bileþen bulunamadý.";

            var largestComp = components.OrderByDescending(c => c.Count).First();
            var largestCompIndex = components.IndexOf(largestComp) + 1;

            return $"Toplam {components.Count} bileþen bulundu. " +
                   $"En büyük bileþen: {largestComp.Count} düðüm (Bileþen-{largestCompIndex})";
        }

        /// <summary>
        /// Her bileþeni detaylý bilgiyle döner (UI ListBox'ta göstermek için).
        /// Format: [("Bileþen-1", [1, 2, 3, 4]), ("Bileþen-2", [5, 6]), ...]
        /// </summary>
        /// <param name="graph">Hedef graf</param>
        /// <returns>
        /// Tuple listesi: (bileþen adý, düðüm ID'leri)
        /// Örnek: ("Bileþen-1", [1, 2, 3, 4])
        /// </returns>
        public static List<(string name, List<int> nodeIds)> GetComponentsWithNames(Graph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var components = ConnectedComponents(graph);
            var result = new List<(string, List<int>)>();

            for (int i = 0; i < components.Count; i++)
            {
                var name = $"Bileþen-{i + 1} ({components[i].Count} düðüm)";
                result.Add((name, components[i]));
            }

            return result;
        }

        /// <summary>
        /// Verilen bir düðüm ID'sinin hangi bileþene ait olduðunu bulur.
        /// </summary>
        /// <param name="graph">Hedef graf</param>
        /// <param name="nodeId">Düðüm ID'si</param>
        /// <returns>
        /// Düðümün ait olduðu bileþen. Eðer düðüm grafta yoksa, boþ bir liste döner.
        /// </returns>
        public static List<int> GetComponentOf(Graph graph, int nodeId)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var components = ConnectedComponents(graph);
            return components.FirstOrDefault(c => c.Contains(nodeId)) ?? new List<int>();
        }

        /// <summary>
        /// Ýki düðümün ayný bileþende olup olmadýðýný kontrol eder.
        /// </summary>
        /// <param name="graph">Hedef graf</param>
        /// <param name="nodeId1">Birinci düðüm ID'si</param>
        /// <param name="nodeId2">Ýkinci düðüm ID'si</param>
        /// <returns>Eðer düðümler ayný bileþendeyse true, aksi halde false</returns>
        public static bool AreInSameComponent(Graph graph, int nodeId1, int nodeId2)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var comp1 = GetComponentOf(graph, nodeId1);
            return comp1.Contains(nodeId2);
        }
    }
}
