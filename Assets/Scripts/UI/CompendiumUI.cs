using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ChoralLake.Core;

namespace ChoralLake.UI
{
    public class CompendiumUI : MonoBehaviour
    {
        public static CompendiumUI Instance { get; private set; }

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panelRect;
        [SerializeField] private RectTransform entryContainer;
        [SerializeField] private CompendiumSlot slotPrefab;
        [SerializeField, Min(0f)] private float fadeInDuration = 0.25f;

        private readonly List<CompendiumSlot> _slots = new();
        private float _fadeElapsed;
        private bool _isOpen;
        private bool _fadeDone;
        private Camera _uiCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
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

            var canvas = GetComponent<Canvas>();
            _uiCamera = canvas != null ? canvas.worldCamera : null;

            RefreshAll();
            SubscribeEvents();
        }

        public void Hide()
        {
            if (!_isOpen) return;
            _isOpen = false;
            UnsubscribeEvents();
            ModalStack.Pop();
            SetVisible(false);
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
            ClearSlots();

            var gm = GameManager.Instance;
            if (gm == null || gm.Database == null || slotPrefab == null || entryContainer == null) return;

            var allFish = gm.Database.FishDatabase.AllEntries;
            var caught  = gm.SaveData.uniqueFishCaughtIds;

            foreach (var fish in allFish)
            {
                if (fish == null) continue;
                var slot = Instantiate(slotPrefab, entryContainer);
                slot.SetData(fish, caught.Contains(fish.Id));
                _slots.Add(slot);
            }
        }

        private void ClearSlots()
        {
            foreach (var slot in _slots)
                if (slot != null) Destroy(slot.gameObject);
            _slots.Clear();
        }

        private void SetVisible(bool visible)
        {
            canvasGroup.alpha          = visible ? 1f : 0f;
            canvasGroup.interactable   = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        private void SubscribeEvents()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnUniqueFishCountChanged += RefreshAll;
        }

        private void UnsubscribeEvents()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            gm.OnUniqueFishCountChanged -= RefreshAll;
        }
    }
}
