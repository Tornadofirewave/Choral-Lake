using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class FishingCompleteUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image boxImage; // the 32x32 box background
    [SerializeField] private Image fishImage; // the fish sprite to display inside the box
    [SerializeField, Min(0f)] private float fadeInDuration = 0.5f;

    private Action onReturn;
    private CanvasGroup canvasGroup;
    private float fadeElapsedTime;
    private bool canClose;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        SetRaycastTargets(false);
    }

    private void OnEnable()
    {
        fadeElapsedTime = 0f;
        canClose = false;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        SetRaycastTargets(false);
    }

    private void Update()
    {
        if (onReturn == null)
        {
            return;
        }

        if (canvasGroup != null && fadeInDuration > 0f && canvasGroup.alpha < 1f)
        {
            fadeElapsedTime += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(fadeElapsedTime / fadeInDuration);
            canClose = canvasGroup.alpha >= 1f;
            return;
        }

        canClose = true;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            OnReturnClicked();
            return;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            OnReturnClicked();
        }
    }

    public void Show(string fishName, Sprite fishSprite, Action onReturnCallback)
    {
        onReturn = onReturnCallback;
        fadeElapsedTime = 0f;
        canClose = false;

        if (titleText != null)
        {
            titleText.text = $"You've caught a {fishName}!";
        }

        if (fishImage != null)
        {
            fishImage.sprite = fishSprite;
            fishImage.enabled = fishSprite != null;
        }

        // boxImage remains as-is; ensure it's visible
        if (boxImage != null)
        {
            boxImage.enabled = true;
        }
    }

    private void OnReturnClicked()
    {
        onReturn?.Invoke();
    }

    private void SetRaycastTargets(bool enabled)
    {
        if (titleText != null)
        {
            titleText.raycastTarget = enabled;
        }

        if (boxImage != null)
        {
            boxImage.raycastTarget = enabled;
        }

        if (fishImage != null)
        {
            fishImage.raycastTarget = enabled;
        }
    }
}
