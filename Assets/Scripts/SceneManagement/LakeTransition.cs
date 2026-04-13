using UnityEngine;
using ChoralLake.Core;
using ChoralLake.Data;

namespace ChoralLake.SceneManagement
{
    /// <summary>
    /// Scene transition gated behind a LakeSO's unique-fish-caught requirement.
    /// On first successful entry, marks the lake as unlocked so it stays open for future visits.
    /// Prompt 5 will replace the blocked-entry log with a dialogue barrier.
    /// </summary>
    public class LakeTransition : SceneTransition
    {
        [Header("Lake")]
        [SerializeField] private LakeSO lake;

        [Header("Blocked Behavior")]
        [Tooltip("Shown in console when entry is blocked. Prompt 5 replaces this with dialogue.")]
        [SerializeField] private string lockedMessage = "The path is blocked. You need more unique fish to proceed.";

        private float _nextBlockedLogTime;

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            if (lake != null)
            {
                var so = new UnityEditor.SerializedObject(this);
                so.FindProperty("targetSceneName").stringValue = lake.SceneName;
                so.ApplyModifiedProperties();
            }
        }
#endif

        protected override bool CanTransition(Collider2D player)
        {
            if (lake == null)
            {
                Debug.LogError($"[LakeTransition] '{name}' has no LakeSO assigned.", this);
                return false;
            }

            var gm = GameManager.Instance;
            if (gm == null) return false;

            bool alreadyUnlocked = gm.IsLakeUnlocked(lake.Id);
            bool meetsRequirement = gm.UniqueFishCount >= lake.UniqueFishRequiredToUnlock;

            if (!alreadyUnlocked && !meetsRequirement)
            {
                if (Time.time >= _nextBlockedLogTime)
                {
                    Debug.Log($"[LakeTransition] Blocked: {lockedMessage} ({gm.UniqueFishCount}/{lake.UniqueFishRequiredToUnlock} unique fish)");
                    _nextBlockedLogTime = Time.time + 1f;
                }
                return false;
            }

            if (!alreadyUnlocked)
                gm.UnlockLake(lake.Id);

            return true;
        }
    }
}
