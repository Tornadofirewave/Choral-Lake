# Ticket System

## Overview
Player spends money at the Ticket Booth to summon a lootbox ship. The ship docks, an NPC walks out, the player talks to them, and receives one randomized reward.

## Purchase Flow
1. Player presses E on `TicketBooth` → `TicketBoothUI.Open()`.
2. Player selects a rarity tier. `GameManager.TryPurchaseTicket(ticket)`:
   - Deducts `ticket.Price` via `TrySpendMoney`.
   - Pre-rolls the loot (`ticket.LootTable.Roll`). Stores `pendingRewardId` + `pendingRewardKind`.
   - If result is `rod_legendary`: selects `GameDatabase.legendaryAttendant`. Otherwise selects a random `NpcAttendantSO` from `GameDatabase.attendants`.
   - Writes `pendingTicketId`, `pendingAttendantId`, `pendingRewardId`, `pendingRewardKind`, `pendingShipPhase` to `PlayerSaveData`.
   - Fires `OnPendingTicketChanged`.
3. UI closes. `TicketShipSpawner` in Town scene detects the pending ticket and spawns `TicketShip.prefab`.

## Ship State Machine (`TicketShip.cs`)
| Phase | Behaviour |
|---|---|
| `Approaching` | Moves right toward dock position via Rigidbody2D.linearVelocity |
| `Docked` | Plays `sfx_ship_dock`, spawns `ShipAttendant` |
| `AwaitingDialogue` | Idle; waits for attendant callback |
| `Departing` | Moves left until off-screen; calls `GameManager.ClearPendingTicket()`, destroys self |

Phase is mirrored to `PlayerSaveData.pendingShipPhase` on every transition, enabling reload recovery.

## NPC Attendant (`ShipAttendant.cs`)
- Walks from ship deck to `attendantExitPoint`. Not interactable in transit.
- Becomes `IInteractable` on arrival. Prompt: "Talk to {name}".
- On interact: subscribes to `DialogueManager.OnConversationEnded`, starts `so.DialogueConversationId`.
- On conversation closed: rolls reward → grants via `GameManager` → shows `RewardPopup` → walks back to ship.
- `onDone` callback notifies `TicketShip` to begin departure.

## Loot Roll (`TicketLootTableSO.Roll`)
- Weighted random pick from `List<LootEntry>`.
- If picked entry is `LootKind.Rod` and `legendaryRodOwned == true`, returns `fallbackBaitIdIfRodOwned` as bait instead.
- Roll happens at **purchase time** (`TryPurchaseTicket`), result stored in `SaveData.pendingRewardId/Kind`.
- `GameManager.RollPendingTicketReward()` reads the stored result and grants it via `GrantRod`/`GrantBait` — no second roll.

## Legendary Rod Removal
- Only the legendary ticket's loot table contains a rod entry (`rod_legendary`).
- `GameManager.OwnsRod("rod_legendary")` is checked at roll time.
- Once owned, the rod slot yields `fallbackBaitIdIfRodOwned` (e.g., `bait_legendary`) forever.

## One-at-a-Time Gate
- `GameManager.HasPendingTicket` blocks new purchases.
- `TicketBoothUI` disables all 4 buttons while a ship is active, showing a status message.
- `TicketShipSpawner` skips spawn if `_liveShip != null`.

## Persistence
Purchase deducts money immediately. If the player quits mid-voyage:
- `pendingTicketId` / `pendingAttendantId` / `pendingRewardId` / `pendingRewardKind` / `pendingShipPhase` survive in `PlayerSaveData`.
- On next load to Town, `TicketShipSpawner.OnEnable` + `OnSceneLoadComplete` re-spawns the ship.

## Reward Popup (`RewardPopup.cs`)
- World-space Canvas, `DontDestroyOnLoad`, follows `PlayerRoot.Instance` via `LateUpdate`.
- `Show(Sprite, string)` fades in (0.25s) → holds (1.5s) → fades out (0.25s). Non-blocking.

## Audio IDs (register clips in `SfxLibrary.asset`)
| ID | Trigger |
|---|---|
| `sfx_ticket_purchase` | Ticket button clicked successfully |
| `sfx_ticket_denied` | Purchase failed (shouldn't occur normally) |
| `sfx_ship_horn` | (Optional) Ship appears off-screen |
| `sfx_ship_dock` | Ship arrives at dock position |
| `sfx_ship_depart` | Ship begins leaving |
| `sfx_reward_chime_common` | Common bait received |
| `sfx_reward_chime_rare` | Rare bait received |
| `sfx_reward_chime_epic` | Epic bait received |
| `sfx_reward_chime_legendary` | Legendary bait or rod received |

## Adding a New Ticket Tier
1. Create `TicketSO` asset under `Assets/ScriptableObjects/Tickets/`.
2. Create a `TicketLootTableSO` asset and populate loot entries with bait/rod IDs + weights.
3. Assign loot table to the TicketSO.
4. Add the TicketSO to `GameDatabase.tickets`.
5. Add a new slot in the `TicketBoothUI` prefab and assign the SO.

## NPC Attendant Appearances
`ShipAttendant` drives two animator parameters at runtime:
- `IsMoving` (bool) — true while walking, false while idle.
- `MoveY` (float) — `+1` = walking up (returning to ship), `-1` = walking down (exiting). Locked to `+1` in idle (face-up).

Each `NpcAttendantSO.AnimatorController` must expose these two parameters and include states: **Walk Down**, **Walk Up**, **Idle Up**.

Attendant pool:
- `GameDatabase.attendants` — random pool used for non-legendary rewards.
- `GameDatabase.legendaryAttendant` — specific NPC used when the pre-rolled reward is `rod_legendary`.

## Adding a New NPC Attendant
1. Create `NpcAttendantSO` asset under `Assets/ScriptableObjects/Attendants/`.
2. Create an `AnimatorController` with `IsMoving` (bool) and `MoveY` (float) parameters. Add Idle Up, Walk Down, Walk Up states with appropriate clip transitions.
3. Set `dialogueConversationId` to a unique string.
4. Add conversation rows to `Assets/Resources/dialogue.csv` for that id.
5. For regular attendants: add the SO to `GameDatabase.attendants`. For the legendary attendant: assign to `GameDatabase.legendaryAttendant`.
