using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChoralLake.Core;
using ChoralLake.Data;
using ChoralLake.Audio;
using ChoralLake.Player;

namespace ChoralLake.UI
{
    public class TicketBoothUI : MonoBehaviour
    {
        public static TicketBoothUI Instance { get; private set; }

        [Serializable]
        private class TicketSlot
        {
            public TicketSO ticket;
            public Button button;
            public Image icon;
            public TMP_Text nameText;
            public TMP_Text priceText;
        }

        [Header("Panel")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text statusLabel;

        [Header("Ticket Slots (Common → Legendary)")]
        [SerializeField] private List<TicketSlot> slots = new();

        [Header("Close")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button clickAwayOverlay;

        public bool IsOpen { get; private set; }

        private PlayerControls _controls;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _controls = new PlayerControls();
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        private void OnEnable()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (clickAwayOverlay != null) clickAwayOverlay.onClick.AddListener(Close);
            foreach (var slot in slots)
            {
                if (slot.button == null) continue;
                var captured = slot;
                captured.button.onClick.AddListener(() => OnTicketClicked(captured.ticket));
            }
        }

        private void OnDisable()
        {
            if (closeButton != null) closeButton.onClick.RemoveListener(Close);
            if (clickAwayOverlay != null) clickAwayOverlay.onClick.RemoveListener(Close);
            foreach (var slot in slots)
                slot.button?.onClick.RemoveAllListeners();
        }

        public void Open()
        {
            if (IsOpen) return;
            IsOpen = true;
            ModalStack.Push();

            _controls.Player.Disable();
            _controls.UI.Enable();
            PlayerRoot.Instance?.Movement?.LockMovement();

            PopulateSlots();
            SubscribeEvents();
            RefreshButtons();

            if (panelRoot != null) panelRoot.SetActive(true);
        }

        private void Close()
        {
            if (!IsOpen) return;
            IsOpen = false;
            ModalStack.Pop();

            UnsubscribeEvents();
            if (panelRoot != null) panelRoot.SetActive(false);

            _controls.UI.Disable();
            _controls.Player.Enable();
            PlayerRoot.Instance?.Movement?.UnlockMovement();
        }

        private void PopulateSlots()
        {
            foreach (var slot in slots)
            {
                if (slot.ticket == null) continue;
                if (slot.icon != null) slot.icon.sprite = slot.ticket.Icon;
                if (slot.nameText != null) slot.nameText.text = slot.ticket.DisplayName;
                if (slot.priceText != null) slot.priceText.text = $"${slot.ticket.Price}";
            }
        }

        private void RefreshButtons()
        {
            var gm = GameManager.Instance;
            bool shipActive = gm != null && gm.HasPendingTicket;
            int money = gm != null ? gm.SaveData.money : 0;

            if (statusLabel != null)
                statusLabel.text = shipActive ? "A ship is already docked — dismiss it first" : string.Empty;

            foreach (var slot in slots)
            {
                if (slot.button == null) continue;
                bool canAfford = slot.ticket != null && money >= slot.ticket.Price;
                slot.button.interactable = !shipActive && canAfford;
            }
        }

        private void OnTicketClicked(TicketSO ticket)
        {
            if (ticket == null) return;
            var gm = GameManager.Instance;
            if (gm == null) return;

            if (gm.TryPurchaseTicket(ticket))
            {
                AudioManager.Instance?.PlaySfx("sfx_ticket_purchase");
                Close();
            }
            else
            {
                AudioManager.Instance?.PlaySfx("sfx_ticket_denied");
            }
        }

        private void SubscribeEvents()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnMoneyChanged += RefreshButtons;
            gm.OnPendingTicketChanged += RefreshButtons;
        }

        private void UnsubscribeEvents()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnMoneyChanged -= RefreshButtons;
            gm.OnPendingTicketChanged -= RefreshButtons;
        }
    }
}
