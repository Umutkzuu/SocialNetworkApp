using System;
using System.Collections.Generic;
using SocialNetworkApp.Models;

namespace SocialNetworkApp.Algorithms
{
    /// <summary>
    /// Dü?ümler ve kenarlar aras?ndaki a??rl?k (weight) de?erlerini hesaplar.
    /// Projede istenen formül: w = 1 / (1 + (aktiflik_fark)² + (etkile?im_fark)² + (ba?lant?_fark)²)
    /// </summary>
    public static class WeightCalculator
    {
        /// <summary>
        /// Proje isterlerdeki formüle göre iki dü?üm aras?ndaki a??rl??? hesaplar.
        /// Formül: w = 1 / (1 + (Aktiflik_i - Aktiflik_j)² + (Etkilesim_i - Etkilesim_j)² + (Neighbors_i - Neighbors_j)²)
        /// 
        /// Benzer özelliklere sahip dü?ümler aras?ndaki a??rl?k daha yüksek olur.
        /// Farkl? özelliklere sahip dü?ümler aras?ndaki a??rl?k daha dü?ük olur.
        /// </summary>
        /// <param name="a">Birinci dü?üm</param>
        /// <param name="b">?kinci dü?üm</param>
        /// <returns>Hesaplanan a??rl?k (pozitif double de?er)</returns>
        /// <exception cref="ArgumentNullException">E?er a veya b null ise</exception>
        public static double Calculate(Node a, Node b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));

            // Özellikler aras?ndaki farklar
            var daAktiflik = a.Aktiflik - b.Aktiflik;
            var daEtkilesim = a.Etkilesim - b.Etkilesim;
            var daNeighbors = a.Neighbors.Count - b.Neighbors.Count;

            // Formül: w = 1 / (1 + da² + db² + dc²)
            var denominator = 1.0 + (daAktiflik * daAktiflik) + (daEtkilesim * daEtkilesim) + (daNeighbors * daNeighbors);
            var weight = 1.0 / denominator;

            return weight;
        }

        /// <summary>
        /// Dinamik özelliklere (features) göre a??rl?k hesaplar.
        /// CSV/JSON'dan gelen verilerde kullan?lacak.
        /// Formül: w = 1 / (1 + ?(feature_i_fark)²)
        /// </summary>
        /// <param name="nodeAFeatures">Birinci dü?ümün özellikleri (örn: {"Aktiflik": 0.8, "Etkile?im": 12})</param>
        /// <param name="nodeBFeatures">?kinci dü?ümün özellikleri</param>
        /// <returns>Hesaplanan a??rl?k (pozitif double de?er)</returns>
        public static double CalculateFromFeatures(Dictionary<string, double> nodeAFeatures, Dictionary<string, double> nodeBFeatures)
        {
            if (nodeAFeatures == null || nodeBFeatures == null)
                return 1.0; // Default weight

            if (nodeAFeatures.Count == 0 || nodeBFeatures.Count == 0)
                return 1.0; // Default weight

            double sumSquaredDiffs = 0.0;

            // Ortak feature'lar? kar??la?t?r
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
        /// A??rl??? 0-1 aras?nda normalize eder.
        /// 0 = çok dü?ük a??rl?k, 1 = çok yüksek a??rl?k
        /// </summary>
        /// <param name="weight">Normalle?tirilecek a??rl?k de?eri</param>
        /// <param name="minValue">Minimum beklenen a??rl?k (default: 0)</param>
        /// <param name="maxValue">Maksimum beklenen a??rl?k (default: 1)</param>
        /// <returns>0-1 aras?nda normalize edilmi? a??rl?k</returns>
        public static double NormalizeTo01(double weight, double minValue = 0.0, double maxValue = 1.0)
        {
            if (maxValue <= minValue)
                return 0.5; // Geçersiz aral?k, orta de?er döndür

            // Clamp: weight'i minValue-maxValue aras?nda s?n?rla
            var clampedWeight = Math.Max(minValue, Math.Min(maxValue, weight));

            // Normalize: (value - min) / (max - min)
            var normalized = (clampedWeight - minValue) / (maxValue - minValue);

            return normalized;
        }

        /// <summary>
        /// A??rl??? 1-10 aras?nda normalize eder.
        /// 1 = çok dü?ük a??rl?k, 10 = çok yüksek a??rl?k
        /// UI'de göstermek için kullan?labilir.
        /// </summary>
        /// <param name="weight">Normalle?tirilecek a??rl?k de?eri</param>
        /// <param name="minValue">Minimum beklenen a??rl?k (default: 0)</param>
        /// <param name="maxValue">Maksimum beklenen a??rl?k (default: 1)</param>
        /// <returns>1-10 aras?nda normalize edilmi? a??rl?k</returns>
        public static double NormalizeTo1_10(double weight, double minValue = 0.0, double maxValue = 1.0)
        {
            var normalized01 = NormalizeTo01(weight, minValue, maxValue);
            return 1.0 + (normalized01 * 9.0); // 1-10 ölçe?ine çevir
        }

        /// <summary>
        /// ?ki dü?üm aras?ndaki kenar a??rl???n? hesaplar ve Edge olu?tururken kullan?lacak de?eri döner.
        /// A??rl?k 0'dan büyük olmal?d?r (negatif a??rl?k geçersiz).
        /// </summary>
        /// <param name="a">Birinci dü?üm</param>
        /// <param name="b">?kinci dü?üm</param>
        /// <returns>Edge'de kullan?lacak a??rl?k (pozitif, minimum 0.01)</returns>
        public static double GetWeightForEdge(Node a, Node b)
        {
            if (a == null || b == null)
                return 1.0; // Default a??rl?k

            var weight = Calculate(a, b);

            // Edge a??rl??? en az 0.01 olmal? (0 de?erine izin verme)
            return Math.Max(0.01, weight);
        }

        /// <summary>
        /// A??rl?k de?erinin geçerli olup olmad???n? kontrol eder.
        /// Geçerli a??rl?k: pozitif ve sonlu (NaN/Infinity de?il)
        /// </summary>
        /// <param name="weight">Kontrol edilecek a??rl?k</param>
        /// <returns>Geçerli ise true, aksi halde false</returns>
        public static bool IsValidWeight(double weight)
        {
            return weight > 0 && !double.IsNaN(weight) && !double.IsInfinity(weight);
        }

        /// <summary>
        /// Verilen a??rl?klar? normalize ederek bir a??rl?k profili olu?turur.
        /// Tüm a??rl?klar? 0-1 aras?nda normalize eder.
        /// </summary>
        /// <param name="weights">Normalize edilecek a??rl?k listesi</param>
        /// <returns>Normalize edilmi? a??rl?klar listesi</returns>
        public static List<double> NormalizeWeights(List<double> weights)
        {
            if (weights == null || weights.Count == 0)
                return new List<double>();

            var minWeight = double.MaxValue;
            var maxWeight = double.MinValue;

            // Min ve max de?erleri bul
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
                // Tüm a??rl?klar ayn?
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
