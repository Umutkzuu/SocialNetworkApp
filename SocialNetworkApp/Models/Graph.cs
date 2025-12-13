using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialNetworkApp.Models
{
    /// <summary>
    /// Sosyal a? graf?n? temsil eder.
    /// Dü?ümler (nodes) ve kenarlar? (edges) yönetir.
    /// Yönsüz, a??rl?kl? graf yap?s?d?r.
    /// </summary>
    public class Graph
    {
        // Tüm dü?ümleri ID'ye göre sakla (h?zl? lookup)
        private readonly Dictionary<int, Node> _nodes = new();
        
        // Kom?uluk listesi: Her dü?ümün kom?ular?n? ve kenarlar?n? sakla
        private readonly Dictionary<int, List<Edge>> _adj = new();

        /// <summary>
        /// Graf?n tüm dü?ümlerine read-only eri?im.
        /// </summary>
        public IReadOnlyDictionary<int, Node> Nodes => _nodes;

        /// <summary>
        /// Grafa yeni dü?üm ekler.
        /// </summary>
        /// <returns>Ba?ar?l? ise true, zaten varsa false</returns>
        public bool AddNode(Node node)
        {
            if (node is null) throw new ArgumentNullException(nameof(node));
            if (_nodes.ContainsKey(node.Id)) return false;

            _nodes.Add(node.Id, node);
            return true;
        }

        /// <summary>
        /// Grafdan dü?üm kald?r?r (ili?kili kenarlar da silinir).
        /// </summary>
        public bool RemoveNode(int nodeId)
        {
            if (!_nodes.ContainsKey(nodeId)) return false;

            // Bu dü?ümün tüm kenarlar?n? sil
            if (_adj.TryGetValue(nodeId, out var edges))
            {
                var neighbors = edges.Select(e => e.TargetId).ToList();
                foreach (var neighborId in neighbors)
                {
                    RemoveEdge(nodeId, neighborId);
                }
                _adj.Remove(nodeId);
            }
            
            // Di?er dü?ümlerin listelerinden bu dü?üme giden kenarlar? sil
            foreach (var list in _adj.Values)
            {
                list.RemoveAll(e => e.TargetId == nodeId);
            }

            _nodes.Remove(nodeId);
            return true;
        }

        /// <summary>
        /// Dü?ümün özelliklerini günceller.
        /// </summary>
        public bool UpdateNode(int nodeId, string name, double aktiflik, double etkilesim)
        {
            if (!_nodes.TryGetValue(nodeId, out var node)) return false;
            node.Update(name, aktiflik, etkilesim);
            return true;
        }

        /// <summary>
        /// ID'ye göre dü?ümü döner.
        /// </summary>
        public Node? GetNode(int nodeId)
        {
            _nodes.TryGetValue(nodeId, out var node);
            return node;
        }

        /// <summary>
        /// Graf?n tüm dü?ümlerini döner.
        /// </summary>
        public IEnumerable<Node> GetAllNodes() => _nodes.Values;

        /// <summary>
        /// Bir dü?ümün tüm kom?ular?n? döner.
        /// Algoritmalarda kom?u bulma için kullan?l?r.
        /// </summary>
        public IEnumerable<int> GetNeighbors(int nodeId)
        {
            if (!_adj.TryGetValue(nodeId, out var list)) return Enumerable.Empty<int>();
            return list.Select(e => e.TargetId);
        }

        // ============ KENAR YÖNET?M? ============

        /// <summary>
        /// Graf?n tüm kenarlar?n? döner.
        /// </summary>
        public IEnumerable<Edge> GetEdges() => _adj.Values.SelectMany(l => l).Distinct();

        /// <summary>
        /// Grafa kenar ekler (yönsüz: her iki yöne eklenir).
        /// </summary>
        /// <returns>Ba?ar?l? ise true, zaten varsa false</returns>
        public bool AddEdge(Edge edge)
        {
            if (edge == null) throw new ArgumentNullException(nameof(edge));
            if (edge.IsLoop) return false;
            
            // Her iki dü?ümün de graf'ta olmas? gerekir
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
            
            // Reverse kenar? ekle
            if (!rev.Any(e => e.TargetId == edge.SourceId))
            {
                rev.Add(new Edge(edge.TargetId, edge.SourceId, edge.Weight));
            }
            
            return true;
        }

        /// <summary>
        /// Graftan kenar kald?r?r (her iki yöndeki kenar silinir).
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
        /// ?ki dü?üm aras?ndaki kenar a??rl???n? döner.
        /// Dijkstra ve A* algoritmalar? taraf?ndan kullan?l?r.
        /// </summary>
        /// <param name="weight">Kenar a??rl??? (ba?ar?s?z ise PositiveInfinity)</param>
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
