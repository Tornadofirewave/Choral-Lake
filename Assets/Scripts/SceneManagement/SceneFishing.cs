using UnityEngine;
using ChoralLake.Data;

namespace ChoralLake.SceneManagement
{
    /// <summary>
    /// Transition trigger placed on a dock in a lake scene.
    /// When the player enters, this loads the fishing minigame scene.
    /// </summary>
    public class SceneFishing : SceneTransition
    {
        [Header("Lake Context")]
        [Tooltip("Lake this dock belongs to. Used for validation and editor convenience.")]
        [SerializeField] private LakeSO sourceLake;

        [Header("Fishing Target")]
        [Tooltip("Fishing minigame scene name.")]
        [SerializeField] private string fishingSceneName = SceneIds.Fishing;
        [Tooltip("Spawn ID in the fishing scene.")]
        [SerializeField] private string fishingSpawnId = "default";

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            ApplyTargetSceneDefaults();
        }

        private void OnValidate()
        {
            ApplyTargetSceneDefaults();
        }

        private void ApplyTargetSceneDefaults()
        {
            var so = new UnityEditor.SerializedObject(this);
            so.FindProperty("targetSceneName").stringValue = fishingSceneName;
            so.FindProperty("targetSpawnId").stringValue = fishingSpawnId;
            so.ApplyModifiedProperties();
        }
#endif

        protected override bool CanTransition(Collider2D player)
        {
            if (sourceLake == null)
            {
                Debug.LogWarning($"[SceneFishing] '{name}' has no sourceLake assigned.", this);
            }

            return true;
        }
    }
}
