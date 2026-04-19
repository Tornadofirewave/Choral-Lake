using ChoralLake.Audio;
using UnityEngine;

namespace ChoralLake.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerMovement movement;
        [SerializeField] private Animator animator;

        [Header("Footsteps")]
        [SerializeField] private string footstepSfxId = "sfx_footstep";
        [SerializeField] private float stepInterval = 0.35f;
        [SerializeField] private Vector2 pitchRange = new Vector2(0.9f, 1.1f);

        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");

        private static readonly Vector2[] DirectionVectors =
        {
            new Vector2( 0f, -1f),
            new Vector2( 0f,  1f),
            new Vector2(-1f,  0f),
            new Vector2( 1f,  0f),
        };

        private float _stepTimer;

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (movement == null) movement  = GetComponentInParent<PlayerMovement>();
        }

        private void Update()
        {
            float speed = movement.CurrentVelocity.magnitude;
            Vector2 dir = DirectionVectors[movement.FacingDirection];

            animator.SetFloat(SpeedHash, speed);
            animator.SetFloat(MoveXHash, dir.x);
            animator.SetFloat(MoveYHash, dir.y);

            TickFootsteps(speed);
        }

        private void TickFootsteps(float speed)
        {
            if (speed < 0.05f) { _stepTimer = 0f; return; }

            _stepTimer -= Time.deltaTime;
            if (_stepTimer > 0f) return;

            _stepTimer = stepInterval;
            float pitch = Random.Range(pitchRange.x, pitchRange.y);
            AudioManager.Instance?.PlaySfxPitched(footstepSfxId, pitch);
        }
    }
}
