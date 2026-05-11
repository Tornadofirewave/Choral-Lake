using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Unity.VisualScripting;
using TMPro;

public enum FishingCircleResult
{
    Miss = 0,
    Bad = 1,
    Good = 2,
    Perfect = 3,
}

public class FishingCircle : MonoBehaviour {
    /// <summary>
    /// Called when the circle is deleted (success, timeout, or fade-out complete).
    /// wasSuccessful = true if player clicked in grace window, false if expired/missed.
    /// </summary>
    public Action<bool, FishingCircleResult> OnCircleCompleted;
    private float timeDuration = 3f;
    private float timeRemaining;
    private float elapsedTime;
    private float perfectWindow = 0.25f;
    private float goodWindow = 0.75f;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private TMP_Text indexLabel;
    private bool debugGraceWindowLogs = true;

    private Transform ringSprite;
    private Transform circleSprite;
    private Collider2D circleCollider;
    private SpriteRenderer[] spriteRenderers;
    private Vector3 ringStartScale;
    private Vector3 ringTargetScale;
    private bool hasLoggedGraceWindowStart;
    private bool isFadingOut;
    private float fadeElapsedTime;

    private void Awake()
    {
        ringSprite = transform.GetChild(0);
        circleSprite = transform.GetChild(1);
        circleCollider = GetComponent<Collider2D>();
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
    }

    private void Start()
    {
        ringStartScale = ringSprite.localScale;
        ringTargetScale = circleSprite.localScale;
        ResetState();
    }

    private void Update()
    {
        if (isFadingOut)
        {
            UpdateFadeOut();
            return;
        }

        elapsedTime += Time.deltaTime;

        // Outer Ring Closes in
        float shrinkElapsedTime = Mathf.Min(elapsedTime, timeDuration);
        timeRemaining = Mathf.Max(0f, timeDuration - shrinkElapsedTime);
        float progress = timeDuration > 0f ? shrinkElapsedTime / timeDuration : 1f;
        ringSprite.localScale = Vector3.Lerp(ringStartScale, ringTargetScale, progress);

        if (debugGraceWindowLogs && !hasLoggedGraceWindowStart && elapsedTime >= timeDuration)
        {
            hasLoggedGraceWindowStart = true;
            Debug.Log($"Fishing ring grace window started ({perfectWindow:F2}s)");
        }

        // Detect on-time Key Presses
        var mouse = Mouse.current;
        var keyboard = Keyboard.current;
        var clickStatus = FishingCircleResult.Miss;
        bool inputCheck = HasFishingInputPressed(keyboard, mouse);

        if (inputCheck && IsMouseOverThisCircle(mouse)) {
            if (timeRemaining <= perfectWindow) {
                clickStatus = FishingCircleResult.Perfect;
                Debug.Log("Perfect!");
            } else if (timeRemaining <= goodWindow) {
                clickStatus = FishingCircleResult.Good;
                Debug.Log("Good!");
            } else {
                clickStatus = FishingCircleResult.Bad;
                Debug.Log("Bad!");
            }

            // Always say circle is complete and fade out, regardless of outcome
            OnCircleCompleted?.Invoke(true, clickStatus);
            StartFadeOut();
        }

        // Delete object once the post-close grace window has passed.
        if (elapsedTime >= timeDuration + perfectWindow)
        {
            if (debugGraceWindowLogs)
            {
                Debug.Log("Fishing ring grace window ended (auto delete)");
            }

            ringSprite.localScale = ringTargetScale;
            OnCircleCompleted?.Invoke(false, FishingCircleResult.Miss); // missed/expired
            StartFadeOut();
            return;
        }
    }

    private bool HasFishingInputPressed(Keyboard keyboard, Mouse mouse)
    {
        if (keyboard == null && mouse == null)
        {
            return false;
        }

        bool keyInput = HasFishingKeyPressed(keyboard);
        bool clickInput = mouse != null && mouse.leftButton.wasPressedThisFrame;

        return keyInput || clickInput;
    }

    private bool HasFishingKeyPressed(Keyboard keyboard)
    {
        if (keyboard == null)
        {
            return false;
        }

        return keyboard.zKey.wasPressedThisFrame || keyboard.xKey.wasPressedThisFrame;
    }

    public void SetIndex(int index)
    {
        if (indexLabel != null) indexLabel.text = index.ToString();
    }

    public void Initialize(float newTimeDuration, float newPerfectWindow, bool enableDebugLogs)
    {
        timeDuration = Mathf.Max(0f, newTimeDuration);
        perfectWindow = Mathf.Max(0f, newPerfectWindow);
        debugGraceWindowLogs = enableDebugLogs;
        ResetState();
    }

    private void ResetState()
    {
        timeDuration = Mathf.Max(0f, timeDuration);
        perfectWindow = Mathf.Max(0f, perfectWindow);
        fadeDuration = Mathf.Max(0f, fadeDuration);
        elapsedTime = 0f;
        timeRemaining = timeDuration;
        hasLoggedGraceWindowStart = false;
        isFadingOut = false;
        fadeElapsedTime = 0f;

        RestoreSpriteAlpha();
        if (circleCollider != null)
        {
            circleCollider.enabled = true;
        }
    }

    private bool IsMouseOverThisCircle(Mouse mouse)
    {
        if (circleCollider == null || Camera.main == null)
        {
            return false;
        }

        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouse.position.ReadValue());
        return circleCollider.OverlapPoint(mouseWorldPos);
    }

    private void StartFadeOut()
    {
        if (isFadingOut)
        {
            return;
        }

        isFadingOut = true;
        fadeElapsedTime = 0f;

        if (circleCollider != null)
        {
            circleCollider.enabled = false;
        }

        if (fadeDuration <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void UpdateFadeOut()
    {
        if (fadeDuration <= 0f)
        {
            return;
        }

        fadeElapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(fadeElapsedTime / fadeDuration);
        float alpha = 1f - t;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            SpriteRenderer renderer = spriteRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }

        // Fade out the index label text as well
        if (indexLabel != null)
        {
            Color textColor = indexLabel.color;
            textColor.a = alpha;
            indexLabel.color = textColor;
        }

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private void RestoreSpriteAlpha()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            SpriteRenderer renderer = spriteRenderers[i];
            if (renderer == null)
            {
                continue;
            }

            Color color = renderer.color;
            color.a = 1f;
            renderer.color = color;
        }

        // Restore index label text alpha
        if (indexLabel != null)
        {
            Color textColor = indexLabel.color;
            textColor.a = 1f;
            indexLabel.color = textColor;
        }
    }

}