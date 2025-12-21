using System.Collections.Generic;
using SocialNetworkApp.Models;

namespace SocialNetworkApp.Algorithms
{
    /// <summary>
    /// Graf traversal algoritmalarýný içerir.
    /// BFS (Breadth-First Search) ve DFS (Depth-First Search).
    /// </summary>
    public static class TraversalAlgorithms
    {
        /// <summary>
        /// Geniþlik-ilk arama (BFS) - Level seviyesinde keþif yapar.
        /// Baþlangýç düðümünden baþlayarak tüm ulaþýlabilir düðümleri gezir.
        /// Queue yapýsý kullanýr.
        /// </summary>
        /// <param name="graph">Traversal yapýlacak graf</param>
        /// <param name="startId">Baþlangýç düðüm ID'si</param>
        /// <returns>BFS sýrasýna göre düðüm ID'leri</returns>
        public static List<int> BFS(Graph graph, int startId)
        {
            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            var order = new List<int>();

            // Baþlangýç düðümünü queue'ye ekle
            visited.Add(startId);
            queue.Enqueue(startId);

            // Queue boþalana kadar devam et
            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                order.Add(u);

                // Komþularý queue'ye ekle (level sýrasýyla)
                foreach (var v in graph.GetNeighbors(u))
                {
                    if (!visited.Contains(v))
                    {
                        visited.Add(v);
                        queue.Enqueue(v);
                    }
                }
            }

            return order;
        }

        /// <summary>
        /// Derinlik-ilk arama (DFS) - Derinliðe doðru keþif yapar.
        /// Baþlangýç düðümünden baþlayarak mümkün olduðunca derine iner.
        /// Stack yapýsý kullanýr.
        /// </summary>
        /// <param name="graph">Traversal yapýlacak graf</param>
        /// <param name="startId">Baþlangýç düðüm ID'si</param>
        /// <returns>DFS sýrasýna göre düðüm ID'leri</returns>
        public static List<int> DFS(Graph graph, int startId)
        {
            var visited = new HashSet<int>();
            var stack = new Stack<int>();
            var order = new List<int>();

            // Baþlangýç düðümünü stack'e ekle
            stack.Push(startId);

            // Stack boþalana kadar devam et
            while (stack.Count > 0)
            {
                var u = stack.Pop();
                if (visited.Contains(u)) continue;
                
                visited.Add(u);
                order.Add(u);

                // Komþularý stack'e ekle (derinliðe gitmek için)
                foreach (var v in graph.GetNeighbors(u))
                {
                    if (!visited.Contains(v))
                        stack.Push(v);
                }
            }

            return order;
        }
    }
}
