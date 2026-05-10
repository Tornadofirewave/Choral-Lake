using System.Collections.Generic;
using UnityEngine;
using ChoralLake.Core;

namespace ChoralLake.Data
{
    [CreateAssetMenu(fileName = "GameDatabase", menuName = "Choral Lake/Game Database")]
    public class GameDatabase : ScriptableObject
    {
        [Header("Aggregated Sub-Databases")]
        [SerializeField] private FishDatabase fishDatabase;
        [SerializeField] private BaitDatabase baitDatabase;

        [Header("Per-Asset Content")]
        [SerializeField] private List<FishingRodSO> rods = new();
        [SerializeField] private List<LakeSO> lakes = new();

        [Header("Ticket System")]
        [SerializeField] private List<TicketSO> tickets = new();
        [SerializeField] private List<NpcAttendantSO> attendants = new();
        [SerializeField] private NpcAttendantSO legendaryAttendant;

        private Dictionary<string, FishingRodSO> _rodsById;
        private Dictionary<string, LakeSO> _lakesById;
        private Dictionary<string, TicketSO> _ticketsById;
        private Dictionary<string, NpcAttendantSO> _attendantsById;

        public FishDatabase FishDatabase => fishDatabase;
        public BaitDatabase BaitDatabase => baitDatabase;
        public IReadOnlyList<FishingRodSO> AllRods => rods;
        public IReadOnlyList<LakeSO> AllLakes => lakes;
        public IReadOnlyList<TicketSO> AllTickets => tickets;
        public IReadOnlyList<NpcAttendantSO> AllAttendants => attendants;

        public FishEntry GetFishById(string id) => fishDatabase != null ? fishDatabase.GetById(id) : null;
        public BaitEntry GetBaitById(string id) => baitDatabase != null ? baitDatabase.GetById(id) : null;
        public FishingRodSO GetRodById(string id) => Lookup(_rodsById, id, "rod");
        public LakeSO GetLakeById(string id) => Lookup(_lakesById, id, "lake");
        public TicketSO GetTicketById(string id) => Lookup(_ticketsById, id, "ticket");
        public NpcAttendantSO GetAttendantById(string id) => Lookup(_attendantsById, id, "attendant");

        public NpcAttendantSO GetLegendaryAttendant() => legendaryAttendant;

        public NpcAttendantSO GetRandomAttendant()
        {
            if (attendants == null || attendants.Count == 0) return null;
            return attendants[UnityEngine.Random.Range(0, attendants.Count)];
        }

        private void OnEnable() => RebuildCaches();

        private void RebuildCaches()
        {
            _rodsById      = BuildCache(rods,      "rod");
            _lakesById     = BuildCache(lakes,     "lake");
            _ticketsById   = BuildCache(tickets,   "ticket");
            _attendantsById = BuildCache(attendants, "attendant");
        }

        private Dictionary<string, T> BuildCache<T>(List<T> source, string category)
            where T : ScriptableObject, IIdentifiable
        {
            var dict = new Dictionary<string, T>();
            foreach (var entry in source)
            {
                if (entry == null) continue;
                if (string.IsNullOrEmpty(entry.Id))
                {
                    Debug.LogError($"[GameDatabase] {category} entry '{entry.name}' has an empty ID.", entry);
                    continue;
                }
                if (dict.ContainsKey(entry.Id))
                {
                    Debug.LogError($"[GameDatabase] Duplicate {category} ID '{entry.Id}' on '{entry.name}'. Previous entry overwritten.", entry);
                }
                dict[entry.Id] = entry;
            }
            return dict;
        }

        private T Lookup<T>(Dictionary<string, T> cache, string id, string category) where T : class
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (cache != null && cache.TryGetValue(id, out var result)) return result;
            Debug.LogError($"[GameDatabase] No {category} found with ID '{id}'.");
            return null;
        }
    }
}
