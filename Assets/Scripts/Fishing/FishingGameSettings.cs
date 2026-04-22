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

    public FishingCircle CirclePrefab => circlePrefab;
    public float SpawnInterval => Mathf.Max(0f, spawnInterval);
    public int CirclesToSpawn => Mathf.Max(0, circlesToSpawn);
    public Vector2 SpawnMin => spawnMin;
    public Vector2 SpawnMax => spawnMax;
    public float TimeDuration => Mathf.Max(0f, timeDuration);
    public float PerfectWindow => Mathf.Max(0f, perfectWindow);
    public float CompletionThreshold => Mathf.Clamp01(completionThreshold);
    public bool DebugGraceWindowLogs => debugGraceWindowLogs;
}
