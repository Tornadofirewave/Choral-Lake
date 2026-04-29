using System;
using UnityEngine;
using UnityEngine.EventSystems;
using ChoralLake.Core;
using ChoralLake.Data;
using ChoralLake.Audio;
using ChoralLake.Dialogue;
using ChoralLake.Gameplay;
using ChoralLake.UI;

namespace ChoralLake.Tickets
{
    /// <summary>
    /// NPC that walks off the ship, waits for the player to talk, grants the ticket reward,
    /// then walks back. Configured at runtime from NpcAttendantSO.
    /// If no dialogue conversation is configured or found, skips straight to granting the reward.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class ShipAttendant : MonoBehaviour, IInteractable, IPointerClickHandler
    {
        [Header("Movement")]
        [SerializeField, Min(0.1f)] private float moveSpeed = 1.8f;
        [SerializeField, Min(0.01f)] private float arrivalEpsilon = 0.15f;

        [Header("References")]
        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator animator;

        private NpcAttendantSO _so;
        private Transform _exitPoint;
        private Vector2 _returnPos;
        private Action _onDone;
        private bool _rewardGranted;

        private enum Phase { WalkingOut, Idle, WalkingBack }
        private Phase _phase = Phase.WalkingOut;

        public string InteractPrompt => _so != null ? $"Talk to {_so.DisplayName}" : "Talk";
        public bool CanInteract => _phase == Phase.Idle && !_rewardGranted && (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen);
        public Transform Transform => transform;

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
        }

        public void Initialize(Transform exitPoint, Vector2 returnPos, Action onDone)
        {
            _exitPoint = exitPoint;
            _returnPos = returnPos;
            _onDone = onDone;

            var gm = GameManager.Instance;
            if (gm != null)
            {
                _so = gm.Database?.GetAttendantById(gm.SaveData.pendingAttendantId);
                if (_so != null)
                {
                    if (spriteRenderer != null && _so.WalkSprite != null)
                        spriteRenderer.sprite = _so.WalkSprite;
                    if (animator != null && _so.AnimatorController != null)
                        animator.runtimeAnimatorController = _so.AnimatorController;
                }
            }
        }

        private void FixedUpdate()
        {
            switch (_phase)
            {
                case Phase.WalkingOut:
                    if (_exitPoint == null) break;
                    MoveToward(_exitPoint.position);
                    if (Vector2.Distance(rb.position, _exitPoint.position) < arrivalEpsilon)
                        OnArrivedAtExit();
                    break;

                case Phase.WalkingBack:
                    MoveToward(_returnPos);
                    if (Vector2.Distance(rb.position, _returnPos) < arrivalEpsilon)
                        OnArrivedAtShip();
                    break;
            }
        }

        private void MoveToward(Vector2 target)
        {
            var dir = (target - rb.position).normalized;
            rb.linearVelocity = dir * moveSpeed;
        }

        private void OnArrivedAtExit()
        {
            rb.linearVelocity = Vector2.zero;
            _phase = Phase.Idle;
        }

        public void Interact()
        {
            if (_phase != Phase.Idle || _rewardGranted) return;

            string convoId = _so?.DialogueConversationId;

            if (!string.IsNullOrEmpty(convoId) && DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnConversationEnded += OnConversationEnded;
                DialogueManager.Instance.StartConversation(convoId);

                // If dialogue failed to open (conversation not found in CSV), skip it.
                if (!DialogueManager.Instance.IsOpen)
                {
                    DialogueManager.Instance.OnConversationEnded -= OnConversationEnded;
                    GrantRewardAndReturn();
                }
            }
            else
            {
                GrantRewardAndReturn();
            }
        }

        private void OnConversationEnded(string conversationId)
        {
            if (conversationId != _so?.DialogueConversationId) return;
            DialogueManager.Instance.OnConversationEnded -= OnConversationEnded;
            GrantRewardAndReturn();
        }

        private void GrantRewardAndReturn()
        {
            if (_rewardGranted) return;
            _rewardGranted = true;

            var gm = GameManager.Instance;
            if (gm == null) { _phase = Phase.WalkingBack; return; }

            var result = gm.RollPendingTicketReward();
            gm.ClearPendingTicket();

            Sprite rewardSprite = null;
            string rewardName = result.Id ?? string.Empty;
            string chimeId = "sfx_reward_chime_common";

            if (result.Kind == LootKind.Rod)
            {
                var rod = gm.Database?.GetRodById(result.Id);
                if (rod != null) { rewardSprite = rod.InventoryIcon; rewardName = rod.DisplayName; }
                chimeId = "sfx_reward_chime_legendary";
            }
            else
            {
                var bait = gm.Database?.GetBaitById(result.Id);
                if (bait != null)
                {
                    rewardSprite = bait.Icon;
                    rewardName = bait.DisplayName;
                    chimeId = bait.GuaranteedRarity switch
                    {
                        Rarity.Common    => "sfx_reward_chime_common",
                        Rarity.Rare      => "sfx_reward_chime_rare",
                        Rarity.Epic      => "sfx_reward_chime_epic",
                        Rarity.Legendary => "sfx_reward_chime_legendary",
                        _                => "sfx_reward_chime_common"
                    };
                }
            }

            AudioManager.Instance?.PlaySfx(chimeId);
            RewardPopup.Instance?.Show(rewardSprite, rewardName);

            _phase = Phase.WalkingBack;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (ModalStack.AnyOpen || ModalStack.JustClosed) return;
            Interact();
        }

        private void OnArrivedAtShip()
        {
            rb.linearVelocity = Vector2.zero;
            _onDone?.Invoke();
            Destroy(gameObject);
        }
    }
}
