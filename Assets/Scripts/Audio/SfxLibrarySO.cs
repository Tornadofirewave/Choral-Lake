using System.Collections.Generic;
using UnityEngine;

namespace ChoralLake.Audio
{
    [CreateAssetMenu(fileName = "SfxLibrary", menuName = "Choral Lake/SFX Library")]
    public class SfxLibrarySO : ScriptableObject
    {
        [SerializeField] private List<SfxEntry> entries = new();

        private Dictionary<string, SfxEntry> _cache;

        private void OnEnable() => RebuildCache();

        private void RebuildCache()
        {
            _cache = new Dictionary<string, SfxEntry>();
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrEmpty(e.Id)) continue;
                if (_cache.ContainsKey(e.Id))
                {
                    Debug.LogWarning($"[SfxLibrarySO] Duplicate SFX id '{e.Id}'.", this);
                    continue;
                }
                _cache[e.Id] = e;
            }
        }

        public bool TryGet(string id, out SfxEntry entry)
        {
            if (_cache == null) RebuildCache();
            return _cache.TryGetValue(id, out entry);
        }
    }
}
