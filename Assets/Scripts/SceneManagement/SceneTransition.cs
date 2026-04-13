using UnityEngine;
using ChoralLake.Player;

namespace ChoralLake.SceneManagement
{
    /// <summary>
    /// Walk-into trigger that loads a target scene. Requires a Collider2D with isTrigger=true.
    /// Uses the "Player" tag to detect the player. Subclass and override CanTransition to gate entry.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SceneTransition : MonoBehaviour
    {
        [Header("Target")]
        [Tooltip("Scene to load. For LakeTransition, this should match the assigned LakeSO's SceneName.")]
        [SerializeField] private string targetSceneName;
        [SerializeField] private string targetSpawnId = "default";

        protected virtual void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            if (!CanTransition(other)) return;
            PerformTransition();
        }

        /// <summary>
        /// Override to gate the transition. Return false to block.
        /// Base implementation always returns true.
        /// </summary>
        protected virtual bool CanTransition(Collider2D player) => true;

        protected void PerformTransition()
        {
            if (string.IsNullOrEmpty(targetSceneName))
            {
                Debug.LogError($"[SceneTransition] '{name}' has no target scene name.", this);
                return;
            }
            SceneLoader.LoadScene(targetSceneName, targetSpawnId);
        }
    }
}
