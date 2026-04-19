using System;
using System.Collections.Generic;
using UnityEngine;

namespace ChoralLake.Data
{
    public enum LootKind { Bait, Rod }

    public struct LootResult
    {
        public LootKind Kind;
        public string Id;
    }

    [CreateAssetMenu(fileName = "LootTable_", menuName = "Choral Lake/Ticket Loot Table")]
    public class TicketLootTableSO : ScriptableObject
    {
        [Serializable]
        public class LootEntry
        {
            public LootKind kind;
            [Tooltip("Used when kind == Bait")]
            public string baitId;
            [Tooltip("Used when kind == Rod")]
            public string rodId;
            [Min(0f)]
            public float weight = 1f;
            [Tooltip("Bait ID granted instead if the rod has already been obtained")]
            public string fallbackBaitIdIfRodOwned;
        }

        [SerializeField] private List<LootEntry> entries = new();

        public LootResult Roll(bool legendaryRodOwned)
        {
            if (entries == null || entries.Count == 0)
            {
                Debug.LogWarning("[TicketLootTableSO] Loot table has no entries.");
                return default;
            }

            float total = 0f;
            foreach (var e in entries)
                total += Mathf.Max(0f, e.weight);

            if (total <= 0f)
            {
                Debug.LogWarning("[TicketLootTableSO] All weights are zero.");
                return default;
            }

            float roll = UnityEngine.Random.Range(0f, total);
            float cumulative = 0f;

            foreach (var e in entries)
            {
                cumulative += Mathf.Max(0f, e.weight);
                if (roll < cumulative)
                    return Resolve(e, legendaryRodOwned);
            }

            return Resolve(entries[entries.Count - 1], legendaryRodOwned);
        }

        private static LootResult Resolve(LootEntry e, bool legendaryRodOwned)
        {
            if (e.kind == LootKind.Rod && legendaryRodOwned)
                return new LootResult { Kind = LootKind.Bait, Id = e.fallbackBaitIdIfRodOwned };
            return e.kind == LootKind.Rod
                ? new LootResult { Kind = LootKind.Rod, Id = e.rodId }
                : new LootResult { Kind = LootKind.Bait, Id = e.baitId };
        }
    }
}
