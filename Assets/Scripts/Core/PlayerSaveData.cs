using System.Collections.Generic;
using UnityEngine;

namespace ChoralLake.Core
{
    [System.Serializable]
    public class PlayerSaveData
    {
        [System.Serializable]
        public class BaitStack
        {
            public string baitId;
            public int count;
        }

        public int money;
        public string currentSceneName;
        public Vector2 playerPosition;
        public string equippedRodId;
        public string equippedBaitId;
        public List<string> ownedRodIds;
        public List<BaitStack> baitInventory;
        public List<string> uniqueFishCaughtIds;
        public List<string> unsoldFishIds;
        public List<string> unlockedLakeIds;

        // Ticket ship persistence — money is spent at purchase, so pending state survives quit.
        public string pendingTicketId;
        public string pendingAttendantId;
        public TicketShipPhase pendingShipPhase;

        public PlayerSaveData()
        {
            money = 0;
            currentSceneName = "Town";
            playerPosition = Vector2.zero;
            equippedRodId = string.Empty;
            equippedBaitId = string.Empty;
            ownedRodIds = new List<string>();
            baitInventory = new List<BaitStack>();
            uniqueFishCaughtIds = new List<string>();
            unsoldFishIds = new List<string>();
            unlockedLakeIds = new List<string>();
            pendingTicketId = string.Empty;
            pendingAttendantId = string.Empty;
            pendingShipPhase = TicketShipPhase.None;
        }

        public static PlayerSaveData NewGame()
        {
            var data = new PlayerSaveData();
            data.unlockedLakeIds.Add(GameManager.DEFAULT_STARTING_LAKE_ID);
            return data;
        }
    }
}
