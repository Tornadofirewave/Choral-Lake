using UnityEngine;

[CreateAssetMenu(fileName = "FishingGameSettings", menuName = "FishingGameSettings")]
public class FishingGameSettings : ScriptableObject
{
    [Header("Spawn")]
    [SerializeField] private FishingCircle circlePrefab;
    [SerializeField] private float spawnInterval;
    [SerializeField] private int circlesToSpawn;
    [SerializeField] private Vector2 spawnMin = new Vector2(-4f, -2f);
    [SerializeField] private Vector2 spawnMax = new Vector2(4f, 2f);

    [Header("Circle Timing")]
    [SerializeField] private float timeDuration;
    [SerializeField] private float perfectWindow;
    [SerializeField] private float completionThreshold;
    [SerializeField] private bool debugGraceWindowLogs;

    [Header("Result Popup")]
    [SerializeField] private FishingResultPopup textPopupPrefab;
    [SerializeField, Min(0f)] private float popupMinDistance = 0.25f;
    [SerializeField, Min(0f)] private float popupMaxDistance = 0.8f;
    [SerializeField, Range(0f, 85f)] private float popupConeHalfAngle = 30f;
    [SerializeField] private Color perfectPopupColor = new Color(0.15f, 0.95f, 0.25f, 1f);
    [SerializeField] private Color goodPopupColor = new Color(1f, 0.92f, 0.2f, 1f);
    [SerializeField] private Color badPopupColor = new Color(0.95f, 0.25f, 0.25f, 1f);
    [SerializeField] private Color missPopupColor = new Color(0.8f, 0.8f, 0.8f, 1f);

    public FishingCircle CirclePrefab => circlePrefab;
    public float SpawnInterval => Mathf.Max(0f, spawnInterval);
    public int CirclesToSpawn => Mathf.Max(0, circlesToSpawn);
    public Vector2 SpawnMin => spawnMin;
    public Vector2 SpawnMax => spawnMax;
    public float TimeDuration => Mathf.Max(0f, timeDuration);
    public float PerfectWindow => Mathf.Max(0f, perfectWindow);
    public float CompletionThreshold => Mathf.Clamp01(completionThreshold);
    public bool DebugGraceWindowLogs => debugGraceWindowLogs;
    public FishingResultPopup TextPopupPrefab => textPopupPrefab;
    public float PopupMinDistance => Mathf.Max(0f, popupMinDistance);
    public float PopupMaxDistance => Mathf.Max(PopupMinDistance, popupMaxDistance);
    public float PopupConeHalfAngle => Mathf.Clamp(popupConeHalfAngle, 0f, 85f);
    public Color PerfectPopupColor => perfectPopupColor;
    public Color GoodPopupColor => goodPopupColor;
    public Color BadPopupColor => badPopupColor;
    public Color MissPopupColor => missPopupColor;
}
