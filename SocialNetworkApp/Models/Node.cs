using System;
using System.Collections.Generic;

namespace SocialNetworkApp.Models
{
    /// <summary>
    /// Sosyal a??n dü?ümünü (node) temsil eder.
    /// Her dü?üm bir kullan?c?/ki?iyi temsil eder.
    /// </summary>
    public class Node
    {
        // Dü?ümün benzersiz kimli?i
        public int Id { get; }
        
        // Dü?ümün ad? (kullan?c? ad?)
        public string Name { get; set; }

        // Kullan?c?n?n aktiflik seviyesi (0.0 - 1.0 aras?nda)
        public double Aktiflik { get; set; }
        
        // Kullan?c?n?n etkile?im say?s?
        public double Etkilesim { get; set; }

        // Kom?u dü?ümler ve aralar?ndaki kenar a??rl?klar?
        private readonly Dictionary<Node, double> _neighbors = new();

        // Kom?ulara read-only eri?im
        public IReadOnlyDictionary<Node, double> Neighbors => _neighbors;

        /// <summary>
        /// Sadece ad ve ID ile node olu?turur.
        /// </summary>
        public Node(int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name bo? olamaz.", nameof(name));

            Id = id;
            Name = name.Trim();
            Aktiflik = 0.0;
            Etkilesim = 0.0;
        }

        /// <summary>
        /// Say?sal özelliklerle birlikte node olu?turur.
        /// </summary>
        public Node(int id, string name, double aktiflik, double etkilesim) : this(id, name)
        {
            Aktiflik = aktiflik;
            Etkilesim = etkilesim;
        }

        /// <summary>
        /// Kom?u dü?üm ve kenar a??rl??? ekler.
        /// </summary>
        public void AddNeighbor(Node other, double weight = 1.0)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other.Id == Id) throw new InvalidOperationException("Kendine ba?lant? eklenemez.");
            if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight), "Weight > 0 olmal?.");

            // Varsa üzerine yazma, yoksa ekleme
            _neighbors[other] = weight;
        }

        /// <summary>
        /// Kom?u dü?ümü kald?r?r.
        /// </summary>
        public bool RemoveNeighbor(Node other)
        {
            if (other == null) return false;
            return _neighbors.Remove(other);
        }

        /// <summary>
        /// Verilen dü?üme ba?l? olup olmad???n? kontrol eder.
        /// </summary>
        public bool IsConnectedTo(Node other)
        {
            if (other == null) return false;
            return _neighbors.ContainsKey(other);
        }

        /// <summary>
        /// Dü?ümün string temsilini döner.
        /// Örn: "Alice (#1)"
        /// </summary>
        public override string ToString() => $"{Name} (#{Id})";

        /// <summary>
        /// Dü?ümleri ID'ye göre e?itlik kontrolü yapar.
        /// </summary>
        public override bool Equals(object? obj) => obj is Node n && n.Id == Id;
        
        /// <summary>
        /// Dü?ümün hash code'unu ID'ye göre döner.
        /// </summary>
        public override int GetHashCode() => Id.GetHashCode();

        /// <summary>
        /// Dü?ümün özelliklerini günceller (opsiyonel).
        /// </summary>
        public void Update(string? name = null, double? aktiflik = null, double? etkilesim = null)
        {
            // Name null de?ilse güncelle
            if (!string.IsNullOrWhiteSpace(name))
                Name = name.Trim();

            // Aktiflik null de?ilse güncelle
            if (aktiflik.HasValue)
                Aktiflik = aktiflik.Value;

            // Etkile?im null de?ilse güncelle
            if (etkilesim.HasValue)
                Etkilesim = etkilesim.Value;
        }
    }
}
