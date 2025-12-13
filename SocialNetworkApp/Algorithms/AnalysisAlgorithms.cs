using System;
using System.Collections.Generic;
using System.Linq;
using SocialNetworkApp.Models;

namespace SocialNetworkApp.Algorithms
{
    public static class AnalysisAlgorithms
    {
      
        public static Dictionary<int, int> DegreeCentrality(Graph graph)
        {
            var result = new Dictionary<int, int>();
            foreach (var node in graph.GetAllNodes())
            {
                var deg = graph.GetNeighbors(node.Id).Count();
                result[node.Id] = deg;
            }

            return result;
        }

        // En yüksek dereceli N dü?ümü döndürür (id, degree)
        public static List<KeyValuePair<int, int>> TopDegreeNodes(Graph graph, int top = 5)
        {
            return DegreeCentrality(graph)
                .OrderByDescending(kv => kv.Value)
                .ThenBy(kv => kv.Key)
                .Take(top)
                .ToList();
        }

        // Welsh-Powell renklendirme (tüm graf için)
        // Dönen sözlük: nodeId -> colorIndex (0-based)
        public static Dictionary<int, int> WelshPowell(Graph graph)
        {
            // Tüm dü?ümleri dereceye göre azalan s?rada al
            var nodes = graph.GetAllNodes()
                .Select(n => new { Id = n.Id, Degree = graph.GetNeighbors(n.Id).Count() })
                .OrderByDescending(x => x.Degree)
                .ThenBy(x => x.Id)
                .Select(x => x.Id)
                .ToList();

            var colorOf = new Dictionary<int, int>();
            int color = 0;

            foreach (var nodeId in nodes)
            {
                if (colorOf.ContainsKey(nodeId)) continue; // zaten boyanm??

                // bu dü?üme yeni renk ata
                colorOf[nodeId] = color;

                // di?er dü?ümleri ayn? renkle boyamaya çal??
                foreach (var other in nodes)
                {
                    if (colorOf.ContainsKey(other)) continue;
                    // di?er dü?ümün bu renkteki herhangi bir kom?usu var m??
                    bool conflict = false;
                    foreach (var colored in colorOf.Where(kv => kv.Value == color).Select(kv => kv.Key))
                    {
                        if (graph.GetNeighbors(colored).Contains(other))
                        {
                            conflict = true;
                            break;
                        }
                    }

                    if (!conflict)
                    {
                        colorOf[other] = color;
                    }
                }

                color++;
            }

            return colorOf;
        }

        // Welsh-Powell her ba?l? bile?en için uygular ve component bazl? renklendirme döndürür
        public static Dictionary<int, int> WelshPowellByComponent(Graph graph)
        {
            var components = CommunityAlgorithms.ConnectedComponents(graph);
            var globalColors = new Dictionary<int, int>();
            int colorOffset = 0;

            foreach (var comp in components)
            {
                if (comp.Count == 0) continue;
                // create sublist ordering by degree
                var nodes = comp.OrderByDescending(id => graph.GetNeighbors(id).Count()).ToList();
                var colorOfComp = new Dictionary<int, int>();
                int localColor = 0;

                foreach (var nodeId in nodes)
                {
                    if (colorOfComp.ContainsKey(nodeId)) continue;
                    colorOfComp[nodeId] = localColor;

                    foreach (var other in nodes)
                    {
                        if (colorOfComp.ContainsKey(other)) continue;
                        bool conflict = false;
                        foreach (var colored in colorOfComp.Where(kv => kv.Value == localColor).Select(kv => kv.Key))
                        {
                            if (graph.GetNeighbors(colored).Contains(other))
                            {
                                conflict = true;
                                break;
                            }
                        }

                        if (!conflict)
                            colorOfComp[other] = localColor;
                    }

                    localColor++;
                }

                // map local colors to global color indices
                foreach (var kv in colorOfComp)
                {
                    globalColors[kv.Key] = kv.Value + colorOffset;
                }

                colorOffset += colorOfComp.Values.DefaultIfEmpty().Max() + 1;
            }

            return globalColors;
        }
    }
}
