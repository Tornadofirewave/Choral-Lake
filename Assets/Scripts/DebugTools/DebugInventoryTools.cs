using UnityEngine;
using ChoralLake.Core;

namespace ChoralLake.DebugTools
{
    /// <summary>
    /// Debug utility for granting fish, money, and bait without gameplay.
    /// Right-click the component in the Inspector to fire any action in Play mode.
    /// Attach to the GameManager GameObject in Boot.
    /// </summary>
    public class DebugInventoryTools : MonoBehaviour
    {
        [Header("Fish IDs to grant (comma-separated)")]
        [SerializeField] private string fishIdsToGrant = "fish_lake1_minnow,fish_lake1_minnow,fish_lake1_trout,fish_lake1_bass";

        [Header("Money")]
        [SerializeField] private int moneyAmount = 100;

        [ContextMenu("Grant Fish From Field")]
        public void GrantFishFromField()
        {
            if (GameManager.Instance == null)
            {
                Debug.LogError("[DebugInventoryTools] GameManager.Instance is null.");
                return;
            }
            if (string.IsNullOrWhiteSpace(fishIdsToGrant)) return;
            int granted = 0;
            foreach (var raw in fishIdsToGrant.Split(','))
            {
                var id = raw.Trim();
                if (string.IsNullOrEmpty(id)) continue;
                GameManager.Instance.AddFishToInventory(id);
                granted++;
            }
            Debug.Log($"[DebugInventoryTools] Granted {granted} fish.");
        }

        [ContextMenu("Grant Money")]
        public void GrantMoney()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.AddMoney(moneyAmount);
            Debug.Log($"[DebugInventoryTools] Added {moneyAmount}. Total: {GameManager.Instance.SaveData.money}.");
        }

        [ContextMenu("Clear Unsold Fish")]
        public void ClearUnsoldFish()
        {
            if (GameManager.Instance == null) return;
            GameManager.Instance.SaveData.unsoldFishIds.Clear();
            Debug.Log("[DebugInventoryTools] Cleared unsold fish list.");
        }

        [ContextMenu("Log Current Inventory")]
        public void LogInventory()
        {
            if (GameManager.Instance == null) return;
            var gm = GameManager.Instance;
            Debug.Log($"[DebugInventoryTools] Money: {gm.SaveData.money}");
            Debug.Log($"[DebugInventoryTools] Unsold fish ({gm.SaveData.unsoldFishIds.Count}): {string.Join(", ", gm.SaveData.unsoldFishIds)}");
            Debug.Log($"[DebugInventoryTools] Unique fish caught: {gm.UniqueFishCount}");
        }
    }
}
