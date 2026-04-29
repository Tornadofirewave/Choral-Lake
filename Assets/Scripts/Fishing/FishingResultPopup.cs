using UnityEngine;
using TMPro;

public class FishingResultPopup : MonoBehaviour {
    [SerializeField] private TMP_Text popupText;
    [SerializeField, Min(0f)] private float fadeInDuration = 0.25f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.75f;

    private float elapsedTime;
    private float lifetime;
    private bool isInitialized;
    private Color baseColor;

    private void Awake()
    {
        if (popupText == null)
        {
            popupText = GetComponentInChildren<TMP_Text>(true);
        }
    }

    private void Update()
    {
        if (!isInitialized || popupText == null)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        float alpha;
        if (elapsedTime <= fadeInDuration)
        {
            alpha = fadeInDuration > 0f ? Mathf.Clamp01(elapsedTime / fadeInDuration) : 1f;
        }
        else
        {
            float fadeOutTime = elapsedTime - fadeInDuration;
            alpha = fadeOutDuration > 0f ? 1f - Mathf.Clamp01(fadeOutTime / fadeOutDuration) : 0f;
        }

        SetAlpha(alpha);

        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void Show(string message, Color color)
    {
        if (popupText == null)
        {
            return;
        }

        fadeInDuration = Mathf.Max(0f, fadeInDuration);
        fadeOutDuration = Mathf.Max(0f, fadeOutDuration);
        lifetime = fadeInDuration + fadeOutDuration;
        elapsedTime = 0f;
        isInitialized = true;

        popupText.text = message;
        baseColor = color;
        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        if (popupText == null)
        {
            return;
        }

        Color color = baseColor;
        color.a = alpha;
        popupText.color = color;
    }
}