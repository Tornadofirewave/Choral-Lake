using ChoralLake.Core;
using ChoralLake.Data;
using ChoralLake.SceneManagement;
using ChoralLake.Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Main fishing game controller. Spawns circles, tracks player success,
/// and rolls fish rewards based on performance and equipped rod.
/// Instantiate this in each lake's fishing scene with LakeSO and FishingCircleSettings assigned.
/// </summary>
public class FishingGame : MonoBehaviour {
	[SerializeField] private FishingGameSettings settings;
	private Transform spawnParent;
	[SerializeField] private LakeSO currentLake;
	[SerializeField, Min(0f)] private float spawnSpacingBuffer = 0.1f;
	
	private float spawnTimer;
	private int spawnedCount;
	private readonly List<FishingCircle> activeCircles = new List<FishingCircle>();
	private int circlesCompleted; // Total circles deleted (success or miss)
	private float totalScore; // Totaled score based on Perfects, Goods, and Bads
	// private bool fishAwarded;
	private bool sessionCompleted;
	private bool playerWasHidden;


	private void Start()
	{
		spawnParent = spawnParent != null ? spawnParent : transform;
		if (currentLake == null)
		{
			currentLake = SceneFishing.ActiveLake;
		}

		HidePlayerForMinigame();
	}

	private void OnDestroy()
	{
		RestorePlayerVisibility();
	}

	private void Update()
	{
		if (Keyboard.current != null && Keyboard.current.backquoteKey.wasPressedThisFrame)
		{
			ReturnToLake();
			return;
		}

		// If all circles have been handled, check if session is complete
		if (!sessionCompleted && circlesCompleted >= settings.CirclesToSpawn)
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
		const int maxSpawnAttempts = 20;

		for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
		{
			Vector2 randomLocalPos = new Vector2(
				Random.Range(settings.SpawnMin.x, settings.SpawnMax.x),
				Random.Range(settings.SpawnMin.y, settings.SpawnMax.y));

			Vector3 spawnWorldPos = transform.TransformPoint(new Vector3(randomLocalPos.x, randomLocalPos.y, 0f));
			FishingCircle circle = Instantiate(settings.CirclePrefab, spawnWorldPos, Quaternion.identity, spawnParent);
			circle.Initialize(settings.TimeDuration, settings.PerfectWindow, settings.DebugGraceWindowLogs);

			float circleRadius = GetCircleRadius(circle);
			if (!IsSpawnPositionClear(spawnWorldPos, circleRadius))
			{
				Destroy(circle.gameObject);
				continue;
			}

			activeCircles.Add(circle);
			FishingCircle spawnedCircle = circle;
			circle.OnCircleCompleted += (wasSuccessful, status) => OnCircleCompleted(spawnedCircle, wasSuccessful, status);

			spawnedCount++;
			return;
		}

		Debug.LogWarning("[FishingGame] Failed to find a valid spawn position for a circle.");
	}

	private void OnCircleCompleted(FishingCircle completedCircle, bool wasSuccessful, FishingCircleResult status)
	{
		if (completedCircle != null)
		{
			activeCircles.Remove(completedCircle);
		}

		SpawnResultPopup(completedCircle, status);

		circlesCompleted++;
		if (wasSuccessful)
		{
			switch (status)
			{
				case FishingCircleResult.Bad:
					totalScore += 0.3f * 10;
					break;
				case FishingCircleResult.Good:
					totalScore += 0.5f * 10;
					break;
				case FishingCircleResult.Perfect:
					totalScore += 1.0f * 10;
					break;
				default:
					break;
			}
		}
		// Debug
		Debug.Log($"[FishingGame] Circle completed: {circlesCompleted}/{settings.CirclesToSpawn}, Total: {totalScore}, {statusType(status)}");
	}

	private void SpawnResultPopup(FishingCircle completedCircle, FishingCircleResult status)
	{
		if (settings == null || settings.TextPopupPrefab == null || completedCircle == null)
		{
			return;
		}

		Vector3 spawnPosition = GetPopupSpawnPosition(completedCircle.transform.position);
		FishingResultPopup popup = Instantiate(settings.TextPopupPrefab, spawnPosition, Quaternion.identity, spawnParent);
		popup.Show(statusType(status), GetStatusColor(status));
	}

	private Vector3 GetPopupSpawnPosition(Vector3 origin)
	{
		float halfAngle = settings.PopupConeHalfAngle;
		float randomAngle = Random.Range(-halfAngle, halfAngle);
		float distance = Random.Range(settings.PopupMinDistance, settings.PopupMaxDistance);
		Vector2 direction = Quaternion.Euler(0f, 0f, randomAngle) * Vector2.up;

		return origin + new Vector3(direction.x, direction.y, 0f) * distance;
	}

	private Color GetStatusColor(FishingCircleResult status)
	{
		return status switch
		{
			FishingCircleResult.Bad => settings.BadPopupColor,
			FishingCircleResult.Good => settings.GoodPopupColor,
			FishingCircleResult.Perfect => settings.PerfectPopupColor,
			_ => settings.MissPopupColor,
		};
	}

	private bool IsSpawnPositionClear(Vector3 candidatePosition, float candidateRadius)
	{
		activeCircles.RemoveAll(circle => circle == null);

		for (int i = 0; i < activeCircles.Count; i++)
		{
			FishingCircle existingCircle = activeCircles[i];
			float existingRadius = GetCircleRadius(existingCircle);
			float minimumDistance = existingRadius + candidateRadius + spawnSpacingBuffer;

			if (Vector3.Distance(candidatePosition, existingCircle.transform.position) < minimumDistance)
			{
				return false;
			}
		}

		return true;
	}

	private float GetCircleRadius(FishingCircle circle)
	{
		if (circle == null)
		{
			return 0f;
		}

		Collider2D circleCollider = circle.GetComponent<Collider2D>();
		if (circleCollider == null)
		{
			return 0f;
		}

		Bounds bounds = circleCollider.bounds;
		return Mathf.Max(bounds.extents.x, bounds.extents.y);
	}

	private string statusType(FishingCircleResult status)
	{
        return status switch
        {
            FishingCircleResult.Bad => "Bad",
            FishingCircleResult.Good => "Good",
            FishingCircleResult.Perfect => "Perfect",
            _ => "Miss",
        };
    }

	private void OnFishingSessionComplete()
	{
		sessionCompleted = true;

		float successRate = totalScore / (settings.CirclesToSpawn * 10);
		bool metThreshold = successRate >= settings.CompletionThreshold;

		Debug.Log($"[FishingGame] Fishing session complete! Success rate: {successRate:P0} (threshold: {settings.CompletionThreshold:P0}). " +
			$"Met threshold: {metThreshold}");

		if (metThreshold)
		{
			RollAndGrantFishReward();
		}

		if (!metThreshold)
		{
			Debug.Log("[FishingGame] Threshold not met. No fish awarded.");
		}
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
			Debug.Log("[FishingGame] Press Escape to return to the lake.");
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

	private void HidePlayerForMinigame()
	{
		var player = PlayerRoot.Instance;
		if (player == null || !player.gameObject.activeSelf) return;

		playerWasHidden = true;
		player.gameObject.SetActive(false);
	}

	private void RestorePlayerVisibility()
	{
		if (!playerWasHidden) return;

		var player = PlayerRoot.Instance;
		if (player != null)
		{
			player.gameObject.SetActive(true);
		}
		playerWasHidden = false;
	}

	private void ReturnToLake()
	{
		if (string.IsNullOrEmpty(SceneFishing.ReturnSceneName))
		{
			Debug.LogWarning("[FishingGame] Return scene is unknown. Staying in fishing scene.");
			return;
		}

		bool transitionStarted = SceneLoader.LoadScene(
			SceneFishing.ReturnSceneName,
			SceneFishing.ReturnSpawnId,
			forceIfBusy: true,
			skipPlayerPlacement: true);

		if (transitionStarted)
		{
			RestorePlayerVisibility();
		}
	}
}
