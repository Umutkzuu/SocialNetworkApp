using System;

namespace SocialNetworkApp.Models
{
    /// <summary>
    /// Grafýn kenarýný (edge) temsil eder.
    /// Aðýrlýklý, yönsüz graf kenarlarýný tanýmlar.
    /// </summary>
    public class Edge
    {
        // Kenarýn baþlangýç düðümü ID'si
        public int SourceId { get; }
        
        // Kenarýn hedef düðümü ID'si
        public int TargetId { get; }
        
        // Kenarýn aðýrlýðý (maliyet, uzaklýk vb.)
        public double Weight { get; set; }

        /// <summary>
        /// Edge (kenar) oluþturur.
        /// </summary>
        /// <param name="sourceId">Baþlangýç node ID (> 0)</param>
        /// <param name="targetId">Hedef node ID (> 0)</param>
        /// <param name="weight">Aðýrlýk deðeri (default: 1.0)</param>
        public Edge(int sourceId, int targetId, double weight = 1.0)
        {
            // ID'lerin pozitif olup olmadýðýný kontrol et
            if (sourceId <= 0) throw new ArgumentOutOfRangeException(nameof(sourceId));
            if (targetId <= 0) throw new ArgumentOutOfRangeException(nameof(targetId));
            
            // Self-loop (kendine baðlanan kenar) engelle
            if (sourceId == targetId) throw new ArgumentException("Self-loops are not allowed.", nameof(targetId));
            
            // Aðýrlýk pozitif olmalý
            if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight));

            SourceId = sourceId;
            TargetId = targetId;
            Weight = weight;
        }

        /// <summary>
        /// Kenarýn self-loop olup olmadýðýný kontrol eder.
        /// </summary>
        public bool IsLoop => SourceId == TargetId;

        /// <summary>
        /// Kenarýn string temsilini döner (debug için).
        /// Örn: "Edge: 1 <-> 2 (w=0.5)"
        /// </summary>
        public override string ToString() => $"Edge: {SourceId} <-> {TargetId} (w={Weight})";
    }
}
