using System;
using System.Collections.Generic;
using System.Linq;
using SocialNetworkApp.Models;

namespace SocialNetworkApp.Algorithms
{
    public static class CommunityAlgorithms
    {
        /// <summary>
        /// Grafta ba?l? bile?enleri (connected components) bulur.
        /// Her bile?en, birbirine ba?l? olan dü?ümlerin bir listesidir.
        /// DFS algoritmas? kullanarak ba?lant?s?z bile?enleri tespit eder.
        /// </summary>
        /// <param name="graph">Hedef graf</param>
        /// <returns>
        /// Ba?l? bile?enlerin listesi. Her bile?en, dü?üm ID'lerinin bir listesidir.
        /// E?er graf bo? ise, bo? bir liste döner.
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
        /// Ba?l? bile?enlerin say?s?n? döner.
        /// </summary>
        /// <param name="graph">Hedef graf</param>
        /// <returns>Ba?l? bile?en say?s?</returns>
        public static int GetComponentCount(Graph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            return ConnectedComponents(graph).Count;
        }

        /// <summary>
        /// Ba?l? bile?enlerin özet bilgisini döner (UI'de göstermek için).
        /// Format: "Toplam 3 bile?en bulundu. En büyük bile?en: 5 dü?üm (Bile?en-1)"
        /// </summary>
        /// <param name="graph">Hedef graf</param>
        /// <returns>Bile?enlerin özeti</returns>
        public static string GetComponentSummary(Graph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var components = ConnectedComponents(graph);

            if (components.Count == 0)
                return "Hiçbir bile?en bulunamad?.";

            var largestComp = components.OrderByDescending(c => c.Count).First();
            var largestCompIndex = components.IndexOf(largestComp) + 1;

            return $"Toplam {components.Count} bile?en bulundu. " +
                   $"En büyük bile?en: {largestComp.Count} dü?üm (Bile?en-{largestCompIndex})";
        }

        /// <summary>
        /// Her bile?eni detayl? bilgiyle döner (UI ListBox'ta göstermek için).
        /// Format: [("Bile?en-1", [1, 2, 3, 4]), ("Bile?en-2", [5, 6]), ...]
        /// </summary>
        /// <param name="graph">Hedef graf</param>
        /// <returns>
        /// Tuple listesi: (bile?en ad?, dü?üm ID'leri)
        /// Örnek: ("Bile?en-1", [1, 2, 3, 4])
        /// </returns>
        public static List<(string name, List<int> nodeIds)> GetComponentsWithNames(Graph graph)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var components = ConnectedComponents(graph);
            var result = new List<(string, List<int>)>();

            for (int i = 0; i < components.Count; i++)
            {
                var name = $"Bile?en-{i + 1} ({components[i].Count} dü?üm)";
                result.Add((name, components[i]));
            }

            return result;
        }

        /// <summary>
        /// Verilen bir dü?üm ID'sinin hangi bile?ene ait oldu?unu bulur.
        /// </summary>
        /// <param name="graph">Hedef graf</param>
        /// <param name="nodeId">Dü?üm ID'si</param>
        /// <returns>
        /// Dü?ümün ait oldu?u bile?en. E?er dü?üm grafta yoksa, bo? bir liste döner.
        /// </returns>
        public static List<int> GetComponentOf(Graph graph, int nodeId)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var components = ConnectedComponents(graph);
            return components.FirstOrDefault(c => c.Contains(nodeId)) ?? new List<int>();
        }

        /// <summary>
        /// ?ki dü?ümün ayn? bile?ende olup olmad???n? kontrol eder.
        /// </summary>
        /// <param name="graph">Hedef graf</param>
        /// <param name="nodeId1">Birinci dü?üm ID'si</param>
        /// <param name="nodeId2">?kinci dü?üm ID'si</param>
        /// <returns>E?er dü?ümler ayn? bile?endeyse true, aksi halde false</returns>
        public static bool AreInSameComponent(Graph graph, int nodeId1, int nodeId2)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var comp1 = GetComponentOf(graph, nodeId1);
            return comp1.Contains(nodeId2);
        }
    }
}
