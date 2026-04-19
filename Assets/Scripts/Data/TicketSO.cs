using UnityEngine;
using ChoralLake.Core;

namespace ChoralLake.Data
{
    [CreateAssetMenu(fileName = "Ticket_", menuName = "Choral Lake/Ticket")]
    public class TicketSO : ScriptableObject, IIdentifiable
    {
        [Header("Identity")]
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Rarity rarity;

        [Header("Economy")]
        [SerializeField, Min(0)] private int price;

        [Header("Presentation")]
        [SerializeField] private Sprite icon;
        [TextArea(2, 4)]
        [SerializeField] private string flavorText;

        [Header("Rewards")]
        [SerializeField] private TicketLootTableSO lootTable;

        public string Id => id;
        public string DisplayName => displayName;
        public Rarity Rarity => rarity;
        public int Price => price;
        public Sprite Icon => icon;
        public string FlavorText => flavorText;
        public TicketLootTableSO LootTable => lootTable;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(id))
                Debug.LogWarning($"[TicketSO] '{name}' has no ID set.", this);
        }
    }
}
