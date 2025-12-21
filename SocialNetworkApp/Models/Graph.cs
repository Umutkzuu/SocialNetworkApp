using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialNetworkApp.Models
{
    /// <summary>
    /// Sosyal að grafýný temsil eder.
    /// Düðümleri (nodes) ve kenarlarý (edges) yönetir.
    /// Yönsüz, aðýrlýklý graf yapýsýdýr.
    /// </summary>
    public class Graph
    {
        // Tüm düðümleri ID'ye göre sakla (hýzlý lookup)
        private readonly Dictionary<int, Node> _nodes = new();
        
        // Komþuluk listesi: Her düðümün komþularýný ve kenarlarýný sakla
        private readonly Dictionary<int, List<Edge>> _adj = new();

        /// <summary>
        /// Grafýn tüm düðümlerine read-only eriþim.
        /// </summary>
        public IReadOnlyDictionary<int, Node> Nodes => _nodes;

        /// <summary>
        /// Grafa yeni düðüm ekler.
        /// </summary>
        /// <returns>Baþarýlý ise true, zaten varsa false</returns>
        public bool AddNode(Node node)
        {
            if (node is null) throw new ArgumentNullException(nameof(node));
            if (_nodes.ContainsKey(node.Id)) return false;

            _nodes.Add(node.Id, node);
            return true;
        }

        /// <summary>
        /// Grafdan düðüm kaldýrýr (iliþkili kenarlar da silinir).
        /// </summary>
        public bool RemoveNode(int nodeId)
        {
            if (!_nodes.ContainsKey(nodeId)) return false;

            // Bu düðümün tüm kenarlarýný sil
            if (_adj.TryGetValue(nodeId, out var edges))
            {
                var neighbors = edges.Select(e => e.TargetId).ToList();
                foreach (var neighborId in neighbors)
                {
                    RemoveEdge(nodeId, neighborId);
                }
                _adj.Remove(nodeId);
            }
            
            // Diðer düðümlerin listelerinden bu düðüme giden kenarlarý sil
            foreach (var list in _adj.Values)
            {
                list.RemoveAll(e => e.TargetId == nodeId);
            }

            _nodes.Remove(nodeId);
            return true;
        }

        /// <summary>
        /// Düðümün özelliklerini günceller.
        /// </summary>
        public bool UpdateNode(int nodeId, string name, double aktiflik, double etkilesim)
        {
            if (!_nodes.TryGetValue(nodeId, out var node)) return false;
            node.Update(name, aktiflik, etkilesim);
            return true;
        }

        /// <summary>
        /// ID'ye göre düðümü döner.
        /// </summary>
        public Node? GetNode(int nodeId)
        {
            _nodes.TryGetValue(nodeId, out var node);
            return node;
        }

        /// <summary>
        /// Grafýn tüm düðümlerini döner.
        /// </summary>
        public IEnumerable<Node> GetAllNodes() => _nodes.Values;

        /// <summary>
        /// Bir düðümün tüm komþularýný döner.
        /// Algoritmalarda komþu bulma için kullanýlýr.
        /// </summary>
        public IEnumerable<int> GetNeighbors(int nodeId)
        {
            if (!_adj.TryGetValue(nodeId, out var list)) return Enumerable.Empty<int>();
            return list.Select(e => e.TargetId);
        }

        // ============ KENAR YÖNETÝMÝ ============

        /// <summary>
        /// Grafýn tüm kenarlarýný döner.
        /// </summary>
        public IEnumerable<Edge> GetEdges() => _adj.Values.SelectMany(l => l).Distinct();

        /// <summary>
        /// Grafa kenar ekler (yönsüz: her iki yöne eklenir).
        /// </summary>
        /// <returns>Baþarýlý ise true, zaten varsa false</returns>
        public bool AddEdge(Edge edge)
        {
            if (edge == null) throw new ArgumentNullException(nameof(edge));
            if (edge.IsLoop) return false;
            
            // Her iki düðümün de graf'ta olmasý gerekir
            if (!_nodes.ContainsKey(edge.SourceId) || !_nodes.ContainsKey(edge.TargetId)) return false;

            // Source ? Target yönü
            if (!_adj.TryGetValue(edge.SourceId, out var list))
            {
                list = new List<Edge>();
                _adj[edge.SourceId] = list;
            }

            // Duplicate kenar engelle
            if (list.Any(e => e.TargetId == edge.TargetId)) return false;
            list.Add(edge);

            // Target ? Source yönü (yönsüz graf)
            if (!_adj.TryGetValue(edge.TargetId, out var rev))
            {
                rev = new List<Edge>();
                _adj[edge.TargetId] = rev;
            }
            
            // Reverse kenarý ekle
            if (!rev.Any(e => e.TargetId == edge.SourceId))
            {
                rev.Add(new Edge(edge.TargetId, edge.SourceId, edge.Weight));
            }
            
            return true;
        }

        /// <summary>
        /// Graftan kenar kaldýrýr (her iki yöndeki kenar silinir).
        /// </summary>
        public bool RemoveEdge(int sourceId, int targetId)
        {
            bool removed = false;
            
            // Source ? Target yönünü sil
            if (_adj.TryGetValue(sourceId, out var list))
            {
                removed |= list.RemoveAll(e => e.TargetId == targetId) > 0;
            }
            
            // Target ? Source yönünü sil (yönsüz)
            if (_adj.TryGetValue(targetId, out var rev))
            {
                removed |= rev.RemoveAll(e => e.TargetId == sourceId) > 0;
            }
            
            return removed;
        }

        /// <summary>
        /// Ýki düðüm arasýndaki kenar aðýrlýðýný döner.
        /// Dijkstra ve A* algoritmalarý tarafýndan kullanýlýr.
        /// </summary>
        /// <param name="weight">Kenar aðýrlýðý (baþarýsýz ise PositiveInfinity)</param>
        /// <returns>Kenar varsa true, yoksa false</returns>
        public bool TryGetEdgeWeight(int sourceId, int targetId, out double weight)
        {
            weight = double.PositiveInfinity;
            if (!_adj.TryGetValue(sourceId, out var list)) return false;
            var e = list.FirstOrDefault(x => x.TargetId == targetId);
            if (e == null) return false;
            weight = e.Weight;
            return true;
        }
    }
}
