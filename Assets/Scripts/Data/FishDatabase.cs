using System.Collections.Generic;
using UnityEngine;
using ChoralLake.Core;

namespace ChoralLake.Data
{
    [CreateAssetMenu(fileName = "FishDatabase", menuName = "Choral Lake/Fish Database")]
    public class FishDatabase : ScriptableObject
    {
        [SerializeField] private List<FishEntry> entries = new();

        private Dictionary<string, FishEntry> _byId;

        public IReadOnlyList<FishEntry> AllEntries => entries;
        public int Count => entries.Count;

        public FishEntry GetById(string id)
        {
            if (TryGetById(id, out var entry)) return entry;
            Debug.LogError($"[FishDatabase] No fish found with ID '{id?.Trim()}'.");
            return null;
        }

        public bool TryGetById(string id, out FishEntry entry)
        {
            entry = null;
            var normalizedId = NormalizeId(id);
            if (string.IsNullOrEmpty(normalizedId)) return false;

            EnsureCache();
            return _byId.TryGetValue(normalizedId, out entry);
        }

        public List<FishEntry> GetByRarity(Rarity rarity)
        {
            var result = new List<FishEntry>();
            foreach (var entry in entries)
            {
                if (entry != null && entry.Rarity == rarity) result.Add(entry);
            }
            return result;
        }

        private void OnEnable() => RebuildCache();

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            RebuildCache();
        }

        private void EnsureCache()
        {
            if (_byId == null) RebuildCache();
        }

        private static string NormalizeId(string id)
        {
            return string.IsNullOrWhiteSpace(id) ? string.Empty : id.Trim();
        }

        private void RebuildCache()
        {
            _byId = new Dictionary<string, FishEntry>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var entry in entries)
            {
                if (entry == null) continue;

                var normalizedId = NormalizeId(entry.Id);
                if (string.IsNullOrEmpty(normalizedId))
                {
                    Debug.LogError($"[FishDatabase] '{name}' contains a fish entry with an empty ID.", this);
                    continue;
                }
                if (_byId.ContainsKey(normalizedId))
                {
                    Debug.LogError($"[FishDatabase] '{name}' has duplicate fish ID '{normalizedId}'. Previous entry overwritten.", this);
                }
                _byId[normalizedId] = entry;
            }
        }
    }
}
