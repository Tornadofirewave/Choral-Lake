using UnityEngine;
using UnityEngine.InputSystem;

namespace ChoralLake.UI
{
    public class FishingTutorialUI : MonoBehaviour
    {
        public static FishingTutorialUI Instance { get; private set; }

        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField, Min(0f)] private float fadeInDuration = 0.25f;
        [SerializeField] private RectTransform panelRect;

        private float _fadeElapsed;
        private bool _isOpen;
        private bool _closeArmed;
        private Camera _uiCamera;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            SetVisible(false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Show()
        {
            if (_isOpen) return;
            _isOpen = true;
            
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
            gameObject.SetActive(true);
            
            _fadeElapsed = 0f;
            _closeArmed = false;

            var canvas = GetComponentInParent<Canvas>();
            _uiCamera = canvas != null ? canvas.worldCamera : null;

            enabled = true;
        }

        private void Hide()
        {
            _isOpen = false;
            _closeArmed = false;
            SetVisible(false);
            enabled = false;
        }

        private void SetVisible(bool visible)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            if (!visible) gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!_isOpen) return;

            // Fade in
            if (canvasGroup.alpha < 1f)
            {
                _fadeElapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(_fadeElapsed / fadeInDuration);
                
                if (!_closeArmed)
                {
                    // Wait until mouse button is released to arm close detection
                    if (Mouse.current == null || !Mouse.current.leftButton.isPressed)
                    {
                        _closeArmed = true;
                    }
                }
                return;
            }

            // Fade in complete, ready for close detection
            if (!_closeArmed)
            {
                if (Mouse.current == null || !Mouse.current.leftButton.isPressed)
                {
                    _closeArmed = true;
                }
                return;
            }

            // Check for click to close
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Hide();
            }
        }
    }
}
