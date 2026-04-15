using UnityEngine;

public class CircleGeneration : MonoBehaviour {
	[SerializeField] private FishingCircleSettings settings;
	[SerializeField] private Transform spawnParent;

	private float spawnTimer;
	private int spawnedCount;

	private void Start()
	{
		spawnParent = spawnParent == null ? transform : spawnParent;
	}

	private void Update()
	{
		if (!CanSpawnMore())
		{
			return;
		}

		spawnTimer += Time.deltaTime;
		if (spawnTimer < settings.SpawnInterval)
		{
			return;
		}

		spawnTimer = 0f;
		SpawnCircle();
	}

	private bool CanSpawnMore()
	{
		if (settings == null || settings.CirclePrefab == null)
		{
			return false;
		}

		return spawnedCount < settings.CirclesToSpawn;
	}

	private void SpawnCircle()
	{
		Vector2 randomLocalPos = new Vector2(
			Random.Range(settings.SpawnMin.x, settings.SpawnMax.x),
			Random.Range(settings.SpawnMin.y, settings.SpawnMax.y));

		Vector3 spawnWorldPos = transform.TransformPoint(new Vector3(randomLocalPos.x, randomLocalPos.y, 0f));
		FishingCircle circle = Instantiate(settings.CirclePrefab, spawnWorldPos, Quaternion.identity, spawnParent);
		circle.Initialize(settings.TimeDuration, settings.PerfectWindow, settings.DebugGraceWindowLogs);

		spawnedCount++;
	}
}