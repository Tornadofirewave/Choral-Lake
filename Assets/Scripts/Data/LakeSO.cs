using System.Collections.Generic;
using UnityEngine;
using ChoralLake.Core;

namespace ChoralLake.Data
{
    /// <summary>
    /// Defines a fishable lake location. Referenced by LakeTransition to gate scene entry
    /// and by GameDatabase for ID-based lookups.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLake", menuName = "Choral Lake/Lake")]
    public class LakeSO : ScriptableObject, IIdentifiable
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;

        [Header("Scene")]
        [Tooltip("Name of the scene to load when entering this lake. Must match a scene in Build Settings.")]
        [SerializeField] private string sceneName;

        [Header("Progression")]
        [Tooltip("How many unique fish species the player must have caught globally before this lake unlocks. 0 = available from start.")]
        [SerializeField, Min(0)] private int uniqueFishRequiredToUnlock;

        [Header("Content")]
        [Tooltip("Fish IDs in this lake's pool. Reference fish by ID; the FishDatabase resolves them at runtime.")]
        [SerializeField] private List<string> fishIdPool = new();

        public string Id => id;
        public string DisplayName => displayName;
        public string SceneName => sceneName;
        public int UniqueFishRequiredToUnlock => uniqueFishRequiredToUnlock;
        public IReadOnlyList<string> FishIdPool => fishIdPool;

        public List<FishEntry> GetFishOfRarity(Rarity rarity, FishDatabase fishDb)
        {
            var result = new List<FishEntry>();
            if (fishDb == null) return result;
            foreach (var id in fishIdPool)
            {
                var fish = fishDb.GetById(id);
                if (fish != null && fish.Rarity == rarity) result.Add(fish);
            }
            return result;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                Debug.LogWarning($"[LakeSO] '{name}' has no ID set.", this);
        }
    }
}
