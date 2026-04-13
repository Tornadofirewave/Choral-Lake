using UnityEngine;
using ChoralLake.Core;
using ChoralLake.Data;

namespace ChoralLake.DebugTools
{
    public class SmokeTest : MonoBehaviour
    {
        [SerializeField] private string fishIdA;
        [SerializeField] private string fishIdB;
        [SerializeField] private string fishIdC;
        [SerializeField] private string commonRodId = "rod_common";

        private void Start()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[SmokeTest] GameManager.Instance is null.");
                return;
            }
            var gm = GameManager.Instance;
            var db = gm.Database;
            if (db == null)
            {
                Debug.LogError("[SmokeTest] GameDatabase is not assigned on the GameManager.");
                return;
            }
            if (db.FishDatabase == null)
            {
                Debug.LogError("[SmokeTest] FishDatabase is not assigned on the GameDatabase.");
                return;
            }

            var rod = db.GetRodById(commonRodId);
            if (rod == null)
            {
                Debug.LogError($"[SmokeTest] Rod '{commonRodId}' not found in database.");
                return;
            }
            gm.GrantRod(commonRodId);
            gm.EquipRod(commonRodId);
            Debug.Log($"[SmokeTest] Equipped rod: {gm.SaveData.equippedRodId} (expected {commonRodId})");

            int expectedTotal = 0;
            foreach (var id in new[] { fishIdA, fishIdB, fishIdC })
            {
                if (string.IsNullOrEmpty(id)) continue;
                var fish = db.GetFishById(id);
                if (fish == null)
                {
                    Debug.LogError($"[SmokeTest] Fish '{id}' not found in FishDatabase.");
                    return;
                }
                expectedTotal += fish.SellCost;
                gm.AddFishToInventory(id);
            }

            Debug.Log($"[SmokeTest] Unique fish count before sell: {gm.UniqueFishCount} (expected 3 if all three IDs were unique)");

            int sold = gm.SellAllFish();
            Debug.Log($"[SmokeTest] Sold for: {sold} (expected {expectedTotal} — real sell costs from FishDatabase)");
            Debug.Log($"[SmokeTest] Unique fish count after sell: {gm.UniqueFishCount} (expected 3 — must NOT be cleared)");

            enabled = false;
        }


    }
}
