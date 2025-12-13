# ?? Social Network Analysis Application

A comprehensive C# .NET 10 Windows Forms application for analyzing and visualizing social networks with advanced graph algorithms.

---

## ?? Project Overview

This application provides a complete social network analysis platform with:
- **Interactive Graph Visualization** with circle layout and node selection
- **Shortest Path Finding** (Dijkstra & A* algorithms)
- **Connected Component Detection** (community detection)
- **Weight Calculation** based on node properties
- **Data Import/Export** in JSON and CSV formats
- **Degree Centrality Analysis** and graph coloring

---

## ??? Architecture

### Core Models (`SocialNetworkApp/Models/`)

#### **Edge.cs**
- Represents weighted, undirected edges in the graph
- Properties: SourceId, TargetId, Weight
- Validation: Prevents self-loops, requires positive weight
- XML documentation for IDE IntelliSense

#### **Node.cs**
- Represents users/entities in the social network
- Properties: Id, Name, Aktiflik (activity level), Etkilesim (interaction count)
- Neighbors dictionary with edge weights
- Update method for property modifications
- Equals/GetHashCode based on Id

#### **Graph.cs**
- Core graph data structure using adjacency list
- **Key Methods:**
  - `AddNode(Node)` / `RemoveNode(int)` - Node management
  - `AddEdge(Edge)` / `RemoveEdge(int, int)` - Bidirectional edge handling
  - `GetNeighbors(int)` - O(1) neighbor lookup
  - `TryGetEdgeWeight(int, int)` - Safe weight retrieval
  - `GetAllNodes()` / `GetEdges()` - Graph enumeration
- Automatic edge cleanup when nodes are removed
- Supports undirected graphs (bidirectional edges)

---

## ?? Algorithms (`SocialNetworkApp/Algorithms/`)

### **PathFindingAlgorithms.cs** ? (SEN)
Implements shortest path algorithms with comprehensive comments:

1. **Dijkstra(Graph, startId, goalId)**
   - Finds shortest path in weighted graphs
   - Returns (List<int> path, double cost)
   - Uses distance dictionary and priority queue logic
   - Handles unreachable nodes

2. **A*(Graph, startId, goalId, heuristic)**
   - Optimized shortest path with heuristic function
   - Default heuristic: Euclidean distance on (Aktiflik, Etkilesim)
   - Returns (List<int> path, double cost)
   - Supports custom heuristic functions

3. **DefaultHeuristic(Node a, Node b)**
   - Euclidean distance calculation: ?((a.Aktiflik - b.Aktiflik)² + (a.Etkilesim - b.Etkilesim)²)
   - Used as default heuristic for A*

4. **Path Reconstruction**
   - `ReconstructPath()` - Dijkstra path building
   - `ReconstructPathAStar()` - A* path building

---

### **CommunityAlgorithms.cs** ? (SEN)
Community detection and component analysis:

1. **ConnectedComponents(Graph)**
   - Finds all connected components using DFS
   - Returns List<List<int>> (each component as node ID list)
   - Includes inline comments explaining DFS traversal

2. **GetComponentCount(Graph)**
   - Returns number of connected components
   - Quick overview of network fragmentation

3. **GetComponentSummary(Graph)**
   - Provides human-readable summary
   - Format: "Found X components. Largest: Y nodes (Component-Z)"
   - Useful for UI display

4. **GetComponentsWithNames(Graph)**
   - Returns tuple list with component names
   - Format: (name="Component-1 (5 nodes)", nodeIds=[...])
   - Optimized for ListBox display

5. **GetComponentOf(Graph, nodeId)**
   - Finds which component a specific node belongs to
   - Returns empty list if node not found

6. **AreInSameComponent(Graph, id1, id2)**
   - Checks if two nodes are connected
   - Useful for relationship analysis

---

### **TraversalAlgorithms.cs** ? (A Ki?isi)
Graph traversal with detailed comments:

1. **BFS(Graph, startId)**
   - Breadth-first search using Queue
   - Returns nodes in level-order
   - Commented: "Queue'ye ekle = ayn? seviyedeki kom?ular? i?le"

2. **DFS(Graph, startId)**
   - Depth-first search using Stack
   - Returns nodes in depth-order
   - Commented: "Stack'e push = derinli?e git"

---

### **AnalysisAlgorithms.cs** ? (A Ki?isi)
Graph analysis and coloring:

1. **DegreeCentrality(Graph)**
   - Calculates degree for each node
   - Returns Dictionary<int, int> mapping node ID to degree

2. **TopDegreeNodes(Graph, top=5)**
   - Returns top N nodes by degree
   - Useful for identifying influential users

3. **WelshPowell(Graph)**
   - Graph coloring algorithm
   - Returns Dictionary<int, int> (node ID -> color index)
   - Greedy approach: colors by descending degree

4. **WelshPowellByComponent(Graph)**
   - Applies Welsh-Powell to each connected component
   - Separate coloring per component
   - Useful for visualizing communities

---

### **WeightCalculator.cs** ? (SEN)
Weight calculation with project formula:

**Formula:** `w = 1 / (1 + (Aktiflik_diff)² + (Etkilesim_diff)² + (Neighbors_diff)²)`

1. **Calculate(Node a, Node b)**
   - Computes weight between two nodes using project formula
   - Similar nodes ? higher weight
   - Different nodes ? lower weight

2. **CalculateFromFeatures(Dictionary, Dictionary)**
   - Handles CSV/JSON feature data
   - Flexible for dynamic properties
   - Returns 1.0 for missing data

3. **NormalizeTo01(weight)**
   - Normalizes to [0, 1] range
   - Useful for algorithms requiring normalized weights

4. **NormalizeTo1_10(weight)**
   - Normalizes to [1, 10] range
   - Better for UI display (progress bars, ratings)

5. **GetWeightForEdge(Node a, Node b)**
   - Safe weight calculation for edge creation
   - Ensures minimum weight of 0.01 (prevents zero weights)

6. **IsValidWeight(weight)**
   - Validates weight (positive, not NaN/Infinity)
   - Used by normalization methods

7. **NormalizeWeights(List<double>)**
   - Batch normalization of weight list
   - Returns all weights in [0, 1] range

---

## ?? Services (`SocialNetworkApp/Services/`)

### **FileManager.cs** ? (SEN)
Data import/export with UTF-8 encoding:

#### JSON Operations
1. **ExportToJson(Graph, path)**
   - Saves graph as formatted JSON
   - Format: `{ "Nodes": [...], "Edges": [...] }`
   - UTF-8 without BOM encoding
   - Includes comprehensive error handling

2. **ImportFromJson(path)**
   - Loads graph from JSON file
   - Creates Node and Edge objects
   - Validates file existence
   - Catches JSON parsing errors

#### CSV Operations
1. **ExportToCsv(Graph, directory)**
   - Creates 3 CSV files:
     - `nodes.csv`: id, name, aktiflik, etkilesim
     - `edges.csv`: sourceId, targetId, weight
     - `adjacency.csv`: nodeId, neighbors (semicolon-separated)
   - Auto-creates directory if needed
   - Prevents duplicate edges (undirected handling)

2. **ImportFromCsv(directory)**
   - Reads nodes.csv and edges.csv
   - Parses numeric values with TryParse
   - Validates file presence
   - Handles CSV format errors gracefully

#### Utility Methods
1. **IsValidFilePath(path)**
   - Checks if file path is valid
   - Validates directory existence

2. **IsWritableFilePath(path)**
   - Checks if directory is writable
   - Useful before export operations

---

## ?? UI (`SocialNetworkApp/UI/`)

### **Visualizer.cs** ? (SEN)
Interactive graph visualization engine:

#### Layout
1. **CalculateCircleLayout(List<int>)**
   - Places nodes evenly on circle
   - Center-based calculation using angles
   - Auto-adjusts to canvas size

#### Rendering
1. **Render(Graphics, Graph)**
   - Main render method (Edges ? Nodes ? Labels)
   - Enables AntiAlias for smooth graphics
   - Handles resize dynamically

2. **DrawEdge(Graphics, Edge, PointF, PointF)**
   - Draws edge as line
   - Highlights path edges in red
   - Displays weight as label at midpoint

3. **DrawNode(Graphics, Node, PointF)**
   - Draws node as filled circle
   - Colored based on component/analysis
   - Red border if selected (3px thickness)

4. **DrawAllLabels(Graphics, Graph)**
   - Draws node IDs centered on circles
   - Uses specified font and color

#### Interaction
1. **GetNodeAtPosition(PointF)**
   - Hit detection using Euclidean distance
   - Returns node ID or -1 if none

2. **SelectNode(int) / DeselectNode(int)**
   - Toggle selection state
   - Updates visual representation

3. **ToggleSelection(int)**
   - Single-click selection handling
   - Supports multi-select with stored selected IDs

#### Highlighting
1. **HighlightPath(List<int>)**
   - Marks path edges for visualization
   - Renders them in red with thicker lines

2. **ClearHighlight()**
   - Removes path highlighting

#### Coloring
1. **SetNodeColors(Dictionary<int, Color>)**
   - Applies colors from algorithm results
   - Useful for Welsh-Powell or community visualization

#### Properties
- `NodeRadius`: 15f (pixel size)
- `EdgeWidth`: 2f (normal edges)
- `HighlightedEdgeWidth`: 4f (path edges)
- Color scheme: LightBlue nodes, DarkBlue borders, Gray edges

---

### **MainForm.cs** ? (SEN)
Main UI with menus, controls, and algorithm integration:

#### Layout
- **SplitContainer**: Left panel (controls) + Right panel (canvas)
- **MenuStrip**: File, Algorithms, Analysis, Help menus
- **ToolStrip**: Quick action buttons
- **StatusStrip**: Status indicator

#### Left Panel Controls
1. **Node Management**
   - TextBox: Node name input
   - NumericUpDown: Node ID, Activity, Interaction
   - Buttons: Add Node, Remove Node
   - ListBox: All nodes display

2. **Edge Management**
   - NumericUpDown: Source ID, Target ID, Weight
   - Buttons: Add Edge, Remove Edge
   - ListBox: All edges display

3. **Selection Display**
   - Label: Shows selected source/target nodes
   - Format: "Source: Alice (#1) \nTarget: Bob (#2)"

#### Menu Items
**File Menu:**
- Import JSON / Export JSON
- Import CSV / Export CSV
- Exit

**Algorithms Menu:**
- BFS/DFS Traversal (requires 1 node selected)
- Dijkstra Shortest Path (requires 2 nodes)
- A* Shortest Path (requires 2 nodes)
- Connected Components (no selection needed)
- Welsh-Powell Coloring (no selection needed)

**Analysis Menu:**
- Degree Centrality (Top 5 influential users)
- Clear Selection

**Help Menu:**
- About

#### Canvas Interaction
- **Single Click**: Select source node
- **Ctrl+Click**: Select target node (multi-select)
- **Double-Click (ListBox)**: Quick select node
- **Algorithm Result**: Highlights path in red

#### Sample Data
- 5 nodes: Alice, Bob, Charlie, Diana, Eve
- 5 edges: Various connections
- Demonstrates all features on startup

---

## ?? Tests (`SocialNetworkApp.Tests/`)

### **TraversalTests.cs**
Unit tests for graph traversal:

1. **BFSTraversal_StartExists_ReturnsNodes()**
   - Creates test graph
   - Verifies BFS returns all reachable nodes
   - Validates traversal order

2. **BFSTraversal_StartMissing_ReturnsEmpty()**
   - Tests error handling
   - Non-existent start node ? empty result

3. **DFSTraversal_StartExists_ReturnsNodes()**
   - Creates test graph
   - Verifies DFS returns all reachable nodes
   - Validates depth-first order

---

## ?? Git Workflow

### Branch Structure
```
main (Production)
??? Initial commit: Complete working application
?
??? develop (Development)
    ??? feature/edge-model-sen ?
    ?   ??? Edge.cs, Node.cs, Graph.cs
    ?
    ??? feature/pathfinding ?
    ?   ??? PathFindingAlgorithms.cs
    ?
    ??? feature/community-detect ?
    ?   ??? CommunityAlgorithms.cs
    ?
    ??? feature/file-operations ?
    ?   ??? FileManager.cs
    ?
    ??? feature/ui-visualization ?
        ??? Visualizer.cs, MainForm.cs
```

### Commit Strategy
Each feature has:
- Separate branch (`feature/*`)
- Individual commit with clear message
- Merged into `develop` with `--no-ff`
- All related files in one commit

---

## ?? Implementation Details

### Code Quality
- ? **Inline Comments**: Every major logic block explained
- ? **XML Documentation**: All public methods documented
- ? **Error Handling**: Try-catch, null checks, validation
- ? **UTF-8 Encoding**: Turkish character support

### Performance Considerations
- `Graph.GetNeighbors()`: O(1) with adjacency list
- `Dijkstra()`: O((V+E)logV) with priority queue
- `A*()`: O((V+E)logV) with heuristic optimization
- `BFS/DFS()`: O(V+E) with HashSet

### Data Structures
- **Dictionary<K,V>**: Fast node/edge lookups
- **List<T>**: Path storage, results
- **HashSet<T>**: Visited tracking, duplicate prevention
- **Queue<T>**: BFS level-order traversal
- **Stack<T>**: DFS depth-first traversal

---

## ?? Usage Example

```csharp
// Create graph
var graph = new Graph();
graph.AddNode(new Node(1, "Alice", 0.8, 12));
graph.AddNode(new Node(2, "Bob", 0.6, 10));
graph.AddEdge(new Edge(1, 2, 0.5));

// Find shortest path
var (path, cost) = PathFindingAlgorithms.Dijkstra(graph, 1, 2);
// path: [1, 2], cost: 0.5

// Find communities
var components = CommunityAlgorithms.ConnectedComponents(graph);
// components: [[1, 2]]

// Export data
FileManager.ExportToJson(graph, "graph.json");
FileManager.ExportToCsv(graph, "./output");

// Visualize
var visualizer = new Visualizer(800, 600);
visualizer.Render(graphics, graph);
```

---

## ??? Technology Stack

- **Language**: C# 14.0
- **Framework**: .NET 10
- **UI**: Windows Forms
- **Data Format**: JSON, CSV
- **Encoding**: UTF-8
- **Build**: Project file-based

---

## ?? File Structure

```
SocialNetworkApp/
??? Models/
?   ??? Edge.cs          (Kenar s?n?f?)
?   ??? Node.cs          (Dü?üm s?n?f?)
?   ??? Graph.cs         (Graf veri yap?s?)
??? Algorithms/
?   ??? TraversalAlgorithms.cs      (BFS/DFS - A Ki?isi)
?   ??? PathFindingAlgorithms.cs    (Dijkstra/A* - SEN)
?   ??? CommunityAlgorithms.cs      (Bile?enler - SEN)
?   ??? AnalysisAlgorithms.cs       (Merkezilik - A Ki?isi)
?   ??? WeightCalculator.cs         (A??rl?k - SEN)
??? Services/
?   ??? FileManager.cs              (Import/Export - SEN)
??? UI/
?   ??? Visualizer.cs               (Çizim motoru - SEN)
?   ??? MainForm.cs                 (Ana form - SEN)
??? Program.cs
??? SocialNetworkApp.csproj

Tests/
??? TraversalTests.cs               (Testler)
```

---

## ? Completion Checklist

- ? Core models (Edge, Node, Graph) with documentation
- ? Path finding (Dijkstra, A*) with heuristics
- ? Community detection (Connected Components)
- ? Weight calculation (Project formula)
- ? File operations (JSON/CSV with UTF-8)
- ? Interactive visualizer (Circle layout, selection, highlighting)
- ? Full UI (Menus, controls, algorithm integration)
- ? Unit tests (Traversal validation)
- ? Inline comments (Every major block)
- ? Git workflow (Separate feature branches)
- ? Build success (Error-free .NET 10)

---

## ?? Contributing

### Current Contributors
- **SEN**: Edge model, Pathfinding, Community detection, File operations, UI/Visualization
- **A Ki?isi**: Node model details, Traversal algorithms, Analysis metrics

### Next Steps
1. Clone repository
2. Checkout `develop` branch
3. Create feature branch from `develop`
4. Commit changes with clear messages
5. Create Pull Request to `develop`
6. After approval, merge to `main` for release

---

## ?? License

MIT License - See repository for details

---

**Last Updated**: 2025  
**Repository**: https://github.com/Umutkzuu/SocialNetworkApp.git
