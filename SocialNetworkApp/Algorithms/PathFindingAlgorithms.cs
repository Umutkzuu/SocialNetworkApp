using System;
using System.Collections.Generic;
using System.Linq;
using SocialNetworkApp.Models;

namespace SocialNetworkApp.Algorithms
{
    public static class PathFindingAlgorithms
    {
        /// <summary>
        /// Dijkstra algoritmas? ile iki dü?üm aras?ndaki en k?sa yolu bulur.
        /// A??rl?kl?, yönsüz grafta çal???r. Tüm a??rl?klar pozitif olmal?d?r.
        /// </summary>
        /// <param name="graph">Hedef graf</param>
        /// <param name="startId">Ba?lang?ç dü?üm ID'si</param>
        /// <param name="goalId">Hedef dü?üm ID'si</param>
        /// <returns>Tuple: (yol listesi, toplam maliyet). Yol yok ise (bo? liste, PositiveInfinity)</returns>
        public static (List<int> path, double cost) Dijkstra(Graph graph, int startId, int goalId)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var dist = new Dictionary<int, double>();
            var prev = new Dictionary<int, int?>();
            var q = new HashSet<int>(graph.GetAllNodes().Select(n => n.Id));

            foreach (var v in q)
            {
                dist[v] = double.PositiveInfinity;
                prev[v] = null;
            }

            if (!dist.ContainsKey(startId) || !dist.ContainsKey(goalId)) 
                return (new List<int>(), double.PositiveInfinity);

            dist[startId] = 0;

            while (q.Count > 0)
            {
                var u = q.OrderBy(x => dist[x]).First();
                q.Remove(u);

                if (double.IsPositiveInfinity(dist[u])) break;
                if (u == goalId) break;

                foreach (var v in graph.GetNeighbors(u))
                {
                    if (!q.Contains(v)) continue;
                    if (!graph.TryGetEdgeWeight(u, v, out var w)) continue;

                    var alt = dist[u] + w;
                    if (alt < dist[v])
                    {
                        dist[v] = alt;
                        prev[v] = u;
                    }
                }
            }

            if (double.IsPositiveInfinity(dist[goalId])) 
                return (new List<int>(), double.PositiveInfinity);

            var path = ReconstructPath(prev, startId, goalId);
            return (path, dist[goalId]);
        }

        /// <summary>
        /// A* algoritmas? ile iki dü?üm aras?ndaki en k?sa yolu bulur.
        /// Heuristic fonksiyonu verilmez ise, özellik uzay?nda (Aktiflik, Etkilesim) Euclidean mesafe kullan?l?r.
        /// </summary>
        /// <param name="graph">Hedef graf</param>
        /// <param name="startId">Ba?lang?ç dü?üm ID'si</param>
        /// <param name="goalId">Hedef dü?üm ID'si</param>
        /// <param name="heuristic">Heuristic fonksiyonu (iki Node aras?ndaki tahmini mesafe). Null ise Euclidean kullan?l?r.</param>
        /// <returns>Tuple: (yol listesi, toplam maliyet). Yol yok ise (bo? liste, PositiveInfinity)</returns>
        public static (List<int> path, double cost) AStar(
            Graph graph, 
            int startId, 
            int goalId, 
            Func<Node, Node, double>? heuristic = null)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));

            var allNodeIds = graph.GetAllNodes().Select(n => n.Id).ToHashSet();
            if (!allNodeIds.Contains(startId) || !allNodeIds.Contains(goalId)) 
                return (new List<int>(), double.PositiveInfinity);

            // Varsay?lan heuristic: Aktiflik ve Etkilesim özellikleri aras?ndaki Euclidean mesafe
            heuristic ??= DefaultHeuristic;

            var openSet = new HashSet<int> { startId };
            var cameFrom = new Dictionary<int, int>();

            var gScore = allNodeIds.ToDictionary(id => id, id => double.PositiveInfinity);
            var fScore = allNodeIds.ToDictionary(id => id, id => double.PositiveInfinity);

            gScore[startId] = 0.0;
            var startNode = graph.GetNode(startId)!;
            var goalNode = graph.GetNode(goalId)!;
            fScore[startId] = heuristic(startNode, goalNode);

            while (openSet.Count > 0)
            {
                var current = openSet.OrderBy(x => fScore[x]).First();
                if (current == goalId)
                {
                    var path = ReconstructPathAStar(cameFrom, current);
                    return (path, gScore[goalId]);
                }

                openSet.Remove(current);

                foreach (var neighbor in graph.GetNeighbors(current))
                {
                    if (!graph.TryGetEdgeWeight(current, neighbor, out var w)) continue;

                    var tentativeG = gScore[current] + w;
                    if (tentativeG < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        var neighborNode = graph.GetNode(neighbor)!;
                        fScore[neighbor] = gScore[neighbor] + heuristic(neighborNode, goalNode);
                        openSet.Add(neighbor);
                    }
                }
            }

            return (new List<int>(), double.PositiveInfinity);
        }

        /// <summary>
        /// Dijkstra algoritmas? sonras?nda geriye do?ru yolu kapat?r.
        /// </summary>
        private static List<int> ReconstructPath(Dictionary<int, int?> prev, int startId, int goalId)
        {
            var path = new List<int>();
            var current = goalId;

            while (current != startId)
            {
                path.Add(current);
                if (prev[current] == null) break;
                current = prev[current].Value;
            }

            path.Add(startId);
            path.Reverse();
            return path;
        }

        /// <summary>
        /// A* algoritmas? sonras?nda geriye do?ru yolu kapat?r.
        /// </summary>
        private static List<int> ReconstructPathAStar(Dictionary<int, int> cameFrom, int current)
        {
            var path = new List<int> { current };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        /// <summary>
        /// Varsay?lan heuristic: Dü?üm özelliklerinin (Aktiflik, Etkilesim) Euclidean mesafesi.
        /// </summary>
        private static double DefaultHeuristic(Node a, Node b)
        {
            if (a == null || b == null) return 0.0;
            
            var da = a.Aktiflik - b.Aktiflik;
            var db = a.Etkilesim - b.Etkilesim;
            return Math.Sqrt(da * da + db * db);
        }
    }
}
