using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SocialNetworkApp.Models;

namespace SocialNetworkApp.Services
{
    /// <summary>
    /// Sosyal að verilerini JSON ve CSV formatlarýnda import/export iþlemlerini yönetir.
    /// Desteklenen formatlar: JSON (nodes+edges), CSV (nodes, edges, komþuluk listesi)
    /// </summary>
    public static class FileManager
    {
        private static readonly Encoding DefaultEncoding = new UTF8Encoding(false); // UTF-8 without BOM

        #region JSON Operations

        /// <summary>
        /// Grafý JSON formatýnda dosyaya dýþa aktarýr.
        /// Format: {"nodes": [...], "edges": [...]}
        /// </summary>
        /// <param name="graph">Dýþa aktarýlacak graf</param>
        /// <param name="path">Hedef dosya yolu</param>
        /// <exception cref="ArgumentNullException">graph veya path null ise</exception>
        /// <exception cref="ArgumentException">path boþ ise</exception>
        /// <exception cref="IOException">Dosya yazma hatasý</exception>
        public static void ExportToJson(Graph graph, string path)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Dosya yolu boþ olamaz.", nameof(path));

            try
            {
                var dto = new
                {
                    Nodes = graph.GetAllNodes().Select(n => new
                    {
                        n.Id,
                        n.Name,
                        n.Aktiflik,
                        n.Etkilesim
                    }).ToList(),
                    Edges = graph.GetEdges().Select(e => new
                    {
                        e.SourceId,
                        e.TargetId,
                        e.Weight
                    }).ToList()
                };

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(dto, options);
                File.WriteAllText(path, json, DefaultEncoding);
            }
            catch (IOException ex)
            {
                throw new IOException($"JSON dosyasý yazýlýrken hata oluþtu: {path}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"JSON export sýrasýnda hata oluþtu: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// JSON dosyasýndan grafý içe aktarýr.
        /// Beklenen format: {"nodes": [...], "edges": [...]}
        /// </summary>
        /// <param name="path">Kaynak dosya yolu</param>
        /// <returns>Ýçe aktarýlan graf</returns>
        /// <exception cref="ArgumentException">path null veya boþ ise</exception>
        /// <exception cref="FileNotFoundException">Dosya bulunamadý</exception>
        /// <exception cref="JsonException">JSON format hatasý</exception>
        public static Graph ImportFromJson(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Dosya yolu boþ olamaz.", nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException($"Dosya bulunamadý: {path}");

            try
            {
                var txt = File.ReadAllText(path, DefaultEncoding);
                using var doc = JsonDocument.Parse(txt);
                var root = doc.RootElement;
                var graph = new Graph();

                // Nodes içe aktar
                if (root.TryGetProperty("Nodes", out var nodesElement))
                {
                    foreach (var n in nodesElement.EnumerateArray())
                    {
                        var id = n.GetProperty("Id").GetInt32();
                        var name = n.GetProperty("Name").GetString() ?? string.Empty;
                        var aktiflik = n.GetProperty("Aktiflik").GetDouble();
                        var etkilesim = n.GetProperty("Etkilesim").GetDouble();

                        graph.AddNode(new Node(id, name, aktiflik, etkilesim));
                    }
                }

                // Edges içe aktar
                if (root.TryGetProperty("Edges", out var edgesElement))
                {
                    foreach (var e in edgesElement.EnumerateArray())
                    {
                        var sourceId = e.GetProperty("SourceId").GetInt32();
                        var targetId = e.GetProperty("TargetId").GetInt32();
                        var weight = e.GetProperty("Weight").GetDouble();

                        graph.AddEdge(new Edge(sourceId, targetId, weight));
                    }
                }

                return graph;
            }
            catch (FileNotFoundException)
            {
                throw;
            }
            catch (JsonException ex)
            {
                throw new JsonException($"JSON format hatasý: {path} - {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"JSON import sýrasýnda hata oluþtu: {ex.Message}", ex);
            }
        }

        #endregion

        #region CSV Operations

        /// <summary>
        /// Grafý CSV formatýnda dosyaya dýþa aktarýr.
        /// Üç ayrý CSV dosyasý oluþturur:
        /// 1. nodes.csv: id,name,aktiflik,etkilesim
        /// 2. edges.csv: sourceId,targetId,weight
        /// 3. adjacency.csv: komþuluk listesi (id,neighbors)
        /// </summary>
        /// <param name="graph">Dýþa aktarýlacak graf</param>
        /// <param name="outputDirectory">Çýktý dizini</param>
        /// <exception cref="ArgumentNullException">graph null ise</exception>
        /// <exception cref="ArgumentException">outputDirectory null veya boþ ise</exception>
        /// <exception cref="IOException">Dosya yazma hatasý</exception>
        public static void ExportToCsv(Graph graph, string outputDirectory)
        {
            if (graph == null) throw new ArgumentNullException(nameof(graph));
            if (string.IsNullOrWhiteSpace(outputDirectory)) throw new ArgumentException("Dizin yolu boþ olamaz.", nameof(outputDirectory));

            try
            {
                // Dizin yoksa oluþtur
                if (!Directory.Exists(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                // Nodes CSV
                ExportNodesCsv(graph, Path.Combine(outputDirectory, "nodes.csv"));

                // Edges CSV
                ExportEdgesCsv(graph, Path.Combine(outputDirectory, "edges.csv"));

                // Adjacency CSV (komþuluk listesi)
                ExportAdjacencyCsv(graph, Path.Combine(outputDirectory, "adjacency.csv"));
            }
            catch (IOException ex)
            {
                throw new IOException($"CSV dosyalarý yazýlýrken hata oluþtu: {outputDirectory}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"CSV export sýrasýnda hata oluþtu: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// CSV dosyalarýndan grafý içe aktarýr.
        /// Beklenen dosyalar: nodes.csv ve edges.csv
        /// </summary>
        /// <param name="inputDirectory">Kaynak dosyalarýn bulunduðu dizin</param>
        /// <returns>Ýçe aktarýlan graf</returns>
        /// <exception cref="ArgumentException">inputDirectory null veya boþ ise</exception>
        /// <exception cref="DirectoryNotFoundException">Dizin bulunamadý</exception>
        /// <exception cref="FileNotFoundException">Gerekli CSV dosyasý bulunamadý</exception>
        public static Graph ImportFromCsv(string inputDirectory)
        {
            if (string.IsNullOrWhiteSpace(inputDirectory)) throw new ArgumentException("Dizin yolu boþ olamaz.", nameof(inputDirectory));
            if (!Directory.Exists(inputDirectory)) throw new DirectoryNotFoundException($"Dizin bulunamadý: {inputDirectory}");

            var nodesPath = Path.Combine(inputDirectory, "nodes.csv");
            var edgesPath = Path.Combine(inputDirectory, "edges.csv");

            if (!File.Exists(nodesPath)) throw new FileNotFoundException($"Dosya bulunamadý: {nodesPath}");
            if (!File.Exists(edgesPath)) throw new FileNotFoundException($"Dosya bulunamadý: {edgesPath}");

            try
            {
                var graph = new Graph();

                // Nodes içe aktar
                using (var reader = new StreamReader(nodesPath, DefaultEncoding))
                {
                    string line;
                    bool isHeader = true;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (isHeader)
                        {
                            isHeader = false;
                            continue;
                        }

                        var parts = line.Split(',');
                        if (parts.Length >= 4 &&
                            int.TryParse(parts[0], out var id) &&
                            double.TryParse(parts[2], out var aktiflik) &&
                            double.TryParse(parts[3], out var etkilesim))
                        {
                            var name = parts[1].Trim('"');
                            graph.AddNode(new Node(id, name, aktiflik, etkilesim));
                        }
                    }
                }

                // Edges içe aktar
                using (var reader = new StreamReader(edgesPath, DefaultEncoding))
                {
                    string line;
                    bool isHeader = true;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (isHeader)
                        {
                            isHeader = false;
                            continue;
                        }

                        var parts = line.Split(',');
                        if (parts.Length >= 3 &&
                            int.TryParse(parts[0], out var sourceId) &&
                            int.TryParse(parts[1], out var targetId) &&
                            double.TryParse(parts[2], out var weight))
                        {
                            graph.AddEdge(new Edge(sourceId, targetId, weight));
                        }
                    }
                }

                return graph;
            }
            catch (Exception ex)
            {
                throw new Exception($"CSV import sýrasýnda hata oluþtu: {ex.Message}", ex);
            }
        }

        #endregion

        #region Private Helper Methods

        private static void ExportNodesCsv(Graph graph, string path)
        {
            using (var writer = new StreamWriter(path, false, DefaultEncoding))
            {
                // Header
                writer.WriteLine("id,name,aktiflik,etkilesim");

                // Nodes
                foreach (var node in graph.GetAllNodes())
                {
                    var escapedName = $"\"{node.Name}\""; // CSV'de string deðerleri týrnak içine al
                    writer.WriteLine($"{node.Id},{escapedName},{node.Aktiflik:F6},{node.Etkilesim:F6}");
                }
            }
        }

        private static void ExportEdgesCsv(Graph graph, string path)
        {
            using (var writer = new StreamWriter(path, false, DefaultEncoding))
            {
                // Header
                writer.WriteLine("sourceId,targetId,weight");

                // Edges (tekrarlý edge'ler olmasýn diye HashSet kullan)
                var exportedEdges = new HashSet<string>();

                foreach (var edge in graph.GetEdges())
                {
                    // Yönsüz graf olduðu için, 1-2 ve 2-1 ayný edge'dir
                    var key = edge.SourceId < edge.TargetId 
                        ? $"{edge.SourceId}-{edge.TargetId}" 
                        : $"{edge.TargetId}-{edge.SourceId}";

                    if (!exportedEdges.Contains(key))
                    {
                        writer.WriteLine($"{edge.SourceId},{edge.TargetId},{edge.Weight:F6}");
                        exportedEdges.Add(key);
                    }
                }
            }
        }

        private static void ExportAdjacencyCsv(Graph graph, string path)
        {
            using (var writer = new StreamWriter(path, false, DefaultEncoding))
            {
                // Header
                writer.WriteLine("nodeId,neighbors");

                // Her düðümün komþularýný listele
                foreach (var node in graph.GetAllNodes())
                {
                    var neighbors = graph.GetNeighbors(node.Id).OrderBy(id => id);
                    var neighborsList = string.Join(";", neighbors); // Semicolon ile ayýr
                    writer.WriteLine($"{node.Id},\"{neighborsList}\"");
                }
            }
        }

        #endregion

        /// <summary>
        /// Verilen dosya yolunun geçerli olup olmadýðýný kontrol eder.
        /// </summary>
        /// <param name="path">Kontrol edilecek dosya yolu</param>
        /// <returns>Dosya geçerli ve eriþilebilir ise true, aksi halde false</returns>
        public static bool IsValidFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            try
            {
                var fileInfo = new FileInfo(path);
                return fileInfo.DirectoryName != null && Directory.Exists(fileInfo.DirectoryName);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verilen dosya yolunun yazýlabilir olup olmadýðýný kontrol eder.
        /// </summary>
        /// <param name="path">Kontrol edilecek dosya yolu</param>
        /// <returns>Dosya yazýlabilir ise true, aksi halde false</returns>
        public static bool IsWritableFilePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;

            try
            {
                var fileInfo = new FileInfo(path);
                var dirInfo = fileInfo.Directory;

                return dirInfo != null && dirInfo.Exists;
            }
            catch
            {
                return false;
            }
        }
    }
}
