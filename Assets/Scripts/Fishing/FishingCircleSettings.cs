using UnityEngine;

[CreateAssetMenu(fileName = "FishingCircleSettings", menuName = "Fishing/Fishing Circle Settings")]
public class FishingCircleSettings : ScriptableObject
{
    [Header("Spawn")]
    [SerializeField] private FishingCircle circlePrefab;
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private int circlesToSpawn = 10;
    [SerializeField] private Vector2 spawnMin = new Vector2(-4f, -2f);
    [SerializeField] private Vector2 spawnMax = new Vector2(4f, 2f);

    [Header("Circle Timing")]
    [SerializeField] private float timeDuration = 3f;
    [SerializeField] private float perfectWindow = 0.25f;
    [SerializeField] private bool debugGraceWindowLogs;

    public FishingCircle CirclePrefab => circlePrefab;
    public float SpawnInterval => Mathf.Max(0f, spawnInterval);
    public int CirclesToSpawn => Mathf.Max(0, circlesToSpawn);
    public Vector2 SpawnMin => spawnMin;
    public Vector2 SpawnMax => spawnMax;
    public float TimeDuration => Mathf.Max(0f, timeDuration);
    public float PerfectWindow => Mathf.Max(0f, perfectWindow);
    public bool DebugGraceWindowLogs => debugGraceWindowLogs;
}
