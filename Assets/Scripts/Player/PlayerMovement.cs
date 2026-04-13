using UnityEngine;
using ChoralLake.Core;

namespace ChoralLake.Player
{
    /// <summary>
    /// Top-down 2D movement via new Input System. Writes velocity to a Rigidbody2D (Dynamic,
    /// gravity scale 0). Facing direction persists when idle — used by interaction raycasts.
    /// 0=Down, 1=Up, 2=Left, 3=Right.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 4f;

        [Header("References")]
        [SerializeField] private Rigidbody2D body;

        public int FacingDirection { get; private set; } = 0;
        public Vector2 CurrentVelocity => body != null ? body.linearVelocity : Vector2.zero;

        private PlayerControls _controls;
        private Vector2 _moveInput;
        private bool _movementLocked;

        private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            _controls = new PlayerControls();
        }

        private void OnEnable()
        {
            _controls.Player.Enable();
            _controls.Player.Move.performed += OnMovePerformed;
            _controls.Player.Move.canceled  += OnMoveCanceled;
        }

        private void OnDisable()
        {
            _controls.Player.Move.performed -= OnMovePerformed;
            _controls.Player.Move.canceled  -= OnMoveCanceled;
            _controls.Player.Disable();
        }

        private void OnMovePerformed(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            _moveInput = ctx.ReadValue<Vector2>();
            UpdateFacingFromInput(_moveInput);
        }

        private void OnMoveCanceled(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
        {
            _moveInput = Vector2.zero;
        }

        private void FixedUpdate()
        {
            if (body == null) return;
            body.linearVelocity = _movementLocked ? Vector2.zero : _moveInput.normalized * moveSpeed;

            // Keep SaveData position in sync for mid-session saves.
            if (GameManager.Instance != null)
                GameManager.Instance.SaveData.playerPosition = transform.position;
        }

        private void UpdateFacingFromInput(Vector2 input)
        {
            if (input.sqrMagnitude < 0.01f) return;
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                FacingDirection = input.x < 0 ? 2 : 3;
            else
                FacingDirection = input.y < 0 ? 0 : 1;
        }

        /// <summary>Sets facing direction. Used by SceneLoader when placing the player at a spawn point.</summary>
        public void SetFacing(int facing) => FacingDirection = Mathf.Clamp(facing, 0, 3);

        /// <summary>
        /// Syncs the Rigidbody2D physics position to the current transform position and clears
        /// velocity and input. Call this after teleporting the parent PlayerRoot so the physics
        /// simulation matches the new world position before the next FixedUpdate.
        /// </summary>
        public void SyncAfterTeleport()
        {
            _moveInput = Vector2.zero;
            if (body == null) return;
            body.position = (Vector2)transform.position;
            body.linearVelocity = Vector2.zero;
        }

        /// <summary>Freezes movement (input still updates facing). Used by dialogue, shops, etc.</summary>
        public void LockMovement() => _movementLocked = true;

        /// <summary>Restores movement after LockMovement.</summary>
        public void UnlockMovement() => _movementLocked = false;
    }
}
