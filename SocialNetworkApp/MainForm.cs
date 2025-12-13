using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SocialNetworkApp.Models;
using SocialNetworkApp.Algorithms;
using SocialNetworkApp.Services;
using SocialNetworkApp.UI;

namespace SocialNetworkApp
{
    /// <summary>
    /// Main form for social network analysis application.
    /// Visualizes graph, runs algorithms, manages import/export.
    /// </summary>
    public partial class Form1 : Form
    {
        // Core objects
        private Graph _graph;
        private Visualizer _visualizer;

        // UI Controls
        private MenuStrip _menuStrip;
        private ToolStrip _toolStrip;
        private StatusStrip _statusStrip;
        private Panel _leftPanel;
        private Panel _canvasPanel;
        private SplitContainer _splitContainer;

        // Left panel controls
        private ListBox _listBoxNodes;
        private ListBox _listBoxEdges;
        private TextBox _textBoxNodeName;
        private NumericUpDown _numericNodeId;
        private NumericUpDown _numericAktiflik;
        private NumericUpDown _numericEtkilesim;
        private NumericUpDown _numericSourceId;
        private NumericUpDown _numericTargetId;
        private NumericUpDown _numericWeight;
        private Button _buttonAddNode;
        private Button _buttonRemoveNode;
        private Button _buttonAddEdge;
        private Button _buttonRemoveEdge;
        private Label _labelSelection;

        // Menu and toolbar items
        private ToolStripMenuItem _menuFile;
        private ToolStripMenuItem _menuAlgorithm;
        private ToolStripMenuItem _menuAnalysis;
        private ToolStripMenuItem _menuHelp;

        // Selection state
        private int _selectedSourceNodeId = -1;
        private int _selectedTargetNodeId = -1;

        public Form1()
        {
            // Set UTF-8 encoding for Turkish character support
            Console.OutputEncoding = Encoding.UTF8;
            
            InitializeComponent();
            InitializeGraph();
            InitializeUI();
            SetupEventHandlers();
            RefreshNodeEdgeList();
            InvalidateCanvas();
        }

        private void InitializeGraph()
        {
            _graph = new Graph();
            AddSampleData();
        }

        private void AddSampleData()
        {
            // Sample nodes
            _graph.AddNode(new Node(1, "Alice", 0.8, 12));
            _graph.AddNode(new Node(2, "Bob", 0.6, 10));
            _graph.AddNode(new Node(3, "Charlie", 0.7, 15));
            _graph.AddNode(new Node(4, "Diana", 0.5, 8));
            _graph.AddNode(new Node(5, "Eve", 0.9, 20));

            // Sample edges
            _graph.AddEdge(new Edge(1, 2, 0.5));
            _graph.AddEdge(new Edge(2, 3, 0.6));
            _graph.AddEdge(new Edge(3, 4, 0.7));
            _graph.AddEdge(new Edge(4, 5, 0.4));
            _graph.AddEdge(new Edge(1, 5, 0.8));
        }

        private void InitializeUI()
        {
            // Form settings
            Text = "Social Network Analysis App";
            ClientSize = new Size(1600, 900);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.LightGray;

            // MenuStrip
            _menuStrip = new MenuStrip();
            CreateMenus();
            Controls.Add(_menuStrip);

            // ToolStrip
            _toolStrip = new ToolStrip { ImageScalingSize = new Size(24, 24) };
            CreateToolStrip();
            Controls.Add(_toolStrip);

            // Main container
            _splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 350,
                Orientation = Orientation.Vertical
            };

            // Left panel (controls)
            _leftPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.WhiteSmoke,
                AutoScroll = true
            };
            CreateLeftPanel();

            // Canvas panel
            _canvasPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            _canvasPanel.Paint += CanvasPanel_Paint;
            _canvasPanel.MouseClick += CanvasPanel_MouseClick;
            _canvasPanel.Resize += CanvasPanel_Resize;

            // Initialize Visualizer
            _visualizer = new Visualizer(_canvasPanel.Width, _canvasPanel.Height);

            _splitContainer.Panel1.Controls.Add(_leftPanel);
            _splitContainer.Panel2.Controls.Add(_canvasPanel);

            Controls.Add(_splitContainer);

            // StatusStrip
            _statusStrip = new StatusStrip();
            _statusStrip.Items.Add(new ToolStripStatusLabel("Ready"));
            Controls.Add(_statusStrip);
        }

        private void CreateMenus()
        {
            // File menu
            _menuFile = new ToolStripMenuItem("&File");
            _menuFile.DropDownItems.Add(new ToolStripMenuItem("&Import JSON", null, MenuImportJson_Click));
            _menuFile.DropDownItems.Add(new ToolStripMenuItem("&Export JSON", null, MenuExportJson_Click));
            _menuFile.DropDownItems.Add(new ToolStripSeparator());
            _menuFile.DropDownItems.Add(new ToolStripMenuItem("Import &CSV", null, MenuImportCsv_Click));
            _menuFile.DropDownItems.Add(new ToolStripMenuItem("Export C&SV", null, MenuExportCsv_Click));
            _menuFile.DropDownItems.Add(new ToolStripSeparator());
            _menuFile.DropDownItems.Add(new ToolStripMenuItem("E&xit", null, MenuExit_Click));

            // Algorithm menu
            _menuAlgorithm = new ToolStripMenuItem("&Algorithms");
            _menuAlgorithm.DropDownItems.Add(new ToolStripMenuItem("&BFS/DFS Traversal", null, MenuTraversal_Click));
            _menuAlgorithm.DropDownItems.Add(new ToolStripMenuItem("&Dijkstra Shortest Path", null, MenuDijkstra_Click));
            _menuAlgorithm.DropDownItems.Add(new ToolStripMenuItem("&A* Shortest Path", null, MenuAStar_Click));
            _menuAlgorithm.DropDownItems.Add(new ToolStripSeparator());
            _menuAlgorithm.DropDownItems.Add(new ToolStripMenuItem("Connected &Components", null, MenuCommunities_Click));
            _menuAlgorithm.DropDownItems.Add(new ToolStripMenuItem("&Welsh-Powell Coloring", null, MenuColoring_Click));

            // Analysis menu
            _menuAnalysis = new ToolStripMenuItem("&Analysis");
            _menuAnalysis.DropDownItems.Add(new ToolStripMenuItem("&Degree Centrality", null, MenuCentrality_Click));
            _menuAnalysis.DropDownItems.Add(new ToolStripMenuItem("&Clear Selection", null, MenuClearSelection_Click));

            // Help menu
            _menuHelp = new ToolStripMenuItem("&Help");
            _menuHelp.DropDownItems.Add(new ToolStripMenuItem("&About", null, MenuAbout_Click));

            _menuStrip.Items.Add(_menuFile);
            _menuStrip.Items.Add(_menuAlgorithm);
            _menuStrip.Items.Add(_menuAnalysis);
            _menuStrip.Items.Add(_menuHelp);
        }

        private void CreateToolStrip()
        {
            _toolStrip.Items.Add(new ToolStripButton("Open", null, (s, e) => MenuImportJson_Click(null, null)));
            _toolStrip.Items.Add(new ToolStripButton("Save", null, (s, e) => MenuExportJson_Click(null, null)));
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(new ToolStripButton("Components", null, (s, e) => MenuCommunities_Click(null, null)));
            _toolStrip.Items.Add(new ToolStripButton("Centrality", null, (s, e) => MenuCentrality_Click(null, null)));
            _toolStrip.Items.Add(new ToolStripSeparator());
            _toolStrip.Items.Add(new ToolStripButton("Clear", null, (s, e) => MenuClearSelection_Click(null, null)));
        }

        private void CreateLeftPanel()
        {
            int y = 10;

            // Node addition
            var labelNodeSection = new Label { Text = "Add Node", Left = 10, Top = y, Width = 200, Font = new Font("Arial", 11, FontStyle.Bold) };
            _leftPanel.Controls.Add(labelNodeSection);
            y += 30;

            _leftPanel.Controls.Add(new Label { Text = "ID:", Left = 10, Top = y, Width = 50 });
            _numericNodeId = new NumericUpDown { Left = 70, Top = y, Width = 100, Minimum = 1, Maximum = 1000, Value = 6 };
            _leftPanel.Controls.Add(_numericNodeId);
            y += 30;

            _leftPanel.Controls.Add(new Label { Text = "Name:", Left = 10, Top = y, Width = 50 });
            _textBoxNodeName = new TextBox { Left = 70, Top = y, Width = 100 };
            _leftPanel.Controls.Add(_textBoxNodeName);
            y += 30;

            _leftPanel.Controls.Add(new Label { Text = "Activity:", Left = 10, Top = y, Width = 50 });
            _numericAktiflik = new NumericUpDown { Left = 70, Top = y, Width = 100, DecimalPlaces = 2, Maximum = 1 };
            _leftPanel.Controls.Add(_numericAktiflik);
            y += 30;

            _leftPanel.Controls.Add(new Label { Text = "Interaction:", Left = 10, Top = y, Width = 50 });
            _numericEtkilesim = new NumericUpDown { Left = 70, Top = y, Width = 100, Maximum = 100 };
            _leftPanel.Controls.Add(_numericEtkilesim);
            y += 30;

            _buttonAddNode = new Button { Text = "Add Node", Left = 10, Top = y, Width = 160, Height = 30 };
            _buttonAddNode.Click += ButtonAddNode_Click;
            _leftPanel.Controls.Add(_buttonAddNode);
            y += 40;

            // Node list
            var labelNodeList = new Label { Text = "Node List", Left = 10, Top = y, Width = 200, Font = new Font("Arial", 11, FontStyle.Bold) };
            _leftPanel.Controls.Add(labelNodeList);
            y += 30;

            _listBoxNodes = new ListBox { Left = 10, Top = y, Width = 300, Height = 80 };
            _listBoxNodes.DoubleClick += ListBoxNodes_DoubleClick;
            _leftPanel.Controls.Add(_listBoxNodes);
            y += 90;

            _buttonRemoveNode = new Button { Text = "Remove Node", Left = 10, Top = y, Width = 160, Height = 30 };
            _buttonRemoveNode.Click += ButtonRemoveNode_Click;
            _leftPanel.Controls.Add(_buttonRemoveNode);
            y += 40;

            // Edge addition
            var labelEdgeSection = new Label { Text = "Add Edge", Left = 10, Top = y, Width = 200, Font = new Font("Arial", 11, FontStyle.Bold) };
            _leftPanel.Controls.Add(labelEdgeSection);
            y += 30;

            _leftPanel.Controls.Add(new Label { Text = "Source ID:", Left = 10, Top = y, Width = 70 });
            _numericSourceId = new NumericUpDown { Left = 90, Top = y, Width = 80, Minimum = 1, Maximum = 1000 };
            _leftPanel.Controls.Add(_numericSourceId);
            y += 30;

            _leftPanel.Controls.Add(new Label { Text = "Target ID:", Left = 10, Top = y, Width = 70 });
            _numericTargetId = new NumericUpDown { Left = 90, Top = y, Width = 80, Minimum = 1, Maximum = 1000 };
            _leftPanel.Controls.Add(_numericTargetId);
            y += 30;

            _leftPanel.Controls.Add(new Label { Text = "Weight:", Left = 10, Top = y, Width = 70 });
            _numericWeight = new NumericUpDown { Left = 90, Top = y, Width = 80, DecimalPlaces = 2, Increment = 0.1m, Value = 0.5m };
            _leftPanel.Controls.Add(_numericWeight);
            y += 30;

            _buttonAddEdge = new Button { Text = "Add Edge", Left = 10, Top = y, Width = 160, Height = 30 };
            _buttonAddEdge.Click += ButtonAddEdge_Click;
            _leftPanel.Controls.Add(_buttonAddEdge);
            y += 40;

            // Edge list
            var labelEdgeList = new Label { Text = "Edge List", Left = 10, Top = y, Width = 200, Font = new Font("Arial", 11, FontStyle.Bold) };
            _leftPanel.Controls.Add(labelEdgeList);
            y += 30;

            _listBoxEdges = new ListBox { Left = 10, Top = y, Width = 300, Height = 80 };
            _leftPanel.Controls.Add(_listBoxEdges);
            y += 90;

            _buttonRemoveEdge = new Button { Text = "Remove Edge", Left = 10, Top = y, Width = 160, Height = 30 };
            _buttonRemoveEdge.Click += ButtonRemoveEdge_Click;
            _leftPanel.Controls.Add(_buttonRemoveEdge);
            y += 40;

            // Selection status
            var labelSelectionSection = new Label { Text = "Selection Status", Left = 10, Top = y, Width = 200, Font = new Font("Arial", 11, FontStyle.Bold) };
            _leftPanel.Controls.Add(labelSelectionSection);
            y += 30;

            _labelSelection = new Label { Left = 10, Top = y, Width = 300, Height = 60, AutoSize = false, Text = "Source: Not Selected\nTarget: Not Selected", BorderStyle = BorderStyle.FixedSingle };
            _leftPanel.Controls.Add(_labelSelection);
        }

        private void SetupEventHandlers()
        {
            _canvasPanel.Paint += CanvasPanel_Paint;
            _canvasPanel.MouseClick += CanvasPanel_MouseClick;
            _canvasPanel.Resize += CanvasPanel_Resize;
        }

        private void CanvasPanel_Paint(object sender, PaintEventArgs e)
        {
            _visualizer.Render(e.Graphics, _graph);
        }

        private void CanvasPanel_MouseClick(object sender, MouseEventArgs e)
        {
            var nodeId = _visualizer.GetNodeAtPosition(new PointF(e.X, e.Y));

            if (nodeId != -1)
            {
                // Multi-select with Ctrl key
                if ((Control.ModifierKeys & Keys.Control) == 0)
                {
                    _visualizer.ClearSelection();
                }

                _visualizer.ToggleSelection(nodeId);
                var selected = _visualizer.GetSelectedNodeIds().ToList();

                if (selected.Count == 1)
                {
                    _selectedSourceNodeId = selected[0];
                    _selectedTargetNodeId = -1;
                }
                else if (selected.Count == 2)
                {
                    _selectedSourceNodeId = selected[0];
                    _selectedTargetNodeId = selected[1];
                }

                UpdateSelectionLabel();
                _canvasPanel.Invalidate();
            }
        }

        private void CanvasPanel_Resize(object sender, EventArgs e)
        {
            _visualizer.SetCanvasSize(_canvasPanel.Width, _canvasPanel.Height);
            _canvasPanel.Invalidate();
        }

        private void UpdateSelectionLabel()
        {
            var sourceName = _selectedSourceNodeId != -1 ? _graph.GetNode(_selectedSourceNodeId)?.Name ?? "Unknown" : "Not Selected";
            var targetName = _selectedTargetNodeId != -1 ? _graph.GetNode(_selectedTargetNodeId)?.Name ?? "Unknown" : "Not Selected";

            _labelSelection.Text = $"Source: {sourceName} (#{_selectedSourceNodeId})\nTarget: {targetName} (#{_selectedTargetNodeId})";
        }

        private void RefreshNodeEdgeList()
        {
            _listBoxNodes.Items.Clear();
            foreach (var node in _graph.GetAllNodes().OrderBy(n => n.Id))
            {
                _listBoxNodes.Items.Add($"#{node.Id}: {node.Name} (A:{node.Aktiflik:F2}, E:{node.Etkilesim:F2})");
            }

            _listBoxEdges.Items.Clear();
            var edges = _graph.GetEdges().Distinct().OrderBy(e => e.SourceId).ThenBy(e => e.TargetId);
            foreach (var edge in edges)
            {
                _listBoxEdges.Items.Add($"{edge.SourceId} <-> {edge.TargetId} (w:{edge.Weight:F2})");
            }
        }

        private void InvalidateCanvas()
        {
            if (_canvasPanel != null)
                _canvasPanel.Invalidate();
        }

        #region Button Click Events

        private void ButtonAddNode_Click(object sender, EventArgs e)
        {
            try
            {
                var id = (int)_numericNodeId.Value;
                var name = _textBoxNodeName.Text.Trim();
                var aktiflik = (double)_numericAktiflik.Value;
                var etkilesim = (double)_numericEtkilesim.Value;

                if (string.IsNullOrWhiteSpace(name))
                {
                    MessageBox.Show("Name cannot be empty!");
                    return;
                }

                var node = new Node(id, name, aktiflik, etkilesim);
                if (_graph.AddNode(node))
                {
                    MessageBox.Show($"Node #{id} added successfully!");
                    _textBoxNodeName.Clear();
                    _numericNodeId.Value = id + 1;
                    RefreshNodeEdgeList();
                    InvalidateCanvas();
                }
                else
                {
                    MessageBox.Show($"Node #{id} already exists!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void ButtonRemoveNode_Click(object sender, EventArgs e)
        {
            if (_listBoxNodes.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a node!");
                return;
            }

            var line = _listBoxNodes.SelectedItem.ToString();
            var nodeId = int.Parse(line.Split(':')[0].Substring(1));

            if (_graph.RemoveNode(nodeId))
            {
                MessageBox.Show("Node removed successfully!");
                RefreshNodeEdgeList();
                InvalidateCanvas();
            }
        }

        private void ButtonAddEdge_Click(object sender, EventArgs e)
        {
            try
            {
                var sourceId = (int)_numericSourceId.Value;
                var targetId = (int)_numericTargetId.Value;
                var weight = (double)_numericWeight.Value;

                if (sourceId == targetId)
                {
                    MessageBox.Show("Self-loop not allowed!");
                    return;
                }

                var edge = new Edge(sourceId, targetId, weight);
                if (_graph.AddEdge(edge))
                {
                    MessageBox.Show("Edge added successfully!");
                    RefreshNodeEdgeList();
                    InvalidateCanvas();
                }
                else
                {
                    MessageBox.Show("Failed to add edge (nodes exist?)!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }

        private void ButtonRemoveEdge_Click(object sender, EventArgs e)
        {
            if (_listBoxEdges.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an edge!");
                return;
            }

            var line = _listBoxEdges.SelectedItem.ToString();
            var parts = line.Split(' ');
            var sourceId = int.Parse(parts[0]);
            var targetId = int.Parse(parts[2]);

            if (_graph.RemoveEdge(sourceId, targetId))
            {
                MessageBox.Show("Edge removed successfully!");
                RefreshNodeEdgeList();
                InvalidateCanvas();
            }
        }

        private void ListBoxNodes_DoubleClick(object sender, EventArgs e)
        {
            if (_listBoxNodes.SelectedIndex == -1) return;

            var line = _listBoxNodes.SelectedItem.ToString();
            var nodeId = int.Parse(line.Split(':')[0].Substring(1));

            _visualizer.ClearSelection();
            _visualizer.SelectNode(nodeId);
            _selectedSourceNodeId = nodeId;
            _selectedTargetNodeId = -1;
            UpdateSelectionLabel();
            InvalidateCanvas();
        }

        #endregion

        #region Menu Click Events

        private void MenuImportJson_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog { Filter = "JSON files (*.json)|*.json" })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _graph = FileManager.ImportFromJson(dialog.FileName);
                        _visualizer.ClearSelection();
                        _visualizer.ClearHighlight();
                        RefreshNodeEdgeList();
                        InvalidateCanvas();
                        MessageBox.Show("Loaded from JSON successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                }
            }
        }

        private void MenuExportJson_Click(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog { Filter = "JSON files (*.json)|*.json", FileName = "graph.json" })
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        FileManager.ExportToJson(_graph, dialog.FileName);
                        MessageBox.Show("Saved as JSON successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                }
            }
        }

        private void MenuImportCsv_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _graph = FileManager.ImportFromCsv(dialog.SelectedPath);
                        _visualizer.ClearSelection();
                        _visualizer.ClearHighlight();
                        RefreshNodeEdgeList();
                        InvalidateCanvas();
                        MessageBox.Show("Loaded from CSV successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                }
            }
        }

        private void MenuExportCsv_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        FileManager.ExportToCsv(_graph, dialog.SelectedPath);
                        MessageBox.Show("Saved as CSV successfully!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}");
                    }
                }
            }
        }

        private void MenuTraversal_Click(object sender, EventArgs e)
        {
            if (_selectedSourceNodeId == -1)
            {
                MessageBox.Show("Please select a start node!");
                return;
            }

            var bfsResult = TraversalAlgorithms.BFS(_graph, _selectedSourceNodeId);
            var message = $"BFS (from {_selectedSourceNodeId}): {string.Join(" -> ", bfsResult)}";
            MessageBox.Show(message);
        }

        private void MenuDijkstra_Click(object sender, EventArgs e)
        {
            if (_selectedSourceNodeId == -1 || _selectedTargetNodeId == -1)
            {
                MessageBox.Show("Please select source and target nodes!");
                return;
            }

            var (path, cost) = PathFindingAlgorithms.Dijkstra(_graph, _selectedSourceNodeId, _selectedTargetNodeId);

            if (path.Count == 0)
            {
                MessageBox.Show("No path found!");
                return;
            }

            _visualizer.HighlightPath(path);
            InvalidateCanvas();

            var pathStr = string.Join(" -> ", path);
            MessageBox.Show($"Dijkstra Shortest Path:\n{pathStr}\n\nCost: {cost:F4}");
        }

        private void MenuAStar_Click(object sender, EventArgs e)
        {
            if (_selectedSourceNodeId == -1 || _selectedTargetNodeId == -1)
            {
                MessageBox.Show("Please select source and target nodes!");
                return;
            }

            var (path, cost) = PathFindingAlgorithms.AStar(_graph, _selectedSourceNodeId, _selectedTargetNodeId);

            if (path.Count == 0)
            {
                MessageBox.Show("No path found!");
                return;
            }

            _visualizer.HighlightPath(path);
            InvalidateCanvas();

            var pathStr = string.Join(" -> ", path);
            MessageBox.Show($"A* Shortest Path:\n{pathStr}\n\nCost: {cost:F4}");
        }

        private void MenuCommunities_Click(object sender, EventArgs e)
        {
            var summary = CommunityAlgorithms.GetComponentSummary(_graph);
            var colors = AnalysisAlgorithms.WelshPowellByComponent(_graph);
            var colorMap = ConvertColorIndices(colors);

            _visualizer.SetNodeColors(colorMap);
            InvalidateCanvas();

            MessageBox.Show(summary);
        }

        private void MenuColoring_Click(object sender, EventArgs e)
        {
            var colors = AnalysisAlgorithms.WelshPowell(_graph);
            var colorMap = ConvertColorIndices(colors);

            _visualizer.SetNodeColors(colorMap);
            InvalidateCanvas();

            MessageBox.Show("Welsh-Powell coloring applied!");
        }

        private void MenuCentrality_Click(object sender, EventArgs e)
        {
            var topNodes = AnalysisAlgorithms.TopDegreeNodes(_graph, 5);

            var message = "Top 5 Most Influential Users (Degree Centrality):\n\n";
            foreach (var (nodeId, degree) in topNodes)
            {
                var node = _graph.GetNode(nodeId);
                message += $"{node?.Name} (#{nodeId}): {degree} connections\n";
            }

            MessageBox.Show(message);
        }

        private void MenuClearSelection_Click(object sender, EventArgs e)
        {
            _visualizer.ClearSelection();
            _visualizer.ClearHighlight();
            _selectedSourceNodeId = -1;
            _selectedTargetNodeId = -1;
            UpdateSelectionLabel();
            InvalidateCanvas();
        }

        private void MenuExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MenuAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Social Network Analysis Application\nVersion 1.0\n\nCopyright 2025", "About");
        }

        #endregion

        private Dictionary<int, Color> ConvertColorIndices(Dictionary<int, int> colorIndices)
        {
            var colors = new Color[]
            {
                Color.Red,
                Color.Green,
                Color.Blue,
                Color.Yellow,
                Color.Cyan,
                Color.Magenta,
                Color.LightCoral,
                Color.LightGreen,
                Color.LightBlue,
                Color.LightYellow
            };

            var result = new Dictionary<int, Color>();
            foreach (var (nodeId, colorIndex) in colorIndices)
            {
                result[nodeId] = colors[colorIndex % colors.Length];
            }

            return result;
        }
    }
}
