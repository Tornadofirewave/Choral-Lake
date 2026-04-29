using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour {
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform fillArea;
    [SerializeField] private RectTransform thresholdMarker;
    [SerializeField, Min(0f)] private float fillLerpSpeed = 12f;

    private float currentValue;
    private float targetValue;
    private float maximum = 1f;
    private float thresholdNormalized = -1f;

    private void Awake()
    {
        if (fillImage == null)
        {
            fillImage = GetComponentInChildren<Image>();
        }

        if (fillArea == null && fillImage != null)
        {
            fillArea = fillImage.rectTransform.parent as RectTransform;
        }
    }
    
    private void Update()
    {
        if (fillImage == null)
        {
            return;
        }

        currentValue = Mathf.MoveTowards(currentValue, targetValue, fillLerpSpeed * Time.deltaTime);
        UpdateFillAmount();
        UpdateThresholdMarker();
    }

    public void SetProgress(float current, float max, float threshold = -1f)
    {
        maximum = Mathf.Max(0.0001f, max);
        targetValue = Mathf.Clamp(current, 0f, maximum);

        if (threshold >= 0f)
        {
            thresholdNormalized = Mathf.Clamp01(threshold);
        }

        if (currentValue > maximum)
        {
            currentValue = maximum;
        }

        UpdateFillAmount();
        UpdateThresholdMarker();
    }

    private void UpdateFillAmount()
    {
        if (fillImage == null)
        {
            return;
        }

        fillImage.fillAmount = Mathf.Clamp01(currentValue / maximum);
    }

    private void UpdateThresholdMarker()
    {
        if (thresholdMarker == null || fillArea == null || thresholdNormalized < 0f)
        {
            return;
        }

        Rect areaRect = fillArea.rect;
        float localX = Mathf.Lerp(areaRect.xMin, areaRect.xMax, thresholdNormalized);
        float localY = areaRect.center.y;
        Vector3 worldPosition = fillArea.TransformPoint(new Vector3(localX, localY, 0f));

        thresholdMarker.position = worldPosition;
    }
}
