using UnityEngine;
using ChoralLake.Core;

namespace ChoralLake.Data
{
    [System.Serializable]
    public class BaitEntry : IIdentifiable
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Rarity guaranteedRarity;
        [SerializeField] private Sprite icon;

        public string Id => id;
        public string DisplayName => displayName;
        public Rarity GuaranteedRarity => guaranteedRarity;
        public Sprite Icon => icon;
    }
}
