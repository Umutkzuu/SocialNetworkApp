using System;
using System.Collections.Generic;

namespace SocialNetworkApp.Models
{
    /// <summary>
    /// Sosyal aðýn düðümünü (node) temsil eder.
    /// Her düðüm bir kullanýcý/kiþiyi temsil eder.
    /// </summary>
    public class Node
    {
        // Düðümün benzersiz kimliði
        public int Id { get; }
        
        // Düðümün adý (kullanýcý adý)
        public string Name { get; set; }

        // Kullanýcýnýn aktiflik seviyesi (0.0 - 1.0 arasýnda)
        public double Aktiflik { get; set; }
        
        // Kullanýcýnýn etkileþim sayýsý
        public double Etkilesim { get; set; }

        // Komþu düðümler ve aralarýndaki kenar aðýrlýklarý
        private readonly Dictionary<Node, double> _neighbors = new();

        // Komþulara read-only eriþim
        public IReadOnlyDictionary<Node, double> Neighbors => _neighbors;

        /// <summary>
        /// Sadece adý ve ID ile node oluþturur.
        /// </summary>
        public Node(int id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name boþ olamaz.", nameof(name));

            Id = id;
            Name = name.Trim();
            Aktiflik = 0.0;
            Etkilesim = 0.0;
        }

        /// <summary>
        /// Sayýsal özelliklerle birlikte node oluþturur.
        /// </summary>
        public Node(int id, string name, double aktiflik, double etkilesim) : this(id, name)
        {
            Aktiflik = aktiflik;
            Etkilesim = etkilesim;
        }

        /// <summary>
        /// Komþu düðüm ve kenar aðýrlýðý ekler.
        /// </summary>
        public void AddNeighbor(Node other, double weight = 1.0)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));
            if (other.Id == Id) throw new InvalidOperationException("Kendine baðlantý eklenemez.");
            if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight), "Weight > 0 olmalý.");

            // Varsa üzerine yazma, yoksa ekleme
            _neighbors[other] = weight;
        }

        /// <summary>
        /// Komþu düðümü kaldýrýr.
        /// </summary>
        public bool RemoveNeighbor(Node other)
        {
            if (other == null) return false;
            return _neighbors.Remove(other);
        }

        /// <summary>
        /// Verilen düðüme baðlý olup olmadýðýný kontrol eder.
        /// </summary>
        public bool IsConnectedTo(Node other)
        {
            if (other == null) return false;
            return _neighbors.ContainsKey(other);
        }

        /// <summary>
        /// Düðümün string temsilini döner.
        /// Örn: "Alice (#1)"
        /// </summary>
        public override string ToString() => $"{Name} (#{Id})";

        /// <summary>
        /// Düðümleri ID'ye göre eþitlik kontrolü yapar.
        /// </summary>
        public override bool Equals(object? obj) => obj is Node n && n.Id == Id;
        
        /// <summary>
        /// Düðümün hash code'unu ID'ye göre döner.
        /// </summary>
        public override int GetHashCode() => Id.GetHashCode();

        /// <summary>
        /// Düðümün özelliklerini günceller (opsiyonel).
        /// </summary>
        public void Update(string? name = null, double? aktiflik = null, double? etkilesim = null)
        {
            // Name null deðilse güncelle
            if (!string.IsNullOrWhiteSpace(name))
                Name = name.Trim();

            // Aktiflik null deðilse güncelle
            if (aktiflik.HasValue)
                Aktiflik = aktiflik.Value;

            // Etkileþim null deðilse güncelle
            if (etkilesim.HasValue)
                Etkilesim = etkilesim.Value;
        }
    }
}
