using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using ChoralLake.Audio;
using ChoralLake.Core;
using ChoralLake.Data;
using ChoralLake.Player;

namespace ChoralLake.UI
{
    /// <summary>
    /// Modal shop receipt. Opened by Shopkeeper after the sale is committed.
    /// Displays a read-only summary grouped by species. Dismissed with UI.Advance (Space/E).
    /// Entries appear instantly in this pass; a future animator drives staged reveals.
    /// </summary>
    public class ReceiptUI : MonoBehaviour
    {
        public static ReceiptUI Instance { get; private set; }

        [Header("Panel Root")]
        [SerializeField] private GameObject panelRoot;

        [Header("Entry List")]
        [Tooltip("Parent transform for entry views (typically a VerticalLayoutGroup).")]
        [SerializeField] private RectTransform entryContainer;
        [SerializeField] private ReceiptEntryView entryPrefab;

        [Header("Totals")]
        [SerializeField] private TMP_Text totalText;
        [Tooltip("Shown when the player has no fish to sell.")]
        [SerializeField] private GameObject emptyStateRoot;
        [Tooltip("Shown when at least one fish is on the receipt.")]
        [SerializeField] private GameObject totalsRoot;

        [Header("Tally Animation")]
        [SerializeField] private float tallyDuration = 1.5f;
        [SerializeField] private string tallySfxId = "sfx_receipt_tick";
        [SerializeField] private float tickInterval = 0.05f;

        [Header("Dismiss Hint")]
        [SerializeField] private TMP_Text dismissHintText;
        [SerializeField] private string dismissHint = "Press E or Space to close";

        public bool IsOpen { get; private set; }

        /// <summary>Visible entry list. A future animator reads this to drive reveal timing.</summary>
        public IReadOnlyList<ReceiptEntryView> VisibleEntries => _visibleEntries;

        private readonly List<ReceiptEntryView> _visibleEntries = new();
        private PlayerControls _controls;
        private Coroutine _tallyCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _controls = new PlayerControls();
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void OnEnable()  => _controls.UI.Advance.performed += OnDismiss;
        private void OnDisable()
        {
            _controls.UI.Advance.performed -= OnDismiss;
            _controls.UI.Disable();
        }

        /// <summary>
        /// Show the receipt after a completed sale. Caller must snapshot unsoldFishIds BEFORE
        /// calling SellAllFish, because SellAllFish clears the list.
        /// </summary>
        public void ShowReceipt(List<string> soldFishIds, int totalEarned, FishDatabase fishDb)
        {
            if (IsOpen)
            {
                Debug.LogWarning("[ReceiptUI] ShowReceipt called while already open. Ignoring.");
                return;
            }

            IsOpen = true;
            ModalStack.Push();
            _controls.Player.Disable();
            _controls.UI.Enable();
            if (PlayerRoot.Instance != null && PlayerRoot.Instance.Movement != null)
                PlayerRoot.Instance.Movement.LockMovement();

            if (panelRoot != null) panelRoot.SetActive(true);
            ClearEntries();

            bool isEmpty = soldFishIds == null || soldFishIds.Count == 0;
            if (emptyStateRoot != null) emptyStateRoot.SetActive(isEmpty);
            if (totalsRoot != null) totalsRoot.SetActive(!isEmpty);
            if (dismissHintText != null) dismissHintText.text = dismissHint;

            if (!isEmpty)
            {
                PopulateEntries(soldFishIds, fishDb);
                StartTally(totalEarned);
            }
        }

        /// <summary>Show the "nothing to sell" state explicitly.</summary>
        public void ShowEmpty() => ShowReceipt(null, 0, null);

        private void PopulateEntries(List<string> soldFishIds, FishDatabase fishDb)
        {
            if (entryPrefab == null || entryContainer == null)
            {
                Debug.LogError("[ReceiptUI] entryPrefab or entryContainer not assigned.");
                return;
            }

            // Group by fish ID, preserving first-seen order.
            var counts = new Dictionary<string, int>();
            var order = new List<string>();
            foreach (var id in soldFishIds)
            {
                if (string.IsNullOrEmpty(id)) continue;
                if (!counts.ContainsKey(id))
                {
                    counts[id] = 0;
                    order.Add(id);
                }
                counts[id]++;
            }

            foreach (var id in order)
            {
                var fish = fishDb != null ? fishDb.GetById(id) : null;
                if (fish == null)
                {
                    Debug.LogWarning($"[ReceiptUI] Could not resolve fish ID '{id}' for receipt display.");
                    continue;
                }
                var entry = Instantiate(entryPrefab, entryContainer);
                float multiplier = fishDb != null ? fishDb.Multiplier : 1f;
                entry.SetData(fish, counts[id], multiplier);
                _visibleEntries.Add(entry);
            }
        }

        private void StartTally(int grandTotal)
        {
            if (_tallyCoroutine != null) StopCoroutine(_tallyCoroutine);
            foreach (var entry in _visibleEntries) entry.SetDisplayedPrice(0);
            if (totalText != null) totalText.text = "Total: $0";
            if (grandTotal > 0)
                _tallyCoroutine = StartCoroutine(TallyCoroutine(grandTotal));
        }

        private IEnumerator TallyCoroutine(int grandTotal)
        {
            float rate = grandTotal / tallyDuration;
            float current = 0f;
            float tickTimer = 0f;

            while (current < grandTotal)
            {
                current = Mathf.Min(current + rate * Time.deltaTime, grandTotal);
                int displayed = Mathf.RoundToInt(current);

                foreach (var entry in _visibleEntries)
                    entry.SetDisplayedPrice(Mathf.Min(displayed, entry.LineTotal));

                if (totalText != null)
                    totalText.text = $"Total: ${displayed}";

                tickTimer -= Time.deltaTime;
                if (tickTimer <= 0f)
                {
                    AudioManager.Instance?.PlaySfx(tallySfxId);
                    tickTimer = tickInterval;
                }

                yield return null;
            }

            foreach (var entry in _visibleEntries)
                entry.SetDisplayedPrice(entry.LineTotal);
            if (totalText != null) totalText.text = $"Total: ${grandTotal}";
            _tallyCoroutine = null;
        }

        private void ClearEntries()
        {
            if (_tallyCoroutine != null) { StopCoroutine(_tallyCoroutine); _tallyCoroutine = null; }
            foreach (var entry in _visibleEntries)
                if (entry != null) Destroy(entry.gameObject);
            _visibleEntries.Clear();
        }

        private void OnDismiss(InputAction.CallbackContext ctx)
        {
            if (!IsOpen) return;
            Close();
        }

        private void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            ClearEntries();
            IsOpen = false;
            ModalStack.Pop();

            _controls.UI.Disable();
            _controls.Player.Enable();
            if (PlayerRoot.Instance != null && PlayerRoot.Instance.Movement != null)
                PlayerRoot.Instance.Movement.UnlockMovement();
        }
    }
}
