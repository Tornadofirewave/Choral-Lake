using UnityEngine;
using UnityEngine.InputSystem;
using ChoralLake.Core;
using ChoralLake.Gameplay;

namespace ChoralLake.Player
{
    /// <summary>
    /// Raycasts in the player's facing direction each FixedUpdate to find a nearby IInteractable.
    /// Fires Interact() on the focused target when the player presses the Interact button.
    /// Attach to PlayerRoot; assign Movement via inspector (it lives on a child GameObject).
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField, Min(0.1f)] private float interactRange = 1.2f;
        [SerializeField] private LayerMask interactableLayers = ~0;
        [SerializeField] private PlayerMovement movement;

        public IInteractable CurrentFocus { get; private set; }

        private PlayerControls _controls;

        private static readonly Vector2[] DirectionVectors =
        {
            new(0f, -1f), // 0 Down
            new(0f,  1f), // 1 Up
            new(-1f, 0f), // 2 Left
            new( 1f, 0f), // 3 Right
        };

        private void Awake()
        {
            _controls = new PlayerControls();
        }

        private void OnEnable()
        {
            _controls.Player.Enable();
            _controls.Player.Interact.performed += OnInteractPressed;
        }

        private void OnDisable()
        {
            _controls.Player.Interact.performed -= OnInteractPressed;
            _controls.Player.Disable();
        }

        private void FixedUpdate()
        {
            CurrentFocus = FindFocus();
        }

        private IInteractable FindFocus()
        {
            if (movement == null) return null;
            Vector2 origin = movement.transform.position;
            Vector2 dir = DirectionVectors[Mathf.Clamp(movement.FacingDirection, 0, 3)];

            var hits = Physics2D.RaycastAll(origin, dir, interactRange, interactableLayers);
            IInteractable nearest = null;
            float nearestDist = float.MaxValue;
            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                var interactable = hit.collider.GetComponentInParent<IInteractable>();
                if (interactable == null || !interactable.CanInteract) continue;
                if (hit.distance < nearestDist)
                {
                    nearest = interactable;
                    nearestDist = hit.distance;
                }
            }
            return nearest;
        }

        private void OnInteractPressed(InputAction.CallbackContext ctx)
        {
            if (CurrentFocus == null || !CurrentFocus.CanInteract) return;
            CurrentFocus.Interact();
        }

        private void OnDrawGizmosSelected()
        {
            if (movement == null) return;
            Vector2 origin = movement.transform.position;
            Vector2 dir = DirectionVectors[Mathf.Clamp(movement.FacingDirection, 0, 3)];
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(origin, origin + dir * interactRange);
        }
    }
}
