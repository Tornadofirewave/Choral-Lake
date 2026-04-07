using UnityEngine;
using ChoralLake.Core;

namespace ChoralLake.DebugTools
{
    /// <summary>
    /// TEMPORARY: verifies GameManager wiring. Delete after smoke test passes.
    /// </summary>
    public class SmokeTest : MonoBehaviour
    {
        private void Start()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[SmokeTest] GameManager.Instance is null. Is the GameManager component on a GameObject in this scene?");
                return;
            }

            var gm = GameManager.Instance;

            Debug.Log($"[SmokeTest] Starting money: {gm.SaveData.money} (expected 0)");
            Debug.Log($"[SmokeTest] Unlocked lakes: {string.Join(", ", gm.SaveData.unlockedLakeIds)} (expected lake_01)");

            gm.AddMoney(50);
            Debug.Log($"[SmokeTest] After AddMoney(50): {gm.SaveData.money} (expected 50)");

            gm.AddFishToInventory("fish_test");
            gm.AddFishToInventory("fish_test"); // duplicate species
            Debug.Log($"[SmokeTest] Unique fish count: {gm.UniqueFishCount} (expected 1)");
            Debug.Log($"[SmokeTest] Unsold fish count: {gm.SaveData.unsoldFishIds.Count} (expected 2)");

            int sold = gm.SellAllFish();
            Debug.Log($"[SmokeTest] Sold for: {sold} (expected 20 — placeholder flat 10/fish)");
            Debug.Log($"[SmokeTest] Money after sell: {gm.SaveData.money} (expected 70)");
            Debug.Log($"[SmokeTest] Unique fish count after sell: {gm.UniqueFishCount} (expected 1 — must NOT be cleared)");

            enabled = false;
        }
    }
}
