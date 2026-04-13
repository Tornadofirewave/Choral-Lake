using UnityEngine;

namespace ChoralLake.SceneManagement
{
    /// <summary>
    /// Marks a location where the player can spawn when entering this scene.
    /// Place these as empty GameObjects in every gameplay scene. Every scene should have at least
    /// one spawn point with spawnId = "default" as a fallback.
    /// </summary>
    public class PlayerSpawnPoint : MonoBehaviour
    {
        [SerializeField] private string spawnId = "default";
        [Tooltip("Direction the player should face after spawning here. 0=Down, 1=Up, 2=Left, 3=Right.")]
        [SerializeField] private int facingDirection = 0;

        public string SpawnId => spawnId;
        public int FacingDirection => facingDirection;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 0.5f);
        }
    }
}