using System.Collections.Generic;
using SocialNetworkApp.Models;

namespace SocialNetworkApp.Algorithms
{
    /// <summary>
    /// Graf traversal algoritmalar?n? içerir.
    /// BFS (Breadth-First Search) ve DFS (Depth-First Search).
    /// </summary>
    public static class TraversalAlgorithms
    {
        /// <summary>
        /// Geni?lik-ilk arama (BFS) - Level seviyesinde ke?if yapar.
        /// Ba?lang?ç dü?ümünden ba?layarak tüm ula??labilir dü?ümleri gezir.
        /// Queue yap?s? kullan?r.
        /// </summary>
        /// <param name="graph">Traversal yap?lacak graf</param>
        /// <param name="startId">Ba?lang?ç dü?üm ID'si</param>
        /// <returns>BFS s?ras?na göre dü?üm ID'leri</returns>
        public static List<int> BFS(Graph graph, int startId)
        {
            var visited = new HashSet<int>();
            var queue = new Queue<int>();
            var order = new List<int>();

            // Ba?lang?ç dü?ümünü queue'ye ekle
            visited.Add(startId);
            queue.Enqueue(startId);

            // Queue bo?alana kadar devam et
            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                order.Add(u);

                // Kom?ular? queue'ye ekle (level s?ras?yla)
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
        /// Derinlik-ilk arama (DFS) - Derinli?e do?ru ke?if yapar.
        /// Ba?lang?ç dü?ümünden ba?layarak mümkün oldu?unca derine iner.
        /// Stack yap?s? kullan?r.
        /// </summary>
        /// <param name="graph">Traversal yap?lacak graf</param>
        /// <param name="startId">Ba?lang?ç dü?üm ID'si</param>
        /// <returns>DFS s?ras?na göre dü?üm ID'leri</returns>
        public static List<int> DFS(Graph graph, int startId)
        {
            var visited = new HashSet<int>();
            var stack = new Stack<int>();
            var order = new List<int>();

            // Ba?lang?ç dü?ümünü stack'e ekle
            stack.Push(startId);

            // Stack bo?alana kadar devam et
            while (stack.Count > 0)
            {
                var u = stack.Pop();
                if (visited.Contains(u)) continue;
                
                visited.Add(u);
                order.Add(u);

                // Kom?ular? stack'e ekle (derinli?e gitmek için)
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
