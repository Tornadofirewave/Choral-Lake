# Save System

## Format
JSON file at `Application.persistentDataPath/save.json`.  
Serialized via `JsonUtility` from `PlayerSaveData`.

## What is saved

| Field | Description |
|-------|-------------|
| `money` | Current currency |
| `currentSceneName` | Active scene on save |
| `playerPosition` | World position (Vector2) |
| `equippedRodId` | Currently equipped rod |
| `equippedBaitId` | Currently equipped bait |
| `ownedRodIds` | All obtained rods |
| `baitInventory` | List of BaitStack (id + count) |
| `unsoldFishIds` | Fish currently held (sellable) |
| `uniqueFishCaughtIds` | Compendium — all species ever caught |
| `unlockedLakeIds` | Accessible lakes |
| `pendingTicketId/AttendantId/ShipPhase` | In-progress ticket event state |

## Compendium
`uniqueFishCaughtIds` is the compendium. A fish species is "discovered" when it first enters `unsoldFishIds`.  
Check via `SaveData.uniqueFishCaughtIds.Contains(fishId)` — true = caught at least once.

## API

```csharp
SaveSystem.Save(data);        // write to disk
SaveSystem.Load();            // returns PlayerSaveData or null
SaveSystem.HasSave();         // true if file exists
SaveSystem.Delete();          // wipe save (new game / debug)
GameManager.Instance.SaveGame(); // convenience wrapper
```

## Save triggers
- **Scene transition** — `NotifySceneLoadComplete()` saves after player is positioned
- **App quit** — `OnApplicationQuit()` saves as fallback
- **Manual** — call `GameManager.Instance.SaveGame()` after high-stakes events (ticket purchase, etc.)
