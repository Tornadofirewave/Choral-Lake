using System.Collections.Generic;
using UnityEngine;
using ChoralLake.Core;

namespace ChoralLake.Data
{
    [CreateAssetMenu(fileName = "BaitDatabase", menuName = "Choral Lake/Bait Database")]
    public class BaitDatabase : ScriptableObject
    {
        [SerializeField] private List<BaitEntry> entries = new();

        private Dictionary<string, BaitEntry> _byId;

        public IReadOnlyList<BaitEntry> AllEntries => entries;
        public int Count => entries.Count;

        public BaitEntry GetById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            EnsureCache();
            if (_byId.TryGetValue(id, out var entry)) return entry;
            Debug.LogError($"[BaitDatabase] No bait found with ID '{id}'.");
            return null;
        }

        public BaitEntry GetByRarity(Rarity rarity)
        {
            foreach (var entry in entries)
            {
                if (entry != null && entry.GuaranteedRarity == rarity) return entry;
            }
            return null;
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

        private void RebuildCache()
        {
            _byId = new Dictionary<string, BaitEntry>();
            foreach (var entry in entries)
            {
                if (entry == null) continue;
                if (string.IsNullOrEmpty(entry.Id))
                {
                    Debug.LogError($"[BaitDatabase] '{name}' contains a bait entry with an empty ID.", this);
                    continue;
                }
                if (_byId.ContainsKey(entry.Id))
                {
                    Debug.LogError($"[BaitDatabase] '{name}' has duplicate bait ID '{entry.Id}'. Previous entry overwritten.", this);
                }
                _byId[entry.Id] = entry;
            }
        }
    }
}
