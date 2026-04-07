using System.Collections.Generic;
using UnityEngine;
using ChoralLake.Core;

namespace ChoralLake.Data
{
    [CreateAssetMenu(fileName = "GameDatabase", menuName = "Choral Lake/Game Database")]
    public class GameDatabase : ScriptableObject
    {
        // TODO: replace ScriptableObject with strongly-typed content classes (FishSO, FishingRodSO, BaitSO, LakeSO) once those are defined.
        [SerializeField] private List<ScriptableObject> fish = new();
        [SerializeField] private List<ScriptableObject> rods = new();
        [SerializeField] private List<ScriptableObject> bait = new();
        [SerializeField] private List<ScriptableObject> lakes = new();

        private Dictionary<string, ScriptableObject> _fishById;
        private Dictionary<string, ScriptableObject> _rodsById;
        private Dictionary<string, ScriptableObject> _baitById;
        private Dictionary<string, ScriptableObject> _lakesById;

        public IReadOnlyList<ScriptableObject> AllFish => fish;
        public IReadOnlyList<ScriptableObject> AllRods => rods;
        public IReadOnlyList<ScriptableObject> AllBait => bait;
        public IReadOnlyList<ScriptableObject> AllLakes => lakes;

        public ScriptableObject GetFishById(string id) => Lookup(_fishById, id, "fish");
        public ScriptableObject GetRodById(string id) => Lookup(_rodsById, id, "rod");
        public ScriptableObject GetBaitById(string id) => Lookup(_baitById, id, "bait");
        public ScriptableObject GetLakeById(string id) => Lookup(_lakesById, id, "lake");

        private void OnEnable() => RebuildCaches();

        private void RebuildCaches()
        {
            _fishById  = BuildCache(fish,  "fish");
            _rodsById  = BuildCache(rods,  "rod");
            _baitById  = BuildCache(bait,  "bait");
            _lakesById = BuildCache(lakes, "lake");
        }

        private Dictionary<string, ScriptableObject> BuildCache(List<ScriptableObject> source, string category)
        {
            var dict = new Dictionary<string, ScriptableObject>();
            foreach (var entry in source)
            {
                if (entry == null) continue;
                if (entry is not IIdentifiable identifiable)
                {
                    // Fields are raw ScriptableObject until content classes are defined — nothing implements IIdentifiable yet.
                    continue;
                }
                if (string.IsNullOrEmpty(identifiable.Id))
                {
                    Debug.LogError($"[GameDatabase] {category} entry '{entry.name}' has an empty ID.", entry);
                    continue;
                }
                if (dict.ContainsKey(identifiable.Id))
                {
                    Debug.LogError($"[GameDatabase] Duplicate {category} ID '{identifiable.Id}' on '{entry.name}'. Previous entry overwritten.", entry);
                }
                dict[identifiable.Id] = entry;
            }
            return dict;
        }

        private ScriptableObject Lookup(Dictionary<string, ScriptableObject> cache, string id, string category)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (cache != null && cache.TryGetValue(id, out var result)) return result;
            Debug.LogError($"[GameDatabase] No {category} found with ID '{id}'.");
            return null;
        }
    }
}
