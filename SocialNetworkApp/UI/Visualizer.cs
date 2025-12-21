using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SocialNetworkApp.Models;

namespace SocialNetworkApp.UI
{
    /// <summary>
    /// Sosyal að grafýný görsel olarak çizer ve render eder.
    /// Node konumlarý, seçim, renklendirme ve highlight iþlemlerini yönetir.
    /// </summary>
    public class Visualizer
    {
        // Canvas boyutlarý
        private int _canvasWidth;
        private int _canvasHeight;

        // Node konumlarý (ID -> PointF)
        private Dictionary<int, PointF> _nodePositions = new();

        // Node seçim durumu
        private HashSet<int> _selectedNodeIds = new();

        // Node renkleri (ID -> Color)
        private Dictionary<int, Color> _nodeColors = new();

        // Path highlight (yol göstermek için)
        private List<int> _highlightedPath = new();

        // Node yarýçapý
        public float NodeRadius { get; set; } = 15f;

        // Edge kalýnlýðý
        public float EdgeWidth { get; set; } = 2f;

        // Edge highlight kalýnlýðý
        public float HighlightedEdgeWidth { get; set; } = 4f;

        // Font
        public Font LabelFont { get; set; } = new Font("Arial", 10, FontStyle.Bold);

        // Renkler
        public Color NodeFillColor { get; set; } = Color.LightBlue;
        public Color NodeBorderColor { get; set; } = Color.DarkBlue;
        public Color SelectedNodeBorderColor { get; set; } = Color.Red;
        public Color EdgeColor { get; set; } = Color.Gray;
        public Color HighlightedEdgeColor { get; set; } = Color.Red;
        public Color TextColor { get; set; } = Color.Black;

        /// <summary>
        /// Visualizer'ý canvas boyutlarýyla baþlatýr.
        /// </summary>
        /// <param name="canvasWidth">Canvas geniþliði</param>
        /// <param name="canvasHeight">Canvas yüksekliði</param>
        public Visualizer(int canvasWidth, int canvasHeight)
        {
            _canvasWidth = canvasWidth;
            _canvasHeight = canvasHeight;
        }

        /// <summary>
        /// Canvas boyutlarýný günceller (resize durumunda).
        /// </summary>
        public void SetCanvasSize(int width, int height)
        {
            _canvasWidth = width;
            _canvasHeight = height;
            // Node konumlarýný yeniden hesapla
            if (_nodePositions.Count > 0)
            {
                var nodeIds = _nodePositions.Keys.ToList();
                CalculateCircleLayout(nodeIds);
            }
        }

        /// <summary>
        /// Grafý render eder (çizer).
        /// Edges ? Nodes ? Labels sýrasýnda çizilir.
        /// </summary>
        /// <param name="g">Graphics nesnesi</param>
        /// <param name="graph">Çizilecek graf</param>
        public void Render(Graphics g, Graph graph)
        {
            if (g == null || graph == null) return;

            g.Clear(Color.White);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Node konumlarýný hesapla (ilk kez)
            if (_nodePositions.Count == 0)
            {
                var nodeIds = graph.GetAllNodes().Select(n => n.Id).ToList();
                CalculateCircleLayout(nodeIds);
            }

            // Kenarlarý çiz (önce, node'larýn altýnda olsun)
            DrawAllEdges(g, graph);

            // Node'larý çiz
            DrawAllNodes(g, graph);

            // Label'larý çiz
            DrawAllLabels(g, graph);
        }

        /// <summary>
        /// Belirli bir node'a týklanýp týklanmadýðýný kontrol eder.
        /// </summary>
        /// <param name="mousePos">Fare pozisyonu</param>
        /// <returns>Týklanan node'un ID'si, yoksa -1</returns>
        public int GetNodeAtPosition(PointF mousePos)
        {
            foreach (var (nodeId, pos) in _nodePositions)
            {
                var distance = Distance(mousePos, pos);
                if (distance <= NodeRadius)
                {
                    return nodeId;
                }
            }
            return -1;
        }

        /// <summary>
        /// Node'u seçilen liste'ye ekler.
        /// </summary>
        public void SelectNode(int nodeId)
        {
            _selectedNodeIds.Add(nodeId);
        }

        /// <summary>
        /// Node'u seçilen liste'den çýkarýr.
        /// </summary>
        public void DeselectNode(int nodeId)
        {
            _selectedNodeIds.Remove(nodeId);
        }

        /// <summary>
        /// Tüm seçimleri temizler.
        /// </summary>
        public void ClearSelection()
        {
            _selectedNodeIds.Clear();
        }

        /// <summary>
        /// Seçili node'larýn ID'lerini döner.
        /// </summary>
        public IEnumerable<int> GetSelectedNodeIds() => _selectedNodeIds;

        /// <summary>
        /// Node'u seçili state'e getir veya çýkar (toggle).
        /// </summary>
        public void ToggleSelection(int nodeId)
        {
            if (_selectedNodeIds.Contains(nodeId))
                DeselectNode(nodeId);
            else
                SelectNode(nodeId);
        }

        /// <summary>
        /// Belirli bir yolu highlight eder (örn: en kýsa yol).
        /// </summary>
        /// <param name="path">Node ID'lerinin listesi</param>
        public void HighlightPath(List<int> path)
        {
            _highlightedPath = path ?? new List<int>();
        }

        /// <summary>
        /// Highlight'ý temizler.
        /// </summary>
        public void ClearHighlight()
        {
            _highlightedPath.Clear();
        }

        /// <summary>
        /// Node'lara renk atar (örn: Welsh-Powell renklendirmesi).
        /// </summary>
        /// <param name="colorMap">Node ID -> Color eþlemesi</param>
        public void SetNodeColors(Dictionary<int, Color> colorMap)
        {
            _nodeColors = colorMap ?? new Dictionary<int, Color>();
        }

        /// <summary>
        /// Belirli bir node'un rengini döner.
        /// </summary>
        private Color GetNodeColor(int nodeId)
        {
            if (_nodeColors.TryGetValue(nodeId, out var color))
                return color;
            return NodeFillColor;
        }

        /// <summary>
        /// Tüm kenarlarý çizer.
        /// </summary>
        private void DrawAllEdges(Graphics g, Graph graph)
        {
            var drawnEdges = new HashSet<string>();

            foreach (var edge in graph.GetEdges())
            {
                // Yönsüz graf ? her kenarý bir kez çiz
                var key = edge.SourceId < edge.TargetId
                    ? $"{edge.SourceId}-{edge.TargetId}"
                    : $"{edge.TargetId}-{edge.SourceId}";

                if (drawnEdges.Contains(key)) continue;
                drawnEdges.Add(key);

                if (_nodePositions.TryGetValue(edge.SourceId, out var p1) &&
                    _nodePositions.TryGetValue(edge.TargetId, out var p2))
                {
                    DrawEdge(g, edge, p1, p2);
                }
            }
        }

        /// <summary>
        /// Tek bir kenarý çizer.
        /// </summary>
        private void DrawEdge(Graphics g, Edge edge, PointF p1, PointF p2)
        {
            // Highlight kontrol
            bool isHighlighted = IsEdgeInPath(edge.SourceId, edge.TargetId);
            var color = isHighlighted ? HighlightedEdgeColor : EdgeColor;
            var width = isHighlighted ? HighlightedEdgeWidth : EdgeWidth;

            using (var pen = new Pen(color, width))
            {
                g.DrawLine(pen, p1, p2);

                // Aðýrlýk etiketini çiz
                if (width == EdgeWidth) // Sadece normal edge'ler için
                {
                    DrawEdgeWeight(g, edge, p1, p2);
                }
            }
        }

        /// <summary>
        /// Kenar aðýrlýðýný çizer (edge ortasýnda).
        /// </summary>
        private void DrawEdgeWeight(Graphics g, Edge edge, PointF p1, PointF p2)
        {
            var midX = (p1.X + p2.X) / 2;
            var midY = (p1.Y + p2.Y) / 2;

            var text = edge.Weight.ToString("F2");
            var brush = new SolidBrush(TextColor);
            var size = g.MeasureString(text, LabelFont);

            g.DrawString(text, LabelFont, brush, midX - size.Width / 2, midY - size.Height / 2);
            brush.Dispose();
        }

        /// <summary>
        /// Tüm node'larý çizer.
        /// </summary>
        private void DrawAllNodes(Graphics g, Graph graph)
        {
            foreach (var node in graph.GetAllNodes())
            {
                if (_nodePositions.TryGetValue(node.Id, out var pos))
                {
                    DrawNode(g, node, pos);
                }
            }
        }

        /// <summary>
        /// Tek bir node'u çizer.
        /// </summary>
        private void DrawNode(Graphics g, Node node, PointF pos)
        {
            var nodeColor = GetNodeColor(node.Id);
            var isBorderColor = _selectedNodeIds.Contains(node.Id) 
                ? SelectedNodeBorderColor 
                : NodeBorderColor;

            // Daire çiz
            using (var brush = new SolidBrush(nodeColor))
            using (var pen = new Pen(isBorderColor, _selectedNodeIds.Contains(node.Id) ? 3f : 2f))
            {
                var rect = new RectangleF(
                    pos.X - NodeRadius,
                    pos.Y - NodeRadius,
                    NodeRadius * 2,
                    NodeRadius * 2
                );

                g.FillEllipse(brush, rect);
                g.DrawEllipse(pen, rect);
            }
        }

        /// <summary>
        /// Tüm node label'larýný çizer.
        /// </summary>
        private void DrawAllLabels(Graphics g, Graph graph)
        {
            using (var brush = new SolidBrush(TextColor))
            {
                foreach (var node in graph.GetAllNodes())
                {
                    if (_nodePositions.TryGetValue(node.Id, out var pos))
                    {
                        var text = node.Id.ToString();
                        var size = g.MeasureString(text, LabelFont);

                        g.DrawString(text, LabelFont, brush,
                            pos.X - size.Width / 2,
                            pos.Y - size.Height / 2);
                    }
                }
            }
        }

        /// <summary>
        /// Node'larý daire þeklinde yerleþtirir (basit layout).
        /// </summary>
        private void CalculateCircleLayout(List<int> nodeIds)
        {
            if (nodeIds.Count == 0) return;

            var centerX = _canvasWidth / 2f;
            var centerY = _canvasHeight / 2f;
            var radius = Math.Min(_canvasWidth, _canvasHeight) / 3f;

            for (int i = 0; i < nodeIds.Count; i++)
            {
                var angle = (2 * Math.PI * i) / nodeIds.Count;
                var x = centerX + radius * (float)Math.Cos(angle);
                var y = centerY + radius * (float)Math.Sin(angle);

                _nodePositions[nodeIds[i]] = new PointF(x, y);
            }
        }

        /// <summary>
        /// Kenar'ýn highlight edilen yolda olup olmadýðýný kontrol eder.
        /// </summary>
        private bool IsEdgeInPath(int sourceId, int targetId)
        {
            if (_highlightedPath.Count < 2) return false;

            for (int i = 0; i < _highlightedPath.Count - 1; i++)
            {
                var from = _highlightedPath[i];
                var to = _highlightedPath[i + 1];

                if ((from == sourceId && to == targetId) || (from == targetId && to == sourceId))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Ýki nokta arasýndaki Euclidean mesafesini hesaplar.
        /// </summary>
        private float Distance(PointF p1, PointF p2)
        {
            var dx = p1.X - p2.X;
            var dy = p1.Y - p2.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Node konumlarýný döner (debug/test için).
        /// </summary>
        public Dictionary<int, PointF> GetNodePositions() => new Dictionary<int, PointF>(_nodePositions);

        /// <summary>
        /// Node konumlarýný manuel olarak ayarlar.
        /// </summary>
        public void SetNodePositions(Dictionary<int, PointF> positions)
        {
            _nodePositions = new Dictionary<int, PointF>(positions ?? new Dictionary<int, PointF>());
        }
    }
}
