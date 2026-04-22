using ChoralLake.Core;
using ChoralLake.Data;
using UnityEngine;

/// <summary>
/// Main fishing game controller. Spawns circles, tracks player success,
/// and rolls fish rewards based on performance and equipped rod.
/// Instantiate this in each lake's fishing scene with LakeSO and FishingCircleSettings assigned.
/// </summary>
public class FishingGame : MonoBehaviour {
	[SerializeField] private FishingCircleSettings settings;
	private Transform spawnParent;
	[SerializeField] private LakeSO currentLake;
	
	private float spawnTimer;
	private int spawnedCount;
	private int circlesCompleted; // Total circles deleted (success or miss)
	private int successfulClicks; // Circles clicked successfully in grace window


	private void Start()
	{
		spawnParent = spawnParent == null ? transform : spawnParent;
	}

	private void Update()
	{
		// If all circles have been handled, check if session is complete
		if (circlesCompleted >= settings.CirclesToSpawn)
		{
			OnFishingSessionComplete();
			return;
		}

		// Spawn new circles until we've reached the target count
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
		
		// Subscribe to circle completion
		circle.OnCircleCompleted += OnCircleCompleted;

		spawnedCount++;
	}

	private void OnCircleCompleted(bool wasSuccessful)
	{
		circlesCompleted++;
		if (wasSuccessful)
		{
			successfulClicks++;
		}
		// Debug
		Debug.Log($"[FishingGame] Circle completed: {circlesCompleted}/{settings.CirclesToSpawn}, Successes: {successfulClicks}");
	}

	private void OnFishingSessionComplete()
	{
		float successRate = (float)successfulClicks / settings.CirclesToSpawn;
		bool metThreshold = successRate >= settings.CompletionThreshold;

		Debug.Log($"[FishingGame] Fishing session complete! Success rate: {successRate:P0} (threshold: {settings.CompletionThreshold:P0}). " +
			$"Met threshold: {metThreshold}");

		if (metThreshold)
		{
			RollAndGrantFishReward();
		}

		// TODO: End scene, show results, return to town, etc.
		enabled = false;
	}

	private void RollAndGrantFishReward()
	{
		var gm = GameManager.Instance;
		if (gm == null || currentLake == null)
		{
			Debug.LogError("[FishingGame] GameManager or currentLake is null.");
			return;
		}

		// Get equipped rod to determine rarity weights
		var rod = gm.Database?.GetRodById(gm.SaveData.equippedRodId);
		if (rod == null)
		{
			Debug.LogWarning("[FishingGame] No rod equipped.");
			return;
		}

		// Roll a random fish from this lake weighted by rod's rarity distribution
		var fish = PickRandomFishByRodWeights(rod);
		if (fish != null)
		{
			gm.AddFishToInventory(fish.Id);
			Debug.Log($"[FishingGame] Granted fish: {fish.DisplayName} ({fish.Rarity})");
		}
		else
		{
			Debug.LogWarning("[FishingGame] Failed to roll a fish.");
		}
	}

	private FishEntry PickRandomFishByRodWeights(FishingRodSO rod)
	{
		var gm = GameManager.Instance;
		if (currentLake == null || gm?.Database?.FishDatabase == null)
			return null;

		var db = gm.Database;
		var fishDb = db.FishDatabase;

		// Group fish by rarity
		var commonFish = currentLake.GetFishOfRarity(Rarity.Common, fishDb);
		var rareFish = currentLake.GetFishOfRarity(Rarity.Rare, fishDb);
		var epicFish = currentLake.GetFishOfRarity(Rarity.Epic, fishDb);
		var legendaryFish = currentLake.GetFishOfRarity(Rarity.Legendary, fishDb);

		// Get rarity weights from rod
		float commonWeight = rod.GetWeight(Rarity.Common);
		float rareWeight = rod.GetWeight(Rarity.Rare);
		float epicWeight = rod.GetWeight(Rarity.Epic);
		float legendaryWeight = rod.GetWeight(Rarity.Legendary);

		float totalWeight = commonWeight + rareWeight + epicWeight + legendaryWeight;
		if (totalWeight <= 0f)
		{
			Debug.LogWarning("[FishingGame] Rod has zero weight. Cannot pick fish.");
			return null;
		}

		// Roll weighted random pick
		float roll = Random.Range(0f, totalWeight);
		float accumulator = 0f;

		accumulator += commonWeight;
		if (roll < accumulator && commonFish.Count > 0)
			return commonFish[Random.Range(0, commonFish.Count)];

		accumulator += rareWeight;
		if (roll < accumulator && rareFish.Count > 0)
			return rareFish[Random.Range(0, rareFish.Count)];

		accumulator += epicWeight;
		if (roll < accumulator && epicFish.Count > 0)
			return epicFish[Random.Range(0, epicFish.Count)];

		accumulator += legendaryWeight;
		if (roll < accumulator && legendaryFish.Count > 0)
			return legendaryFish[Random.Range(0, legendaryFish.Count)];

		// Fallback: return any available fish
		if (commonFish.Count > 0) return commonFish[Random.Range(0, commonFish.Count)];
		if (rareFish.Count > 0) return rareFish[Random.Range(0, rareFish.Count)];
		if (epicFish.Count > 0) return epicFish[Random.Range(0, epicFish.Count)];
		if (legendaryFish.Count > 0) return legendaryFish[Random.Range(0, legendaryFish.Count)];

		return null;
	}
}