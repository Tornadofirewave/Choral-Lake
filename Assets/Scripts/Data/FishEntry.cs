using UnityEngine;
using ChoralLake.Core;

namespace ChoralLake.Data
{
    [System.Serializable]
    public class FishEntry : IIdentifiable
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Rarity rarity;
        [SerializeField, Min(0)] private int sellCost;
        [SerializeField] private Sprite icon;
        [TextArea, SerializeField] private string flavorText;

        public string Id => id;
        public string DisplayName => displayName;
        public Rarity Rarity => rarity;
        public int SellCost => sellCost;
        public Sprite Icon => icon;
        public string FlavorText => flavorText;
    }
}
