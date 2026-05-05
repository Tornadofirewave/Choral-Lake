using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ChoralLake.Core;

namespace ChoralLake.UI
{
    public class TackleBoxUI : MonoBehaviour
    {
        public static TackleBoxUI Instance { get; private set; }

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private List<TackleBoxRodSlot> rodSlots;
        [SerializeField] private List<TackleBoxBaitSlot> baitSlots;
        [SerializeField, Min(0f)] private float fadeInDuration = 0.25f;

        private float _fadeElapsed;
        private bool _isOpen;
        private bool _fadeDone;
        private Camera _uiCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            SetVisible(false);
        }

        public void Show()
        {
            if (_isOpen) return;
            _isOpen = true;
            ModalStack.Push();
            canvasGroup.alpha          = 0f;
            canvasGroup.interactable   = false;
            canvasGroup.blocksRaycasts = true;
            _fadeElapsed = 0f;
            _fadeDone    = false;

            var canvas = GetComponentInParent<Canvas>();
            _uiCamera = canvas != null ? canvas.worldCamera : null;

            SubscribeEvents();
            RefreshAll();
        }

        public void Hide()
        {
            if (!_isOpen) return;
            _isOpen = false;
            UnsubscribeEvents();
            ModalStack.Pop();
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            canvasGroup.alpha          = visible ? 1f : 0f;
            canvasGroup.interactable   = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        private void Update()
        {
            if (!_isOpen) return;

            if (!_fadeDone)
            {
                _fadeElapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(_fadeElapsed / fadeInDuration);
                if (canvasGroup.alpha >= 1f)
                {
                    _fadeDone = true;
                    canvasGroup.interactable = true;
                }
                return;
            }

            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                var pos = Mouse.current.position.ReadValue();
                if (!RectTransformUtility.RectangleContainsScreenPoint(panelRect, pos, _uiCamera))
                    Hide();
            }
        }

        private void RefreshAll()
        {
            foreach (var slot in rodSlots)  slot.Refresh();
            foreach (var slot in baitSlots) slot.Refresh();
        }

        private void SubscribeEvents()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnEquippedRodChanged  += RefreshAll;
            gm.OnEquippedBaitChanged += RefreshAll;
            gm.OnInventoryChanged    += RefreshAll;
        }

        private void UnsubscribeEvents()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnEquippedRodChanged  -= RefreshAll;
            gm.OnEquippedBaitChanged -= RefreshAll;
            gm.OnInventoryChanged    -= RefreshAll;
        }
    }
}
