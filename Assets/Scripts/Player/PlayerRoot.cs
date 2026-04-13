using UnityEngine;

namespace ChoralLake.Player
{
    /// <summary>
    /// Persistent root of the player hierarchy. Survives scene loads. Movement logic lives on
    /// PlayerMovement (child GameObject) to isolate Rigidbody2D from non-physics concerns.
    /// </summary>
    public class PlayerRoot : MonoBehaviour
    {
        public static PlayerRoot Instance { get; private set; }

        [SerializeField] private PlayerMovement movement;
        public PlayerMovement Movement => movement;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Hard-set world position and facing direction. Called by SceneLoader after a transition.
        /// </summary>
        public void TeleportTo(Vector3 worldPosition, int facingDirection)
        {
            transform.position = worldPosition;
            if (movement != null)
            {
                movement.SyncAfterTeleport();
                movement.SetFacing(facingDirection);
            }
        }
    }
}
