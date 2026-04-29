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
    [SerializeField, Min(0f)] private float closeInputDelay = 0.08f;

    private Action onReturn;
    private float shownTime;
    private bool canClose;

    private void Awake()
    {
        SetRaycastTargets(false);
    }

    private void OnEnable()
    {
        shownTime = Time.unscaledTime;
        canClose = false;
        SetRaycastTargets(false);
    }

    private void Update()
    {
        if (onReturn == null)
        {
            return;
        }

        if (!canClose)
        {
            if (Time.unscaledTime - shownTime >= closeInputDelay)
            {
                canClose = true;
            }

            return;
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
