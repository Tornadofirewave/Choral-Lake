using System.Collections.Generic;
using UnityEngine;

namespace ChoralLake.Audio
{
    [CreateAssetMenu(fileName = "MusicLibrary", menuName = "Choral Lake/Music Library")]
    public class MusicLibrarySO : ScriptableObject
    {
        [SerializeField] private List<MusicEntry> entries = new();

        private Dictionary<string, MusicEntry> _cache;

        private void OnEnable() => RebuildCache();

        private void RebuildCache()
        {
            _cache = new Dictionary<string, MusicEntry>();
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrEmpty(e.Id)) continue;
                if (_cache.ContainsKey(e.Id))
                {
                    Debug.LogWarning($"[MusicLibrarySO] Duplicate music id '{e.Id}'.", this);
                    continue;
                }
                _cache[e.Id] = e;
            }
        }

        public bool TryGet(string id, out MusicEntry entry)
        {
            if (_cache == null) RebuildCache();
            return _cache.TryGetValue(id, out entry);
        }
    }
}
