using UnityEngine;
using ChoralLake.Data;
using ChoralLake.Core;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace ChoralLake.SceneManagement
{
    /// <summary>
    /// Transition trigger placed on a dock in a lake scene.
    /// When the player enters, this loads the fishing minigame scene.
    /// </summary>
    public class SceneFishing : SceneTransition
    {
        public static LakeSO ActiveLake { get; private set; }
        public static string ReturnSceneName { get; private set; }
        public static string ReturnSpawnId { get; private set; } = "default";

        private bool _playerInRange;
        private Collider2D _playerCollider;
        private PlayerControls _controls;

        [Header("Lake Context")]
        [Tooltip("Lake this dock belongs to. Used for validation and editor convenience.")]
        [SerializeField] private LakeSO sourceLake;

        [Header("Return Target")]
        [Tooltip("Spawn ID in the lake scene to use when leaving the fishing minigame.")]
        [SerializeField] private string returnSpawnId = "default";

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

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = true;
            _playerCollider = other;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (_playerCollider != other) return;
            _playerInRange = false;
            _playerCollider = null;
        }

        private void Awake()
        {
            _controls = new PlayerControls();
        }

        private void OnEnable()
        {
            _controls.Player.Interact.performed += OnInteractPressed;
            _controls.Player.Enable();
        }

        private void OnDisable()
        {
            _controls.Player.Interact.performed -= OnInteractPressed;
            _controls.Player.Disable();
        }

        private void OnInteractPressed(InputAction.CallbackContext ctx)
        {
            if (!_playerInRange || _playerCollider == null) return;
            if (!CanTransition(_playerCollider)) return;

            bool started = SceneLoader.LoadScene(
                fishingSceneName,
                fishingSpawnId,
                forceIfBusy: false,
                skipPlayerPlacement: true);

            if (started)
            {
                _playerInRange = false;
                _playerCollider = null;
            }
        }

        protected override bool CanTransition(Collider2D player)
        {
            var gm = GameManager.Instance;
            if (gm == null)
            {
                Debug.LogWarning("[SceneFishing] GameManager.Instance is null. Blocking fishing entry.");
                return false;
            }

            string equippedRodId = gm.SaveData.equippedRodId;
            if (string.IsNullOrEmpty(equippedRodId) || !gm.OwnsRod(equippedRodId))
            {
                Debug.Log("[SceneFishing] You need to equip a rod before fishing.");
                return false;
            } else {
                Debug.Log($"[SceneFishing] '{gm.SaveData.equippedRodId}'");
            }

            if (sourceLake == null)
            {
                Debug.LogWarning($"[SceneFishing] '{name}' has no sourceLake assigned.", this);
            }

            ActiveLake = sourceLake;
            ReturnSceneName = SceneManager.GetActiveScene().name;
            ReturnSpawnId = string.IsNullOrEmpty(returnSpawnId) ? "default" : returnSpawnId;

            return true;
        }
    }
}
