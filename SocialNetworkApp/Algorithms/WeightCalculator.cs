using System;
using System.Collections.Generic;
using SocialNetworkApp.Models;

namespace SocialNetworkApp.Algorithms
{
    /// <summary>
    /// Düðümler ve kenarlar arasýndaki aðýrlýk (weight) deðerlerini hesaplar.
    /// Projede istenen formül: w = 1 / (1 + (aktiflik_fark)² + (etkileþim_fark)² + (baðlantý_fark)²)
    /// </summary>
    public static class WeightCalculator
    {
        /// <summary>
        /// Proje isterlerdeki formüle göre iki düðüm arasýndaki aðýrlýðý hesaplar.
        /// Formül: w = 1 / (1 + (Aktiflik_i - Aktiflik_j)² + (Etkilesim_i - Etkilesim_j)² + (Neighbors_i - Neighbors_j)²)
        /// 
        /// Benzer özelliklere sahip düðümler arasýndaki aðýrlýk daha yüksek olur.
        /// Farklý özelliklere sahip düðümler arasýndaki aðýrlýk daha düþük olur.
        /// </summary>
        /// <param name="a">Birinci düðüm</param>
        /// <param name="b">Ýkinci düðüm</param>
        /// <returns>Hesaplanan aðýrlýk (pozitif double deðer)</returns>
        /// <exception cref="ArgumentNullException">Eðer a veya b null ise</exception>
        public static double Calculate(Node a, Node b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));

            // Özellikler arasýndaki farklar
            var daAktiflik = a.Aktiflik - b.Aktiflik;
            var daEtkilesim = a.Etkilesim - b.Etkilesim;
            var daNeighbors = a.Neighbors.Count - b.Neighbors.Count;

            // Formül: w = 1 / (1 + da² + db² + dc²)
            var denominator = 1.0 + (daAktiflik * daAktiflik) + (daEtkilesim * daEtkilesim) + (daNeighbors * daNeighbors);
            var weight = 1.0 / denominator;

            return weight;
        }

        /// <summary>
        /// Dinamik özelliklere (features) göre aðýrlýk hesaplar.
        /// CSV/JSON'dan gelen verilerde kullanýlacak.
        /// Formül: w = 1 / (1 + ?(feature_i_fark)²)
        /// </summary>
        /// <param name="nodeAFeatures">Birinci düðümün özellikleri (örn: {"Aktiflik": 0.8, "Etkileþim": 12})</param>
        /// <param name="nodeBFeatures">Ýkinci düðümün özellikleri</param>
        /// <returns>Hesaplanan aðýrlýk (pozitif double deðer)</returns>
        public static double CalculateFromFeatures(Dictionary<string, double> nodeAFeatures, Dictionary<string, double> nodeBFeatures)
        {
            if (nodeAFeatures == null || nodeBFeatures == null)
                return 1.0; // Default weight

            if (nodeAFeatures.Count == 0 || nodeBFeatures.Count == 0)
                return 1.0; // Default weight

            double sumSquaredDiffs = 0.0;

            // Ortak feature'larý karþýlaþtýr
            foreach (var key in nodeAFeatures.Keys)
            {
                if (nodeBFeatures.TryGetValue(key, out var bValue))
                {
                    var diff = nodeAFeatures[key] - bValue;
                    sumSquaredDiffs += diff * diff;
                }
            }

            var denominator = 1.0 + sumSquaredDiffs;
            var weight = 1.0 / denominator;

            return weight;
        }

        /// <summary>
        /// Aðýrlýðý 0-1 arasýnda normalize eder.
        /// 0 = çok düþük aðýrlýk, 1 = çok yüksek aðýrlýk
        /// </summary>
        /// <param name="weight">Normalleþtirilecek aðýrlýk deðeri</param>
        /// <param name="minValue">Minimum beklenen aðýrlýk (default: 0)</param>
        /// <param name="maxValue">Maksimum beklenen aðýrlýk (default: 1)</param>
        /// <returns>0-1 arasýnda normalize edilmiþ aðýrlýk</returns>
        public static double NormalizeTo01(double weight, double minValue = 0.0, double maxValue = 1.0)
        {
            if (maxValue <= minValue)
                return 0.5; // Geçersiz aralýk, orta deðer döndür

            // Clamp: weight'i minValue-maxValue arasýnda sýnýrla
            var clampedWeight = Math.Max(minValue, Math.Min(maxValue, weight));

            // Normalize: (value - min) / (max - min)
            var normalized = (clampedWeight - minValue) / (maxValue - minValue);

            return normalized;
        }

        /// <summary>
        /// Aðýrlýðý 1-10 arasýnda normalize eder.
        /// 1 = çok düþük aðýrlýk, 10 = çok yüksek aðýrlýk
        /// UI'de göstermek için kullanýlabilir.
        /// </summary>
        /// <param name="weight">Normalleþtirilecek aðýrlýk deðeri</param>
        /// <param name="minValue">Minimum beklenen aðýrlýk (default: 0)</param>
        /// <param name="maxValue">Maksimum beklenen aðýrlýk (default: 1)</param>
        /// <returns>1-10 arasýnda normalize edilmiþ aðýrlýk</returns>
        public static double NormalizeTo1_10(double weight, double minValue = 0.0, double maxValue = 1.0)
        {
            var normalized01 = NormalizeTo01(weight, minValue, maxValue);
            return 1.0 + (normalized01 * 9.0); // 1-10 ölçeðine çevir
        }

        /// <summary>
        /// Ýki düðüm arasýndaki kenar aðýrlýðýný hesaplar ve Edge oluþtururken kullanýlacak deðeri döner.
        /// Aðýrlýk 0'dan büyük olmalýdýr (negatif aðýrlýk geçersiz).
        /// </summary>
        /// <param name="a">Birinci düðüm</param>
        /// <param name="b">Ýkinci düðüm</param>
        /// <returns>Edge'de kullanýlacak aðýrlýk (pozitif, minimum 0.01)</returns>
        public static double GetWeightForEdge(Node a, Node b)
        {
            if (a == null || b == null)
                return 1.0; // Default aðýrlýk

            var weight = Calculate(a, b);

            // Edge aðýrlýðý en az 0.01 olmalý (0 deðerine izin verme)
            return Math.Max(0.01, weight);
        }

        /// <summary>
        /// Aðýrlýk deðerinin geçerli olup olmadýðýný kontrol eder.
        /// Geçerli aðýrlýk: pozitif ve sonlu (NaN/Infinity deðil)
        /// </summary>
        /// <param name="weight">Kontrol edilecek aðýrlýk</param>
        /// <returns>Geçerli ise true, aksi halde false</returns>
        public static bool IsValidWeight(double weight)
        {
            return weight > 0 && !double.IsNaN(weight) && !double.IsInfinity(weight);
        }

        /// <summary>
        /// Verilen aðýrlýklarý normalize ederek bir aðýrlýk profili oluþturur.
        /// Tüm aðýrlýklarý 0-1 arasýnda normalize eder.
        /// </summary>
        /// <param name="weights">Normalize edilecek aðýrlýk listesi</param>
        /// <returns>Normalize edilmiþ aðýrlýklar listesi</returns>
        public static List<double> NormalizeWeights(List<double> weights)
        {
            if (weights == null || weights.Count == 0)
                return new List<double>();

            var minWeight = double.MaxValue;
            var maxWeight = double.MinValue;

            // Min ve max deðerleri bul
            foreach (var w in weights)
            {
                if (IsValidWeight(w))
                {
                    minWeight = Math.Min(minWeight, w);
                    maxWeight = Math.Max(maxWeight, w);
                }
            }

            // Normalize et
            var normalized = new List<double>();
            if (maxWeight <= minWeight)
            {
                // Tüm aðýrlýklar ayný
                foreach (var w in weights)
                    normalized.Add(0.5);
            }
            else
            {
                foreach (var w in weights)
                {
                    if (IsValidWeight(w))
                    {
                        var norm = (w - minWeight) / (maxWeight - minWeight);
                        normalized.Add(norm);
                    }
                    else
                    {
                        normalized.Add(0.0);
                    }
                }
            }

            return normalized;
        }
    }
}
