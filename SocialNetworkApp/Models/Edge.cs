using System;

namespace SocialNetworkApp.Models
{
    /// <summary>
    /// Graf?n kenar?n? (edge) temsil eder.
    /// A??rl?kl?, yönsüz graf kenarlar?n? tan?mlar.
    /// </summary>
    public class Edge
    {
        // Kenar?n ba?lang?ç dü?ümü ID'si
        public int SourceId { get; }
        
        // Kenar?n hedef dü?ümü ID'si
        public int TargetId { get; }
        
        // Kenar?n a??rl??? (maliyet, uzakl?k vb.)
        public double Weight { get; set; }

        /// <summary>
        /// Edge (kenar) olu?turur.
        /// </summary>
        /// <param name="sourceId">Ba?lang?ç node ID (> 0)</param>
        /// <param name="targetId">Hedef node ID (> 0)</param>
        /// <param name="weight">A??rl?k de?eri (default: 1.0)</param>
        public Edge(int sourceId, int targetId, double weight = 1.0)
        {
            // ID'lerin pozitif olup olmad???n? kontrol et
            if (sourceId <= 0) throw new ArgumentOutOfRangeException(nameof(sourceId));
            if (targetId <= 0) throw new ArgumentOutOfRangeException(nameof(targetId));
            
            // Self-loop (kendine ba?lanan kenar) engelle
            if (sourceId == targetId) throw new ArgumentException("Self-loops are not allowed.", nameof(targetId));
            
            // A??rl?k pozitif olmal?
            if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight));

            SourceId = sourceId;
            TargetId = targetId;
            Weight = weight;
        }

        /// <summary>
        /// Kenar?n self-loop olup olmad???n? kontrol eder.
        /// </summary>
        public bool IsLoop => SourceId == TargetId;

        /// <summary>
        /// Kenar?n string temsilini döner (debug için).
        /// Örn: "Edge: 1 <-> 2 (w=0.5)"
        /// </summary>
        public override string ToString() => $"Edge: {SourceId} <-> {TargetId} (w={Weight})";
    }
}
