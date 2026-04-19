using System;
using UnityEngine;
using ChoralLake.Data;

namespace ChoralLake.Core
{
    public class GameManager : MonoBehaviour
    {
        public const string LEGENDARY_ROD_ID = "rod_legendary";
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameDatabase database;
        public GameDatabase Database => database;

        public PlayerSaveData SaveData { get; private set; }
        public ChoralLake.Dialogue.DialogueDatabase DialogueDatabase { get; private set; }

        public const string DEFAULT_STARTING_LAKE_ID = "lake_01";

        public event Action OnMoneyChanged;
        public event Action OnInventoryChanged;
        public event Action OnUniqueFishCountChanged;
        public event Action OnEquippedRodChanged;
        public event Action OnEquippedBaitChanged;
        public event Action OnLakeUnlocked;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SaveData = PlayerSaveData.NewGame();
            // TODO: Save/Load — replace with disk load or new game based on title-screen choice.
            DialogueDatabase = new ChoralLake.Dialogue.DialogueDatabase();
            DialogueDatabase.LoadFromResources("dialogue");
        }

        // --- Money ---
        public void AddMoney(int amount)
        {
            if (amount == 0) return;
            SaveData.money += amount;
            OnMoneyChanged?.Invoke();
        }

        public bool TrySpendMoney(int amount)
        {
            if (amount < 0) return false;
            if (SaveData.money < amount) return false;
            SaveData.money -= amount;
            OnMoneyChanged?.Invoke();
            return true;
        }

        // --- Fish inventory ---
        public void AddFishToInventory(string fishId)
        {
            if (string.IsNullOrEmpty(fishId)) return;
            SaveData.unsoldFishIds.Add(fishId);
            bool isNewSpecies = !SaveData.uniqueFishCaughtIds.Contains(fishId);
            if (isNewSpecies)
            {
                SaveData.uniqueFishCaughtIds.Add(fishId);
            }
            OnInventoryChanged?.Invoke();
            if (isNewSpecies) OnUniqueFishCountChanged?.Invoke();
        }

        public int SellAllFish()
        {
            int total = 0;
            foreach (var id in SaveData.unsoldFishIds)
            {
                var fish = database != null ? database.GetFishById(id) : null;
                if (fish != null)
                {
                    total += fish.SellCost;
                }
                else
                {
                    Debug.LogWarning($"[GameManager] SellAllFish could not resolve fish ID '{id}'. It will be sold for 0.");
                }
            }
            SaveData.unsoldFishIds.Clear();
            OnInventoryChanged?.Invoke();
            if (total != 0)
            {
                SaveData.money += total;
                OnMoneyChanged?.Invoke();
            }
            return total;
        }

        public int UniqueFishCount => SaveData.uniqueFishCaughtIds.Count;

        // --- Rods ---
        public void GrantRod(string rodId)
        {
            if (string.IsNullOrEmpty(rodId)) return;
            if (SaveData.ownedRodIds.Contains(rodId)) return;
            SaveData.ownedRodIds.Add(rodId);
        }

        public bool OwnsRod(string rodId) => SaveData.ownedRodIds.Contains(rodId);

        public void EquipRod(string rodId)
        {
            if (!OwnsRod(rodId))
            {
                Debug.LogWarning($"[GameManager] Tried to equip rod '{rodId}' which the player does not own.");
                return;
            }
            if (SaveData.equippedRodId == rodId) return;
            SaveData.equippedRodId = rodId;
            OnEquippedRodChanged?.Invoke();
        }

        // --- Bait ---
        public void GrantBait(string baitId, int count)
        {
            if (string.IsNullOrEmpty(baitId) || count <= 0) return;
            var stack = SaveData.baitInventory.Find(s => s.baitId == baitId);
            if (stack == null)
            {
                SaveData.baitInventory.Add(new PlayerSaveData.BaitStack { baitId = baitId, count = count });
            }
            else
            {
                stack.count += count;
            }
            OnInventoryChanged?.Invoke();
        }

        public bool ConsumeBait(string baitId)
        {
            var stack = SaveData.baitInventory.Find(s => s.baitId == baitId);
            if (stack == null || stack.count <= 0) return false;
            stack.count--;
            if (stack.count == 0)
            {
                SaveData.baitInventory.Remove(stack);
                if (SaveData.equippedBaitId == baitId)
                {
                    SaveData.equippedBaitId = string.Empty;
                    OnEquippedBaitChanged?.Invoke();
                }
            }
            OnInventoryChanged?.Invoke();
            return true;
        }

        public int GetBaitCount(string baitId)
        {
            var stack = SaveData.baitInventory.Find(s => s.baitId == baitId);
            return stack?.count ?? 0;
        }

        public void EquipBait(string baitId)
        {
            // Empty string means "unequip".
            if (!string.IsNullOrEmpty(baitId) && GetBaitCount(baitId) <= 0)
            {
                Debug.LogWarning($"[GameManager] Tried to equip bait '{baitId}' with zero count.");
                return;
            }
            if (SaveData.equippedBaitId == baitId) return;
            SaveData.equippedBaitId = baitId;
            OnEquippedBaitChanged?.Invoke();
        }

        // --- Lakes ---
        public void UnlockLake(string lakeId)
        {
            if (string.IsNullOrEmpty(lakeId)) return;
            if (SaveData.unlockedLakeIds.Contains(lakeId)) return;
            SaveData.unlockedLakeIds.Add(lakeId);
            OnLakeUnlocked?.Invoke();
        }

        public bool IsLakeUnlocked(string lakeId) => SaveData.unlockedLakeIds.Contains(lakeId);

        // --- Ticket System ---
        public event Action OnPendingTicketChanged;

        public bool HasPendingTicket => !string.IsNullOrEmpty(SaveData.pendingTicketId);

        public bool TryPurchaseTicket(TicketSO ticket)
        {
            if (ticket == null) return false;
            if (HasPendingTicket)
            {
                Debug.LogWarning("[GameManager] TryPurchaseTicket called while a ticket is already pending.");
                return false;
            }
            if (!TrySpendMoney(ticket.Price)) return false;

            var attendant = database?.GetRandomAttendant();
            SaveData.pendingTicketId = ticket.Id;
            SaveData.pendingAttendantId = attendant != null ? attendant.Id : string.Empty;
            SaveData.pendingShipPhase = TicketShipPhase.Approaching;
            OnPendingTicketChanged?.Invoke();
            return true;
        }

        public void ClearPendingTicket()
        {
            SaveData.pendingTicketId = string.Empty;
            SaveData.pendingAttendantId = string.Empty;
            SaveData.pendingShipPhase = TicketShipPhase.None;
            OnPendingTicketChanged?.Invoke();
        }

        public LootResult RollPendingTicketReward()
        {
            var ticket = database?.GetTicketById(SaveData.pendingTicketId);
            if (ticket == null || ticket.LootTable == null)
            {
                Debug.LogError("[GameManager] RollPendingTicketReward: ticket or loot table not found.");
                return default;
            }

            var result = ticket.LootTable.Roll(OwnsRod(LEGENDARY_ROD_ID));

            if (result.Kind == LootKind.Rod)
                GrantRod(result.Id);
            else
                GrantBait(result.Id, 1);

            return result;
        }

        // --- Scene load hook ---
        /// <summary>
        /// Fired by SceneLoader once the player is positioned in the new scene.
        /// Subscribe to react to scene-ready state (NPC spawns, HUD refresh, etc.).
        /// </summary>
        public event Action OnSceneLoadComplete;

        public void NotifySceneLoadComplete()
        {
            OnSceneLoadComplete?.Invoke();
        }
    }
}
